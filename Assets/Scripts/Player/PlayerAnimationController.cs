using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    Animator animator;
    SpriteRenderer spriteRenderer;

    private static readonly int speedHash = Animator.StringToHash("Speed");
    private static readonly int velocityYHash = Animator.StringToHash("VelocityY");
    private static readonly int groundedHash = Animator.StringToHash("Grounded");
    private static readonly int landHash = Animator.StringToHash("Land");
    private static readonly int rollHash = Animator.StringToHash("Roll");
    private static readonly int dashHash = Animator.StringToHash("Dash");

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void UpdateMovementAnimation(float speed, float velocityY, bool grounded)
    {
        animator.SetFloat(speedHash, speed);
        animator.SetFloat(velocityYHash, velocityY);
        animator.SetBool(groundedHash, grounded);
    }


    public void PlayLand()
    {
        animator.SetTrigger(landHash);
    }


    public void PlayRoll()
    {
        animator.SetTrigger(rollHash);
    }


    public void PlayDash()
    {
        animator.SetTrigger(dashHash);
    }


    public void FlipSprite(float direction)
    {
        if (direction > 0)
            spriteRenderer.flipX = false;

        else if (direction < 0)
            spriteRenderer.flipX = true;
    }
}
