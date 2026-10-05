using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
namespace TrainingArena {
[DefaultExecutionOrder(-20)]
public sealed class CameraCoordinator : MonoBehaviour {
    public Transform player, targetPivot, fixedAnchor;
    public PlayerInputHandler input;
    
    public CinemachineCamera tps, fps, topDown, fixedCamera; 
    public CinemachineDeoccluder deoccluder;
    public CinemachineImpulseSource impulse;
    
    public bool damping = true, avoidObstacles = true, shake = true;
    public bool ModeWide { get; private set; }
    public string ModeName => ModeWide ? "WIDE ANGLE" : "TPS";
    
    float yaw, pitch = 18, focusHeight = 1.55f;
    Vector3 smoothPosition;
    PlayerMotor motor;
    
    void Start() {
        motor = player.GetComponent<PlayerMotor>();
        if (tps) smoothPosition = tps.transform.position;
    }
    
    public void ResetCamera() {
        ModeWide = false;
        yaw = 0; pitch = 18; focusHeight = 1.55f;
        if (tps) {
            smoothPosition = player.position + new Vector3(0, 3, -5);
            tps.PreviousStateIsValid = false;
        }
        if (topDown) topDown.PreviousStateIsValid = false;
    }
    
    void Update() {
        var k = Keyboard.current;
        if (k == null) return;
        if (k.cKey.wasPressedThisFrame) ModeWide = !ModeWide;
    }
    
    void LateUpdate() {
        if (!player || !motor) return;
        
        yaw += input.Look.x * 0.12f;
        pitch = Mathf.Clamp(pitch - input.Look.y * 0.1f, -30, 70);
        
        float blend = 1 - Mathf.Exp(-9 * Time.deltaTime);
        // Follow the crouched head: a fixed 1.55 m focus sits inside the 1.25 m crouch beam and the Deoccluder has nowhere to go.
        focusHeight = Mathf.Lerp(focusHeight, motor.Crouched ? .95f : 1.55f, blend);
        Vector3 focus = player.position + Vector3.up * focusHeight;
        if (targetPivot) targetPivot.position = focus;
        
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desired = focus - rotation * Vector3.forward * 5.2f;
        
        smoothPosition = Vector3.Lerp(smoothPosition, desired, blend);
        
        if (tps) {
            tps.transform.SetPositionAndRotation(smoothPosition, rotation);
            tps.Priority = !ModeWide ? 20 : 0;
        }
        if (topDown) { // Using topDown reference as wideAngle
            topDown.transform.SetPositionAndRotation(player.position + new Vector3(0, 10, -12), Quaternion.Euler(45, 0, 0));
            topDown.Priority = ModeWide ? 20 : 0;
        }
    }
}
}
