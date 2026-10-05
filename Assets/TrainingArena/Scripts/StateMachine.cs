using UnityEngine;
namespace TrainingArena {
// Spec 6.2.
public interface IState {
    string Name { get; }
    void Enter();
    void Tick();
    void Exit();
}

public sealed class StateMachine {
    public IState Current { get; private set; }
    public void ChangeState(IState next) {
        if (next == null || next == Current) return;
        Current?.Exit();
        Current = next;
        Current.Enter();
    }
    public void Update() => Current?.Tick();
}

// Shared context so each state stays a few lines.
public abstract class PlayerState : IState {
    protected readonly PlayerStateMachine owner;
    protected PlayerController Player => owner.player;
    protected Animator Animator => owner.animator;
    protected PlayerState(PlayerStateMachine owner) => this.owner = owner;
    public abstract string Name { get; }
    public virtual void Enter() { }
    public abstract void Tick();
    public virtual void Exit() { }
    protected bool HasInput => Player.InputMagnitude > .1f;
}

public sealed class IdleState : PlayerState {
    public IdleState(PlayerStateMachine o) : base(o) { }
    public override string Name => "Idle";
    public override void Enter() => Animator.SetFloat("Speed", 0);
    public override void Tick() {
        if (Player.Jumped) owner.ChangeState(owner.Jump);
        else if (!Player.IsGrounded) owner.ChangeState(owner.Fall);
        else if (HasInput) owner.ChangeState(owner.Move);
    }
}

public sealed class MoveState : PlayerState {
    public MoveState(PlayerStateMachine o) : base(o) { }
    public override string Name => "Move";
    public override void Tick() { // Speed itself is written every frame by AnimatorBridge
        if (Player.Jumped) owner.ChangeState(owner.Jump);
        else if (!Player.IsGrounded) owner.ChangeState(owner.Fall);
        else if (!HasInput) owner.ChangeState(owner.Idle);
    }
}

public sealed class JumpState : PlayerState {
    public JumpState(PlayerStateMachine o) : base(o) { }
    public override string Name => "Jump";
    public override void Enter() => Animator.SetTrigger("Jump");
    public override void Tick() { if (Player.VelocityY <= 0) owner.ChangeState(owner.Fall); }
}

public sealed class FallState : PlayerState {
    public FallState(PlayerStateMachine o) : base(o) { }
    public override string Name => "Fall";
    public override void Enter() => Animator.SetBool("IsGrounded", false);
    public override void Tick() { if (Player.IsGrounded) owner.ChangeState(HasInput ? owner.Move : owner.Idle); }
}

// Extensions (spec 12): added without touching Idle/Move/Jump/Fall. PlayerStateMachine enters them by rule.
public sealed class CrouchState : PlayerState {
    public CrouchState(PlayerStateMachine o) : base(o) { }
    public override string Name => "Crouch";
    public override void Tick() {
        if (!Player.IsGrounded) owner.ChangeState(owner.Fall);
        else if (!Player.Crouched) owner.ChangeState(HasInput ? owner.Move : owner.Idle);
    }
}

public sealed class AttackState : PlayerState {
    public AttackState(PlayerStateMachine o) : base(o) { }
    public override string Name => "Attack";
    public override void Enter() => Animator.SetTrigger("Attack");
    public override void Tick() {
        if (Player.Jumped) owner.ChangeState(owner.Jump);
        else if (!Player.IsGrounded) owner.ChangeState(owner.Fall);
        else if (!owner.combat.Attacking) owner.ChangeState(Player.Crouched ? owner.Crouch : HasInput ? owner.Move : owner.Idle);
    }
}
}
