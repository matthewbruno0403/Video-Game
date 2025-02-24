using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField] float speed = 2f;
    Vector2 motionVector;
    public Vector2 lastMotionVector;
    Animator animator;

    float lastHorizontal;
    float lastVertical;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        motionVector.Set(horizontal, vertical);

        // Check if player is moving
        bool isMoving = motionVector.sqrMagnitude > 0;
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            // Set direction based on movement
            if (horizontal != 0)
            {
                animator.SetFloat("horizontal", horizontal);
                animator.SetFloat("vertical", 0);
            }
            else
            {
                animator.SetFloat("horizontal", 0);
                animator.SetFloat("vertical", vertical);
            }

            lastHorizontal = horizontal;
            lastVertical = vertical;
            lastMotionVector = new Vector2(horizontal, vertical);
        }
        else
        {
            // Set idle direction based on the last movement direction
            animator.SetFloat("horizontal", lastHorizontal);
            animator.SetFloat("vertical", lastVertical);
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        rb.velocity = motionVector * speed;
    }

    public Vector2 facingDirection
    {
        get
        {
            // If lastMotionVector is zero, default to down (or any direction you prefer)
            return lastMotionVector == Vector2.zero ? Vector2.down : lastMotionVector.normalized;
        }
    }
}
