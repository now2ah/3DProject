using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;

public class PlayerStatsForUI
{
    public float maxHP;
    public float curHP;
    public bool hasRifle;
    public bool isAiming;
    public int bulletCount;

    public PlayerStatsForUI(float maxHP, float curHP, bool hasRifle, bool isAiming, int bulletCount)
    {
        this.maxHP = maxHP;
        this.curHP = curHP;
        this.hasRifle = hasRifle;
        this.isAiming = isAiming;
        this.bulletCount = bulletCount;
    }
}

public class Player : MonoBehaviour
{
    [Header("Input")]
    public float mouseSensityvity = 100.0f;

    [Header("Camera")]
    public float thirdPersonDistance = 3.0f;
    public Vector3 thirdPersonOffset = new Vector3(0f, 1.0f, 0f);
    public float zoomDistance = 0.5f;
    public float zoomSpeed = 5.0f;
    public float defaultFov = 60.0f;
    public float zoomFov = 30.0f;

    [Header("Status")]
    public float maxHP = 15f;

    [Header("Move")]
    public float jumpHeight = 2f;
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;

    [Header("Item")]
    public LayerMask itemLayerMask;
    public GameObject flashLightObj;

    [Header("Weapon")]
    public GameObject rifleObject;
    public LayerMask targetLayerMask;
    public ParticleSystem muzzleFlashParticle;
    public ParticleSystem hitParticle;

    //camera
    private Transform _cameraTransform;
    private Camera _mainCamera;
    private float _currentCamDistance;
    private float _targetCamDistance;
    private float _targetFov;
    private Coroutine _zoomCoroutine;

    //jump
    private float _gravity = -9.81f;
    private float _verticalVelocity = 0f;

    //components
    private CharacterController _characterController;
    private Animator _animator;
    private Transform _playerLookTransform;
    private BoxCollider _itemPickUpTrigger;

    //move
    private bool _isRunning = false;
    private float _horizontal;
    private float _vertical;
    private float _pitch = 0.0f;
    private float _yaw = 0.0f;
    private float _currentSpeed = 2.0f;

    private bool _isDead = false;
    private float _currentHP;
    private bool _isNearItem = false;

    public bool IsDead => _isDead;
    
    //shooting
    private bool _isAiming = false;
    private bool _canFire = true;

    //weapon
    private bool _hasRifle = false;
    private float weaponMaxDistance = 100f;
    private float rifleShootDelay = 0.5f;
    private float rifleDamage = 3f;
    private int _bulletCount = 0;

    //item
    private bool _isPicking = false;
    private List<Item> _itemList;
    private bool _hasFlash = false;
    private bool _isFlashOn = false;

    //ambience sound effect
    private float _curTime = 0f;
    private float _nextSfxTime = 1f;

    //coroutine
    private Coroutine _beHitCoroutine = null;
    private Coroutine _checkPickUpCoroutine = null;
    private Coroutine _pickUpCoroutine = null;
    private Coroutine _shootDelayCoroutine = null;

    public event EventHandler<PlayerStatsForUI> OnPlayerStatsChange;
    public event EventHandler OnStartTutorial;
    public event EventHandler OnPickUpLight;
    public event EventHandler OnPickUpRifle;

    private void OnEnable()
    {
        InputManager.Instance.OnLookInput += _ProcessMouseInput;
        InputManager.Instance.OnMoveInput += _ProcessMovement;
        InputManager.Instance.OnEquip1Input += _ProcessChangeWeapon;
        InputManager.Instance.OnFireInput += _Fire;
        InputManager.Instance.OnAimStartInput += _ZoomIn;
        InputManager.Instance.OnAimEndInput += _ZoomOut;
        InputManager.Instance.OnPickUpInput += _CheckPickUp;
        InputManager.Instance.OnJumpInput += _Jump;
        InputManager.Instance.OnLightInput += _LightSwitch;
    }

    private void OnDisable()
    {
        GameManager.Instance.Player = null;
        _UnsubscribeEvents();
    }

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _itemPickUpTrigger = GetComponent<BoxCollider>();
        _itemList = new List<Item>();

        Transform[] transforms = GetComponentsInChildren<Transform>();
        foreach(var tr in transforms)
        {
            if (tr.gameObject.name == "PlayerLookObj")
            {
                _playerLookTransform = tr;
                break;
            }
        }

        _currentHP = maxHP;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _cameraTransform = Camera.main.transform;
        _mainCamera = _cameraTransform.GetComponent<Camera>();
        
        _currentCamDistance = thirdPersonDistance;
        _targetCamDistance = thirdPersonDistance;
        _targetFov = defaultFov;
        
        _mainCamera.fieldOfView = defaultFov;
        rifleObject.SetActive(false);

        OnPlayerStatsChange.Invoke(this, _GetPlayerStats());
    }

    void Update()
    {
        _curTime += Time.deltaTime;
        _PlayZombieAmbienceSounds();
        _SetAnimationParams();
    }

    private void LateUpdate()
    {
        _UpdateCameraPosition();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            _isNearItem = true;
            _PickUpItem(other);
        }
        else if (other.tag == "TutorialObject")
        {
            if (other.name == "StartTutorial" && !_HasItem(EItemType.FLASH_LIGHT))
            {
                OnStartTutorial?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void SetTargetDistance(float distance)
    {
        _targetCamDistance = distance;
    }

    public void SetTargetFov(float fov)
    {
        _targetFov = fov;
    }

    public void ApplyDamage(float damage)
    {
        if (!_isDead)
        {
            _currentHP -= damage;
            OnPlayerStatsChange.Invoke(this, _GetPlayerStats());

            if (_currentHP <= 0) { _Die(); }
        }
    }

    public void BeHit()
    {
        if (_beHitCoroutine == null)
        {
            _beHitCoroutine = StartCoroutine(BeHitCoroutine());
        }
    }

    IEnumerator BeHitCoroutine()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("IsHit");
            AudioManager.Instance.PlaySfx(AudioManager.ESfx.BEHIT);
            yield return null;
            _beHitCoroutine = null;
        }
    }

    //public void CreateItemBoxCast()
    //{
    //    Vector3 origin = pickUpTransform.position;
    //    Vector3 direction = pickUpTransform.forward;
    //    Vector3 boxSize = Vector3.one;
    //    float boxCastDistance = 5f;

    //    //_DrawDebugBox(origin, direction);
    //    RaycastHit[] hits = Physics.BoxCastAll(origin, boxSize / 2, direction, Quaternion.identity, boxCastDistance, itemLayerMask);
    //    foreach (var hit in hits)
    //    {
    //        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Item"))
    //        {
    //            hasRifle = true;
    //            OnPlayerStatsChange.Invoke(this, _GetPlayerStats());
    //        }

    //        hit.collider.gameObject.SetActive(false);
    //    }
    //}

    PlayerStatsForUI _GetPlayerStats()
    {
        return new PlayerStatsForUI(maxHP, _currentHP, _hasRifle, _isAiming, _bulletCount);
    }

    void _UnsubscribeEvents()
    {
        if (OnPlayerStatsChange != null)
        {
            foreach (var d in OnPlayerStatsChange.GetInvocationList())
            {
                OnPlayerStatsChange -= d as EventHandler<PlayerStatsForUI>;
            }
        }

        if (OnStartTutorial != null)
        {
            foreach (var d in OnStartTutorial.GetInvocationList())
            {
                OnStartTutorial -= d as EventHandler;
            }
        }

        if (OnPickUpLight != null)
        {
            foreach (var d in OnPickUpLight.GetInvocationList())
            {
                OnPickUpLight -= d as EventHandler;
            }
        }

        if (OnPickUpRifle != null)
        {
            foreach (var d in OnPickUpRifle.GetInvocationList())
            {
                OnPickUpRifle -= d as EventHandler;
            }
        }
    }

    void _UpdateCameraPosition()
    {
        _currentCamDistance = thirdPersonDistance;
        transform.rotation = Quaternion.Euler(0f, _yaw, 0);

        Vector3 direction = new Vector3(0, 0, -_currentCamDistance);

        Vector3 rayOrigin = transform.position + transform.up * 2f;
        Vector3 rayDirection = _playerLookTransform.position + thirdPersonOffset + Quaternion.Euler(_pitch, _yaw, 0) * direction;

        Ray ray = new Ray(rayOrigin, rayDirection);

        if (Physics.Linecast(rayOrigin, rayDirection, out RaycastHit hit))
        {
            _cameraTransform.position = hit.point;
        }
        else
        {
            _cameraTransform.position = _playerLookTransform.position + thirdPersonOffset + Quaternion.Euler(_pitch, _yaw, 0) * direction;
        }

        _cameraTransform.LookAt(_playerLookTransform.position + new Vector3(0, thirdPersonOffset.y, 0));
    }

    void _ProcessMouseInput(object sender, Vector2 mouseInput)
    {
        if (_isDead)
            return;

        float mouseX = mouseInput.x * mouseSensityvity * Time.deltaTime * 2f;
        float mouseY = mouseInput.y * mouseSensityvity * Time.deltaTime;

        _yaw += mouseX;
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, -45f, 45f);
    }

    void _ProcessMovement(object o, Vector2 inputAxis)
    {
        if (_isDead)
            return;

        _horizontal = inputAxis.x;
        _vertical = inputAxis.y;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _isRunning = true;
            _currentSpeed = runSpeed;
        }
        else
        {
            _isRunning = false;
            _currentSpeed = walkSpeed;
        }

        if (_characterController.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = 0f;

        _verticalVelocity += _gravity * Time.deltaTime;

        Vector3 moveDirection = transform.right * _horizontal + transform.forward * _vertical + transform.up * _verticalVelocity;
        _characterController.Move(moveDirection * _currentSpeed * Time.deltaTime);

        //_UpdateCameraPosition();
    }

    void _ZoomIn(object sender, EventArgs e)
    {
        if (_isDead)
            return;

        if (!_hasRifle)
            return;

        if (_zoomCoroutine != null)
        {
            StopCoroutine(_zoomCoroutine);
        }

        _isAiming = true;
        OnPlayerStatsChange.Invoke(this, _GetPlayerStats());
        _animator.SetLayerWeight(1, 1);
        SetTargetDistance(zoomDistance);
        _zoomCoroutine = StartCoroutine(ZoomCameraCoroutine(_targetCamDistance));
    }

    void _ZoomOut(object sender, EventArgs e)
    {
        if (_isDead)
            return;

        if (!_hasRifle)
            return;

        if (_zoomCoroutine != null)
        {
            StopCoroutine(_zoomCoroutine);
        }

        _isAiming = false;
        OnPlayerStatsChange.Invoke(this, _GetPlayerStats());
        //multiAimConstraint.data.offset = Vector3.zero;
        _animator.SetLayerWeight(1, 0);
        SetTargetDistance(thirdPersonDistance);
        _zoomCoroutine = StartCoroutine(ZoomCameraCoroutine(_targetCamDistance));
    }

    IEnumerator ZoomCameraCoroutine(float targetDistance)
    {
        while(Mathf.Abs(_currentCamDistance - _targetCamDistance) > 0.01f)
        {
            _currentCamDistance = Mathf.Lerp(_currentCamDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        _currentCamDistance = targetDistance;
    }

    void _Fire(object sender, EventArgs e)
    {
        if (_isDead)
            return;

        if (!_isAiming)
            return;

        if (_canFire && _bulletCount > 0)
        {
            _animator.SetTrigger("FireTrigger");

            Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);

            //multi hit
            RaycastHit[] hits = Physics.RaycastAll(ray, weaponMaxDistance, targetLayerMask);
            if (hits.Length > 0)
            {
                int hitCount = 4;
                RaycastHitCompare compare = new RaycastHitCompare();

                Array.Sort(hits, compare);

                for(int i=0; i<hits.Length; ++i)
                {
                    if (hitCount <= 0)
                        break;

                    _CreateRifleHitParticle(hits[i]);

                    if (hits[i].transform.TryGetComponent<Enemy>(out Enemy enemy))
                    {
                        enemy.ApplyDamage(rifleDamage);
                    }

                    if (hits[i].collider.transform.tag == "Seperatable")
                    {
                        hits[i].collider.transform.gameObject.SetActive(false);
                        enemy.ApplyDamage(rifleDamage * 2f);
                    }

                    AudioManager.Instance.PlaySfx(AudioManager.ESfx.FIRE);
                    hitCount--;
                }
            }

            muzzleFlashParticle.gameObject.SetActive(true);
            muzzleFlashParticle.Play(true);

            if (_shootDelayCoroutine == null)
            {
                _shootDelayCoroutine = StartCoroutine(ShootDelayCoroutine());
            }

            _bulletCount--;
            OnPlayerStatsChange.Invoke(this, _GetPlayerStats());
        }
    }

    class RaycastHitCompare : IComparer
    {
        public int Compare(object x, object y)
        {
            RaycastHit rayHitX = (RaycastHit)x;
            RaycastHit rayHitY = (RaycastHit)y;

            if (rayHitX.distance > rayHitY.distance)
                return 1;
            else if (rayHitX.distance < rayHitY.distance)
                return -1;
            else
                return 0;
        }
    }

    void _CreateRifleHitParticle(RaycastHit hit)
    {
        ParticleSystem hitParticleObj = Instantiate(hitParticle);
        hitParticleObj.transform.position = hit.point;
        hitParticleObj.Play();
        Destroy(hitParticleObj.gameObject, 1.0f);
    }

    IEnumerator ShootDelayCoroutine()
    {
        _canFire = false;
        yield return new WaitForSeconds(rifleShootDelay);
        _canFire = true;
        _shootDelayCoroutine = null;
    }

    void _ProcessChangeWeapon(object sender, EventArgs e)
    {
        if (_isDead)
            return;

        if (_hasRifle)
        {
            AudioManager.Instance.PlaySfx(AudioManager.ESfx.EQUIP);
            _animator.SetTrigger("IsWeaponChange");
            rifleObject.SetActive(true);
        }
    }

    void _CheckPickUp(object sender, EventArgs e)
    {
        if (null == _checkPickUpCoroutine)
            _checkPickUpCoroutine = StartCoroutine(CheckPickUpCoroutine());
    }

    IEnumerator CheckPickUpCoroutine()
    {
        _itemPickUpTrigger.enabled = true;
        yield return new WaitForSeconds(0.1f);
        _itemPickUpTrigger.enabled = false;
        _checkPickUpCoroutine = null;
    }

    void _PickUpItem(Collider col)
    {
        if (_isDead)
            return;

        if (_isPicking)
            return;

        if (col.TryGetComponent<Item>(out Item item))
        {
            if (_pickUpCoroutine != null)
            {
                StopCoroutine(_pickUpCoroutine);
            }

            _pickUpCoroutine = StartCoroutine(PickUpCoroutine(item));
        }
    }

    IEnumerator PickUpCoroutine(Item item)
    {
        _isPicking = true;
        _animator.SetLayerWeight(1, 0.8f);
        _animator.SetTrigger("IsPickUp");
        float animationLength = _animator.GetCurrentAnimatorStateInfo(1).length;
        float untilPickupLength = 1.0f;
        yield return new WaitForSeconds(untilPickupLength);
        item.gameObject.SetActive(false);
        _isNearItem = false;
        AudioManager.Instance.PlaySfx(AudioManager.ESfx.PICKUP);
        yield return new WaitForSeconds(animationLength - untilPickupLength);
        
        if (_itemList != null)
        {
            _itemList.Add(item);
        }

        _CheckItem(item);

        OnPlayerStatsChange.Invoke(this, _GetPlayerStats());
        _animator.SetLayerWeight(1, 0);
        _itemPickUpTrigger.enabled = false;
        _isPicking = false;
    }

    void _CheckItem(Item item)
    {
        if (item.ItemType == EItemType.RIFLE)
        {
            _hasRifle = true;
            OnPickUpRifle?.Invoke(this, EventArgs.Empty);
        }

        if (item.ItemType == EItemType.BULLET)
        {
            if (item.TryGetComponent<Bullet>(out Bullet bullet))
            {
                _bulletCount += bullet.amount;
                OnPlayerStatsChange?.Invoke(this, _GetPlayerStats());
            }
        }

        if (item.ItemType == EItemType.FLASH_LIGHT)
        {
            _hasFlash = true;
            OnPickUpLight?.Invoke(this, EventArgs.Empty);
        }
    }

    bool _HasItem(EItemType type)
    {
        foreach(var item in _itemList)
        {
            if (item.ItemType == type)
                return true;
        }

        return false;
    }

    void _Jump(object sender, EventArgs e)
    {
        if (_isDead)
            return;

        if (_characterController.isGrounded)
        {
            _verticalVelocity += Mathf.Sqrt(jumpHeight * -3f * _gravity);
            _animator.SetTrigger("JumpTrigger");
        }
    }

    void _LightSwitch(object sender, EventArgs e)
    {
        if (_isDead)
            return;

        if (_hasFlash)
        {
            _isFlashOn = !_isFlashOn;
            flashLightObj.SetActive(_isFlashOn);
            AudioManager.Instance.PlaySfx(AudioManager.ESfx.CLICK);
        }
    }

    void _SetAnimationParams()
    {
        _animator.SetFloat("Horizontal", _horizontal);
        _animator.SetFloat("Vertical", _vertical);
        _animator.SetBool("IsRunning", _isRunning);
        _animator.SetBool("IsAiming", _isAiming);
    }

    void _Die()
    {
        if (!_isDead)
        {
            _isDead = true;
            _animator.SetTrigger("IsDead");
            GameManager.Instance.GameOver();
        }
    }

    void _PlayZombieAmbienceSounds()
    {
        if (_curTime > _nextSfxTime)
        {
            _nextSfxTime = UnityEngine.Random.Range(5f, 15f);
            int clipNum = UnityEngine.Random.Range(1, 4);

            if (clipNum == 1)
            {
                AudioManager.Instance.PlaySfxAt(AudioManager.ESfx.GROWL1, transform.position + transform.forward * -1f * 3f);
            }
            else if (clipNum == 2)
            {
                AudioManager.Instance.PlaySfxAt(AudioManager.ESfx.GROWL2, transform.position + transform.forward * -1f * 3f);
            }
            else if (clipNum == 3)
            {
                AudioManager.Instance.PlaySfxAt(AudioManager.ESfx.GROWL3, transform.position + transform.forward * -1f * 3f);
            }
            
            _curTime = 0f;
        }
    }

    //void _DrawDebugBox(Vector3 origin, Vector3 direction)
    //{
    //    Vector3 endPoint = origin + direction * boxCastDistance;

    //    Vector3[] corners = new Vector3[8];
    //    corners[0] = origin + new Vector3(-boxSize.x, -boxSize.y, -boxSize.z) / 2;
    //    corners[1] = origin + new Vector3(boxSize.x, -boxSize.y, -boxSize.z) / 2;
    //    corners[2] = origin + new Vector3(-boxSize.x, boxSize.y, -boxSize.z) / 2;
    //    corners[3] = origin + new Vector3(boxSize.x, boxSize.y, -boxSize.z) / 2;
    //    corners[4] = origin + new Vector3(-boxSize.x, -boxSize.y, boxSize.z) / 2;
    //    corners[5] = origin + new Vector3(boxSize.x, -boxSize.y, boxSize.z) / 2;
    //    corners[6] = origin + new Vector3(-boxSize.x, boxSize.y, boxSize.z) / 2;
    //    corners[7] = origin + new Vector3(boxSize.x, boxSize.y, boxSize.z) / 2;
        
    //    Debug.DrawLine(corners[0], corners[1], Color.green, 3f);
    //    Debug.DrawLine(corners[1], corners[3], Color.green, 3f);
    //    Debug.DrawLine(corners[3], corners[2], Color.green, 3f);
    //    Debug.DrawLine(corners[2], corners[0], Color.green, 3f);
    //    Debug.DrawLine(corners[4], corners[5], Color.green, 3f);
    //    Debug.DrawLine(corners[5], corners[7], Color.green, 3f);
    //    Debug.DrawLine(corners[7], corners[6], Color.green, 3f);
    //    Debug.DrawLine(corners[6], corners[4], Color.green, 3f);
    //    Debug.DrawLine(corners[0], corners[4], Color.green, 3f);
    //    Debug.DrawLine(corners[1], corners[5], Color.green, 3f);
    //    Debug.DrawLine(corners[2], corners[6], Color.green, 3f);
    //    Debug.DrawLine(corners[3], corners[7], Color.green, 3f);
    //}
}
