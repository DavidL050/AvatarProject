using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    public float speed = 3f;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isAttacking = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.speed = speed;
        animator.applyRootMotion = false;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && !isAttacking)
        {
            StartCoroutine(AttackLoop());
        }
        else if (distanceToPlayer <= detectionRange)
        {
            isAttacking = false;
            agent.SetDestination(player.position);
        }
        else
        {
            Idle();
        }

        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    IEnumerator AttackLoop()
    {
        isAttacking = true;
        agent.ResetPath();
        animator.SetTrigger("jabTrigger");

        yield return new WaitForSeconds(1.5f); // Tiempo de ataque
        isAttacking = false;
    }

    void Idle()
    {
        animator.SetFloat("Speed", 0f);
        agent.ResetPath();
    }
}








