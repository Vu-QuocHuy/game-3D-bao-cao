using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainingArena {
public sealed class DemoController : MonoBehaviour {
 public PlayerBrain player;public CameraCoordinator cameraCoordinator;public bool topic5=true;
 void Start(){Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;Application.targetFrameRate=60;}
 void Update(){var k=Keyboard.current;if(k==null)return;if(k.escapeKey.wasPressedThisFrame){bool locked=Cursor.lockState==CursorLockMode.Locked;Cursor.lockState=locked?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=locked;}
  if(k.f6Key.wasPressedThisFrame){topic5=!topic5;player.topic5=topic5;ResetDemo();}
  if(k.backspaceKey.wasPressedThisFrame||player.transform.position.y< -10)ResetDemo();
 }
 public void ResetDemo(){player.ResetPlayer();cameraCoordinator.ResetCamera();player.GetComponentInChildren<PlayerAnimator>()?.ResetAnimation();foreach(var zone in FindObjectsByType<DamageZone>(FindObjectsSortMode.None))zone.Clear();foreach(var h in FindObjectsByType<Health>(FindObjectsSortMode.None))h.Restore();}
 void OnApplicationFocus(bool focus){if(!focus){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}}
}}
