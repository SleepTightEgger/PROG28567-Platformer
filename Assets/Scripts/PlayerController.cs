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

    Vector2 playerInput = new Vector2();

    public enum FacingDirection
    {
        left, right
    }

    // Start is called before the first frame update
    void Start()
    {
        acceleration = maxSpeed/accelerationTime;
        deceleration = maxSpeed/decelerationTime;
    }

    // Update is called once per frame
    void Update()
    {
        // The input from the player needs to be determined and
        // then passed in the to the MovementUpdate which should
        // manage the actual movement of the character.
        playerInput.x = Input.GetAxisRaw("Horizontal");
        MovementUpdate(playerInput);
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        if (playerInput.x != 0)
        {
            velocity.x += playerInput.x * acceleration * Time.deltaTime;
            velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);
        }
        else if (Mathf.Abs(velocity.x) > 0)
        {
            velocity.x += -Mathf.Sign(velocity.x) * deceleration * Time.deltaTime;
        }
        else
        {
            velocity.x = 0;
        }

        body.linearVelocity = velocity;
    }

    private void PlayerMovement()
    {
        
    }

    public bool IsWalking()
    {
        return false;
    }
    public bool IsGrounded()
    {
        return false;
    }

    public FacingDirection GetFacingDirection()
    {
        return FacingDirection.left;
    }
}
