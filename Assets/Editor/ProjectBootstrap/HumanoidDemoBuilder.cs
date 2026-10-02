using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace ProjectBootstrap {
public static class HumanoidDemoBuilder {
 const string Root="Assets/TrainingArena/Animations";
 static Transform Bone(string name,Transform parent,Vector3 offset){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=offset;return t;}
 static void Part(Transform parent,Vector3 position,Vector3 size,Material material){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="Mesh";UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;}
 static AnimationClip Clip(string name,float amplitude,float duration){
  var clip=new AnimationClip{name=name,frameRate=30};
  void Muscle(string property,params float[] values){var keys=new Keyframe[values.Length];for(int i=0;i<values.Length;i++)keys[i]=new Keyframe(duration*i/(values.Length-1),values[i]);AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Animator),property),new AnimationCurve(keys));}
  foreach(string side in new[]{"Left","Right"}){float sign=side=="Left"?1:-1;Muscle(side+" Upper Leg Front-Back",-amplitude*sign,amplitude*sign,-amplitude*sign);Muscle(side+" Lower Leg Stretch",.1f,.45f,.1f);Muscle(side+" Arm Down-Up",-.8f,-.8f);Muscle(side+" Arm Front-Back",amplitude*sign,-amplitude*sign,amplitude*sign);}
  var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,settings);AssetDatabase.CreateAsset(clip,Root+"/Clips/"+name+".anim");return clip;
 }
 static GameObject Body(string name,Vector3 position,float legScale,float armScale,Material material,AnimatorController controller){
  var go=new GameObject(name);go.transform.position=position;var bones=new Dictionary<string,Transform>();
  Transform Add(string human,string node,Transform parent,Vector3 offset){var bone=Bone(node,parent,offset);bones[human]=bone;return bone;}
  float hipHeight=.9f*legScale;
  var hips=Add("Hips","Hips",go.transform,new Vector3(0,hipHeight,0));Part(hips,Vector3.zero,new Vector3(.45f,.2f,.3f),material);
  var spine=Add("Spine","Spine",hips,new Vector3(0,.2f,0));Part(spine,new Vector3(0,.15f,0),new Vector3(.5f,.4f,.3f),material);
  var head=Add("Head","Head",spine,new Vector3(0,.55f,0));Part(head,Vector3.zero,new Vector3(.33f,.32f,.3f),material);
  foreach(int sign in new[]{-1,1}){string side=sign<0?"Left":"Right";
   var upperArm=Add(side+"UpperArm",side+"UpperArm",spine,new Vector3(sign*.32f,.3f,0));upperArm.localRotation=Quaternion.Euler(0,0,sign*90);
   Part(upperArm,new Vector3(0,-.16f*armScale,0),new Vector3(.16f,.32f*armScale,.18f),material);
   var lowerArm=Add(side+"LowerArm",side+"LowerArm",upperArm,new Vector3(0,-.32f*armScale,0));Part(lowerArm,new Vector3(0,-.14f*armScale,0),new Vector3(.15f,.28f*armScale,.16f),material);
   var hand=Add(side+"Hand",side+"Hand",lowerArm,new Vector3(0,-.28f*armScale,0));Part(hand,new Vector3(0,-.05f,0),new Vector3(.17f,.1f,.17f),material);
   var upperLeg=Add(side+"UpperLeg",side+"UpperLeg",hips,new Vector3(sign*.15f,-.08f,0));Part(upperLeg,new Vector3(0,-.19f*legScale,0),new Vector3(.2f,.38f*legScale,.22f),material);
   var lowerLeg=Add(side+"LowerLeg",side+"LowerLeg",upperLeg,new Vector3(0,-.38f*legScale,0));Part(lowerLeg,new Vector3(0,-.19f*legScale,0),new Vector3(.18f,.38f*legScale,.2f),material);
   var foot=Add(side+"Foot",side+"Foot",lowerLeg,new Vector3(0,-.38f*legScale,0));Part(foot,new Vector3(0,-.04f,.07f),new Vector3(.22f,.12f,.32f),material);
  }
  var human=new List<HumanBone>();foreach(var pair in bones)human.Add(new HumanBone{humanName=pair.Key,boneName=pair.Value.name,limit=new HumanLimit{useDefaultValues=true}});
  go.transform.position=Vector3.zero;var skeleton=new List<SkeletonBone>();var skeletonTransforms=new List<Transform>{go.transform};skeletonTransforms.AddRange(bones.Values);foreach(var t in skeletonTransforms)skeleton.Add(new SkeletonBone{name=t.name,position=t.localPosition,rotation=t.localRotation,scale=t.localScale});
  var description=new HumanDescription{human=human.ToArray(),skeleton=skeleton.ToArray(),upperArmTwist=.5f,lowerArmTwist=.5f,upperLegTwist=.5f,lowerLegTwist=.5f,armStretch=.05f,legStretch=.05f,feetSpacing=0,hasTranslationDoF=false};
  var avatar=AvatarBuilder.BuildHumanAvatar(go,description);avatar.name=name+" Avatar";if(!avatar.isValid||!avatar.isHuman)throw new InvalidOperationException("Invalid Humanoid Avatar: "+name);
  AssetDatabase.CreateAsset(avatar,Root+"/"+name+".asset");go.transform.position=position;var animator=go.AddComponent<Animator>();animator.avatar=avatar;animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;go.AddComponent<TrainingArena.HumanoidRetargetDemo>().animator=animator;
  return go;
 }
 public static void Generate(Material source,Material target){
  var controller=AnimatorController.CreateAnimatorControllerAtPath(Root+"/Controllers/HumanoidRetarget.controller");controller.AddParameter("Speed",AnimatorControllerParameterType.Float);
  var state=controller.layers[0].stateMachine.AddState("Shared Humanoid Locomotion");controller.layers[0].stateMachine.defaultState=state;
  var blend=new BlendTree{name="Shared muscle clips",blendType=BlendTreeType.Simple1D,blendParameter="Speed",useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(blend,controller);blend.AddChild(Clip("HumanoidIdle",0,1),0);blend.AddChild(Clip("HumanoidWalk",.35f,.8f),3);blend.AddChild(Clip("HumanoidRun",.65f,.55f),6);state.motion=blend;
  Body("Humanoid Source",new Vector3(-13,.05f,-11),1,1,source,controller);Body("Humanoid Retarget",new Vector3(-10,.05f,-11),1.4f,.8f,target,controller);
 }
}}
