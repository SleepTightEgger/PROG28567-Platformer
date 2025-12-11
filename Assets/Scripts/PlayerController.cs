using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Vector2 velocity;

    public Rigidbody2D body;

    public float acceleration;
    public float deceleration;
    public float accelerationTime = 1f;
    public float decelerationTime = 0.5f;

    public float maxSpeed = 10f;
    public float gravity = -20f;

    public float jumpVelocity = 10f;
    public float apexHeight = 3f;
    public float apexTime = 0.5f;

    public float terminalSpeed = -30f;
    public float coyoteTime = 0.5f;

    public bool jumpPressed = false;
    private bool jumping;

    private bool dashRequested = false;
    private float dashVelocity = 10f;
    private float dashTime = 0.25f;
    public bool isDashing = false;
    private float dashCooldown;
    private bool canDash = true;
    private bool airDash = true;

    public bool shockwave = false;
    public GameObject shockwaveSlash;

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
        playerInput.y = Input.GetButton("Jump") ? 1 : 0;
        if (playerInput.y == 1)
        {
            if (!jumpPressed)
            {
                apexHeight = 2f;
            }
            jumpPressed = true;
        }
        else
        {
            jumpPressed = false;
        }

        dashRequested = Input.GetButtonDown("Fire3") ? true : false;
        shockwave = Input.GetMouseButtonDown(0) ? true : false;

        MovementUpdate();
        Gravity();
        Shockwave();
    }

    private void MovementUpdate()
    {
        WalkingLogic();
        JumpingLogic();
        DashLogic();
        CoyoteTimeLogic();

        body.linearVelocity = velocity;
    }

    public void WalkingLogic()
    {
        if (!isDashing)
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
        }
    }

    public void JumpingLogic()
    {
        if (jumpPressed && (IsGrounded() || coyoteTime > 0))
        {
            apexHeight += 2f * Time.deltaTime;
            apexHeight = Mathf.Clamp(apexHeight, 2f, 4f);
        }
        if (Input.GetButtonUp("Jump") && IsGrounded())
        {
            jumpVelocity = 2 * apexHeight / apexTime;
            velocity.y = jumpVelocity;
            coyoteTime = 0;
            jumping = true;
        }
    }

    public void DashLogic()
    {
        if (dashRequested && canDash && airDash)
        {
            dashVelocity = (lastFacingDirection == FacingDirection.left) ? -10 : 10;
            dashTime = 0.25f;
            isDashing = true;
            velocity = Vector2.zero;
            velocity.x = dashVelocity;
            if (!IsGrounded())
            {
                airDash = false;
            }
            StartCoroutine(DashCooldown());
        }

        if (isDashing)
        {
            dashTime -= Time.deltaTime;
            dashTime = Mathf.Clamp(dashTime, 0, 0.25f);

            if (dashTime == 0)
            {
                isDashing = false;
                velocity.x = (lastFacingDirection == FacingDirection.left) ? -0.75f : 0.75f;
            }
        }
    }

    public void CoyoteTimeLogic()
    {
        if (IsGrounded())
        {
            coyoteTime = 0f;
            if (!airDash)
            {
                airDash = true;
            }
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
    }
    public void Shockwave()
    {
        if (shockwave)
        {
            GameObject sw = Instantiate(shockwaveSlash, transform.position, transform.rotation);
            Vector2 direction = (lastFacingDirection == FacingDirection.left) ? Vector2.left : Vector2.right;
            sw.GetComponent<shockwaveSlash>().body.AddForce(direction * 10, ForceMode2D.Impulse);
            sw.GetComponent<shockwaveSlash>().direction = direction;
        }
    }

    public void Gravity()
    {
        if (!IsGrounded() && !isDashing)
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
        if (velocity.x != 0 && !isDashing)
        {
            return true;
        }
        return false;
    }
    public bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, new Vector2(0.75f, 1), 0, Vector2.down, 0.2f);
        if (hit.collider != null)
        {
            return true;
        }
        return false;
    }

    public FacingDirection GetFacingDirection()
    {
        if (!isDashing)
        {
            if (playerInput.x > 0)
            {
                lastFacingDirection = FacingDirection.right;
            }
            if (playerInput.x < 0)
            {
                lastFacingDirection = FacingDirection.left;
            }
        }

        return lastFacingDirection;
    }

    IEnumerator DashCooldown()
    {
        canDash = false;
        dashCooldown = 0.5f;
        while (dashCooldown > 0)
        {
            dashCooldown -= Time.deltaTime;
            yield return null;
        }
        canDash = true;
    }
}
