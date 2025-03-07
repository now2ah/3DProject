using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

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
    private bool isRotateAroundPlayer = true;

    #region GRAVITY VARIABLES
    public float gravity = -9.81f;
    public float jumpHeight = 2.0f;
    private Vector3 velocity;
    private bool isGround;
    #endregion

    private Animator animator;
    private float horizontal;
    private float vertical;

    private bool isRunning = false;
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;

    public GameObject rifleObject;
    private bool isAiming = false;
    private bool isFiring = false;
    private Coroutine shootCoroutine;

    public AudioClip audioClipFire;
    public AudioClip audioClipEquipWeapon;
    private AudioSource audioSource;
    

    public void SetTargetDistance(float distance)
    {
        camTargetDistance = distance;
    }

    public void SetTargetFov(float fov)
    {
        camTargetFov = fov;
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

    void _GroundCheck()
    {
        isGround = characterController.isGrounded;

        if (isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void _ProcessCameraModeInput()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isFirstPerson ? "first person mode" : "third person mode");
        }

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
        
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        _UpdateCameraPosition();
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
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("FireTrigger");
            audioSource.PlayOneShot(audioClipFire);
        }
    }

    void _ProcessChangeWeapons()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            audioSource.PlayOneShot(audioClipEquipWeapon);
            animator.SetTrigger("IsWeaponChange");
            rifleObject.SetActive(true);
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
        _GroundCheck();
        _ProcessCameraModeInput();

        if (isFirstPerson)
        {
            _FirstPersonMovement();
        }
        else
        {
            _ThirdPersonMovement();
        }

        _ProcessZoomInOut();

        if(isAiming)
        {
            _ProcessFireRifle();
        }

        _ProcessChangeWeapons();

        _SetAnimationParams();
    }
}
