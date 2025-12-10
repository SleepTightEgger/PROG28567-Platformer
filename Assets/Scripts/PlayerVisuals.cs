using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer bodyRenderer;
    public PlayerController playerController;

    private readonly int isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int isGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int jumpWarmupHash = Animator.StringToHash("jumpPressed");
    private readonly int dashingHash = Animator.StringToHash("isDashing");
    void Update()
    {
        animator.SetBool(isWalkingHash, playerController.IsWalking());
        animator.SetBool(isGroundedHash, playerController.IsGrounded());
        animator.SetBool(jumpWarmupHash, playerController.jumpPressed);
        animator.SetBool(dashingHash, playerController.isDashing);

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
