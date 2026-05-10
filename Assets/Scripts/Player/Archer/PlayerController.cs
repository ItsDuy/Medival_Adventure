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

    private Vector2 velocity;
    private Camera mainCamera;

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

        mainCamera = Camera.main;
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
        ApplyMouseFlip();
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

    private void ApplyMouseFlip()
    {
        if (mainCamera == null || visualsRoot == null)
        {
            return;
        }

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        float deltaX = mouseWorld.x - transform.position.x;

        if (Mathf.Abs(deltaX) > 0.001f)
        {
            Vector3 scale = visualsRoot.localScale;
            scale.x = Mathf.Abs(scale.x) * (deltaX < 0f ? -1f : 1f);
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
