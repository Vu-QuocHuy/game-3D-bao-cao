using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
namespace TrainingArena {
[DefaultExecutionOrder(-20)]
public sealed class CameraCoordinator : MonoBehaviour {
 public Transform player,targetPivot;public PlayerInputHandler input;public Transform fixedAnchor;
 public CinemachineCamera tps,fps,topDown,fixedCamera;public CinemachineDeoccluder deoccluder;public CinemachineImpulseSource impulse;
 public bool damping=true,avoidObstacles=true,shake=true;public int Mode {get;private set;}public bool InZone=>zones.Count>0;
 public string ModeName=>InZone?"FIXED ZONE":new[]{"TPS","FPS","TOP DOWN"}[Mode];
 readonly HashSet<CameraZoneTrigger> zones=new HashSet<CameraZoneTrigger>();float yaw,pitch=18;Vector3 smoothPosition;Renderer[] visualRenderers;PlayerMotor motor;
 void Start(){motor=player.GetComponent<PlayerMotor>();smoothPosition=tps.transform.position;visualRenderers=player.GetComponentsInChildren<Renderer>();motor.Landed+=Landing;}
 void OnDestroy(){if(motor)motor.Landed-=Landing;}
 void Landing(float speed){if(shake&&speed>8)impulse.GenerateImpulseWithForce(Mathf.Clamp(speed*.02f,.12f,.45f));}
 public void SetZone(CameraZoneTrigger zone,bool active){if(active)zones.Add(zone);else zones.Remove(zone);}
 public void ResetCamera(){Mode=0;zones.Clear();yaw=0;pitch=18;damping=avoidObstacles=shake=true;smoothPosition=player.position+new Vector3(0,3,-5);foreach(var c in new[]{tps,fps,topDown,fixedCamera})c.PreviousStateIsValid=false;}
 void Update(){var k=Keyboard.current;if(k==null)return;if(k.cKey.wasPressedThisFrame)Mode=(Mode+1)%3;if(k.f3Key.wasPressedThisFrame)damping=!damping;if(k.f4Key.wasPressedThisFrame)avoidObstacles=!avoidObstacles;if(k.f5Key.wasPressedThisFrame)shake=!shake;}
 void LateUpdate(){
  if(!player||!motor)return;
  zones.RemoveWhere(zone=>!zone||!zone.Contains(player));
  yaw+=input.Look.x*.12f;pitch=Mathf.Clamp(pitch-input.Look.y*.1f,-30,70);
  Vector3 focus=player.position+Vector3.up*(motor.Crouched?.9f:1.55f);targetPivot.position=focus;
  Quaternion rotation=Quaternion.Euler(pitch,yaw,0);Vector3 desired=focus-rotation*Vector3.forward*5.2f;
  float blend=damping?1-Mathf.Exp(-9*Time.deltaTime):1;smoothPosition=Vector3.Lerp(smoothPosition,desired,blend);tps.transform.SetPositionAndRotation(smoothPosition,rotation);
  fps.transform.SetPositionAndRotation(focus+rotation*Vector3.forward*.15f,rotation);
  topDown.transform.SetPositionAndRotation(player.position+new Vector3(0,14,-5),Quaternion.Euler(70,0,0));
  fixedCamera.transform.SetPositionAndRotation(fixedAnchor.position,Quaternion.LookRotation(focus-fixedAnchor.position));
  tps.Priority=!InZone&&Mode==0?20:0;fps.Priority=!InZone&&Mode==1?20:0;topDown.Priority=!InZone&&Mode==2?20:0;fixedCamera.Priority=InZone?30:0;
  deoccluder.enabled=avoidObstacles;
  bool show=Mode!=1||InZone;foreach(var r in visualRenderers)if(r)r.enabled=show;
 }
}}
