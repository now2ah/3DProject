using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;

public enum eSFX
{
    FIRE,
    EQUIP,
    BEHIT,
    PICKUP
}

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    public float mouseSensityvity = 100.0f;
    public Transform cameraTransform;
    public CharacterController characterController;
    public Transform playerHeadTransform;
    public float thirdPersonDistance = 3.0f;
    public Vector3 thirdPersonOffset = new Vector3(0f, 1.0f, 0f);
    public Transform playerLookTransform;

    public float zoomDistance = 1.0f;
    public float zoomSpeed = 5.0f;
    public float defaultFov = 60.0f;
    public float zoomFov = 30.0f;

    private float camCurrentDistance;
    private float camTargetDistance;
    private float camTargetFov;
    private bool isZoomed = false;
    private Coroutine zoomCoroutine;
    private Camera mainCamera;

    private float pitch = 0.0f;
    private float yaw = 0.0f;
    private bool isFirstPerson = false;
    private bool isRotateAroundPlayer = false;

    #region GRAVITY VARIABLES
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    private bool isJump = false;
    private float verticalVelocity = 0f;
    private bool isGround;
    #endregion

    private Animator animator;
    private float horizontal;
    private float vertical;

    private bool isRunning = false;
    private Coroutine beHitCoroutine = null;
    private Coroutine pickUpCoroutine = null;
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;

    private bool isAiming = false;
    private bool canFire = true;
    private float weaponMaxDistance = 100f;
    private float rifleShootDelay = 0.5f;
    private Coroutine shootDelayCoroutine;
    public GameObject rifleObject;
    public Transform aimTarget;    
    public LayerMask targetLayerMask;
    public ParticleSystem muzzleFlashParticle;

    public MultiAimConstraint multiAimConstraint;

    public Vector3 boxSize = Vector3.one;
    public float boxCastDistance = 5f;
    public LayerMask itemLayerMask;
    public Transform pickUpTransform;
    private bool hasRifle = false;
    private int bulletCount = 5;

    public GameObject crossHairUI;
    public GameObject rifleUI;
    public GameObject bulletUI;

    public AudioClip audioClipFire;
    public AudioClip audioClipEquipWeapon;
    public AudioClip audioClipBeHit;
    public AudioClip audioClipGetItem;
    private AudioSource audioSource;
    
    public void SetTargetDistance(float distance)
    {
        camTargetDistance = distance;
    }

    public void SetTargetFov(float fov)
    {
        camTargetFov = fov;
    }

    public void PlayAudio(eSFX sfx)
    {
        if (null == audioSource)
            return;

        switch(sfx)
        {
            case eSFX.FIRE:
                audioSource.PlayOneShot(audioClipFire);
                break;

            case eSFX.EQUIP:
                audioSource.PlayOneShot(audioClipEquipWeapon);
                break;

            case eSFX.BEHIT:
                audioSource.PlayOneShot(audioClipBeHit);
                break;

            case eSFX.PICKUP:
                audioSource.PlayOneShot(audioClipGetItem);
                break;
        }
    }

    public void BeHit()
    {
        if (beHitCoroutine == null)
        {
            beHitCoroutine = StartCoroutine(BeHitCoroutine());
        }
    }

    public void CreateItemBoxCast()
    {
        Vector3 origin = pickUpTransform.position;
        Vector3 direction = pickUpTransform.forward;
        RaycastHit[] hits = Physics.BoxCastAll(origin, boxSize / 2, direction, Quaternion.identity, boxCastDistance, itemLayerMask);
        foreach (var hit in hits)
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Item"))
            {
                hasRifle = true;
                rifleUI.SetActive(true);
            }

            hit.collider.gameObject.SetActive(false);
        }
    }

    IEnumerator BeHitCoroutine()
    {
        if (animator != null)
        {
            animator.SetTrigger("IsHit");
            PlayAudio(eSFX.BEHIT);
            float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animationLength);
            _WarpToStartPosition();
            beHitCoroutine = null;
        }
    }

    void _WarpToStartPosition()
    {
        characterController.enabled = false;
        transform.position = Vector3.zero;
        characterController.enabled = true;
    }

    void _UpdateCameraPosition()
    {
        if (isRotateAroundPlayer)
        {
            //camCurrentDistance = thirdPersonDistance;
            Vector3 direction = new Vector3(0f, 0f, -camCurrentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;

            cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
        else
        {
            //camCurrentDistance = thirdPersonDistance;
            transform.rotation = Quaternion.Euler(0f, yaw, 0);

            Vector3 direction = new Vector3(0, 0, -camCurrentDistance);

            cameraTransform.position = playerLookTransform.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
            cameraTransform.LookAt(playerLookTransform.position + new Vector3(0, thirdPersonOffset.y, 0));

            _UpdateAimTarget();
        }
    }

    void _ProcessMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensityvity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensityvity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -45f, 45f);
    }

    void _ProcessCameraModeInput()
    {
        //if (Input.GetKeyDown(KeyCode.V))
        //{
        //    isFirstPerson = !isFirstPerson;
        //    Debug.Log(isFirstPerson ? "first person mode" : "third person mode");
        //}

        if (Input.GetKeyDown(KeyCode.F))
        {
            isRotateAroundPlayer = !isRotateAroundPlayer;
            Debug.Log(isRotateAroundPlayer ? "camera is rotating around the player" : "player rotates camera directly");
        }
    }

    void _FirstPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        //move character controller to camera's direction
        Vector3 moveDirection = cameraTransform.right * horizontal + cameraTransform.forward * vertical;
        moveDirection.y = 0f;
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        //change camera position to player's head postion
        cameraTransform.position = playerHeadTransform.position;

        //change camera rotation to pitch, yaw values
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0);

        //change player rotation to camera's yaw direction
        transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
    }

    void _ThirdPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
            moveSpeed = runSpeed;
        }
        else
        {
            isRunning = false;
            moveSpeed = walkSpeed;
        }

        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = 0f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical + transform.up * verticalVelocity;
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        _UpdateCameraPosition();
    }

    float _CalculateJumpMagnitude()
    {
        return jumpHeight * Mathf.Sqrt(2 * gravity);
    }

    void _ProcessZoomInOut()
    {
        if (Input.GetMouseButtonDown(1))
        {
            //if zoomcoroutine is playing, stop it
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }

            //if it's first person mode start zoomFOV coroutine
            if (isFirstPerson)
            {
                SetTargetFov(zoomFov);
                zoomCoroutine = StartCoroutine(ZoomFieldOfViewCoroutine(camTargetFov));
            }
            //if not zoomDistance coroutine
            else
            {
                isAiming = true;
                crossHairUI.SetActive(true);
                animator.SetLayerWeight(1, 1);
                SetTargetDistance(zoomDistance);
                zoomCoroutine = StartCoroutine(ZoomCameraCoroutine(camTargetDistance));
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }

            if (isFirstPerson)
            {
                SetTargetFov(defaultFov);
                zoomCoroutine = StartCoroutine(ZoomFieldOfViewCoroutine(camTargetFov));
            }
            else
            {
                isAiming = false;
                crossHairUI.SetActive(false);
                //multiAimConstraint.data.offset = Vector3.zero;
                animator.SetLayerWeight(1, 0);
                SetTargetDistance(thirdPersonDistance);
                zoomCoroutine = StartCoroutine(ZoomCameraCoroutine(camTargetDistance));
            }
        }
    }

    IEnumerator ZoomCameraCoroutine(float targetDistance)
    {
        while(Mathf.Abs(camCurrentDistance - camTargetDistance) > 0.01f)
        {
            camCurrentDistance = Mathf.Lerp(camCurrentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        camCurrentDistance = targetDistance;
    }

    IEnumerator ZoomFieldOfViewCoroutine(float targetFov)
    {
        while (Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.01f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        mainCamera.fieldOfView = targetFov;
    }

    void _ProcessFireRifle()
    {
        if (canFire && bulletCount > 0 && Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("FireTrigger");

            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            
            //single hit
            //RaycastHit hit;
            //if (Physics.Raycast(ray, out hit, weaponMaxDistance, targetLayerMask))
            //{
            //    Debug.Log(hit.collider.gameObject.name);
            //    Debug.DrawLine(ray.origin, hit.point, Color.red);

            //    if (hit.transform.TryGetComponent<ZombieManager>(out ZombieManager zombie))
            //    {
            //        zombie.gameObject.SetActive(false);
            //    }
            //}
            //else
            //{
            //    Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green);
            //}

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

                    hits[i].transform.gameObject.SetActive(false);
                    hitCount--;
                }
            }

            muzzleFlashParticle.gameObject.SetActive(true);
            muzzleFlashParticle.Play(true);

            bulletCount--;

            if (shootDelayCoroutine == null)
            {
                shootDelayCoroutine = StartCoroutine(ShootDelayCoroutine());
            }
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

    IEnumerator ShootDelayCoroutine()
    {
        canFire = false;
        yield return new WaitForSeconds(rifleShootDelay);
        canFire = true;
        shootDelayCoroutine = null;
    }

    void _ProcessChangeWeapons()
    {
        if (hasRifle && Input.GetKeyDown(KeyCode.Alpha1))
        {
            //audioSource.PlayOneShot(audioClipEquipWeapon);
            animator.SetTrigger("IsWeaponChange");
            rifleObject.SetActive(true);
        }
    }

    void _ProcessPickUp()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (pickUpCoroutine != null)
            {
                StopCoroutine(pickUpCoroutine);
            }

            pickUpCoroutine = StartCoroutine(PickUpCoroutine());
        }
    }

    IEnumerator PickUpCoroutine()
    {
        animator.SetLayerWeight(1, 0.8f);
        animator.SetTrigger("IsPickUp");
        float animationLength = animator.GetCurrentAnimatorStateInfo(1).length;
        yield return new WaitForSeconds(animationLength);
        animator.SetLayerWeight(1, 0);
    }

    void _UpdateAimTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        aimTarget.position = ray.GetPoint(10f);
    }

    void _ProcessJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && characterController.isGrounded)
        {
            verticalVelocity += Mathf.Sqrt(jumpHeight * -3f * gravity);
        }
    }

    void _SetAnimationParams()
    {
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsAiming", isAiming);
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        camCurrentDistance = thirdPersonDistance;
        camTargetDistance = thirdPersonDistance;
        camTargetFov = defaultFov;
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;
        rifleObject.SetActive(false);

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        _ProcessMouseInput();
        _ProcessCameraModeInput();
        _ThirdPersonMovement();
        _ProcessJump();

        if (hasRifle)
            _ProcessZoomInOut();

        if(isAiming)
        {
            _ProcessFireRifle();
        }

        _ProcessChangeWeapons();
        _ProcessPickUp();
        _SetAnimationParams();
    }
}
