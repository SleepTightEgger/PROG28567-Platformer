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
        // set acceleration and deceleration to maxSpeed divided by acceleration time
        acceleration = maxSpeed/accelerationTime;
        deceleration = maxSpeed/decelerationTime;

        // set gravity value based on apexTime
        gravity = -2 * apexHeight / (Mathf.Pow(apexTime, 2));
        // set the velocity of the jump to 2 times apexHeight divided by apexTime
        jumpVelocity = 2 * apexHeight / apexTime;
    }

    // Update is called once per frame
    void Update()
    {
        // set player x input to all controls mapped to "Horizontal" (WASD/Arrow Keys)
        playerInput.x = Input.GetAxisRaw("Horizontal");
        // set player y input to all controls mapped to "Jump" (Space) while pressed, returning 1 if pressed 0 if not
        playerInput.y = Input.GetButton("Jump") ? 1 : 0;
        // if the playerInput.y equals 1 ("Jump" held down) then..
        if (playerInput.y == 1)
        {
            // if the player was not already pressing the jump button
            if (!jumpPressed)
            {
                // set base apexHeight (to allow for quick tap jump for some decent height)
                apexHeight = 2f;
            }
            // set jumpPressed boolean to true
            jumpPressed = true;
        }
        // if playerInput.y is NOT = 1 (is 0) then
        else
        {
            // set jumpPressed boolean to false
            jumpPressed = false;
        }

        // map dash requested boolean to all controls mapped to "Fire3" (left shift) return true if pressed false when not
        dashRequested = Input.GetButtonDown("Fire3") ? true : false;
        // map shockwave boolean to left mouse button return true if pressed false when not
        shockwave = Input.GetMouseButtonDown(0) ? true : false;

        // call movement update, gravity, and shockwave functions each frame
        MovementUpdate();
        Gravity();
        Shockwave();
    }

    // movement update function
    private void MovementUpdate()
    {
        // call walking, jumping, dash, and coyote time logic functions
        WalkingLogic();
        JumpingLogic();
        DashLogic();
        CoyoteTimeLogic();

        // set linear velocity to velocity boolean
        body.linearVelocity = velocity;
    }

    // walking logic function
    public void WalkingLogic()
    {
        // if the player is not currently dashing then
        if (!isDashing)
        {
            // if the x input is not 0
            if (playerInput.x != 0)
            {
                // increment the horizontal velocity based on which direction the player is trying to go times the acceleration value each second
                velocity.x += playerInput.x * acceleration * Time.deltaTime;
                // clamp horizontal velocity to the max speed (negative max speed for left movement, positive max speed for right)
                velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);
            }
            // if the absolute value of the horizontal velocity is very low
            else if (Mathf.Abs(velocity.x) > 0.005f)
            {
                // reduce the velocity by the sign of the current horizontal velocity times deceleration value every second
                velocity.x += -Mathf.Sign(velocity.x) * deceleration * Time.deltaTime;
            }
            // in any other case (player is already slowed to a crawl)
            else
            {
                // set horizontal velocity to 0
                velocity.x = 0;
            }
        }
    }

    // jumping logic function
    public void JumpingLogic()
    {
        // if the jumpPressed value is true (player is holding down space) and the player is
        // grounded OR the coyoteTime variable is larger than 0
        if (jumpPressed && (IsGrounded() || coyoteTime > 0))
        {
            // increment the apexHeight by 2 each second
            apexHeight += 2f * Time.deltaTime;
            // clamp the apex height between 2 and 4 so as not to allow for infinite jump height charging
            apexHeight = Mathf.Clamp(apexHeight, 2f, 4f);
        }
        // on the frame that the player releases the jump button and if they are grounded
        if (Input.GetButtonUp("Jump") && IsGrounded())
        {
            // set the jump velocity based on the newly calculated apexHeight over apexTime
            jumpVelocity = 2 * apexHeight / apexTime;
            // set the vertical velocity to the newly calculated jumpVelocity
            velocity.y = jumpVelocity;
            // set coyoteTime variable to 0 to disallow jumping while in air if not from a fall
            coyoteTime = 0;
            // set a flag that considers the player to be jumping
            jumping = true;
        }
    }

    // dash logic function
    public void DashLogic()
    {
        // if dashRequested boolean is true (player pressed left shift)
        // and they can dash (it is not on cooldown) and have access to an airdash
        if (dashRequested && canDash && airDash)
        {
            // set the velocity of the dash based on the direction the player is facing by
            // checking if the lastDirection was left, returning -10 if true and 10 if false
            dashVelocity = (lastFacingDirection == FacingDirection.left) ? -10 : 10;
            // create a timer/duration for the dash
            dashTime = 0.25f;
            // set a flag that considers the player to be dashing
            isDashing = true;
            // clear all current velocity before the dash
            velocity = Vector2.zero;
            // set a new horizontal velocity based on dash velocity/direction (-10 for left, 10 for right)
            velocity.x = dashVelocity;
            // if the player is not grounded when the dash is called then,
            if (!IsGrounded())
            {
                // set air dash boolean to false (to disallow multiple dashes in the air
                airDash = false;
            }
            // begin the dash cooldown timer coroutine
            StartCoroutine(DashCooldown());
        }

        // if the player is dashing then
        if (isDashing)
        {
            // decrement the dash timer/duration by time.Deltatime (just count it down)
            dashTime -= Time.deltaTime;
            // clamp dashtime between 0 and 0.25
            dashTime = Mathf.Clamp(dashTime, 0, 0.25f);

            // once the dash time has been decremented to 0
            if (dashTime == 0)
            {
                // set flag to consider the player no longer dashing
                isDashing = false;
                // set the horizontal velocity to be very low (to slow down the
                // player movement but still make it feel like there's some momentum
                // without sliding too much)
                velocity.x = (lastFacingDirection == FacingDirection.left) ? -0.75f : 0.75f;
            }
        }
    }

    // coyote time logic function
    public void CoyoteTimeLogic()
    {
        // if the player is grounded
        if (IsGrounded())
        {
            // set coyote time variable to true
            coyoteTime = 0f;
            if (!airDash)
            {
                // set airdash to true (allowing the player to air dash once again)
                airDash = true;
            }
        }
        // if the player is not grounded...
        else
        {
            // and if the vertical velocity is downward while the coyote timer is still 0 and
            // the player is not considered jumping
            if (velocity.y < 0 && coyoteTime == 0f && !jumping)
            {
                // set the coyote time veriable to 0.5
                coyoteTime = 0.5f;
            }
            // else, if the vertical velocity is just downward then
            else if (velocity.y < 0)
            {
                // decrement the coyote time variable, giving the player
                // until it reaches 0 to call for a jump
                coyoteTime -= Time.deltaTime;
                // clamp coyote time between 0 and 0.5
                coyoteTime = Mathf.Clamp(coyoteTime, 0f, 0.5f);
            }
        }
    }
    
    //shockwave function
    public void Shockwave()
    {
        // if the shockwave boolean is true (pressed left click) then...
        if (shockwave)
        {
            // instantiate the shockwaveSlash prefab at the players location
            GameObject sw = Instantiate(shockwaveSlash, transform.position, transform.rotation);
            // calculate a direction vector based on current facing direction
            Vector2 direction = (lastFacingDirection == FacingDirection.left) ? Vector2.left : Vector2.right;
            // add a force to the instantiated prefab in that direction
            sw.GetComponent<shockwaveSlash>().body.AddForce(direction * 10, ForceMode2D.Impulse);
            // pass the direction into the prafab
            sw.GetComponent<shockwaveSlash>().direction = direction;
        }
    }

    // gravity function
    public void Gravity()
    {
        // if the player is not grounded or dashing
        if (!IsGrounded() && !isDashing)
        {
            // decrement the vertical velocity by the previously calculated gravity each second
            // gravity value is negative which is why it is += and not -=
            velocity.y += gravity * Time.deltaTime;
            // clamp vertical velocity to the terminal speed variable so the player does not exceed a 
            // terminal speed
            velocity.y = Mathf.Clamp(velocity.y, terminalSpeed, -terminalSpeed);
        }
        // if the vertical velocity is less than 0 (downward)
        else if (velocity.y < 0)
        {
            // the player is no longer considered jumping
            jumping = false;
            // set the velocity to 0
            velocity.y = 0;
        }
    }

    // IsWalking boolean function
    public bool IsWalking()
    {
        // if the player has a horizontal velocity and is not dashing
        if (velocity.x != 0 && !isDashing)
        {
            // the player is considered to be walking
            return true;
        }
        // in all other cases, the player is not considered to be walking
        return false;
    }

    // IsGrounded Boolean
    public bool IsGrounded()
    {
        // create a boxcast roughly the size of the player sprite positioned slightly below the sprites feet
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, new Vector2(0.75f, 1), 0, Vector2.down, 0.2f);
        // if that boxcast collides with something that has a collider that does not ignore raycasts
        if (hit.collider != null)
        {
            // the player is considered to be grounded
            return true;
        }
        // in all other cases, the player is not considered to be grounded
        return false;
    }

    // facing direction enum function
    public FacingDirection GetFacingDirection()
    {
        // if the player is not dashing
        // (to disallow the player to change
        // facing direction while dashing)
        if (!isDashing)
        {
            // if the player x input greater than zero (player moving to right)
            if (playerInput.x > 0)
            {
                // set the lastFacingDirection to right
                lastFacingDirection = FacingDirection.right;
            }
            // if the player x input less than zero (player moving to left)
            if (playerInput.x < 0)
            {
                // set the lastFacingDirection to left
                lastFacingDirection = FacingDirection.left;
            }
        }

        // return the value of the last facing direction variable
        // as determined above
        return lastFacingDirection;
    }

    // dash cooldown coroutine
    IEnumerator DashCooldown()
    {
        // set can dash boolean to false
        canDash = false;
        // create a dash cooldown float and set it to 5 seconds
        float dashCooldown = 0.5f;
        // while dash cooldown is greater than 0
        while (dashCooldown > 0)
        {
            // decrement it by the amount of seconds that has passed since last frame
            dashCooldown -= Time.deltaTime;
            // return to the beginning of the while loop
            yield return null;
        }
        // once the while loop condition is false (dash cooldown is less than
        // or equal to zero) set the dash boolean back to true
        // (player can now dash again as cooldown is over)
        canDash = true;
    }
}
