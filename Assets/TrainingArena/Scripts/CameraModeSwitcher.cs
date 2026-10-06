using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainingArena {
public enum CameraMode { ThirdPerson, FirstPerson, TopDown }

// T cycles the three camera angles of the project's first commit: third person -> first person -> top down.
// Only the live camera's Priority changes (the Brain blends), so the player is never moved.
// First person and top down are Cinemachine Cameras with no procedural components; this script places them.
[DefaultExecutionOrder(-20)]
public sealed class CameraModeSwitcher : MonoBehaviour {
    public PlayerInput playerInput;
    public Transform player, cameraTarget;
    public CinemachineOrbitalFollow orbit; // its yaw/pitch also aim the first-person view
    public CinemachineCamera thirdPerson, firstPerson, topDown;
    public Vector3 topDownOffset = new Vector3(0, 14, -5);
    public float topDownPitch = 70, eyeForward = .15f;

    public CameraMode Mode { get; private set; }
    public string ModeName => Mode == CameraMode.ThirdPerson ? "THIRD PERSON" : Mode == CameraMode.FirstPerson ? "FIRST PERSON" : "TOP DOWN";

    InputAction toggle;
    Renderer[] model;

    void Awake() {
        toggle = playerInput.actions["ToggleCamera"];
        model = player.GetComponentsInChildren<Renderer>();
    }

    void Update() {
        if (toggle.WasPressedThisFrame()) SetMode((CameraMode)(((int)Mode + 1) % 3));
    }

    void LateUpdate() {
        var look = Quaternion.Euler(orbit.VerticalAxis.Value, orbit.HorizontalAxis.Value, 0);
        firstPerson.transform.SetPositionAndRotation(cameraTarget.position + look * Vector3.forward * eyeForward, look);
        topDown.transform.SetPositionAndRotation(player.position + topDownOffset, Quaternion.Euler(topDownPitch, 0, 0));
    }

    public void SetMode(CameraMode mode) {
        Mode = mode;
        thirdPerson.Priority = mode == CameraMode.ThirdPerson ? 20 : 0;
        firstPerson.Priority = mode == CameraMode.FirstPerson ? 20 : 0;
        topDown.Priority = mode == CameraMode.TopDown ? 20 : 0;
        foreach (var r in model) if (r) r.enabled = mode != CameraMode.FirstPerson; // first person: don't look at the inside of your own head
    }

    // Straight back to third person (no blend), used by R.
    public void ResetMode() {
        SetMode(CameraMode.ThirdPerson);
        firstPerson.PreviousStateIsValid = topDown.PreviousStateIsValid = false;
    }
}
}
