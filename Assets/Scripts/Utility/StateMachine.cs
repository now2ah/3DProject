using UnityEngine;

public abstract class State
{
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void EndState();
}

public class StateMachine : MonoBehaviour
{
    private bool _isRunning = false;
    private State _currentState;

    public State CurrentState { get { return _currentState; } }

    void Update()
    {
        if (_isRunning)
            _currentState.UpdateState();
    }

    public void StartState(State startState)
    {
        _currentState = startState;
        _isRunning = true;
    }

    public void ChangeState(State nextState)
    {
        _currentState.EndState();
        _currentState = nextState;
        _currentState.EnterState();
    }
}
