using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainingArena {
// Runs before PlayerBrain so H/K resolve before Jump in the same frame (Hit/Dead win).
[DefaultExecutionOrder(-15)]
public sealed class DemoController : MonoBehaviour {
    public PlayerBrain player;
    public CameraCoordinator cameraCoordinator;
    public DebugOverlay overlay;
    public bool topic5 = true;
    
    void Start() {
        if (!overlay) overlay = FindAnyObjectByType<DebugOverlay>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Application.targetFrameRate = 60;
    }
    
    void Update() {
        var k = Keyboard.current;
        if (k == null) return;
        
        // Spec: Esc frees the cursor to switch windows; a click locks it again (in LateUpdate, so that click is not an attack).
        if (k.escapeKey.wasPressedThisFrame) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        if (k.rKey.wasPressedThisFrame || player.transform.position.y < -10) {
            ResetDemo();
        }
        
        if (k.hKey.wasPressedThisFrame) {
            player.ReceiveHit();
        }
        
        if (k.kKey.wasPressedThisFrame) {
            player.ReceiveKill();
        }
        
        if (k.f1Key.wasPressedThisFrame) {
            if (overlay) overlay.ToggleHUD();
        }
    }
    
    void LateUpdate() {
        if (Cursor.lockState != CursorLockMode.Locked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ResetDemo() {
        player.ResetPlayer();
        cameraCoordinator.ResetCamera();
        player.GetComponentInChildren<PlayerAnimator>()?.ResetAnimation();
        foreach (var zone in FindObjectsByType<DamageZone>(FindObjectsSortMode.None)) zone.Clear();
        foreach (var h in FindObjectsByType<Health>(FindObjectsSortMode.None)) if (h.gameObject != player.gameObject) h.Restore(); // training dummy
    }
    
    void OnApplicationFocus(bool focus) {
        if (!focus) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
}
