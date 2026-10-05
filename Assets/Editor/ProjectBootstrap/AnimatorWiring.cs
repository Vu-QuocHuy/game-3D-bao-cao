using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace ProjectBootstrap {
// Builds the player's Generic clips and wires Player.controller to the FSM contract:
// Mode 0 Grounded, 1 Airborne, 2 Hit, 3 Dead. Speed blends Idle/Walk/Run, VerticalSpeed picks Jump vs Fall,
// Crouch (bool) picks the crouch blend tree inside Grounded. C# owns every gameplay transition.
public static class AnimatorWiring {
 const string Root="Assets/TrainingArena/Animations";
 const string Path=Root+"/Controllers/Player.controller";
 const string H="Hips",B="Hips/UpperBody",Hd=B+"/Head",LA=B+"/LeftArm",RA=B+"/RightArm",LF=LA+"/LeftForearm",RF=RA+"/RightForearm",LL="Hips/LeftLeg",RL="Hips/RightLeg",LS=LL+"/LeftShin",RS=RL+"/RightShin";
 const AnimatorConditionMode Eq=AnimatorConditionMode.Equals,Gt=AnimatorConditionMode.Greater,Lt=AnimatorConditionMode.Less,If=AnimatorConditionMode.If,IfNot=AnimatorConditionMode.IfNot;

 [MenuItem("Training Arena/Wire Player Animator")]
 public static void WireMenu(){Wire(AssetDatabase.LoadAssetAtPath<AnimatorController>(Path));AssetDatabase.SaveAssets();Debug.Log("ANIMATOR_WIRED: "+Path);}
 public static void WireBatch(){WireMenu();EditorApplication.Exit(0);}

 // ---------- clips ----------
 // Bones start with identity rotation; arms/legs hang along -Y. Rotation X: negative swings a limb forward,
 // positive bends a knee back or leans the torso forward. Arm Z: Left negative / Right positive raises it sideways.
 sealed class Pose {
  public readonly AnimationClip clip;readonly float length;readonly Dictionary<string,HashSet<string>> rot=new Dictionary<string,HashSet<string>>();
  public Pose(AnimationClip clip,float length){this.clip=clip;this.length=length;}
  public Pose R(string path,char axis,params float[] v){Key(path,"localEulerAnglesRaw."+axis,v);if(!rot.TryGetValue(path,out var s))rot[path]=s=new HashSet<string>();s.Add(axis.ToString());return this;}
  public Pose Y(params float[] v){Key(H,"localPosition.y",v);return this;}
  void Key(string path,string prop,float[] v){
   var keys=new Keyframe[Mathf.Max(2,v.Length)];
   for(int i=0;i<keys.Length;i++)keys[i]=new Keyframe(v.Length==1?length*i:length*i/(v.Length-1),v[Mathf.Min(i,v.Length-1)]);
   var curve=new AnimationCurve(keys);
   AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(path,typeof(Transform),prop),curve);
  }
  // Generic euler curves need all three components bound, otherwise Unity fills them unpredictably.
  public void Finish(){foreach(var kv in rot)foreach(var a in new[]{"x","y","z"})if(!kv.Value.Contains(a))Key(kv.Key,"localEulerAnglesRaw."+a,new[]{0f});EditorUtility.SetDirty(clip);}
 }
 static Pose Clip(string name,float length,bool loop){
  string path=Root+"/Clips/"+name+".anim";var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
  if(!clip){clip=new AnimationClip{name=name,frameRate=30};AssetDatabase.CreateAsset(clip,path);}
  clip.ClearCurves(); // keeps the asset GUID and its Footstep events
  var s=AnimationUtility.GetAnimationClipSettings(clip);s.loopTime=loop;AnimationUtility.SetAnimationClipSettings(clip,s);
  return new Pose(clip,length);
 }
 static Dictionary<string,AnimationClip> BuildClips(){
  var c=new Dictionary<string,AnimationClip>();void Add(Pose p){p.Finish();c[p.clip.name]=p.clip;}
  Add(Clip("Idle",2.4f,true).Y(.95f,.935f,.95f).R(B,'x',2,4,2).R(Hd,'x',0,-3,0).R(LA,'z',-4,-7,-4).R(RA,'z',4,7,4).R(LF,'x',-10,-15,-10).R(RF,'x',-10,-15,-10));
  Add(Clip("Walk",.9f,true).Y(.95f,.92f,.95f,.92f,.95f).R(B,'x',4,6,4,6,4).R(B,'y',-5,0,5,0,-5)
   .R(LL,'x',-25,0,25,0,-25).R(RL,'x',25,0,-25,0,25).R(LS,'x',5,5,10,45,5).R(RS,'x',10,45,5,5,10)
   .R(LA,'x',20,0,-20,0,20).R(RA,'x',-20,0,20,0,-20).R(LA,'z',-5).R(RA,'z',5).R(LF,'x',-20).R(RF,'x',-20));
  Add(Clip("Run",.6f,true).Y(.9f,.85f,.9f,.85f,.9f).R(B,'x',14,17,14,17,14).R(B,'y',-10,0,10,0,-10)
   .R(LL,'x',-45,0,40,0,-45).R(RL,'x',40,0,-45,0,40).R(LS,'x',10,15,30,95,10).R(RS,'x',30,95,10,15,30)
   .R(LA,'x',45,0,-45,0,45).R(RA,'x',-45,0,45,0,-45).R(LA,'z',-8).R(RA,'z',8).R(LF,'x',-80).R(RF,'x',-80));
  Add(Clip("Jump",.5f,false).Y(.95f).R(B,'x',10,0).R(LL,'x',-10,-50).R(LS,'x',20,70).R(RL,'x',0,-20).R(RS,'x',10,40)
   .R(LA,'x',0,-30).R(RA,'x',0,-30).R(LA,'z',-20,-60).R(RA,'z',20,60));
  Add(Clip("Fall",.8f,true).Y(.95f).R(B,'x',-5,-8,-5).R(Hd,'x',-10).R(LL,'x',-20,-15,-20).R(LS,'x',30,25,30).R(RL,'x',10,5,10).R(RS,'x',20,25,20)
   .R(LA,'z',-70,-85,-70).R(RA,'z',70,85,70));
  Add(Clip("Land",.3f,false).Y(.7f,.95f).R(B,'x',25,5).R(LL,'x',-45,0).R(RL,'x',-45,0).R(LS,'x',90,0).R(RS,'x',90,0).R(LA,'z',-30,-5).R(RA,'z',30,5));
  Add(Clip("Hit",.45f,false).Y(.95f,.9f,.93f,.95f).R(B,'x',0,-30,-10,0).R(Hd,'x',0,-25,-5,0).R(LA,'z',-5,-45,-20,-5).R(RA,'z',5,45,20,5).R(LL,'x',0,10,5,0).R(RL,'x',0,10,5,0));
  Add(Clip("Dead",.9f,false).Y(.95f,.7f,.25f,.22f).R(H,'x',0,-40,-90,-90).R(Hd,'x',0,10,-20,-25).R(LA,'z',-5,-40,-80,-85).R(RA,'z',5,40,80,85)
   .R(LL,'x',0,-20,-10,-5).R(RL,'x',0,-10,-15,-10).R(LS,'x',0,40,20,10).R(RS,'x',0,30,15,5));
  Add(Clip("Crouch",1.6f,true).Y(.5f,.49f,.5f).R(B,'x',25,28,25).R(LL,'x',-70).R(RL,'x',-70).R(LS,'x',120).R(RS,'x',120)
   .R(LA,'x',-20).R(RA,'x',-20).R(LF,'x',-40).R(RF,'x',-40).R(LA,'z',-10).R(RA,'z',10));
  Add(Clip("CrouchWalk",1f,true).Y(.5f,.48f,.5f,.48f,.5f).R(B,'x',25).R(LL,'x',-85,-70,-55,-70,-85).R(RL,'x',-55,-70,-85,-70,-55)
   .R(LS,'x',120,125,115,135,120).R(RS,'x',115,135,120,125,115).R(LA,'x',-30,-20,-10,-20,-30).R(RA,'x',-10,-20,-30,-20,-10).R(LF,'x',-40).R(RF,'x',-40).R(LA,'z',-10).R(RA,'z',10));
  // Upper body only (layer mask). Events OpenHitbox 0.16 s / CloseHitbox 0.65 s survive ClearCurves.
  Add(Clip("Attack",.85f,false).R(B,'y',0,-30,35,0).R(B,'x',0,5,10,0).R(RA,'x',0,-120,-70,0).R(RA,'z',0,20,10,0).R(RF,'x',0,-40,-5,0).R(LA,'x',0,25,15,0).R(LF,'x',0,-30,-30,0));
  return c;
 }

 // ---------- controller ----------
 public static void Wire(AnimatorController controller){
  var clips=BuildClips();
  foreach(var(name,type)in new[]{("Speed",AnimatorControllerParameterType.Float),("Mode",AnimatorControllerParameterType.Int),("VerticalSpeed",AnimatorControllerParameterType.Float),("Crouch",AnimatorControllerParameterType.Bool),("Attack",AnimatorControllerParameterType.Trigger)})
   if(controller.parameters.All(p=>p.name!=name))controller.AddParameter(name,type);
  var sm=controller.layers[0].stateMachine;
  AnimatorState S(string n)=>sm.states.First(s=>s.state.name==n).state;
  var loco=S("Locomotion");var jump=S("Jump");var fall=S("Fall");var land=S("Land");var hit=S("Hit");var dead=S("Dead");var crouch=S("Crouch");
  var crouchTree=crouch.motion as BlendTree;
  if(!crouchTree){crouchTree=new BlendTree{name="Speed — Crouch Idle Walk",blendType=BlendTreeType.Simple1D,blendParameter="Speed",useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(crouchTree,controller);crouch.motion=crouchTree;}
  crouchTree.children=new ChildMotion[0];crouchTree.AddChild(clips["Crouch"],0);crouchTree.AddChild(clips["CrouchWalk"],1.6f);
  sm.defaultState=loco;
  foreach(var t in sm.anyStateTransitions)sm.RemoveAnyStateTransition(t);
  foreach(var s in sm.states)foreach(var t in s.state.transitions)s.state.RemoveTransition(t);

  // Priority 1/2 from the FSM table: Dead and Hit can interrupt anything, never re-enter themselves.
  Any(sm,dead,.1f,("Mode",Eq,3));
  Any(sm,hit,.08f,("Mode",Eq,2));
  // Grounded: stand <-> crouch.
  To(loco,crouch,.15f,("Mode",Eq,0),("Crouch",If,0));
  To(crouch,loco,.15f,("Mode",Eq,0),("Crouch",IfNot,0));
  // Grounded -> Airborne. Jump when rising, Fall when dropping (walked off a ledge or past the apex).
  foreach(var ground in new[]{loco,crouch,land}){To(ground,jump,.08f,("Mode",Eq,1),("VerticalSpeed",Gt,.1f));To(ground,fall,.15f,("Mode",Eq,1),("VerticalSpeed",Lt,.1f));}
  To(jump,fall,.2f,("Mode",Eq,1),("VerticalSpeed",Lt,0));
  To(jump,land,.05f,("Mode",Eq,0));
  To(fall,land,.05f,("Mode",Eq,0));
  var landDone=land.AddTransition(loco);landDone.hasExitTime=true;landDone.exitTime=.8f;landDone.duration=.1f;
  // Hit exits only when C# changes Mode; the clip never ends the stun on its own.
  To(hit,loco,.15f,("Mode",Eq,0));
  To(hit,fall,.15f,("Mode",Eq,1));
  // Safety net after R: Rebind already returns to Locomotion, this covers a Mode reset without Rebind.
  To(dead,loco,.1f,("Mode",Eq,0));

  // Layer 1 "Upper Body Action" (Avatar Mask = Hips/UpperBody): Attack overrides arms/torso while legs keep the base layer.
  var layers=controller.layers;layers[1].defaultWeight=1;layers[1].blendingMode=AnimatorLayerBlendingMode.Override;controller.layers=layers;
  var action=controller.layers[1].stateMachine;
  AnimatorState A(string n)=>action.states.First(s=>s.state.name==n).state;
  var empty=A("Empty");var attack=A("Attack");attack.motion=clips["Attack"];action.defaultState=empty;
  foreach(var s in action.states)foreach(var t in s.state.transitions)s.state.RemoveTransition(t);
  To(empty,attack,.05f,("Attack",If,0));
  var swingDone=attack.AddTransition(empty);swingDone.hasExitTime=true;swingDone.exitTime=.95f;swingDone.duration=.1f;
  To(attack,empty,.05f,("Mode",Gt,1)); // Hit or Dead interrupt the swing
  EditorUtility.SetDirty(controller);
 }

 static void Setup(AnimatorStateTransition t,float duration,(string param,AnimatorConditionMode mode,float value)[] conditions){
  t.hasExitTime=false;t.duration=duration;t.hasFixedDuration=true;
  foreach(var c in conditions)t.AddCondition(c.mode,c.value,c.param);
 }
 static void To(AnimatorState from,AnimatorState to,float duration,params(string,AnimatorConditionMode,float)[] c)=>Setup(from.AddTransition(to),duration,c);
 static void Any(AnimatorStateMachine sm,AnimatorState to,float duration,params(string,AnimatorConditionMode,float)[] c){var t=sm.AddAnyStateTransition(to);Setup(t,duration,c);t.canTransitionToSelf=false;}
}}
