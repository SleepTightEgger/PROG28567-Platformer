using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Vector2 velocity;

    public Rigidbody2D body;

    private float acceleration;
    private float deceleration;
    private float accelerationTime = 1f;
    private float decelerationTime = 1f;

    private float maxSpeed = 10f;
    private float gravity = -20f;

    private float jumpVelocity = 10f;
    public float apexHeight = 3f;
    public float apexTime = 0.5f;

    public float terminalSpeed = 0.01f;
    public float coyoteTime = 0.5f;

    private bool jumpPressed = false;
    private bool jumping;

    Vector2 playerInput = new Vector2();

    FacingDirection lastFacingDirection = FacingDirection.right;

    public enum FacingDirection
    {
        left, right
    }

    // Start is called before the first frame update
    void Start()
    {
        acceleration = maxSpeed/accelerationTime;
        deceleration = maxSpeed/decelerationTime;

        gravity = -2 * apexHeight / (Mathf.Pow(apexTime, 2));
        jumpVelocity = 2 * apexHeight / apexTime;
    }

    // Update is called once per frame
    void Update()
    {
        // The input from the player needs to be determined and
        // then passed in the to the MovementUpdate which should
        // manage the actual movement of the character.
        playerInput.x = Input.GetAxisRaw("Horizontal");
        playerInput.y = Input.GetButtonDown("Jump") ? 1 : 0;
        if (playerInput.y == 1)
        {
            jumpPressed = true;
        }
        else
        {
            jumpPressed = false;
        }

        MovementUpdate(playerInput);
        Gravity();
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        if (playerInput.x != 0)
        {
            velocity.x += playerInput.x * acceleration * Time.deltaTime;
            velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);
        }
        else if (Mathf.Abs(velocity.x) > 0.005f)
        {
            velocity.x += -Mathf.Sign(velocity.x) * deceleration * Time.deltaTime;
        }
        else
        {
            velocity.x = 0;
        }

        if (jumpPressed && (IsGrounded() || coyoteTime > 0))
        {
            velocity.y = jumpVelocity;
            coyoteTime = 0;
            jumping = true;
        }

        if (IsGrounded())
        {
            coyoteTime = 0f;
        }
        else
        {
            if (velocity.y < 0 && coyoteTime == 0f && !jumping)
            {
                coyoteTime = 0.5f;
            }
            else if (velocity.y < 0)
            {
                coyoteTime -= Time.deltaTime;
                coyoteTime = Mathf.Clamp(coyoteTime, 0f, 0.5f);
            }
        }

        body.linearVelocity = velocity;
    }

    public void Gravity()
    {
        if (!IsGrounded())
        {
            velocity.y += gravity * Time.deltaTime;
            velocity.y = Mathf.Clamp(velocity.y, terminalSpeed, -terminalSpeed);
        }
        else if (velocity.y < 0)
        {
            jumping = false;
            velocity.y = 0;
        }
    }

    public bool IsWalking()
    {
        if (velocity.x != 0)
        {
            return true;
        }
        return false;
    }
    public bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one, 0, Vector2.down, 0.2f);
        if (hit.collider != null)
        {
            return true;
        }
        return false;
    }

    public FacingDirection GetFacingDirection()
    {
        if (playerInput.x > 0)
        {
            lastFacingDirection = FacingDirection.right;
        }
        if (playerInput.x < 0)
        {
            lastFacingDirection = FacingDirection.left;
        }

        return lastFacingDirection;
    }
}
