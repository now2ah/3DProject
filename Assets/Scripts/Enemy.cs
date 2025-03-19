using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    [Header("Status")]
    public float maxHP;
    public float speed;
    public float attackDamage;
    public float attackRange;
    public float attackDelay;
    public float detectRange;
    
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;

    private Transform _target;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }
}
