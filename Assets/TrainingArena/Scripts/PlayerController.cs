using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainingArena {
// Spec 6.1: reads input, computes velocity, calls CharacterController.Move exactly once per frame,
// turns the character and publishes data for the state machine, animator bridge and HUD.
[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public sealed class PlayerController : MonoBehaviour {
    [Header("Movement")]
    public float runSpeed = 5, walkSpeed = 2, crouchSpeed = 1.6f, gravity = -20, jumpHeight = 1.2f, turnSpeed = 720;
    public Transform cameraTarget, spawn;
    public LayerMask groundMask = 1 << 6; // Environment

    public float InputMagnitude { get; private set; }
    public float HorizontalSpeed { get; private set; }
    public float VelocityY { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool Jumped { get; private set; }        // jump impulse applied this frame
    public bool Walking { get; private set; }
    public bool Crouched { get; private set; }
    public bool AttackStarted { get; private set; } // a swing began this frame
    public string BlockReason => Time.time < blockUntil ? blockReason : "";
    // Set only by DemoVerifier: -batchmode players cannot lock the cursor.
    internal static bool IgnoreCursorLock;

    CharacterController controller;
    PlayerCombat combat;
    Camera view;
    InputAction move, jump, walk, crouch, attack;
    float standingHeight, standingStep, blockUntil;
    Vector3 standingCenter;
    string blockReason = "";

    void Awake() {
        controller = GetComponent<CharacterController>();
        combat = GetComponent<PlayerCombat>();
        var actions = GetComponent<PlayerInput>().actions;
        move = actions["Move"]; jump = actions["Jump"]; walk = actions["Walk"];
        crouch = actions["Crouch"]; attack = actions["Attack"];
        standingHeight = controller.height; standingCenter = controller.center; standingStep = controller.stepOffset;
        view = Camera.main;
    }

    void Block(string reason, float seconds = 1.5f) { blockReason = reason; blockUntil = Time.time + seconds; }

    void Update() {
        float dt = Time.deltaTime;
        Vector2 input = Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1);
        InputMagnitude = input.magnitude;
        JumpPressed = jump.WasPressedThisFrame();
        Walking = walk.IsPressed();
        Jumped = AttackStarted = false;

        SetCrouch(crouch.IsPressed() && IsGrounded);

        // Camera-relative: camera forward/right flattened onto the ground.
        Vector3 forward = Vector3.forward, right = Vector3.right;
        if (view) {
            forward = Vector3.ProjectOnPlane(view.transform.forward, Vector3.up);
            right = Vector3.ProjectOnPlane(view.transform.right, Vector3.up);
            if (forward.sqrMagnitude < .001f) forward = Vector3.ProjectOnPlane(view.transform.up, Vector3.up);
            forward.Normalize(); right.Normalize();
        }
        Vector3 dir = Vector3.ClampMagnitude(forward * input.y + right * input.x, 1);
        float speed = Crouched ? crouchSpeed : Walking ? walkSpeed : runSpeed;

        if (IsGrounded && VelocityY < 0) VelocityY = -2;
        if (JumpPressed) {
            if (!IsGrounded) Block("Jump only when grounded");
            else if (Crouched) Block("Jump blocked while crouched");
            else { VelocityY = Mathf.Sqrt(jumpHeight * -2 * gravity); Jumped = true; }
        }
        VelocityY += gravity * dt;

        if (attack.WasPressedThisFrame() && (Cursor.lockState == CursorLockMode.Locked || IgnoreCursorLock)) {
            if (!IsGrounded || Jumped) Block("Attack only when grounded");
            else if (combat && combat.TryStart()) AttackStarted = true;
            else Block("Attack on cooldown", .5f);
        }

        if (dir.sqrMagnitude > .0001f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * dt);

        bool wasGrounded = IsGrounded;
        var flags = controller.Move((dir * speed + Vector3.up * VelocityY) * dt);
        IsGrounded = controller.isGrounded;
        // Walking down 0.2 m stairs would otherwise flicker Fall for a few frames: snap to ground within step height.
        if (wasGrounded && !IsGrounded && !Jumped &&
            Physics.Raycast(transform.position + Vector3.up * .1f, Vector3.down, out var hit, controller.stepOffset + .15f, groundMask, QueryTriggerInteraction.Ignore))
            IsGrounded = (controller.Move(Vector3.down * (hit.distance - .1f + controller.skinWidth)) & CollisionFlags.Below) != 0;
        if ((flags & CollisionFlags.Above) != 0 && VelocityY > 0) VelocityY = 0;
        HorizontalSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

        // Camera looks at the head; drop it when crouched so it never sits inside the crouch beam.
        if (cameraTarget) {
            var p = cameraTarget.localPosition;
            p.y = Mathf.Lerp(p.y, Crouched ? .9f : 1.55f, 1 - Mathf.Exp(-10 * dt));
            cameraTarget.localPosition = p;
        }
    }

    public bool CanStand() =>
        !Physics.SphereCast(transform.position + Vector3.up * .5f, controller.radius * .9f, Vector3.up, out _, standingHeight - .5f, groundMask, QueryTriggerInteraction.Ignore);

    void SetCrouch(bool desired) {
        if (desired == Crouched) return;
        if (!desired && !CanStand()) { Block("Ceiling: cannot stand up", .2f); return; }
        Crouched = desired;
        controller.height = desired ? 1.05f : standingHeight;
        controller.center = desired ? new Vector3(0, .525f, 0) : standingCenter;
        // PhysX lifts the capsule by stepOffset before moving; 1.05 m + 0.3 m would hit a 1.25 m beam.
        controller.stepOffset = desired ? .1f : standingStep;
    }

    public void ResetTo(Vector3 position, Quaternion rotation) {
        controller.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        controller.enabled = true;
        SetCrouch(false);
        VelocityY = 0; HorizontalSpeed = 0; blockUntil = 0;
        IsGrounded = (controller.Move(Vector3.down * .2f) & CollisionFlags.Below) != 0;
        if (cameraTarget) cameraTarget.localPosition = new Vector3(0, 1.55f, 0);
    }
}
}
