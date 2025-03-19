using UnityEngine;

public abstract class State
{
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void EndState();
}

public class StateMachine : MonoBehaviour
{
    private State _currentState;

    public State CurrentState { get { return _currentState; } }

    void Update()
    {
        _currentState.UpdateState();
    }

    public void ChangeState(State nextState)
    {
        _currentState.EndState();
        _currentState = nextState;
        _currentState.EndState();
    }
}
