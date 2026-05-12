using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float arriveDistance = 0.1f;

    private Transform target;
    private float damage;

    public void Initialize(Transform targetTransform, float damageAmount)
    {
        target = targetTransform;
        damage = damageAmount;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = target.position;
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        Vector3 direction = targetPosition - transform.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (Vector3.Distance(transform.position, targetPosition) <= arriveDistance)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable == null)
        {
            damageable = target.GetComponentInParent<IDamageable>();
        }

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
        else
        {
            Debug.Log("Arrow hit, but target has no IDamageable.");
        }

        // TODO: Add hit VFX, SFX, and pooling later.
        Destroy(gameObject);
    }
}
