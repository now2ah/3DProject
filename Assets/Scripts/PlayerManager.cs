using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5.0f;
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

    void _UpdateCameraPosition()
    {
        if (isRotateAroundPlayer)
        {
            Vector3 direction = new Vector3(0f, 0f, -camCurrentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;

            cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
        else
        {
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
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

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
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        _UpdateCameraPosition();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        camCurrentDistance = thirdPersonDistance;
        camTargetDistance = thirdPersonDistance;
        camTargetFov = defaultFov;
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;
    }

    void Update()
    {
        _ProcessMouseInput();

        isGround = characterController.isGrounded;

        if (isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        _ProcessCameraModeInput();

        if (isFirstPerson)
        {
            _FirstPersonMovement();
        }
        else
        {
            _ThirdPersonMovement();
        }
    }
}
