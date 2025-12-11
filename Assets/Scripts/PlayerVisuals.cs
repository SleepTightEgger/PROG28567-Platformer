using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer bodyRenderer;
    public PlayerController playerController;

    private readonly int isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int isGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int jumpWarmupHash = Animator.StringToHash("jumpPressed");
    private readonly int dashingTriggerHash = Animator.StringToHash("isDashing");
    private readonly int shockwaveTriggerHash = Animator.StringToHash("shockwave");
    void Update()
    {
        animator.SetBool(isWalkingHash, playerController.IsWalking());
        animator.SetBool(isGroundedHash, playerController.IsGrounded());
        animator.SetBool(jumpWarmupHash, playerController.jumpPressed);

        if (Input.GetButtonDown("Fire3"))
        {
            animator.SetTrigger(dashingTriggerHash);
        }
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger(shockwaveTriggerHash);
        }

        switch (playerController.GetFacingDirection())
        {
            case PlayerController.FacingDirection.left:
                bodyRenderer.flipX = true;
                break;
            case PlayerController.FacingDirection.right:
                bodyRenderer.flipX = false;
                break;
        }
    }
}
