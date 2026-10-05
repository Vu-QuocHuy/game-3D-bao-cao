using System.Collections.Generic;
using UnityEngine;
namespace TrainingArena {
[RequireComponent(typeof(PlayerMotor), typeof(PlayerInputHandler), typeof(Health))]
[DefaultExecutionOrder(-10)]
public sealed class PlayerBrain : MonoBehaviour {
    public enum StateMode { Grounded = 0, Airborne = 1, Hit = 2, Dead = 3 }
    public StateMode CurrentState { get; private set; } = StateMode.Grounded;
    public Queue<string> History { get; } = new Queue<string>();
    // Block reasons stay visible briefly so the HUD can show one-frame rejections (Jump in air, H off ground).
    public string BlockReason => Time.time < blockUntil ? blockReason : "";
    string blockReason = "";
    float blockUntil;
    
    public Transform spawn;
    public bool topic5 = true; // For UI display compatibility
    PlayerMotor motor; 
    PlayerInputHandler input; 
    Health health;
    PlayerCombat combat;
    float hitTimer;

    void Awake() {
        motor = GetComponent<PlayerMotor>();
        input = GetComponent<PlayerInputHandler>();
        health = GetComponent<Health>();
        combat = GetComponent<PlayerCombat>();
        Record("Initialize → Grounded");
    }

    void OnEnable() {
        health.Damaged += OnDamaged;
        health.Died += OnDied;
    }

    void OnDisable() {
        health.Damaged -= OnDamaged;
        health.Died -= OnDied;
    }

    void Record(string s) {
        History.Enqueue(s);
        while (History.Count > 5) History.Dequeue();
    }
    
    void ChangeState(StateMode next) {
        if (CurrentState == next) return;
        Record($"{CurrentState} → {next}");
        CurrentState = next;
    }

    void Block(string reason, float seconds = 1.5f) {
        blockReason = reason;
        blockUntil = Time.time + seconds;
    }

    void OnDamaged() {
        if (CurrentState != StateMode.Grounded) return;
        combat.Cancel();
        ChangeState(StateMode.Hit);
        hitTimer = 0.7f; // duration of hit
        Block("Hit stun", hitTimer);
    }

    void OnDied() {
        ChangeState(StateMode.Dead);
        combat.Cancel();
        input.Clear();
        Block("Dead blocks all input");
    }

    public void ReceiveHit() {
        if (CurrentState != StateMode.Grounded) {
            Block("H only works when Grounded");
            return;
        }
        health.Damage(25);
    }

    public void ReceiveKill() {
        if (CurrentState != StateMode.Dead) {
            health.Damage(health.Current);
        }
    }

    void Update() {
        input.Sample();
        
        if (CurrentState == StateMode.Dead) {
            Block("Dead blocks all input", 0.1f);
            motor.SetCrouch(false);
            motor.Step(Vector2.zero, false, false, false, false);
            return;
        }

        if (CurrentState == StateMode.Hit) {
            hitTimer -= Time.deltaTime;
            if (!motor.Grounded) {
                ChangeState(StateMode.Airborne); 
            } else if (hitTimer <= 0) {
                ChangeState(StateMode.Grounded);
            }
        } else if (CurrentState == StateMode.Grounded) {
            if (!motor.Grounded) {
                ChangeState(StateMode.Airborne);
            }
        } else if (CurrentState == StateMode.Airborne) {
            if (motor.Grounded && motor.VerticalVelocity <= 0) {
                ChangeState(StateMode.Grounded);
            } else if (input.JumpPressed) {
                Block("Jump blocked in air");
            }
        }

        bool canMove = CurrentState == StateMode.Grounded || CurrentState == StateMode.Airborne;
        bool canJump = CurrentState == StateMode.Grounded;

        // Crouch is a speed level inside Grounded, not a fifth FSM state. The motor refuses to stand under a ceiling.
        motor.SetCrouch(CurrentState == StateMode.Grounded && input.CrouchHeld);
        if (motor.Crouched && !input.CrouchHeld) Block("Ceiling: cannot stand up", .2f);
        if (motor.Crouched && canJump && input.JumpPressed) Block("Jump blocked while crouched");

        // Attack is an upper-body action layered on Grounded: walking/running while swinging is allowed.
        if (input.AttackPressed) {
            if (CurrentState != StateMode.Grounded) Block("Attack only when Grounded");
            else if (!combat.TryStart()) Block("Attack on cooldown", .5f);
        }

        motor.Step(input.Move, input.SprintHeld, input.JumpPressed, canMove, canJump);
    }

    public void ResetPlayer() {
        health.Restore();
        input.Clear();
        combat.Cancel();
        motor.ResetAt(spawn ? spawn.position : Vector3.up);
        CurrentState = StateMode.Grounded;
        hitTimer = 0;
        blockUntil = 0;
        History.Clear();
        Record("Reset → Grounded");
    }
}
}
