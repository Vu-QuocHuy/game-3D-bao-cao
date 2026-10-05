using UnityEngine;
namespace TrainingArena {
// Spec 6.3: copies PlayerController data into Animator parameters. Lives on Model (next to the Animator),
// so it also receives the clips' Animation Events.
[DefaultExecutionOrder(20)]
public sealed class AnimatorBridge : MonoBehaviour {
    public PlayerController player;
    public PlayerCombat combat;
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip footstep;

    // 0 = stand, 0.5 = walk (2 m/s), 1 = run (5 m/s); crouch walk (1.6 m/s) lands at 0.4.
    public float NormalizedSpeed {
        get {
            float s = player.HorizontalSpeed;
            return s <= player.walkSpeed ? .5f * s / player.walkSpeed
                                         : Mathf.Min(1, .5f + .5f * (s - player.walkSpeed) / (player.runSpeed - player.walkSpeed));
        }
    }

    void Update() {
        if (!animator || !animator.isActiveAndEnabled) return;
        animator.SetFloat("Speed", NormalizedSpeed, .1f, Time.deltaTime);
        animator.SetBool("IsGrounded", player.IsGrounded);
        animator.SetBool("Crouch", player.Crouched);
        // Humanoid override layers pin the masked body even in an empty state, so the upper-body layer only
        // gets weight while a swing is running.
        bool swinging = combat && combat.Attacking;
        animator.SetLayerWeight(1, Mathf.MoveTowards(animator.GetLayerWeight(1), swinging ? 1 : 0, Time.deltaTime / (swinging ? .05f : .2f)));
    }

    public void ResetAnimation() {
        if (!animator) return;
        animator.Rebind();
        animator.Update(0);
    }

    // Animation Events.
    public void Footstep() {
        if (player.IsGrounded && player.HorizontalSpeed > .2f && audioSource && footstep) audioSource.PlayOneShot(footstep, .12f);
    }
    public void OpenHitbox() { if (combat) combat.SetWindow(true); }
    public void CloseHitbox() { if (combat) combat.SetWindow(false); }
}
}
