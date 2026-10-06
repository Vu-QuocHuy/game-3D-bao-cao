using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainingArena {
// Restores the "wide angle" view: V toggles third-person <-> a fixed high angle behind the player, blended by the
// Cinemachine Brain. Only the live camera's Priority changes, so the player is never moved. The wide camera is a
// Cinemachine Camera with no procedural components, so it keeps the transform this script gives it every frame.
[DefaultExecutionOrder(-20)]
public sealed class CameraModeSwitcher : MonoBehaviour {
    public PlayerInput playerInput;
    public Transform player;
    public CinemachineCamera thirdPerson, wide;
    public Vector3 wideOffset = new Vector3(0, 10, -12);
    public float widePitch = 45;

    public bool Wide { get; private set; }
    public string ModeName => Wide ? "WIDE ANGLE" : "THIRD PERSON";

    InputAction toggle;

    void Awake() => toggle = playerInput.actions["ToggleCamera"];

    void Update() {
        if (toggle.WasPressedThisFrame()) SetWide(!Wide);
    }

    void LateUpdate() {
        // Follow the player but keep a fixed world rotation, so the picture stays steady while the character turns.
        wide.transform.SetPositionAndRotation(player.position + wideOffset, Quaternion.Euler(widePitch, 0, 0));
    }

    public void SetWide(bool value) {
        Wide = value;
        thirdPerson.Priority = value ? 0 : 20;
        wide.Priority = value ? 20 : 0;
    }

    // Jump straight to the third-person camera (no blend), used by R.
    public void ResetMode() {
        SetWide(false);
        wide.PreviousStateIsValid = false;
    }
}
}
