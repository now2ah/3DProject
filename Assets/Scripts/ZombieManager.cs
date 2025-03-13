using System.Collections;
using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    public enum EZombieState
    {
        IDLE,
        ROAM,
        CHASE,
        ATTACK,
        DIE
    }

    public float moveSpeed = 2f;
    public float attackRange = 1f;
    public float attackDelay = 2f;
    public Transform[] patrolPoints;
    public float trackingRange = 3f;
    public float transitionTime = 2f;
    public float detectRange = 3f;

    private bool isPatrol = true;
    private bool isAttack = false;
    private int patrolPointIndex = 0;
    private float nextAttackTime = 0f;
    private Transform target;
    private EZombieState currentState = EZombieState.IDLE;
    private float evadeRange = 5f;
    private float zombieHP = 10f;
    private float distanceToTarget;
    private bool isTransition = false;
    private Coroutine attackCoroutine;

    public void DeliverDamage(float damage)
    {
        if (target != null)
        {
            if (target.TryGetComponent<PlayerManager>(out PlayerManager player))
            {
                //apply damage
            }
        }
    }

    void _DetectPlayer()
    {
        if (target == null) { target = GameObject.FindGameObjectWithTag("Player").transform; }
        
        distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget < detectRange)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.LookAt(direction);

            if (!isAttack)
                _Move(direction);
        }

        if (distanceToTarget < attackRange)
        {
            _Attack(target);
        }
    }

    void _Move(Vector3 direction)
    {
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void _Attack(Transform target)
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }

        attackCoroutine = StartCoroutine(AttackCoroutine());
    }

    IEnumerator AttackCoroutine()
    {
        isAttack = true;
        yield return new WaitForSeconds(attackDelay);
        isAttack = false;
    }

    void _Patrol()
    {
        Transform targetPoint = patrolPoints[patrolPointIndex];
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        transform.LookAt(patrolPoints[patrolPointIndex]);
        _Move(direction);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
        {
            patrolPointIndex = (patrolPointIndex + 1) % patrolPoints.Length;
        }
    }

    private void Update()
    {
        _DetectPlayer();

        if (isPatrol) { _Patrol(); }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerManager>(out PlayerManager player))
            {
                player.BeHit();
            }
        }
    }
}
