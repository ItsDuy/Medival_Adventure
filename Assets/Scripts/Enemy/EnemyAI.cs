using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float sightRadius = 6f;
    [SerializeField] private float stopDistance = 1.1f;
    [SerializeField] private Transform target;

    [Header("Movement Randomization")]
    [SerializeField] private float minSpeed = 1.5f;
    [SerializeField] private float maxSpeed = 3.5f;
    [SerializeField] private Vector2 moveDurationRange = new Vector2(0.6f, 1.4f);
    [SerializeField] private Vector2 stopDurationRange = new Vector2(0.25f, 0.75f);

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualsRoot;

    private Vector2 currentVelocity;
    private bool isMoving;
    private float stateTimer;
    private float currentSpeed;

    protected virtual void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (visualsRoot == null)
        {
            visualsRoot = transform;
        }

        if (target == null)
        {
            TryFindTarget();
        }
    }

    protected virtual void Update()
    {
        if (target == null)
        {
            TryFindTarget();
            SetIdle();
            UpdateAnimator();
            return;
        }

        if (!IsTargetInSight())
        {
            SetIdle();
            UpdateAnimator();
            return;
        }

        UpdateChase(Time.deltaTime);
        UpdateAnimator();
    }

    protected virtual void UpdateChase(float deltaTime)
    {
        Vector2 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;
        if (distance <= stopDistance)
        {
            SetIdle();
            return;
        }

        AdvanceMovementCycle(deltaTime);
        if (!isMoving)
        {
            currentVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = toTarget.normalized;
        currentVelocity = direction * currentSpeed;
        transform.position += (Vector3)(currentVelocity * deltaTime);
        ApplyFlip(direction);
    }

    protected virtual void AdvanceMovementCycle(float deltaTime)
    {
        stateTimer -= deltaTime;
        if (stateTimer > 0f)
        {
            return;
        }

        isMoving = !isMoving;
        if (isMoving)
        {
            currentSpeed = Random.Range(minSpeed, maxSpeed);
            stateTimer = Random.Range(moveDurationRange.x, moveDurationRange.y);
        }
        else
        {
            currentSpeed = 0f;
            stateTimer = Random.Range(stopDurationRange.x, stopDurationRange.y);
        }
    }

    protected virtual bool IsTargetInSight()
    {
        float radius = Mathf.Max(0f, sightRadius);
        float sqrDistance = (target.position - transform.position).sqrMagnitude;
        return sqrDistance <= radius * radius;
    }

    protected virtual void SetIdle()
    {
        isMoving = false;
        currentVelocity = Vector2.zero;
    }

    protected virtual void ApplyFlip(Vector2 direction)
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

    protected virtual void UpdateAnimator()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool("isMoving", currentVelocity.sqrMagnitude > 0.01f);
    }

    protected virtual void TryFindTarget()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            target = player.transform;
            return;
        }

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            target = playerHealth.transform;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
