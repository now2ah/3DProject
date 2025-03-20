using UnityEngine;

public class ZombieIdleState : State
{
    Zombie _zombie;
    float _actTime = 0f;

    public ZombieIdleState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public override void EnterState()
    {
        _zombie.ChangeStateValue(Zombie.EZombieState.IDLE, true);
        _zombie.StopMove();
    }

    public override void UpdateState()
    {
        _actTime += Time.deltaTime;
        
        if (_zombie.IsDetectedPlayer())
        {
            _zombie.ChangeState(Zombie.EZombieState.CHASE);
        }

        if (_actTime > _zombie.actInterval)
        {
            int randNum = Random.Range(0, 2);

            if (true)
            {
                _zombie.ChangeState(Zombie.EZombieState.ROAM);
            }
            _actTime = 0f;
        }

        _zombie.StopMove();
    }

    public override void EndState()
    {
        _zombie.ChangeStateValue(Zombie.EZombieState.IDLE, false);
    }
}

public class ZombieRoamState : State
{
    Zombie _zombie;

    public ZombieRoamState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public override void EnterState()
    {
        _zombie.ChangeStateValue(Zombie.EZombieState.ROAM, true);
    }

    public override void UpdateState()
    {
        if (_zombie.IsDetectedPlayer())
        {
            _zombie.ChangeState(Zombie.EZombieState.CHASE);
        }

        _zombie.RoamToRandomDirection(() =>
        {
            _zombie.ChangeState(Zombie.EZombieState.IDLE);
        });
    }

    public override void EndState()
    {
        _zombie.ChangeStateValue(Zombie.EZombieState.ROAM, false);
    }
}

public class ZombieChaseState : State
{
    Zombie _zombie;

    public ZombieChaseState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public override void EnterState()
    {
        _zombie.ChangeStateValue(Zombie.EZombieState.CHASE, true);
    }

    public override void UpdateState()
    {
        if (!_zombie.IsDetectedPlayer())
        {
            _zombie.ChangeState(Zombie.EZombieState.IDLE);
        }

        _zombie.Chase(_zombie.Target);

        if (_zombie.IsInAttackRange())
        {
            _zombie.ChangeState(Zombie.EZombieState.ATTACK);
        }
    }

    public override void EndState()
    {
        _zombie.ChangeStateValue(Zombie.EZombieState.CHASE, false);
    }
}

public class ZombieAttackState : State
{
    Zombie _zombie;

    public ZombieAttackState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public override void EnterState()
    {
        _zombie.ChangeStateValue(Zombie.EZombieState.ATTACK, true);
    }

    public override void UpdateState()
    {
        _zombie.Attack(_zombie.Target, () =>
        {
            _zombie.ChangeState(Zombie.EZombieState.IDLE);
        });
    }
    public override void EndState()
    {
        _zombie.ChangeStateValue(Zombie.EZombieState.ATTACK, false);
    }
}

public class ZombieDieState : State
{
    Zombie _zombie;

    public ZombieDieState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public override void EnterState()
    {
        _zombie.ChangeStateValue(Zombie.EZombieState.DIE, true);
    }

    public override void UpdateState()
    {

    }

    public override void EndState()
    {
        _zombie.ChangeStateValue(Zombie.EZombieState.DIE, false);
    }
}

public class ZombieStateMachine : StateMachine
{

}
