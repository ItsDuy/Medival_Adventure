using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private float deceleration = 35f;
    [SerializeField] private float turnDeceleration = 50f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualsRoot;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private PlayerAttack playerAttack;

    [Header("Facing")]
    [SerializeField] private bool faceEnemyInRange = true;
    [SerializeField] private float enemyFaceRadius = 1.5f;
    [SerializeField] private LayerMask enemyMask;

    private Vector2 velocity;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (visualsRoot == null)
        {
            visualsRoot = transform;
        }

        if (playerAttack == null)
        {
            playerAttack = GetComponent<PlayerAttack>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        ApplyMovement(input, Time.deltaTime);
        UpdateFacing(input);
        UpdateAnimator(input);
    }

    private void ApplyMovement(Vector2 input, float deltaTime)
    {
        Vector2 targetVelocity = input * maxSpeed;

        float accelRate = acceleration;
        if (input.sqrMagnitude < 0.001f)
        {
            accelRate = deceleration;
        }
        else if (velocity.sqrMagnitude > 0.001f)
        {
            float alignment = Vector2.Dot(velocity.normalized, input);
            if (alignment < 0f)
            {
                accelRate = turnDeceleration;
            }
        }

        velocity = Vector2.MoveTowards(velocity, targetVelocity, accelRate * deltaTime);
        transform.position += (Vector3)(velocity * deltaTime);
    }

    private void ApplyInputFlip(Vector2 input)
    {
        if (visualsRoot == null)
        {
            return;
        }

        if (Mathf.Abs(input.x) > 0.001f)
        {
            Vector3 scale = visualsRoot.localScale;
            scale.x = Mathf.Abs(scale.x) * (input.x < 0f ? -1f : 1f);
            visualsRoot.localScale = scale;
        }
    }

    private void UpdateFacing(Vector2 input)
    {
        if (!faceEnemyInRange)
        {
            ApplyInputFlip(input);
            return;
        }

        Transform enemy = FindEnemyInRange();
        if (enemy != null)
        {
            Vector2 direction = enemy.position - transform.position;
            ApplyTargetFlip(direction);
            return;
        }

        ApplyInputFlip(input);
    }

    private Transform FindEnemyInRange()
    {
        if (playerAttack != null && playerAttack.CurrentTarget != null)
        {
            return playerAttack.CurrentTarget;
        }

        float radius = enemyFaceRadius;
        LayerMask mask = enemyMask;
        if (playerAttack != null)
        {
            if (radius <= 0f)
            {
                radius = playerAttack.AttackRadius;
            }

            if (mask.value == 0)
            {
                mask = playerAttack.EnemyMask;
            }
        }

        if (radius <= 0f || mask.value == 0)
        {
            return null;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, mask);
        if (hits == null || hits.Length == 0)
        {
            return null;
        }

        Transform closest = hits[0].transform;
        float bestSqr = (closest.position - transform.position).sqrMagnitude;
        for (int i = 1; i < hits.Length; i++)
        {
            float sqr = (hits[i].transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                closest = hits[i].transform;
            }
        }

        return closest;
    }

    private void OnDrawGizmosSelected()
    {
        float radius = enemyFaceRadius;
        if (playerAttack != null && radius <= 0f)
        {
            radius = playerAttack.AttackRadius;
        }

        if (radius > 0f)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }

    private void ApplyTargetFlip(Vector2 direction)
    {
        if (visualsRoot == null)
        {
            return;
        }

        if (Mathf.Abs(direction.x) > 0.001f)
        {
            Vector3 scale = visualsRoot.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction.x < 0f ? -1f : 1f);
            visualsRoot.localScale = scale;
        }
    }

    private void UpdateAnimator(Vector2 input)
    {
        if (animator == null)
        {
            return;
        }

        bool isMoving = velocity.sqrMagnitude > 0.01f;
        animator.SetBool("isMoving", isMoving);
        animator.SetFloat("MoveX", input.x);
        animator.SetFloat("MoveY", input.y);
    }
}
