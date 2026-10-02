using UnityEngine;
namespace TrainingArena {
[DefaultExecutionOrder(10)]
public sealed class PlayerAnimator : MonoBehaviour {
 public PlayerBrain brain;public PlayerMotor motor;public Animator animator;public PlayerCombat combat;public AudioSource audioSource;public AudioClip footstep;
 string lastBase,lastAction;
 void OnEnable(){if(motor)motor.Landed+=Landing;}
 void OnDisable(){if(motor)motor.Landed-=Landing;}
 void Landing(float speed){if(speed>3&&audioSource&&footstep)audioSource.PlayOneShot(footstep,.22f);}
 void Update(){
  string state=brain.Priority=="Normal"?brain.Locomotion.Current?.Name:brain.Priority;
  string target=state=="Idle"||state=="Walk"||state=="Run"?"Locomotion":state;
  animator.SetFloat("Speed",motor.Speed,.1f,Time.deltaTime);
  if(target!=lastBase){animator.CrossFadeInFixedTime(target,.1f,0);lastBase=target;}
  bool action=brain.Attacking&&brain.Priority=="Normal";animator.SetLayerWeight(1,action?1:0);
  string a=action?"Attack":"Empty";if(a!=lastAction){animator.Play(a,1,0);lastAction=a;if(!action)CloseHitbox();}
 }
 public void OpenHitbox(){if(brain.Attacking&&brain.Priority=="Normal")combat.SetWindow(true);}
 public void CloseHitbox()=>combat.SetWindow(false);
 public void Footstep(){if(motor.Grounded&&motor.Speed>.2f&&audioSource&&footstep)audioSource.PlayOneShot(footstep,.12f);}
 public void ResetAnimation(){lastBase=lastAction=null;animator.Rebind();animator.Update(0);CloseHitbox();}
}}
