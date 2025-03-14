using System;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : Singleton<InputManager>
{
    public KeyCode pickUpInput = KeyCode.E;
    public KeyCode jumpInput = KeyCode.Space;
    public KeyCode reloadInput = KeyCode.R;
    
    public event EventHandler<Vector2> OnLookInput;
    public event EventHandler<Vector2> OnMoveInput;
    public event EventHandler OnFireInput;
    public event EventHandler OnPickUpInput;
    public event EventHandler OnJumpInput;

    // Update is called once per frame
    void Update()
    {
        _HandleMouseInput();
        _HandleClickInput();
        _HandleMoveInput();
        _HandleButtonDownInput();
    }

    private void OnDisable()
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

        if (OnFireInput != null)
        {
            foreach (var d in OnFireInput.GetInvocationList())
            {
                OnFireInput -= d as EventHandler;
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
        OnMoveInput.Invoke(this, keyboardInput);
    }

    void _HandleClickInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnFireInput.Invoke(this, EventArgs.Empty);
        }
    }

    void _HandleButtonDownInput()
    {
        if (Input.GetKeyDown(pickUpInput))
        {
            OnPickUpInput.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetKeyDown(jumpInput))
        {
            OnJumpInput.Invoke(this, EventArgs.Empty);
        }
    }
}
