using System;
using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Zombie : Enemy
{
    public enum EZombieState
    {
        IDLE,
        ROAM,
        CHASE,
        ATTACK,
        DIE
    }
    
    public float actInterval = 3f;

    public GameObject attackTrigger;

    private ZombieStateMachine _stateMachine;
    private ZombieIdleState _idleState;
    private ZombieRoamState _roamState;
    private ZombieChaseState _chaseState;
    private ZombieAttackState _attackState;
    private ZombieDieState _dieState;

    private bool _isIdle = false;
    private bool _isRoam = false;
    private bool _isChase = false;
    private bool _isAttack = false;

    private float distanceToTarget;

    public Transform Target { get { return _target; } }

    private Coroutine _roamCoroutine = null;
    private Coroutine _attackCoroutine = null;
    private Coroutine _dieCoroutine = null;

    private new void Awake()
    {
        base.Awake();
        walkSpeed = 1f;
        runSpeed = 1.5f;
        angularSpeed = 360f;
        attackDamage = 3f;
        attackRange = 1f;
        //attackDelay = 2f;
        detectRange = 3f;
        maxHP = 9f;
        _currentHP = maxHP;
        _isDead = false;

        _navMeshAgent.speed = walkSpeed;
        _navMeshAgent.angularSpeed = angularSpeed;
        _stateMachine = gameObject.AddComponent<ZombieStateMachine>();
        _idleState = new ZombieIdleState(this);
        _roamState = new ZombieRoamState(this);
        _chaseState = new ZombieChaseState(this);
        _attackState = new ZombieAttackState(this);
        _dieState = new ZombieDieState(this);
    }

    private void Start()
    {
        GameManager.Instance.OnPlayerSpawned += _OnPlayerSpawned;
        _stateMachine.StartState(_idleState);
    }

    public void ChangeState(EZombieState newState)
    {
        State state = null;

        if (newState == EZombieState.IDLE)
        {
            state = _idleState;
        }
        else if(newState == EZombieState.ROAM)
        {
            state = _roamState;
        }
        else if (newState == EZombieState.CHASE)
        {
            state = _chaseState;
        }
        else if (newState == EZombieState.ATTACK)
        {
            state = _attackState;
        }
        else if (newState == EZombieState.DIE)
        {
            state = _dieState;
        }

        _stateMachine.ChangeState(state);
    }

    public void ChangeStateValue(EZombieState state, bool isOn)
    {
        if (state == EZombieState.IDLE)
        {
            _isIdle = isOn;
        }
        else if (state == EZombieState.ROAM)
        {
            _isRoam = isOn;
        }
        else if (state == EZombieState.CHASE)
        {
            _isChase = isOn;
        }
        else if (state == EZombieState.ATTACK)
        {
            _isAttack = isOn;
        }
        else if (state == EZombieState.DIE)
        {
            _isDead = isOn;
        }

        _SetAnimationParam();
    }

    public void StopMove()
    {
        if (_navMeshAgent != null)
        {
            _navMeshAgent.ResetPath();
            _navMeshAgent.velocity = Vector3.zero;
        }
    }

    public bool IsDetectedPlayer()
    {
        if (_target != null)
        {
            distanceToTarget = Vector3.Distance(transform.position, _target.position);
            if (distanceToTarget < detectRange)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    public void RoamToRandomDirection(UnityAction callBack)
    {
        if (_roamCoroutine == null)
            _roamCoroutine = StartCoroutine(RoamCoroutine(callBack));
    }

    IEnumerator RoamCoroutine(UnityAction callBack)
    {
        if (_navMeshAgent != null)
            _navMeshAgent.speed = walkSpeed;
        float randomTime = UnityEngine.Random.Range(0, 5f);
        float randomX = UnityEngine.Random.Range(-5, 6);
        float randomZ = UnityEngine.Random.Range(-5, 6);
        Vector3 roamPosition = new Vector3(randomX, 0f, randomZ);
        if (_navMeshAgent != null)
            _navMeshAgent.destination = roamPosition;
        yield return new WaitForSeconds(randomTime);
        if (_navMeshAgent != null)
            _navMeshAgent.ResetPath();

        if (!_isDead)
            callBack?.Invoke();

        _roamCoroutine = null;
    }

    public void Chase(Transform target)
    {
        _navMeshAgent.speed = runSpeed;
        _navMeshAgent.destination = target.position;
    }

    public bool IsInAttackRange()
    {
        if (distanceToTarget < attackRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Attack(Transform target, UnityAction callBack)
    {
        if (_attackCoroutine == null)
            _attackCoroutine = StartCoroutine(AttackCoroutine(callBack));
    }

    IEnumerator AttackCoroutine(UnityAction callBack)
    {
        if (_animator != null)
            _animator.SetTrigger("AttackTrigger");
        float animationLength = _animator.GetNextAnimatorStateInfo(0).length;
        float deliverDamageTime = 0.3f;
        yield return new WaitForSeconds(deliverDamageTime);
        if (_target.TryGetComponent<Player>(out Player player))
        {
            player.BeHit();
            player.ApplyDamage(attackDamage);
        }
        yield return new WaitForSeconds(animationLength - deliverDamageTime);
        callBack?.Invoke();
        _attackCoroutine = null;
    }

    void _OnPlayerSpawned(object sender, EventArgs e)
    {
        _target = GameManager.Instance.Player.transform;
    }

    void _SetAnimationParam()
    {
        if (null == _animator)
            return;

        _animator.SetBool("IsIdle", _isIdle);
        _animator.SetBool("IsRoam", _isRoam);
        _animator.SetBool("IsChase", _isChase);
        _animator.SetBool("IsAttack", _isAttack);
    }

    protected override void _Die()
    {
        ChangeState(EZombieState.DIE);
        _navMeshAgent.enabled = false;
        if (null == _dieCoroutine)
            _dieCoroutine = StartCoroutine(DieCoroutine());
    }

    IEnumerator DieCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        _rigidBody.constraints = 0;
        Vector3 direction = (_target.transform.position - transform.position).normalized * 5.0f;
        _rigidBody.AddRelativeForce(direction, ForceMode.Impulse);
        yield return new WaitForSeconds(2.0f);
        Destroy(gameObject);
    }
}
