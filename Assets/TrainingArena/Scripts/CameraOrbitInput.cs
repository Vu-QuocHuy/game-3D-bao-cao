using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainingArena {
// Feeds the Look action into the Orbital Follow axes. Mouse delta is already per-frame; sticks
// (gamepad right stick or the on-screen look area) are rates and get scaled by deltaTime.
[RequireComponent(typeof(CinemachineOrbitalFollow))]
public sealed class CameraOrbitInput : MonoBehaviour {
    public PlayerInput playerInput;
    // First-commit feel: 0.12 deg/px yaw, 0.1 deg/px pitch, start pitch 18.
    public float yawDegreesPerPixel = .12f, pitchDegreesPerPixel = .1f, stickDegreesPerSecond = 160;
    public float startYaw = 0, startPitch = 18;

    CinemachineOrbitalFollow orbit;
    InputAction look;

    void Awake() {
        orbit = GetComponent<CinemachineOrbitalFollow>();
        look = playerInput.actions["Look"];
        ResetView();
    }

    void Update() {
        Vector2 v = look.ReadValue<Vector2>();
        if (v == Vector2.zero) return;
        bool pointer = look.activeControl?.device is Pointer;
        if (pointer && Cursor.lockState != CursorLockMode.Locked && !PlayerController.IgnoreCursorLock) return; // Esc: cursor free, camera still
        Vector2 deg = pointer ? new Vector2(v.x * yawDegreesPerPixel, v.y * pitchDegreesPerPixel) : v * stickDegreesPerSecond * Time.deltaTime;
        orbit.HorizontalAxis.Value = orbit.HorizontalAxis.ClampValue(orbit.HorizontalAxis.Value + deg.x);
        orbit.VerticalAxis.Value = orbit.VerticalAxis.ClampValue(orbit.VerticalAxis.Value - deg.y);
    }

    public void ResetView() {
        orbit.HorizontalAxis.Value = startYaw;
        orbit.VerticalAxis.Value = startPitch;
        var cam = GetComponent<CinemachineCamera>();
        if (cam) cam.PreviousStateIsValid = false;
    }
}
}
