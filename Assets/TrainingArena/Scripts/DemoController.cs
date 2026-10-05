using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace TrainingArena {
// Demo-only tools (spec 9): R reset, F1 HUD, Esc cursor, auto reset after falling off the map.
public sealed class DemoController : MonoBehaviour {
    public PlayerController player;
    public PlayerStateMachine states;
    public AnimatorBridge animatorBridge;
    public CameraOrbitInput cameraInput;
    public DebugHUD hud;

    void Start() {
        Application.targetFrameRate = 60;
        // Touch-only devices (phone, Device Simulator) keep the cursor free for the on-screen controls.
        if (Mouse.current != null) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    }

    void Update() {
        var k = Keyboard.current;
        if (player.transform.position.y < -10) ResetDemo();
        if (k == null) return;
        if (k.rKey.wasPressedThisFrame) ResetDemo();
        if (k.f1Key.wasPressedThisFrame && hud) hud.Toggle();
        if (k.escapeKey.wasPressedThisFrame) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }

    // LateUpdate: the click that re-locks the cursor has already been ignored as an attack this frame.
    void LateUpdate() {
        bool overUI = EventSystem.current && EventSystem.current.IsPointerOverGameObject();
        if (Cursor.lockState != CursorLockMode.Locked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !overUI) {
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        }
    }

    public void ResetDemo() {
        player.ResetTo(player.spawn.position, player.spawn.rotation);
        player.GetComponent<PlayerCombat>()?.Cancel();
        states.ResetMachine();
        if (animatorBridge) animatorBridge.ResetAnimation();
        if (cameraInput) cameraInput.ResetView();
        foreach (var h in FindObjectsByType<Health>(FindObjectsSortMode.None)) h.Restore(); // training dummy
    }

    void OnApplicationFocus(bool focus) {
        if (!focus) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }
}
}
