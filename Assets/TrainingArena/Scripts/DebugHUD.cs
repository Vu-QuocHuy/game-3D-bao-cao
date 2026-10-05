using TMPro;
using UnityEngine;
namespace TrainingArena {
// Spec 6.4: top-left text updated every frame. F1 hides it for recording.
[DefaultExecutionOrder(30)]
public sealed class DebugHUD : MonoBehaviour {
    public PlayerController player;
    public PlayerStateMachine states;
    public PlayerCombat combat;
    public TMP_Text status, help;

    public bool Visible => !status || status.gameObject.activeSelf;

    void Start() {
        if (help) help.text = "WASD / JOYSTICK  MOVE   |   MOUSE / DRAG RIGHT  LOOK   |   SPACE  JUMP   |   CTRL  WALK   |   C  CROUCH   |   LMB  ATTACK\n" +
                              "T  CAMERA / WORLD MOVE   |   R  RESET   |   F1  HUD   |   ESC  FREE CURSOR, CLICK TO LOCK";
    }

    public void Toggle() {
        bool show = !Visible;
        if (status) status.gameObject.SetActive(show);
        if (help) help.gameObject.SetActive(show);
    }

    void Update() {
        if (!status || !status.gameObject.activeSelf) return;
        string state = states.CurrentName;
        if (state == "Move") state += player.Walking ? " (Walk)" : " (Run)";
        string text =
            $"State: {state}\n" +
            $"Speed: {player.HorizontalSpeed:0.0} m/s\n" +
            $"Grounded: {player.IsGrounded}\n" +
            $"VelocityY: {player.VelocityY:0.0}\n" +
            $"Move mode: {(player.moveRelativeToCamera ? "Camera-relative" : "World axes")}\n" +
            $"Crouched: {player.Crouched}   Hitbox: {(combat && combat.WindowOpen ? "OPEN" : "closed")}";
        if (!string.IsNullOrEmpty(player.BlockReason)) text += $"\n<color=#ff6070>{player.BlockReason}</color>";
        status.text = text;
    }
}
}
