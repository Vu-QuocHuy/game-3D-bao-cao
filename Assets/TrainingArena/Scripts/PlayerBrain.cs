using System.Collections.Generic;
using UnityEngine;
namespace TrainingArena {
[RequireComponent(typeof(PlayerMotor),typeof(PlayerInputHandler),typeof(Health))]
[DefaultExecutionOrder(-10)]
public sealed class PlayerBrain : MonoBehaviour {
 public StateMachine Locomotion {get;}=new StateMachine();public StateMachine Action {get;}=new StateMachine();
 public string Priority {get;private set;}="Normal";public Queue<string> History {get;}=new Queue<string>();
 public float AttackTime {get;private set;}public bool Attacking=>Action.Current?.Name=="Attack";public bool AttackWindow=>Attacking&&AttackTime>.16f&&AttackTime<.65f;
 public bool allowMovingAttack=true,topic5=true;public Transform spawn;
 PlayerMotor motor;PlayerInputHandler input;Health health;PlayerCombat combat;Dictionary<string,IState> states=new Dictionary<string,IState>();IState noAction,attack;float hitTimer,landTimer,attackCooldown;bool fallBeforeLand;
 void Awake(){motor=GetComponent<PlayerMotor>();input=GetComponent<PlayerInputHandler>();health=GetComponent<Health>();combat=GetComponent<PlayerCombat>();foreach(string n in new[]{"Idle","Walk","Run","Jump","Fall","Land","Crouch"})states[n]=new PlayerState(n);noAction=new PlayerState("None",()=>combat?.SetWindow(false));attack=new PlayerState("Attack",()=>AttackTime=0,null,()=>{AttackTime=0;combat?.SetWindow(false);});Locomotion.Changed+=(a,b)=>Record(a+" → "+b);Action.Changed+=(a,b)=>Record("Action: "+a+" → "+b);Locomotion.Change(states["Idle"]);Action.Change(noAction);}
 void OnEnable(){health.Damaged+=Hit;health.Died+=Die;motor.Landed+=Land;}
 void OnDisable(){health.Damaged-=Hit;health.Died-=Die;motor.Landed-=Land;}
 void Record(string s){History.Enqueue(s);while(History.Count>5)History.Dequeue();}
 void Hit(){Priority="Hit";hitTimer=.45f;Action.Change(noAction);Record("Priority → Hit");}
 void Die(){Priority="Dead";Action.Change(noAction);Record("Priority → Dead");}
 void Land(float velocity){landTimer=.16f;if(Locomotion.Current?.Name=="Jump")fallBeforeLand=true;}
 void Update(){
  input.Sample();attackCooldown=Mathf.Max(0,attackCooldown-Time.deltaTime);
  if(Priority=="Hit"){hitTimer-=Time.deltaTime;if(hitTimer<=0){Priority="Normal";Record("Hit → Normal");}}
  bool normal=Priority=="Normal";
  if(normal)motor.SetCrouch(topic5&&input.CrouchHeld&&motor.Grounded || motor.Crouched&&!motor.CanStand());
  if(topic5&&normal&&input.AttackPressed&&motor.Grounded&&!motor.Crouched&&!Attacking&&attackCooldown<=0){Action.Change(attack);attackCooldown=1f;}
  if(Attacking){AttackTime+=Time.deltaTime;if(AttackTime>.85f)Action.Change(noAction);}
  motor.Step(input.Move,input.SprintHeld,input.JumpPressed,normal&&(!Attacking||allowMovingAttack),normal&&!Attacking);
  string next=!motor.Grounded?(motor.VerticalVelocity>0?"Jump":"Fall"):landTimer>0?"Land":motor.Crouched?"Crouch":motor.Speed>.1f?(input.SprintHeld?"Run":"Walk"):"Idle";
  if(fallBeforeLand){if(normal)next="Fall";fallBeforeLand=false;}
  Locomotion.Change(states[next]);Locomotion.Tick();Action.Tick();landTimer=Mathf.Max(0,landTimer-Time.deltaTime);
 }
 public void ResetPlayer(){Priority="Normal";hitTimer=landTimer=attackCooldown=0;fallBeforeLand=false;Action.Change(noAction);health.Restore();input.Clear();motor.ResetAt(spawn?spawn.position:Vector3.up);Locomotion.Change(states["Idle"]);History.Clear();Record("Reset → Idle");}
}}
