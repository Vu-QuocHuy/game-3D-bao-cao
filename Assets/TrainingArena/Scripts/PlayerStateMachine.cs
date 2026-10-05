using System.Collections.Generic;
using UnityEngine;
namespace TrainingArena {
// Runs after PlayerController (execution order 10 > 0) so states read this frame's movement data.
[DefaultExecutionOrder(10)]
public sealed class PlayerStateMachine : MonoBehaviour {
    public PlayerController player;
    public PlayerCombat combat;
    public Animator animator;

    public IState Idle { get; private set; }
    public IState Move { get; private set; }
    public IState Jump { get; private set; }
    public IState Fall { get; private set; }
    public IState Crouch { get; private set; }
    public IState Attack { get; private set; }
    public string CurrentName => machine.Current?.Name ?? "-";
    public Queue<string> History { get; } = new Queue<string>();

    readonly StateMachine machine = new StateMachine();

    void Awake() {
        if (!player) player = GetComponent<PlayerController>();
        if (!combat) combat = GetComponent<PlayerCombat>();
        Idle = new IdleState(this); Move = new MoveState(this); Jump = new JumpState(this); Fall = new FallState(this);
        Crouch = new CrouchState(this); Attack = new AttackState(this);
    }

    void Start() => ResetMachine(); // Animator is initialised by now (Idle.Enter writes Speed)

    public void ChangeState(IState next) {
        if (next == machine.Current) return;
        History.Enqueue($"{CurrentName} → {next.Name}");
        while (History.Count > 5) History.Dequeue();
        machine.ChangeState(next);
    }

    void Update() {
        // Extension rules: grounded locomotion states can swing or crouch; the four core states stay untouched.
        var current = machine.Current;
        bool groundedLocomotion = current == Idle || current == Move || current == Crouch;
        if (player.AttackStarted && groundedLocomotion) ChangeState(Attack);
        else if (player.Crouched && (current == Idle || current == Move)) ChangeState(Crouch);
        machine.Update();
    }

    public void ResetMachine() {
        History.Clear();
        machine.ChangeState(Idle);
        History.Enqueue("Reset → Idle");
    }
}
}
