using System;
using UnityEngine;
namespace TrainingArena {
[RequireComponent(typeof(CharacterController))]
public sealed class PlayerMotor : MonoBehaviour {
 public PlayerConfig config;
 public float walkSpeed=3,runSpeed=6,crouchSpeed=1.6f,jumpHeight=1.8f,gravity=-22,turnRate=650;
 public Transform cameraTransform;public LayerMask groundMask=1;
 public bool Grounded {get;private set;}public float VerticalVelocity {get;private set;}public float Speed {get;private set;}public bool Crouched {get;private set;}
 public event Action<float> Landed;CharacterController controller;float standingHeight,standingStep;Vector3 standingCenter;
 void Awake(){controller=GetComponent<CharacterController>();standingHeight=controller.height;standingCenter=controller.center;standingStep=controller.stepOffset;if(config){walkSpeed=config.walkSpeed;runSpeed=config.runSpeed;crouchSpeed=config.crouchSpeed;jumpHeight=config.jumpHeight;gravity=config.gravity;turnRate=config.turnRate;}}
 public bool CanStand(){Vector3 start=transform.position+Vector3.up*.5f;return !Physics.SphereCast(start,.28f,Vector3.up,out _,1.2f,groundMask,QueryTriggerInteraction.Ignore);}
 public void SetCrouch(bool desired){if(!desired&&Crouched&&!CanStand())return;Crouched=desired;controller.height=desired?1.05f:standingHeight;controller.center=desired?new Vector3(0,.525f,0):standingCenter;controller.stepOffset=desired?.1f:standingStep;}// PhysX lifts the capsule by stepOffset before moving; a crouched 1.05 m capsule + 0.45 m step would hit a 1.25 m ceiling
 public void Step(Vector2 input,bool sprint,bool jump,bool canMove,bool canJump){
  bool wasGrounded=Grounded;Grounded=controller.isGrounded||Physics.CheckSphere(transform.position+Vector3.up*.12f,.16f,groundMask,QueryTriggerInteraction.Ignore);
  if(Grounded&&VerticalVelocity<0)VerticalVelocity=-2;
  if(jump&&Grounded&&canJump&&!Crouched){VerticalVelocity=Mathf.Sqrt(-2*gravity*jumpHeight);Grounded=false;}
  Vector3 forward=cameraTransform?cameraTransform.forward:Vector3.forward;forward.y=0;if(forward.sqrMagnitude<.01f)forward=Vector3.forward;forward.Normalize();Vector3 right=Vector3.Cross(Vector3.up,forward);
  Vector3 direction=canMove?Vector3.ClampMagnitude(forward*input.y+right*input.x,1):Vector3.zero;
  Speed=direction.magnitude*(Crouched?crouchSpeed:sprint?runSpeed:walkSpeed);
  if(direction.sqrMagnitude>.001f)transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(direction),turnRate*Time.deltaTime);
  VerticalVelocity+=gravity*Time.deltaTime;float fallingSpeed=VerticalVelocity;
  var flags=controller.Move((direction*(Crouched?crouchSpeed:sprint?runSpeed:walkSpeed)+Vector3.up*VerticalVelocity)*Time.deltaTime);
  Speed=new Vector2(controller.velocity.x,controller.velocity.z).magnitude;
  if((flags&CollisionFlags.Above)!=0&&VerticalVelocity>0)VerticalVelocity=0;
  Grounded=(flags&CollisionFlags.Below)!=0;
  if(!wasGrounded&&Grounded&&fallingSpeed<0)Landed?.Invoke(-fallingSpeed);
 }
 public void ResetAt(Vector3 position){controller.enabled=false;transform.position=position;transform.rotation=Quaternion.identity;controller.enabled=true;SetCrouch(false);Speed=0;VerticalVelocity=0;Grounded=(controller.Move(Vector3.down*.2f)&CollisionFlags.Below)!=0;}// snap to floor so reset does not log a fake Airborne
}}
