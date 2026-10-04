using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainingArena {
public sealed class DemoController : MonoBehaviour {
    public PlayerBrain player;
    public CameraCoordinator cameraCoordinator;
    public DebugOverlay overlay;
    public bool topic5 = true;
    
    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Application.targetFrameRate = 60;
    }
    
    void Update() {
        var k = Keyboard.current;
        if (k == null) return;
        
        if (k.escapeKey.wasPressedThisFrame) {
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = locked;
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
    
    public void ResetDemo() {
        player.ResetPlayer();
        cameraCoordinator.ResetCamera();
        player.GetComponentInChildren<PlayerAnimator>()?.ResetAnimation();
        foreach (var zone in FindObjectsByType<DamageZone>(FindObjectsSortMode.None)) zone.Clear();
    }
    
    void OnApplicationFocus(bool focus) {
        if (!focus) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
}
