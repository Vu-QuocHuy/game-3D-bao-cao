using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
namespace TrainingArena {
[DefaultExecutionOrder(20)]
public sealed class DebugOverlay : MonoBehaviour {
    public PlayerBrain player;
    public CameraCoordinator cameraCoordinator;
    public TMP_Text status, history, help;
    public Image healthFill;
    
    public bool showHUD = true;
    float fps;

    void Start() {
        if (help) help.text = "WASD  MOVE   |   SHIFT  RUN   |   SPACE  JUMP   |   CTRL  CROUCH   |   LMB  ATTACK   |   MOUSE  LOOK   |   C  TPS / WIDE\n" +
                              "DEBUG:  H  -25 HP   |   K  KILL   |   R  RESET   |   F1  HUD   |   ESC  FREE CURSOR, CLICK TO LOCK";
    }

    public void ToggleHUD() {
        showHUD = !showHUD;
        if (status) status.gameObject.SetActive(showHUD);
        if (history) history.gameObject.SetActive(showHUD);
        if (help) help.gameObject.SetActive(showHUD);
    }

    void Update() {
        if (!player) return;
        fps = Mathf.Lerp(fps, 1 / Mathf.Max(0.001f, Time.unscaledDeltaTime), 0.08f);
        
        var motor = player.GetComponent<PlayerMotor>();
        var health = player.GetComponent<Health>();
        var combat = player.GetComponent<PlayerCombat>();
        if (healthFill && health) healthFill.fillAmount = health.Current / health.maxHealth;
        
        if (status) {
            status.text = $"TRAINING ARENA\nHP {(health ? health.Current : 0):0}/{(health ? health.maxHealth : 100):0}   |   {fps:0} FPS\n" +
                          $"Camera: {cameraCoordinator.ModeName}\n" +
                          $"State: {player.CurrentState}\n" +
                          $"Speed: {motor.Speed:0.00}   Vertical: {motor.VerticalVelocity:0.00}\n" +
                          $"Grounded: {motor.Grounded}   Crouched: {motor.Crouched}\n" +
                          $"Action: {(combat && combat.Attacking ? "Attack" : "-")}   Hitbox: {(combat && combat.WindowOpen ? "OPEN" : "closed")}";
                          
            if (!string.IsNullOrEmpty(player.BlockReason)) {
                status.text += $"\n<color=red>Blocked: {player.BlockReason}</color>";
            }
        }

        if (history) {
            history.text = "TRANSITION HISTORY\n" + string.Join("\n", player.History);
        }
    }
}
}
