using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;
using TMPro;
using TrainingArena;
using Unity.Cinemachine;
using Object=UnityEngine.Object;
namespace ProjectBootstrap {
public static class ArenaBuilder {
 const string Root="Assets/TrainingArena";
 static Material floor,wall,teal,amber,red,white,dark;static TMP_FontAsset font;
 [MenuItem("Training Arena/Generate Demo Scene")]
 public static void Generate(){
  foreach(string folder in new[]{"Scenes","Prefabs","Config","Animations/Clips","Animations/Controllers","Animations/Masks","Art/Materials","UI","Input","Audio"})Directory.CreateDirectory(Root+"/"+folder);
  AssetDatabase.Refresh();
  if(!AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset"))TMP_PackageResourceImporter.ImportResources(true,false,false);
  AssetDatabase.Refresh();font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
  if(!font)throw new InvalidOperationException("Import TMP Essential Resources before generating the scene. A persistent font asset is required.");
  floor=Mat("Floor",new Color(.055f,.1f,.17f));wall=Mat("Architecture",new Color(.14f,.21f,.3f));teal=Mat("Teal",new Color(.04f,.78f,.74f));amber=Mat("Amber",new Color(1,.64f,.12f));red=Mat("Damage",new Color(.9f,.12f,.22f));white=Mat("Robot",new Color(.8f,.88f,.94f));dark=Mat("RobotDark",new Color(.045f,.07f,.11f));
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var light=new GameObject("Sun").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.4f;light.shadows=LightShadows.Soft;light.transform.rotation=Quaternion.Euler(48,-32,0);
  RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.45f,.55f,.7f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.05f,.09f,.15f);RenderSettings.fogDensity=.009f;
  Cube("Arena Floor",new Vector3(0,-.25f,3),new Vector3(36,.5f,42),floor);
  HumanoidDemoBuilder.Generate(teal,amber);
  Label("HUMANOID SOURCE",new Vector3(-13,2.5f,-11),3,teal.color,false);
  Label("RETARGET: LONGER LEGS",new Vector3(-10,2.7f,-11),3,amber.color,false);
  for(int x=-16;x<=16;x+=4)Cube("Grid X",new Vector3(x,.005f,3),new Vector3(.025f,.015f,40),wall,false);
  for(int z=-16;z<=22;z+=4)Cube("Grid Z",new Vector3(0,.006f,z),new Vector3(34,.015f,.025f),wall,false);
  for(int i=0;i<12;i++)Cube("Step "+i,new Vector3(0,.125f*(i+1),1+.65f*i),new Vector3(3,.25f*(i+1),.65f),teal);
  Cube("Landing Platform",new Vector3(0,1.5f,10),new Vector3(5,3,4),wall);
  var ramp=Cube("Slope",new Vector3(6,.7f,5),new Vector3(3,.3f,6),teal);ramp.transform.rotation=Quaternion.Euler(-18,0,0);
  Cube("Corridor Left",new Vector3(-12,2,10),new Vector3(.5f,4,12),wall);
  Cube("Corridor Right",new Vector3(-8,2,10),new Vector3(.5f,4,12),wall);
  Cube("Corridor End",new Vector3(-10,2,16),new Vector3(4.5f,4,.5f),wall);
  Cube("Crouch Beam",new Vector3(-6,1.5f,-2),new Vector3(4,.5f,2),amber);
  Cube("Beam Support Left",new Vector3(-8,.75f,-2),new Vector3(.25f,1.5f,2),wall);
  Cube("Beam Support Right",new Vector3(-4,.75f,-2),new Vector3(.25f,1.5f,2),wall);
  Label("01  MOVEMENT + JUMP",new Vector3(0,.025f,0),4,teal.color,true);
  Label("02  CAMERA COLLISION",new Vector3(-10,.025f,2),3,teal.color,true);
  Label("03  CROUCH",new Vector3(-6,.025f,-5),3,amber.color,true);
  Label("04  ATTACK",new Vector3(5,.025f,-7),3,amber.color,true);
  Label("05  HIT / DEAD",new Vector3(11,.025f,-7),3,red.color,true);
  Label("06  CAMERA BLEND",new Vector3(11,.025f,7),3,teal.color,true);
  var spawn=new GameObject("Spawn").transform;spawn.position=new Vector3(0,.05f,-10);
  var player=new GameObject("Player");player.layer=8;player.transform.position=spawn.position;
  var cc=player.AddComponent<CharacterController>();cc.height=1.8f;cc.center=new Vector3(0,.9f,0);cc.radius=.3f;cc.stepOffset=.45f;cc.slopeLimit=45;cc.skinWidth=.035f;
  var input=player.AddComponent<PlayerInputHandler>();var motor=player.AddComponent<PlayerMotor>();var health=player.AddComponent<Health>();var brain=player.AddComponent<PlayerBrain>();brain.spawn=spawn;var combat=player.AddComponent<PlayerCombat>();
  input.controls=Controls();var config=ScriptableObject.CreateInstance<PlayerConfig>();AssetDatabase.CreateAsset(config,Root+"/Config/PlayerConfig.asset");motor.config=config;
  var visual=Robot(player.transform);var animator=visual.AddComponent<Animator>();animator.runtimeAnimatorController=Controller();animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
  var animationBridge=visual.AddComponent<PlayerAnimator>();animationBridge.brain=brain;animationBridge.motor=motor;animationBridge.animator=animator;animationBridge.combat=combat;animationBridge.audioSource=visual.AddComponent<AudioSource>();animationBridge.footstep=Footstep();
  var camera=new GameObject("Main Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.fieldOfView=60;camera.nearClipPlane=.08f;camera.farClipPlane=120;camera.backgroundColor=new Color(.035f,.06f,.1f);camera.clearFlags=CameraClearFlags.SolidColor;camera.transform.position=new Vector3(0,3,-15);camera.gameObject.AddComponent<AudioListener>();
  camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
  var cameraRig=new GameObject("Camera Director");cameraRig.transform.position=camera.transform.position;var cameraControl=cameraRig.AddComponent<CameraCoordinator>();cameraControl.player=player.transform;cameraControl.input=input;motor.cameraTransform=camera.transform;
  AttachCinemachine(camera,cameraControl);
  var fixedAnchor=new GameObject("Fixed Camera Anchor").transform;fixedAnchor.position=new Vector3(16,8,16);cameraControl.fixedAnchor=fixedAnchor;
  var zone=Cube("Fixed Camera Zone",new Vector3(11,.05f,12),new Vector3(6,.1f,8),teal,false);var trigger=zone.AddComponent<BoxCollider>();trigger.isTrigger=true;trigger.size=new Vector3(1,35,1);zone.AddComponent<CameraZoneTrigger>().cameraCoordinator=cameraControl;var zoneRb=zone.AddComponent<Rigidbody>();zoneRb.isKinematic=true;zoneRb.useGravity=false;
  var trap=Cube("Damage Zone",new Vector3(11,.15f,-4),new Vector3(3,.3f,3),red);var trapTrigger=trap.AddComponent<BoxCollider>();trapTrigger.isTrigger=true;trapTrigger.size=new Vector3(1,7,1);trap.AddComponent<DamageZone>();var trapRb=trap.AddComponent<Rigidbody>();trapRb.isKinematic=true;trapRb.useGravity=false;
  var dummy=new GameObject("Training Dummy");dummy.transform.position=new Vector3(5,0,-4);dummy.AddComponent<Health>();var dummyBody=Cube("Body",new Vector3(5,1,-4),new Vector3(.9f,1.8f,.7f),amber);dummyBody.transform.SetParent(dummy.transform,true);var feedback=dummy.AddComponent<DummyFeedback>();feedback.body=dummyBody.GetComponent<Renderer>();feedback.label=Label("DUMMY\n100 HP",new Vector3(5,2.4f,-4),2,Color.white,false);
  var demo=new GameObject("Demo Controller").AddComponent<DemoController>();demo.player=brain;demo.cameraCoordinator=cameraControl;
  Hud(brain,cameraControl);
  var volume=new GameObject("Global Volume").AddComponent<Volume>();volume.isGlobal=true;var profile=new VolumeProfile();AssetDatabase.CreateAsset(profile,Root+"/Art/ArenaVolume.asset");var tone=profile.Add<Tonemapping>(true);tone.mode.value=TonemappingMode.ACES;var bloom=profile.Add<Bloom>(true);bloom.intensity.value=.25f;bloom.threshold.value=1;var vignette=profile.Add<Vignette>(true);vignette.intensity.value=.16f;foreach(var c in profile.components)AssetDatabase.AddObjectToAsset(c,profile);volume.sharedProfile=profile;
  QualitySettings.vSyncCount=0;PlayerSettings.productName="Training Arena";PlayerSettings.companyName="Training Arena Team";PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.runInBackground=true;
  PrefabUtility.SaveAsPrefabAsset(player,Root+"/Prefabs/Player.prefab");
  string scenePath=Root+"/Scenes/TrainingArena.unity";EditorSceneManager.SaveScene(scene,scenePath);brain.topic5=false;demo.topic5=false;EditorSceneManager.SaveScene(scene,Root+"/Scenes/TrainingArena_T3.unity",true);brain.topic5=true;demo.topic5=true;EditorSceneManager.SaveScene(scene,scenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scenePath,true)};AssetDatabase.SaveAssets();SpecSceneSetup.Apply();Debug.Log("ARENA_GENERATED: "+scenePath);
 }
 static void AttachCinemachine(Camera camera,CameraCoordinator rig){
  var brain=camera.gameObject.AddComponent<CinemachineBrain>();brain.DefaultBlend=new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut,.65f);
  rig.targetPivot=new GameObject("Camera Target Pivot").transform;rig.targetPivot.position=rig.player.position+Vector3.up*1.55f;
  CinemachineCamera Vcam(string name){var c=new GameObject(name).AddComponent<CinemachineCamera>();c.Follow=rig.targetPivot;c.LookAt=rig.targetPivot;c.Lens.FieldOfView=60;c.Lens.NearClipPlane=.08f;c.transform.position=camera.transform.position;c.Priority=0;c.gameObject.AddComponent<CinemachineImpulseListener>();return c;}
  rig.tps=Vcam("CM TPS");rig.fps=Vcam("CM FPS");rig.topDown=Vcam("CM Top Down");rig.fixedCamera=Vcam("CM Fixed Zone");rig.tps.Priority=20;
  rig.deoccluder=rig.tps.gameObject.AddComponent<CinemachineDeoccluder>();rig.deoccluder.CollideAgainst=1;rig.deoccluder.AvoidObstacles.Enabled=true;rig.deoccluder.AvoidObstacles.CameraRadius=.25f;rig.deoccluder.AvoidObstacles.DampingWhenOccluded=0;rig.deoccluder.AvoidObstacles.Damping=.15f;
  rig.impulse=rig.player.gameObject.AddComponent<CinemachineImpulseSource>();
 }
 static Material Mat(string name,Color color){string path=Root+"/Art/Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}mat.color=color;return mat;}
 static InputActionAsset Controls(){var asset=ScriptableObject.CreateInstance<InputActionAsset>();var map=new InputActionMap("Player");asset.AddActionMap(map);var move=map.AddAction("Move",InputActionType.Value);move.AddCompositeBinding("2DVector").With("Up","<Keyboard>/w").With("Down","<Keyboard>/s").With("Left","<Keyboard>/a").With("Right","<Keyboard>/d");move.AddBinding("<Gamepad>/leftStick");map.AddAction("Look",InputActionType.Value,"<Mouse>/delta");map.AddAction("Jump",InputActionType.Button,"<Keyboard>/space").AddBinding("<Gamepad>/buttonSouth");map.AddAction("Sprint",InputActionType.Button,"<Keyboard>/leftShift");map.AddAction("Crouch",InputActionType.Button,"<Keyboard>/leftCtrl");map.AddAction("Attack",InputActionType.Button,"<Mouse>/leftButton");string path=Root+"/Input/PlayerControls.inputactions";File.WriteAllText(path,asset.ToJson());Object.DestroyImmediate(asset);AssetDatabase.ImportAsset(path);return AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);}
 static GameObject Cube(string name,Vector3 position,Vector3 scale,Material material,bool collision=true){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.position=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;if(!collision)Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
 static Transform Bone(string name,Transform parent,Vector3 local){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=local;return t;}
 static void Part(string name,Transform bone,Vector3 offset,Vector3 scale,Material material){var go=Cube(name,Vector3.zero,scale,material,false);go.layer=8;go.transform.SetParent(bone,false);go.transform.localPosition=offset;}
 static GameObject Robot(Transform parent){
  var root=Bone("Visual",parent,Vector3.zero);var hips=Bone("Hips",root,new Vector3(0,.95f,0));Part("Pelvis",hips,Vector3.zero,new Vector3(.48f,.24f,.3f),dark);
  var body=Bone("UpperBody",hips,new Vector3(0,.18f,0));Part("Chest",body,new Vector3(0,.15f,0),new Vector3(.56f,.45f,.32f),white);Part("Chest Emblem",body,new Vector3(0,.18f,.17f),new Vector3(.24f,.1f,.04f),teal);
  var head=Bone("Head",body,new Vector3(0,.55f,0));Part("Helmet",head,Vector3.zero,new Vector3(.36f,.34f,.33f),white);Part("Visor",head,new Vector3(0,.025f,.18f),new Vector3(.28f,.1f,.04f),teal);
  foreach(int side in new[]{-1,1}){string prefix=side<0?"Left":"Right";var arm=Bone(prefix+"Arm",body,new Vector3(side*.4f,.32f,0));Part("Upper Arm",arm,new Vector3(0,-.18f,0),new Vector3(.19f,.37f,.2f),white);var forearm=Bone(prefix+"Forearm",arm,new Vector3(0,-.37f,0));Part("Forearm",forearm,new Vector3(0,-.15f,0),new Vector3(.18f,.32f,.2f),teal);
   var leg=Bone(prefix+"Leg",hips,new Vector3(side*.16f,-.08f,0));Part("Thigh",leg,new Vector3(0,-.2f,0),new Vector3(.22f,.4f,.24f),white);var shin=Bone(prefix+"Shin",leg,new Vector3(0,-.4f,0));Part("Shin",shin,new Vector3(0,-.2f,0),new Vector3(.19f,.4f,.21f),dark);Part("Boot",shin,new Vector3(0,-.38f,.07f),new Vector3(.24f,.14f,.37f),teal);
  }return root.gameObject;
 }
 static void Curve(AnimationClip clip,string path,string property,float duration,params float[] values){var keys=new Keyframe[values.Length];for(int i=0;i<values.Length;i++)keys[i]=new Keyframe(duration*i/(values.Length-1),values[i]);clip.SetCurve(path,typeof(Transform),property,new AnimationCurve(keys));}
 static AnimationClip Clip(string name,float length,bool loop){
  string path=Root+"/Animations/Clips/"+name+".anim";var clip=new AnimationClip{name=name,frameRate=30};Curve(clip,"Hips","localPosition.y",length,.95f,.95f);
  string left="Hips/LeftLeg",right="Hips/RightLeg",body="Hips/UpperBody";
  if(name=="Walk"||name=="Run"){float a=name=="Run"?42:24;Curve(clip,left,"localEulerAnglesRaw.x",length,-a,a,-a);Curve(clip,right,"localEulerAnglesRaw.x",length,a,-a,a);Curve(clip,body+"/LeftArm","localEulerAnglesRaw.x",length,a,-a,a);Curve(clip,body+"/RightArm","localEulerAnglesRaw.x",length,-a,a,-a);AnimationUtility.SetAnimationEvents(clip,new[]{new AnimationEvent{time=.08f,functionName="Footstep"},new AnimationEvent{time=length*.55f,functionName="Footstep"}});}
  else if(name=="Jump"||name=="Fall"){Curve(clip,left,"localEulerAnglesRaw.x",length,-24,-24);Curve(clip,right,"localEulerAnglesRaw.x",length,18,18);Curve(clip,body+"/LeftArm","localEulerAnglesRaw.z",length,40,40);Curve(clip,body+"/RightArm","localEulerAnglesRaw.z",length,-40,-40);}
  else if(name=="Crouch"){Curve(clip,"Hips","localPosition.y",length,.48f,.48f);Curve(clip,body,"localEulerAnglesRaw.x",length,55,55);Curve(clip,left,"localEulerAnglesRaw.x",length,-60,-60);Curve(clip,right,"localEulerAnglesRaw.x",length,-60,-60);Curve(clip,left+"/LeftShin","localEulerAnglesRaw.x",length,120,120);Curve(clip,right+"/RightShin","localEulerAnglesRaw.x",length,120,120);}
  else if(name=="Land"){Curve(clip,"Hips","localPosition.y",length,.7f,.95f);}
  else if(name=="Attack"){Curve(clip,body+"/RightArm","localEulerAnglesRaw.x",length,0,-110,-75,0);Curve(clip,body,"localEulerAnglesRaw.y",length,0,-25,25,0);AnimationUtility.SetAnimationEvents(clip,new[]{new AnimationEvent{time=.16f,functionName="OpenHitbox"},new AnimationEvent{time=.65f,functionName="CloseHitbox"}});}
  else if(name=="Hit")Curve(clip,body,"localEulerAnglesRaw.x",length,0,-30,0);
  else if(name=="Dead"){Curve(clip,"Hips","localEulerAnglesRaw.x",length,0,-90,-90);Curve(clip,"Hips","localPosition.y",length,.95f,.2f,.2f);}
  var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=loop;AnimationUtility.SetAnimationClipSettings(clip,settings);AssetDatabase.CreateAsset(clip,path);return clip;
 }
 static AnimatorController Controller(){
  string path=Root+"/Animations/Controllers/Player.controller";var controller=AnimatorController.CreateAnimatorControllerAtPath(path);controller.AddParameter("Speed",AnimatorControllerParameterType.Float);
  var machine=controller.layers[0].stateMachine;var locomotion=machine.AddState("Locomotion");machine.defaultState=locomotion;
  var tree=new BlendTree{name="Speed — Idle Walk Run",blendType=BlendTreeType.Simple1D,blendParameter="Speed",useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(tree,controller);tree.AddChild(Clip("Idle",1,true),0);tree.AddChild(Clip("Walk",.8f,true),3);tree.AddChild(Clip("Run",.55f,true),6);locomotion.motion=tree;
  foreach(string name in new[]{"Jump","Fall","Land","Crouch","Hit","Dead"})machine.AddState(name).motion=Clip(name,name=="Land"?.2f:name=="Hit"?.45f:.8f,name=="Fall"||name=="Crouch");
  controller.AddLayer("Upper Body Action");var layers=controller.layers;layers[1].defaultWeight=1;layers[1].blendingMode=AnimatorLayerBlendingMode.Override;
  var mask=new AvatarMask{name="Upper Body"};var paths=new[]{"","Hips","Hips/UpperBody","Hips/UpperBody/Head","Hips/UpperBody/LeftArm","Hips/UpperBody/LeftArm/LeftForearm","Hips/UpperBody/RightArm","Hips/UpperBody/RightArm/RightForearm","Hips/LeftLeg","Hips/LeftLeg/LeftShin","Hips/RightLeg","Hips/RightLeg/RightShin"};mask.transformCount=paths.Length;for(int i=0;i<paths.Length;i++){mask.SetTransformPath(i,paths[i]);mask.SetTransformActive(i,paths[i].StartsWith("Hips/UpperBody"));}AssetDatabase.CreateAsset(mask,Root+"/Animations/Masks/UpperBody.mask");layers[1].avatarMask=mask;controller.layers=layers;
  var action=layers[1].stateMachine;var empty=action.AddState("Empty");action.defaultState=empty;action.AddState("Attack").motion=Clip("Attack",.85f,false);AnimatorWiring.Wire(controller);return controller;
 }
 static AudioClip Footstep(){string path=Root+"/Audio/Footstep.wav";const int count=2205;using(var w=new BinaryWriter(File.Open(path,FileMode.Create))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+count*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(22050);w.Write(44100);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(count*2);for(int i=0;i<count;i++)w.Write((short)(Math.Sin(i*.08)*Math.Exp(-i/300.0)*11000));}AssetDatabase.ImportAsset(path);return AssetDatabase.LoadAssetAtPath<AudioClip>(path);}
 static TMP_Text Label(string text,Vector3 position,float width,Color color,bool ground){var go=new GameObject(text,typeof(TextMeshPro));go.transform.position=position;if(ground)go.transform.rotation=Quaternion.Euler(90,0,0);else go.AddComponent<BillboardLabel>();var t=go.GetComponent<TextMeshPro>();t.font=font;t.text=text;t.fontSize=2.5f;t.color=color;t.alignment=TextAlignmentOptions.Center;t.rectTransform.sizeDelta=new Vector2(width,1);return t;}
 static TMP_Text Text(Transform parent,string name,Vector2 anchor,Vector2 position,Vector2 size,int fontsize){var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);var t=go.GetComponent<TextMeshProUGUI>();t.font=font;t.fontSize=fontsize;t.color=new Color(.86f,.93f,.98f);t.raycastTarget=false;var r=t.rectTransform;r.anchorMin=r.anchorMax=r.pivot=anchor;r.anchoredPosition=position;r.sizeDelta=size;return t;}
 static void Hud(PlayerBrain brain,CameraCoordinator camera){
  var go=new GameObject("HUD",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;var scaler=go.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
  var overlay=go.AddComponent<TrainingArena.DebugOverlay>();overlay.player=brain;overlay.cameraCoordinator=camera;
  overlay.status=Text(go.transform,"Status",new Vector2(0,1),new Vector2(26,-25),new Vector2(650,250),20);
  overlay.history=Text(go.transform,"History",Vector2.one,new Vector2(-26,-25),new Vector2(360,220),19);
  overlay.help=Text(go.transform,"Controls",Vector2.zero,new Vector2(26,24),new Vector2(1300,95),20);overlay.help.text="WASD  MOVE   |   SHIFT  RUN   |   SPACE  JUMP   |   CTRL  CROUCH   |   LMB  ATTACK\nC  CAMERA   |   F3  DAMPING   |   F4  COLLISION   |   F5  SHAKE   |   F6  T3 / T5\nBACKSPACE  RESET   |   ESC  CURSOR   |   F1  HELP   |   F2  DEBUG";
  var bar=new GameObject("Health",typeof(RectTransform),typeof(UnityEngine.UI.Image));bar.transform.SetParent(go.transform,false);var rect=bar.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(26,-265);rect.sizeDelta=new Vector2(300,9);var image=bar.GetComponent<UnityEngine.UI.Image>();image.color=new Color(.1f,.85f,.7f);image.type=UnityEngine.UI.Image.Type.Filled;image.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;overlay.healthFill=image;
 }
 public static void GenerateBatch(){try{Generate();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 public static void BuildLinux(){foreach(string topic in new[]{"T3","T5"}){var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Root+"/Scenes/"+(topic=="T3"?"TrainingArena_T3":"TrainingArena")+".unity"},locationPathName="Builds/"+topic+"/TrainingArena",target=BuildTarget.StandaloneLinux64,options=BuildOptions.None});Debug.Log("ARENA_BUILD_"+topic+": "+result.summary.result);if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded){EditorApplication.Exit(1);return;}}EditorApplication.Exit(0);}
 public static void GenerateAndBuildLinux(){try{Generate();BuildLinux();}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
}}
