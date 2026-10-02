using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace TrainingArena {
// Opt-in integration checks. Never attached to the shipped scene by the builder.
public sealed class DemoVerifier : MonoBehaviour {
 [Serializable]public class CheckResult {public string name;public bool passed;public string detail;}
 [Serializable]public class Report {public int startFrame,endFrame;public List<CheckResult> checks=new List<CheckResult>();}
 public string reportPath="/tmp/training-arena-validation.json";
 bool quitAfterReport;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void StartFromCommandLine(){var args=Environment.GetCommandLineArgs();if(Array.IndexOf(args,"--verify-demo")<0)return;var verifier=new GameObject("Standalone Verification").AddComponent<DemoVerifier>();verifier.quitAfterReport=true;int index=Array.IndexOf(args,"--verification-report");if(index>=0&&index+1<args.Length)verifier.reportPath=args[index+1];}
 Report report=new Report();PlayerBrain brain;PlayerMotor motor;CameraCoordinator camera;Health health;
 bool originalTopic5;
 void Check(string name,bool condition,string detail=""){report.checks.Add(new CheckResult{name=name,passed=condition,detail=detail});Debug.Log((condition?"PASS: ":"FAIL: ")+name+" "+detail);}
 void Keys(params Key[] keys){InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(keys));}
 void Mouse(bool down){var state=new MouseState();if(down)state=state.WithButton(MouseButton.Left);InputSystem.QueueStateEvent(UnityEngine.InputSystem.Mouse.current,state);}
 void Teleport(Vector3 position){var cc=brain.GetComponent<CharacterController>();cc.enabled=false;brain.transform.SetPositionAndRotation(position,Quaternion.identity);cc.enabled=true;}
 IEnumerator Start(){
  InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
  InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
  InputSystem.EnableDevice(Keyboard.current);InputSystem.EnableDevice(UnityEngine.InputSystem.Mouse.current);
  report.startFrame=Time.frameCount;brain=FindFirstObjectByType<PlayerBrain>();motor=brain.GetComponent<PlayerMotor>();health=brain.GetComponent<Health>();camera=FindFirstObjectByType<CameraCoordinator>();var demo=FindFirstObjectByType<DemoController>();originalTopic5=demo.topic5;Check("Scene launches in its configured topic",brain.topic5==originalTopic5,"T5="+originalTopic5);brain.topic5=true;Cursor.lockState=CursorLockMode.Locked;Time.timeScale=1;
  Keys();Mouse(false);yield return new WaitForSeconds(.6f);
  Check("Spawn grounded",motor.Grounded);Check("Default state Idle",brain.Locomotion.Current.Name=="Idle");
  var source=GameObject.Find("Humanoid Source").GetComponent<Animator>();var target=GameObject.Find("Humanoid Retarget").GetComponent<Animator>();Check("Two valid Humanoid Avatars",source.avatar.isValid&&source.avatar.isHuman&&target.avatar.isValid&&target.avatar.isHuman);Check("Humanoid controller shared across proportions",source.runtimeAnimatorController==target.runtimeAnimatorController&&source.avatar!=target.avatar);
  var joint=source.GetBoneTransform(HumanBodyBones.LeftUpperLeg);Quaternion pose=joint.localRotation;yield return new WaitForSeconds(.3f);Check("Humanoid muscle animation moves joints",Quaternion.Angle(pose,joint.localRotation)>1);
  Vector3 forward=motor.cameraTransform.forward;forward.y=0;forward.Normalize();Vector3 start=brain.transform.position;Keys(Key.W);yield return new WaitForSeconds(.5f);Check("W moves forward relative to camera",Vector3.Dot(brain.transform.position-start,forward)>1.1f);Check("Walking speed",Mathf.Abs(motor.Speed-3)<.1f);Keys();yield return new WaitForSeconds(.1f);
  start=brain.transform.position;Keys(Key.W,Key.LeftShift);yield return new WaitForSeconds(.5f);Check("Sprint speed",Mathf.Abs(motor.Speed-6)<.1f);Check("Sprint displacement",Vector3.Distance(brain.transform.position,start)>2.4f);Keys();yield return new WaitForSeconds(.1f);
  Keys(Key.Space);yield return new WaitForSeconds(.1f);Check("Jump enters airborne",!motor.Grounded&&motor.VerticalVelocity>0&&brain.Locomotion.Current.Name=="Jump");Keys();yield return new WaitForSeconds(.4f);bool falling=brain.Locomotion.Current.Name=="Fall"&&!motor.Grounded&&motor.VerticalVelocity<0;if(!falling)yield return new WaitForSeconds(.15f);string jumpHistory=string.Join(" | ",brain.History.ToArray());Check("Fall after apex",jumpHistory.Contains("Jump → Fall"),"transitions="+jumpHistory);
  Keys(Key.LeftCtrl);yield return new WaitForSeconds(.15f);Check("Crouch changes capsule",motor.Crouched&&brain.GetComponent<CharacterController>().height<1.2f);Keys(Key.LeftCtrl,Key.Space);yield return new WaitForSeconds(.15f);Check("Jump blocked while crouched",motor.Grounded&&motor.VerticalVelocity<=0);
  Keys(Key.LeftCtrl);Teleport(new Vector3(-6,.05f,-2));yield return new WaitForSeconds(.1f);Keys();yield return new WaitForSeconds(.2f);Check("Ceiling blocks standing",motor.Crouched&&!motor.CanStand());Teleport(new Vector3(-6,.05f,-5));yield return new WaitForSeconds(.2f);Check("Can stand after leaving beam",!motor.Crouched);
  motor.ResetAt(new Vector3(5,.05f,-5));yield return new WaitForSeconds(.25f);var dummy=GameObject.Find("Training Dummy").GetComponent<Health>();var combat=brain.GetComponent<PlayerCombat>();Mouse(true);yield return new WaitForSeconds(.08f);bool attackStarted=brain.Attacking;Mouse(false);yield return new WaitForSeconds(.25f);bool hitboxOpened=combat.WindowOpen;yield return new WaitForSeconds(.65f);Check("Animation event hitbox damages dummy once",dummy.Current==75,"HP="+dummy.Current+" started="+attackStarted+" opened="+hitboxOpened+" targetHits="+combat.HitCount);
  Check("Attack ends",!brain.Attacking);Keys(Key.Space);yield return new WaitForSeconds(.12f);Mouse(true);yield return new WaitForSeconds(.08f);Check("Attack blocked in air",!brain.Attacking);Mouse(false);Keys();yield return new WaitForSeconds(1);
  Teleport(new Vector3(11,.05f,-4));yield return new WaitForSeconds(.15f);Check("Trap causes Hit",health.Current==75&&brain.Priority=="Hit","HP="+health.Current+" priority="+brain.Priority);yield return new WaitForSeconds(3.4f);Check("Repeated damage leads to Dead",health.IsDead&&brain.Priority=="Dead");start=brain.transform.position;Keys(Key.W,Key.Space);Mouse(true);yield return new WaitForSeconds(.3f);Check("Dead blocks movement and attack",Vector3.Distance(start,brain.transform.position)<.1f&&!brain.Attacking);Keys();Mouse(false);
  FindFirstObjectByType<DemoController>().ResetDemo();yield return new WaitForSeconds(.3f);Check("Reset restores all player state",health.Current==100&&brain.Priority=="Normal"&&!motor.Crouched&&!brain.Attacking);
  for(int i=0;i<3;i++){Keys(Key.C);yield return new WaitForSeconds(.08f);Keys();yield return new WaitForSeconds(.08f);Check("Camera mode "+i,camera.Mode==(i+1)%3);}
  Teleport(new Vector3(11,.05f,12));yield return new WaitForSeconds(.2f);Check("Fixed camera zone override",camera.InZone);Keys(Key.C);yield return new WaitForSeconds(.08f);Keys();Teleport(new Vector3(5,.05f,0));yield return new WaitForSeconds(.2f);Check("Camera mode remembered after zone",!camera.InZone&&camera.Mode==1);
  FindFirstObjectByType<DemoController>().ResetDemo();yield return new WaitForSeconds(.3f);
  motor.ResetAt(new Vector3(0,.05f,0));yield return new WaitForSeconds(.2f);motor.cameraTransform.rotation=Quaternion.LookRotation(Vector3.forward);motor.transform.rotation=Quaternion.identity;float highWater=brain.transform.position.y,walkTime=0;Keys(Key.W);while(walkTime<5){yield return null;walkTime+=Time.deltaTime;highWater=Mathf.Max(highWater,brain.transform.position.y);}Keys();Check("Stairs reach upper platform",highWater>2.8f,"maxY="+highWater.ToString("0.00")+" final="+brain.transform.position);
  motor.ResetAt(new Vector3(-10,.05f,14.7f));yield return new WaitForSeconds(.7f);
  InputSystem.QueueStateEvent(UnityEngine.InputSystem.Mouse.current,new MouseState{delta=new Vector2(1500,0)});yield return new WaitForSeconds(1);
  Vector3 focus=brain.transform.position+Vector3.up*1.55f;Check("Camera line to player stays unobstructed",!Physics.Linecast(focus,Camera.main.transform.position,1,QueryTriggerInteraction.Ignore),"camera="+Camera.main.transform.position);
  demo.topic5=originalTopic5;brain.topic5=originalTopic5;demo.ResetDemo();yield return new WaitForSeconds(.7f);report.endFrame=Time.frameCount;File.WriteAllText(reportPath,JsonUtility.ToJson(report,true));Debug.Log("DEMO_VERIFICATION_COMPLETE: "+reportPath);
  if(quitAfterReport){ScreenCapture.CaptureScreenshot(reportPath+".png");yield return new WaitForSeconds(.3f);bool passed=report.checks.TrueForAll(c=>c.passed);Application.Quit(passed?0:1);}Destroy(this);
 }
 void OnDisable(){if(Keyboard.current!=null)Keys();if(UnityEngine.InputSystem.Mouse.current!=null)Mouse(false);}
}}
