using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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

    public enum EZombieSFX
    {
        IDLE,
        ATTACK
    }

    private NavMeshAgent navMeshAgent;
    private Animator animator;

    public float moveSpeed = 2f;
    public float attackRange = 1f;
    public float attackDelay = 2f;
    public Transform[] patrolPoints;
    public float transitionTime = 2f;
    public float detectRange = 3f;

    private bool isPatrol = true;
    private bool isAttack = false;
    private int patrolPointIndex = 0;
    private float nextAttackTime = 3f;
    private Transform target;
    [SerializeField] private EZombieState currentState = EZombieState.IDLE;
    private float evadeRange = 5f;
    private float zombieHP = 10f;
    private float distanceToTarget;
    private bool isTransition = false;
    private Coroutine attackCoroutine;
    private Coroutine currentCoroutine;

    private AudioSource audioSource;
    public AudioClip audioClipZombieIdle;
    public AudioClip audioClipZombieAttack;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        GameManager.Instance.OnPlayerSpawned += _OnPlayerSpawned;
        ChangeState(EZombieState.IDLE);
    }

    private void Update()
    {
        //switch (currentState)
        //{
        //    case EZombieState.IDLE:
        //        currentState = EZombieState.ROAM;
        //        break;

        //    case EZombieState.ROAM:
        //        animator.SetBool("IsRun", true);
        //        _Patrol();

        //        if (_IsDetectedPlayer())
        //        {
        //            currentState = EZombieState.CHASE;
        //        }
        //        break;

        //    case EZombieState.CHASE:
        //        if (_IsDetectedPlayer())
        //        {
        //            _Chase(target);
        //        }
        //        else
        //        {
        //            currentState = EZombieState.IDLE;
        //        }

        //        if (_IsInAttackRange())
        //        {
        //            currentState = EZombieState.ATTACK;
        //        }
        //        break;

        //    case EZombieState.ATTACK:
        //        if (nextAttackTime > attackDelay)
        //        {
        //            _Attack(target);
        //            nextAttackTime = 0f;
        //        }
        //        nextAttackTime += Time.deltaTime;
        //        break;

        //    case EZombieState.DIE:
        //        _Die();
        //        break;
        //}
    }

    private void OnCollisionEnter(Collision collision)
    {
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

    public void PlayAudio(EZombieSFX sfx)
    {
        if (sfx == EZombieSFX.IDLE)
        {
            audioSource.clip = audioClipZombieIdle;
            audioSource.Play();
        }
        else if (sfx == EZombieSFX.ATTACK)
        {
            audioSource.PlayOneShot(audioClipZombieAttack);
        }
    }

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

    public void ChangeState(EZombieState newState)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentState = newState;

        switch(currentState)
        {
            case EZombieState.IDLE:
                currentCoroutine = StartCoroutine(IdleCoroutine());
                break;
            case EZombieState.ROAM:
                currentCoroutine = StartCoroutine(RoamCoroutine());
                break;
            case EZombieState.CHASE:
                currentCoroutine = StartCoroutine(ChaseCoroutine());
                break;
            case EZombieState.ATTACK:
                currentCoroutine = StartCoroutine(AttackCoroutine());
                break;
        }
    }

    IEnumerator IdleCoroutine()
    {
        animator.Play("Z_Idle");

        while(currentState == EZombieState.IDLE)
        {
            if (_IsDetectedPlayer())
                ChangeState(EZombieState.CHASE);
            else if (_IsInAttackRange())
                ChangeState(EZombieState.ATTACK);
            else
                ChangeState(EZombieState.ROAM);

                yield return null;
        }
    }

    IEnumerator RoamCoroutine()
    {
        while (currentState == EZombieState.ROAM)
        {
            animator.Play("Z_Run");
            _Patrol();

            if (_IsDetectedPlayer())
                ChangeState(EZombieState.CHASE);

            yield return null;
        }
    }

    IEnumerator ChaseCoroutine()
    {
        animator.Play("Z_Run");
        while (currentState == EZombieState.CHASE)
        {
            _Chase(target);

            if (!_IsDetectedPlayer())
                ChangeState(EZombieState.IDLE);

            if (_IsInAttackRange())
                ChangeState(EZombieState.ATTACK);

            yield return null;
        }
    }

    IEnumerator AttackCoroutine()
    {
        animator.Play("Z_Attack");
        yield return new WaitForSeconds(attackDelay);
        ChangeState(EZombieState.IDLE);
    }

    void _OnPlayerSpawned(object sender, EventArgs e)
    {
        target = GameManager.Instance.Player.transform;
    }

    bool _IsDetectedPlayer()
    {
        if (target != null)
        {
            distanceToTarget = Vector3.Distance(transform.position, target.position);
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

    void _Move(Vector3 position)
    {
        //transform.position += direction * moveSpeed * Time.deltaTime;
        navMeshAgent.speed = 2f;
        navMeshAgent.stoppingDistance = 1f;
        navMeshAgent.destination = position;
        //navMeshAgent.Move(direction);
    }

    void _Attack(Transform target)
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }

        attackCoroutine = StartCoroutine(AttackCoroutine());
    }

    //IEnumerator AttackCoroutine()
    //{
    //    isAttack = true;
    //    animator.SetBool("IsAttack", isAttack);
    //    yield return new WaitForSeconds(attackDelay);
    //    isAttack = false;
    //    animator.SetBool("IsAttack", isAttack);
    //    currentState = EZombieState.IDLE;
    //}

    void _Patrol()
    {
        Transform targetPoint = patrolPoints[patrolPointIndex];
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        //transform.LookAt(patrolPoints[patrolPointIndex]);
        _Move(targetPoint.position);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
        {
            patrolPointIndex = (patrolPointIndex + 1) % patrolPoints.Length;
        }
    }

    void _Chase(Transform target)
    {
        //isPatrol = false;
        Vector3 direction = (target.position - transform.position).normalized;
        //transform.LookAt(target.position);
        //if (!isAttack)
            _Move(target.position);

        //if (!_IsDetectedPlayer())
        //    isPatrol = true;
    }

    bool _IsInAttackRange()
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

    void _Die()
    {

    }
}
