using System;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : Singleton<InputManager>
{
    public float mouseSensitivity = 100.0f;

    public KeyCode equip1Input = KeyCode.Alpha1;
    public KeyCode pickUpInput = KeyCode.E;
    public KeyCode jumpInput = KeyCode.Space;
    public KeyCode reloadInput = KeyCode.R;
    public KeyCode pauseInput = KeyCode.Escape;
    
    public event EventHandler<Vector2> OnLookInput;
    public event EventHandler<Vector2> OnMoveInput;
    public event EventHandler OnEquip1Input;
    public event EventHandler OnFireInput;
    public event EventHandler OnAimStartInput;
    public event EventHandler OnAimEndInput;
    public event EventHandler OnPickUpInput;
    public event EventHandler OnJumpInput;
    public event EventHandler OnLightInput;
    public event EventHandler OnPauseInput;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        GameManager.Instance.OnGameOver += _OnGameOver;
        GameManager.Instance.OnGameSucceeded += _OnGameSucceeded;
    }

    void Update()
    {
        _HandleMouseInput();
        _HandleClickInput();
        _HandleMoveInput();
        _HandleButtonDownInput();
    }

    private void OnDisable()
    {
        _UnsubscribeEvents();

        if (OnPauseInput != null)
        {
            foreach (var d in OnPauseInput.GetInvocationList())
            {
                OnPauseInput -= d as EventHandler;
            }
        }
    }

    void _OnGameOver(object sender, EventArgs e)
    {
        _UnsubscribeEvents();
    }

    void _OnGameSucceeded(object sender, EventArgs e)
    {
        _UnsubscribeEvents();
    }

    void _UnsubscribeEvents()
    {
        if (OnLookInput != null)
        {
            foreach (var d in OnLookInput.GetInvocationList())
            {
                OnLookInput -= d as EventHandler<Vector2>;
            }
        }

        if (OnMoveInput != null)
        {
            foreach (var d in OnMoveInput.GetInvocationList())
            {
                OnMoveInput -= d as EventHandler<Vector2>;
            }
        }

        if (OnEquip1Input != null)
        {
            foreach (var d in OnEquip1Input.GetInvocationList())
            {
                OnEquip1Input -= d as EventHandler;
            }
        }

        if (OnFireInput != null)
        {
            foreach (var d in OnFireInput.GetInvocationList())
            {
                OnFireInput -= d as EventHandler;
            }
        }

        if (OnAimStartInput != null)
        {
            foreach (var d in OnAimStartInput.GetInvocationList())
            {
                OnAimStartInput -= d as EventHandler;
            }
        }

        if (OnAimEndInput != null)
        {
            foreach (var d in OnAimEndInput.GetInvocationList())
            {
                OnAimEndInput -= d as EventHandler;
            }
        }

        if (OnPickUpInput != null)
        {
            foreach (var d in OnPickUpInput.GetInvocationList())
            {
                OnPickUpInput -= d as EventHandler;
            }
        }

        if (OnJumpInput != null)
        {
            foreach (var d in OnJumpInput.GetInvocationList())
            {
                OnJumpInput -= d as EventHandler;
            }
        }

        if (OnLightInput != null)
        {
            foreach (var d in OnLightInput.GetInvocationList())
            {
                OnLightInput -= d as EventHandler;
            }
        }
    }

    void _HandleMouseInput()
    {
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        Vector2 mousePosition = new Vector2(x, y);
        OnLookInput?.Invoke(this, mousePosition);
    }

    void _HandleMoveInput()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        Vector2 keyboardInput = new Vector2(x, y);
        OnMoveInput?.Invoke(this, keyboardInput);
    }

    void _HandleClickInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnFireInput?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonDown(1))
        {
            OnAimStartInput?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonUp(1))
        {
            OnAimEndInput?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonDown(2))
        {
            OnLightInput?.Invoke(this, EventArgs.Empty);
        }
    }

    void _HandleButtonDownInput()
    {
        if (Input.GetKeyDown(equip1Input))
        {
            OnEquip1Input?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetKeyDown(pickUpInput))
        {
            OnPickUpInput?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetKeyDown(jumpInput))
        {
            OnJumpInput?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetKeyDown(pauseInput))
        {
            OnPauseInput?.Invoke(this, EventArgs.Empty);
        }
    }
}
