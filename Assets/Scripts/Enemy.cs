using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    [Header("Status")]
    public float maxHP;
    public float walkSpeed;
    public float runSpeed;
    public float angularSpeed;
    public float attackDamage;
    public float attackRange;
    //public float attackDelay;
    public float detectRange;

    protected float _currentHP;
    protected bool _isDead;

    public float CurrentHP { get { return _currentHP; } }

    protected Rigidbody _rigidBody;
    protected NavMeshAgent _navMeshAgent;
    protected Animator _animator;
    protected Transform _target;

    public NavMeshAgent NavMeshAgent { get { return _navMeshAgent; } }
    public Animator Animator { get { return _animator; } }

    protected void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    public void ApplyDamage(float damage)
    {
        if (!_isDead)
        {
            _currentHP -= damage;

            if (_currentHP <= 0) { _Die(); }
        }
    }

    protected virtual void _Die() { }
}
