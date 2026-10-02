using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
namespace TrainingArena {
[DefaultExecutionOrder(20)]
public sealed class DebugOverlay : MonoBehaviour {
 public PlayerBrain player;public CameraCoordinator cameraCoordinator;public TMP_Text status,history,help;public Image healthFill;
 bool debug=true;float fps;readonly System.Collections.Generic.List<AnimatorClipInfo> clips=new System.Collections.Generic.List<AnimatorClipInfo>();
 void Update(){if(!player)return;var k=Keyboard.current;if(k!=null){if(k.f1Key.wasPressedThisFrame)help.gameObject.SetActive(!help.gameObject.activeSelf);if(k.f2Key.wasPressedThisFrame)debug=!debug;}
  fps=Mathf.Lerp(fps,1/Mathf.Max(.001f,Time.unscaledDeltaTime),.08f);var motor=player.GetComponent<PlayerMotor>();var health=player.GetComponent<Health>();healthFill.fillAmount=health.Current/health.maxHealth;
  status.text=$"TRAINING ARENA  •  {(player.topic5?"T5  STATES + ANIMATION":"T3  MOVEMENT + CAMERA")}\nHP {health.Current:0}/{health.maxHealth:0}   |   {fps:0} FPS\n{(player.Priority=="Dead"?"DEAD — BACKSPACE TO RESTART":player.Priority)}\nCamera: {cameraCoordinator.ModeName}\nLocomotion: {player.Locomotion.Current?.Name}   Action: {player.Action.Current?.Name}\nSpeed: {motor.Speed:0.00}   Vertical: {motor.VerticalVelocity:0.00}\nGrounded: {motor.Grounded}   Crouched: {motor.Crouched}\nDamping [{cameraCoordinator.damping}]   Collision [{cameraCoordinator.avoidObstacles}]   Shake [{cameraCoordinator.shake}]";
  history.text="TRANSITION HISTORY\n"+string.Join("\n",player.History);status.gameObject.SetActive(debug);history.gameObject.SetActive(debug);
  var bridge=player.GetComponentInChildren<PlayerAnimator>();if(status&&bridge&&bridge.animator){var animator=bridge.animator;var state=animator.GetCurrentAnimatorStateInfo(0);string routed=player.Priority=="Normal"?player.Locomotion.Current.Name:player.Priority;status.text+=$"\nAnimator: {routed}  phase {Mathf.Repeat(state.normalizedTime,1):0.00}   Action layer: {animator.GetLayerWeight(1):0.0}";}
 }
}}
