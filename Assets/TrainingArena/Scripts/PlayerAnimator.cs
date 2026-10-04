using UnityEngine;
namespace TrainingArena {
[DefaultExecutionOrder(10)]
public sealed class PlayerAnimator : MonoBehaviour {
    public PlayerBrain brain;
    public PlayerMotor motor;
    public Animator animator;
    public PlayerCombat combat; // Kept to avoid missing reference, unused
    public AudioSource audioSource;
    public AudioClip footstep;
    
    void OnEnable() {
        if (motor) motor.Landed += Landing;
    }
    
    void OnDisable() {
        if (motor) motor.Landed -= Landing;
    }
    
    void Landing(float speed) {
        if (speed > 3 && audioSource && footstep) audioSource.PlayOneShot(footstep, 0.22f);
    }
    
    void Update() {
        if (!animator || !brain || !motor) return;
        animator.SetInteger("Mode", (int)brain.CurrentState);
        animator.SetFloat("Speed", motor.Speed, 0.1f, Time.deltaTime);
        animator.SetFloat("VerticalSpeed", motor.VerticalVelocity, 0.1f, Time.deltaTime);
    }
    
    public void OpenHitbox() {} // Dummy
    public void CloseHitbox() {} // Dummy
    
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
