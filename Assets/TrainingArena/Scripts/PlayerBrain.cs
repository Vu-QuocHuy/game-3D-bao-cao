using System.Collections.Generic;
using UnityEngine;
namespace TrainingArena {
[RequireComponent(typeof(PlayerMotor), typeof(PlayerInputHandler), typeof(Health))]
[DefaultExecutionOrder(-10)]
public sealed class PlayerBrain : MonoBehaviour {
    public enum StateMode { Grounded = 0, Airborne = 1, Hit = 2, Dead = 3 }
    public StateMode CurrentState { get; private set; } = StateMode.Grounded;
    public Queue<string> History { get; } = new Queue<string>();
    public string BlockReason { get; private set; } = "";
    
    public Transform spawn;
    public bool topic5 = true; // For UI display compatibility
    PlayerMotor motor; 
    PlayerInputHandler input; 
    Health health;
    float hitTimer;

    void Awake() {
        motor = GetComponent<PlayerMotor>();
        input = GetComponent<PlayerInputHandler>();
        health = GetComponent<Health>();
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

    void OnDamaged() {
        // Automatically handled in ReceiveHit or demo debug keys
    }

    void OnDied() {
        ChangeState(StateMode.Dead);
        BlockReason = "Dead blocks all input";
    }

    public void ReceiveHit() {
        if (CurrentState == StateMode.Grounded) {
            health.Damage(25);
            if (health.Current > 0) {
                ChangeState(StateMode.Hit);
                hitTimer = 0.7f; // duration of hit
                BlockReason = "Hit stun";
            }
        } else {
            BlockReason = "H only works when Grounded";
        }
    }

    public void ReceiveKill() {
        if (CurrentState != StateMode.Dead) {
            health.Damage(health.Current);
        }
    }

    void Update() {
        input.Sample();
        BlockReason = "";
        
        if (CurrentState == StateMode.Dead) {
            BlockReason = "Dead blocks all input";
            motor.Step(Vector2.zero, false, false, false, false);
            return;
        }

        if (CurrentState == StateMode.Hit) {
            hitTimer -= Time.deltaTime;
            if (!motor.Grounded) {
                ChangeState(StateMode.Airborne); 
            } else if (hitTimer <= 0) {
                ChangeState(StateMode.Grounded);
            } else {
                BlockReason = "Hit stun";
            }
        } else if (CurrentState == StateMode.Grounded) {
            if (!motor.Grounded) {
                ChangeState(StateMode.Airborne);
            }
        } else if (CurrentState == StateMode.Airborne) {
            if (motor.Grounded && motor.VerticalVelocity <= 0) {
                ChangeState(StateMode.Grounded);
            } else if (input.JumpPressed) {
                BlockReason = "Jump blocked in air";
            }
        }

        bool canMove = CurrentState == StateMode.Grounded || CurrentState == StateMode.Airborne;
        bool canJump = CurrentState == StateMode.Grounded;

        motor.Step(input.Move, input.SprintHeld, input.JumpPressed, canMove, canJump);
    }

    public void ResetPlayer() {
        health.Restore();
        input.Clear();
        motor.ResetAt(spawn ? spawn.position : Vector3.up);
        CurrentState = StateMode.Grounded;
        hitTimer = 0;
        BlockReason = "";
        History.Clear();
        Record("Reset → Grounded");
    }
}
}
