using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputManagerSO", menuName = "ScriptableObjects/InputManagerSO")]
public class InputManagerSO : ScriptableObject
{
    [SerializeField] private InputActionAsset _inputActionAsset;

    public event Action<Vector2> OnLookInput;
    public event Action<Vector2> OnMoveInput;
    public event Action OnEquip1Input;
    public event Action OnFireInput;
    public event Action OnAimStartInput;
    public event Action OnAimEndInput;
    public event Action OnPickUpInput;
    public event Action OnJumpInput;
    public event Action OnLightInput;
    public event Action OnPauseInput;

    private InputAction _lookAction;
    private InputAction _moveAction;
    private InputAction _equip1Action;
    private InputAction _fireAction;
    private InputAction _aimAction;
    private InputAction _pickUpAction;
    private InputAction _jumpAction;
    private InputAction _lightAction;
    private InputAction _pauseAction;

    private void Awake()
    {
        _inputActionAsset.FindAction("Look");
        _inputActionAsset.FindAction("Move");
        _inputActionAsset.FindAction("Equip1");
        _inputActionAsset.FindAction("Fire");
        _inputActionAsset.FindAction("Aim");
        _inputActionAsset.FindAction("PickUp");
        _inputActionAsset.FindAction("Jump");
        _inputActionAsset.FindAction("Light");
        _inputActionAsset.FindAction("Pause");
    }

    private void OnEnable()
    {
        _lookAction.performed += _lookAction_performed;
        _moveAction.performed += _moveAction_performed;
        _equip1Action.performed += _equip1Action_performed;
        _fireAction.performed += _fireAction_performed;
        _aimAction.started += _aimAction_started;
        _aimAction.canceled += _aimAction_canceled;
        _pickUpAction.performed += _pickUpAction_performed;
        _jumpAction.performed += _jumpAction_performed;
        _lightAction.performed += _lightAction_performed;
        _pauseAction.performed += _pauseAction_performed;
    }

    private void OnDisable()
    {
        _lookAction.performed -= _lookAction_performed;
        _moveAction.performed -= _moveAction_performed;
        _equip1Action.performed -= _equip1Action_performed;
        _fireAction.performed -= _fireAction_performed;
        _aimAction.started -= _aimAction_started;
        _aimAction.canceled -= _aimAction_canceled;
        _pickUpAction.performed -= _pickUpAction_performed;
        _jumpAction.performed -= _jumpAction_performed;
        _lightAction.performed -= _lightAction_performed;
        _pauseAction.performed -= _pauseAction_performed;
    }

    private void _lookAction_performed(InputAction.CallbackContext ctx)
    {
        OnLookInput?.Invoke(ctx.ReadValue<Vector2>());
    }

    private void _moveAction_performed(InputAction.CallbackContext ctx)
    {
        OnMoveInput?.Invoke(ctx.ReadValue<Vector2>());
    }

    private void _equip1Action_performed(InputAction.CallbackContext ctx)
    {
        OnEquip1Input?.Invoke();
    }


    private void _pauseAction_performed(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void _lightAction_performed(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void _jumpAction_performed(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void _pickUpAction_performed(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void _aimAction_canceled(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void _aimAction_started(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void _fireAction_performed(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }
}
