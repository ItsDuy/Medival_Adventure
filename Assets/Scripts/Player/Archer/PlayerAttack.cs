using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private float arrowSpawnDelay = 0.45f;
    [SerializeField] private LayerMask enemyMask;

    [Header("Projectile")]
    [SerializeField] private Arrow arrowPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float arrowDamage = 10f;

    [Header("References")]
    [SerializeField] private Animator animator;

    private float nextAttackTime;

    public Transform CurrentTarget { get; private set; }
    public float AttackRadius => attackRadius;
    public LayerMask EnemyMask => enemyMask;
    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (firePoint == null)
        {
            firePoint = transform;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        // Detect any enemy within radius.
        Collider2D enemy = Physics2D.OverlapCircle(transform.position, attackRadius, enemyMask);
        CurrentTarget = enemy != null ? enemy.transform : null;
        if (CurrentTarget == null)
        {
            return;
        }

        StartCoroutine(AttackRoutine(CurrentTarget));
        nextAttackTime = Time.time + attackCooldown;
    }

    private IEnumerator AttackRoutine(Transform target)
    {
        if (animator != null)
        {
            animator.SetTrigger("isAttacking");
        }

        yield return new WaitForSeconds(arrowSpawnDelay);

        if (target == null || arrowPrefab == null)
        {
            yield break;
        }

        Arrow arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        arrow.Initialize(target, arrowDamage);

        // TODO: Add target prediction or leading shots later.
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
