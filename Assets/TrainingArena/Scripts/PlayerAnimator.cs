using UnityEngine;
namespace TrainingArena {
[DefaultExecutionOrder(10)]
public sealed class PlayerAnimator : MonoBehaviour {
    public PlayerBrain brain;
    public PlayerMotor motor;
    public Animator animator;
    public PlayerCombat combat;
    public AudioSource audioSource;
    public AudioClip footstep;
    
    void OnEnable() {
        if (!combat && brain) combat = brain.GetComponent<PlayerCombat>();
        if (motor) motor.Landed += Landing;
        if (combat) { combat.Started += AttackStarted; combat.Cancelled += AttackCancelled; }
    }
    
    void OnDisable() {
        if (motor) motor.Landed -= Landing;
        if (combat) { combat.Started -= AttackStarted; combat.Cancelled -= AttackCancelled; }
    }

    void AttackStarted() => animator.SetTrigger("Attack");
    void AttackCancelled() { animator.ResetTrigger("Attack"); animator.CrossFadeInFixedTime("Empty", .05f, 1); }
    
    void Landing(float speed) {
        if (speed > 3 && audioSource && footstep) audioSource.PlayOneShot(footstep, 0.22f);
    }
    
    void Update() {
        if (!animator || !brain || !motor) return;
        animator.SetInteger("Mode", (int)brain.CurrentState);
        animator.SetFloat("Speed", motor.Speed, 0.1f, Time.deltaTime);
        animator.SetBool("Crouch", motor.Crouched);
        animator.SetFloat("VerticalSpeed", motor.VerticalVelocity); // undamped: Jump vs Fall needs the sign on the first airborne frame
    }
    
    // Animation Events on the Attack clip.
    public void OpenHitbox() { if (combat) combat.SetWindow(true); }
    public void CloseHitbox() { if (combat) combat.SetWindow(false); }
    
    public void Footstep() {
        if (motor && motor.Grounded && motor.Speed > 0.2f && audioSource && footstep) {
            audioSource.PlayOneShot(footstep, 0.12f);
        }
    }
    
    public void ResetAnimation() {
        if (!animator) return;
        animator.Rebind();
        animator.Update(0);
    }
}
}
