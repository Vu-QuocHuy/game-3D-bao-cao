using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using TrainingArena;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace ProjectBootstrap {
// Builds the single demo scene described in dac-ta-game-demo-3d.md: test areas (section 3), Humanoid player
// (4), Input Actions + on-screen controls (5), Cinemachine third-person camera (7) and Animator Controller (8).
// Re-running overwrites the scene, clips and controller.
public static class DemoSceneBuilder {
 const string Root = "Assets/TrainingArena";
 const string ScenePath = Root + "/Scenes/TrainingArena.unity";
 const int Environment = 6, PlayerLayer = 8;
 static TMP_FontAsset font;

 [MenuItem("Training Arena/Build Demo Scene")]
 public static void Build() {
  foreach (var f in new[] { "Scenes", "Animations/Clips", "Animations/Controllers", "Animations/Masks", "Art/Materials", "Input", "Audio" }) Directory.CreateDirectory(Root + "/" + f);
  SetLayers();
  font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
  if (!font) throw new InvalidOperationException("Import TMP Essential Resources first.");
  var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

  var floor = CheckerMaterial();
  Material wall = Mat("Architecture", new Color(.16f, .23f, .33f)), teal = Mat("Teal", new Color(.04f, .72f, .7f)), amber = Mat("Amber", new Color(1, .64f, .12f)),
   red = Mat("Damage", new Color(.86f, .16f, .24f)), white = Mat("Robot", new Color(.85f, .9f, .95f)), dark = Mat("RobotDark", new Color(.08f, .1f, .14f));

  var sun = new GameObject("Directional Light").AddComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.3f; sun.shadows = LightShadows.Soft; sun.transform.rotation = Quaternion.Euler(50, -35, 0);
  RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.5f, .56f, .66f);

  // ---- Section 3: test areas. Start at the origin facing +Z. ----
  Env("Floor 40x40", new Vector3(0, -.25f, 0), new Vector3(40, .5f, 40), floor);
  for (int i = 0; i < 5; i++) Env("Stair " + (i + 1), new Vector3(-2.75f - .5f * i, .1f * (i + 1), 0), new Vector3(.5f, .2f * (i + 1), 2.4f), teal);
  Env("Ledge Platform 1.5m", new Vector3(-7.5f, .75f, 0), new Vector3(5, 1.5f, 4), wall);
  Env("Tall Step 0.6m", new Vector3(5, .3f, 0), new Vector3(2, .6f, 2.4f), red);
  Ramp("Slope 30", new Vector3(-3, 0, 4), 30, 3.2f, teal); Env("Slope 30 Top", new Vector3(-3, .8f, 7.77f), new Vector3(2.4f, 1.6f, 2), wall);
  Ramp("Slope 60", new Vector3(3, 0, 4), 60, 2f, red); Env("Slope 60 Top", new Vector3(3, .87f, 5.5f), new Vector3(2.4f, 1.73f, 1), wall);
  Env("Corridor Wall Left", new Vector3(-1.1f, 1.5f, 14), new Vector3(.3f, 3, 8), wall);
  Env("Corridor Wall Right", new Vector3(1.1f, 1.5f, 14), new Vector3(.3f, 3, 8), wall);
  Env("Crouch Beam", new Vector3(-6, 1.5f, -6), new Vector3(4, .5f, 2), amber);
  Env("Beam Support L", new Vector3(-8, .75f, -6), new Vector3(.25f, 1.5f, 2), wall);
  Env("Beam Support R", new Vector3(-4, .75f, -6), new Vector3(.25f, 1.5f, 2), wall);
  foreach (var p in new[] { new Vector3(-3, .5f, -13), new Vector3(4, .5f, -15), new Vector3(9, .5f, -11) }) { var c = Env("Obstacle", p, new Vector3(1, 1, 1), wall); c.GetComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx"); Object.DestroyImmediate(c.GetComponent<BoxCollider>()); c.AddComponent<CapsuleCollider>(); }
  Label("START", new Vector3(0, .02f, -1.4f), teal.color);
  Label("STAIRS 0.2 m", new Vector3(-4, .02f, -2), teal.color);
  Label("LEDGE 1.5 m", new Vector3(-7.5f, 1.52f, 0), Color.white);
  Label("STEP 0.6 m", new Vector3(5, .02f, -2), red.color);
  Label("SLOPE 30 deg", new Vector3(-3, .02f, 3), teal.color);
  Label("SLOPE 60 deg", new Vector3(3, .02f, 3), red.color);
  Label("CAMERA CORRIDOR", new Vector3(0, .02f, 9.4f), Color.white);
  Label("CROUCH  (C)", new Vector3(-6, .02f, -8.4f), amber.color);
  Label("ATTACK  (LMB)", new Vector3(6, .02f, -8.4f), amber.color);

  var dummy = new GameObject("Training Dummy"); dummy.transform.position = new Vector3(6, 0, -6); dummy.AddComponent<Health>();
  var body = Cube("Body", dummy.transform, new Vector3(0, .9f, 0), new Vector3(.9f, 1.8f, .7f), amber, true);
  var dummyLabel = Label("TRAINING DUMMY\n100 HP", new Vector3(6, 2.4f, -6), Color.white, billboard: true);
  var feedback = dummy.AddComponent<DummyFeedback>(); feedback.body = body.GetComponent<Renderer>(); feedback.label = dummyLabel;

  // ---- Section 4: player. ----
  var spawn = new GameObject("Spawn").transform; spawn.position = Vector3.zero;
  var player = new GameObject("Player") { tag = "Player", layer = PlayerLayer };
  var cc = player.AddComponent<CharacterController>(); cc.height = 1.8f; cc.radius = .3f; cc.center = new Vector3(0, .9f, 0); cc.stepOffset = .3f; cc.slopeLimit = 45; cc.skinWidth = .08f;
  var input = player.AddComponent<PlayerInput>(); input.actions = InputActions(); input.defaultActionMap = "Player"; input.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
  var combat = player.AddComponent<PlayerCombat>();
  var controller = player.AddComponent<PlayerController>(); controller.spawn = spawn;
  var states = player.AddComponent<PlayerStateMachine>();
  var cameraTarget = new GameObject("CameraTarget").transform; cameraTarget.SetParent(player.transform, false); cameraTarget.localPosition = new Vector3(0, 1.5f, 0); controller.cameraTarget = cameraTarget;

  var model = HumanoidModel(white, teal, dark, out var bones, out var avatar);
  var animator = model.AddComponent<Animator>(); animator.avatar = avatar; animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
  animator.runtimeAnimatorController = Controller(Clips(model, bones, avatar));
  model.transform.SetParent(player.transform, false); model.transform.localPosition = new Vector3(0, -cc.skinWidth, 0); // feet on the floor, not on the skin
  var bridge = model.AddComponent<AnimatorBridge>(); bridge.player = controller; bridge.combat = combat; bridge.animator = animator;
  bridge.audioSource = model.AddComponent<AudioSource>(); bridge.audioSource.playOnAwake = false; bridge.footstep = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/Footstep.wav");
  states.player = controller; states.combat = combat; states.animator = animator;

  // ---- Section 7: camera. ----
  var mainCam = new GameObject("Main Camera") { tag = "MainCamera" }.AddComponent<Camera>(); mainCam.fieldOfView = 55; mainCam.nearClipPlane = .1f; mainCam.backgroundColor = new Color(.05f, .08f, .12f); mainCam.clearFlags = CameraClearFlags.SolidColor;
  mainCam.gameObject.AddComponent<AudioListener>(); mainCam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
  mainCam.transform.position = new Vector3(0, 2.5f, -4);
  var cmBrain = mainCam.gameObject.AddComponent<CinemachineBrain>(); cmBrain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, .5f);
  var vcam = new GameObject("CinemachineCamera").AddComponent<CinemachineCamera>(); vcam.Follow = cameraTarget; // Tracking Target vcam.Lens.FieldOfView = 55; vcam.Lens.NearClipPlane = .1f;
  var orbit = vcam.gameObject.AddComponent<CinemachineOrbitalFollow>(); orbit.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere; orbit.Radius = 4;
  var tracker = orbit.TrackerSettings; tracker.BindingMode = BindingMode.WorldSpace; tracker.PositionDamping = new Vector3(.2f, .2f, .2f); orbit.TrackerSettings = tracker;
  var h = orbit.HorizontalAxis; h.Range = new Vector2(-180, 180); h.Wrap = true; h.Value = 0; orbit.HorizontalAxis = h;
  var v = orbit.VerticalAxis; v.Range = new Vector2(-20, 70); v.Value = 15; v.Center = 15; orbit.VerticalAxis = v;
  var composer = vcam.gameObject.AddComponent<CinemachineRotationComposer>(); composer.Damping = new Vector2(.1f, .1f);
  var deoccluder = vcam.gameObject.AddComponent<CinemachineDeoccluder>(); deoccluder.CollideAgainst = 1 << Environment; deoccluder.IgnoreTag = "Player"; deoccluder.MinimumDistanceFromTarget = .3f;
  var avoid = deoccluder.AvoidObstacles; avoid.Enabled = true; avoid.CameraRadius = .2f; avoid.Strategy = CinemachineDeoccluder.ObstacleAvoidance.ResolutionStrategy.PullCameraForward; avoid.Damping = .2f; avoid.DampingWhenOccluded = 0; deoccluder.AvoidObstacles = avoid;
  var orbitInput = vcam.gameObject.AddComponent<CameraOrbitInput>(); orbitInput.playerInput = input;

  // ---- Sections 5 + 6.4: HUD, on-screen joystick and jump button. ----
  var hud = Ui(controller, states, combat);
  new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

  var demo = new GameObject("Demo Controller").AddComponent<DemoController>();
  demo.player = controller; demo.states = states; demo.animatorBridge = bridge; demo.cameraInput = orbitInput; demo.hud = hud;

  var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(Root + "/Art/ArenaVolume.asset");
  if (profile) { var vol = new GameObject("Global Volume").AddComponent<Volume>(); vol.isGlobal = true; vol.sharedProfile = profile; }

  PlayerSettings.productName = "Training Arena"; PlayerSettings.companyName = "Training Arena Team";
  PlayerSettings.defaultScreenWidth = 1920; PlayerSettings.defaultScreenHeight = 1080; PlayerSettings.fullScreenMode = FullScreenMode.Windowed; PlayerSettings.runInBackground = true;
  EditorSceneManager.SaveScene(scene, ScenePath);
  EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
  AssetDatabase.SaveAssets();
  Debug.Log("DEMO_SCENE_BUILT: " + ScenePath);
 }

 // ---------- environment helpers ----------
 static void SetLayers() {
  var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
  var layers = tm.FindProperty("layers");
  layers.GetArrayElementAtIndex(Environment).stringValue = "Environment";
  layers.GetArrayElementAtIndex(PlayerLayer).stringValue = "Player";
  tm.ApplyModifiedProperties();
 }
 static Material Mat(string name, Color color) {
  string path = Root + "/Art/Materials/" + name + ".mat"; var m = AssetDatabase.LoadAssetAtPath<Material>(path);
  if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
  m.color = color; EditorUtility.SetDirty(m); return m;
 }
 static Material CheckerMaterial() {
  string texPath = Root + "/Art/Checker.png";
  var tex = new Texture2D(64, 64); var a = new Color(.2f, .25f, .32f); var b = new Color(.28f, .34f, .42f);
  for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++) tex.SetPixel(x, y, (x < 32) ^ (y < 32) ? a : b);
  File.WriteAllBytes(texPath, tex.EncodeToPNG()); Object.DestroyImmediate(tex); AssetDatabase.ImportAsset(texPath);
  var imp = (TextureImporter)AssetImporter.GetAtPath(texPath); imp.filterMode = FilterMode.Point; imp.wrapMode = TextureWrapMode.Repeat; imp.SaveAndReimport();
  var m = Mat("Floor Checker", Color.white); m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texPath)); m.SetTextureScale("_BaseMap", new Vector2(20, 20)); // 1 m squares
  return m;
 }
 static GameObject Cube(string name, Transform parent, Vector3 local, Vector3 scale, Material m, bool collider) {
  var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = local; go.transform.localScale = scale;
  go.GetComponent<Renderer>().sharedMaterial = m; if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
 }
 static GameObject Env(string name, Vector3 pos, Vector3 scale, Material m) { var go = Cube(name, null, pos, scale, m, true); go.layer = Environment; return go; }
 // Ramp rising toward +Z from `start` (bottom edge on the floor).
 static void Ramp(string name, Vector3 start, float degrees, float length, Material m) {
  float rad = degrees * Mathf.Deg2Rad;
  var go = Env(name, start + new Vector3(0, length / 2 * Mathf.Sin(rad), length / 2 * Mathf.Cos(rad)), new Vector3(2.4f, .2f, length), m);
  go.transform.rotation = Quaternion.Euler(-degrees, 0, 0);
 }
 static TMP_Text Label(string text, Vector3 pos, Color color, bool billboard = false) {
  var go = new GameObject(text.Split('\n')[0], typeof(TextMeshPro)); var t = go.GetComponent<TextMeshPro>();
  t.font = font; t.text = text; t.fontSize = billboard ? 2.6f : 3.2f; t.color = color; t.alignment = TextAlignmentOptions.Center; t.rectTransform.sizeDelta = new Vector2(5, 1.4f);
  t.rectTransform.anchoredPosition3D = pos; // set on the RectTransform, not transform.position
  if (billboard) go.AddComponent<BillboardLabel>(); else go.transform.rotation = Quaternion.Euler(90, 0, 0);
  return t;
 }

 // ---------- input (section 5) ----------
 static InputActionAsset InputActions() {
  var asset = ScriptableObject.CreateInstance<InputActionAsset>(); var map = new InputActionMap("Player"); asset.AddActionMap(map);
  var move = map.AddAction("Move", InputActionType.Value); move.expectedControlType = "Vector2";
  move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
  move.AddBinding("<Gamepad>/leftStick"); // also the On-Screen Stick
  var look = map.AddAction("Look", InputActionType.Value); look.expectedControlType = "Vector2"; look.AddBinding("<Mouse>/delta"); look.AddBinding("<Gamepad>/rightStick");
  map.AddAction("Jump", InputActionType.Button, "<Keyboard>/space").AddBinding("<Gamepad>/buttonSouth"); // also the On-Screen Button
  map.AddAction("Walk", InputActionType.Button, "<Keyboard>/leftCtrl");
  map.AddAction("ToggleWorldMove", InputActionType.Button, "<Keyboard>/t");
  map.AddAction("Crouch", InputActionType.Button, "<Keyboard>/c");
  map.AddAction("Attack", InputActionType.Button, "<Mouse>/leftButton");
  string path = Root + "/Input/PlayerControls.inputactions"; File.WriteAllText(path, asset.ToJson()); Object.DestroyImmediate(asset);
  AssetDatabase.ImportAsset(path); return AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
 }

 // ---------- UI ----------
 static DebugHUD Ui(PlayerController player, PlayerStateMachine states, PlayerCombat combat) {
  var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
  go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
  var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
  var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

  // Right part of the screen: drag to look (On-Screen Stick on the right stick). Added first so buttons sit on top.
  var lookArea = Img(go.transform, "Look Drag Area", null, new Color(1, 1, 1, 0));
  var lr = lookArea.rectTransform; lr.anchorMin = new Vector2(.55f, .25f); lr.anchorMax = new Vector2(1, .85f); lr.offsetMin = lr.offsetMax = Vector2.zero;
  var lookStick = lookArea.gameObject.AddComponent<OnScreenStick>(); lookStick.controlPath = "<Gamepad>/rightStick"; lookStick.movementRange = 160;

  var stickBase = Img(go.transform, "Joystick", knob, new Color(1, 1, 1, .18f)); Place(stickBase.rectTransform, Vector2.zero, new Vector2(230, 230), new Vector2(260, 260));
  var stick = Img(stickBase.transform, "Stick", knob, new Color(1, 1, 1, .6f)); stick.rectTransform.sizeDelta = new Vector2(120, 120);
  var onStick = stick.gameObject.AddComponent<OnScreenStick>(); onStick.controlPath = "<Gamepad>/leftStick"; onStick.movementRange = 100;

  var jump = Img(go.transform, "Jump Button", knob, new Color(.05f, .75f, .72f, .55f)); Place(jump.rectTransform, new Vector2(1, 0), new Vector2(-220, 220), new Vector2(180, 180));
  jump.gameObject.AddComponent<OnScreenButton>().controlPath = "<Gamepad>/buttonSouth";
  var jt = Text(jump.transform, "Label", new Vector2(.5f, .5f), Vector2.zero, new Vector2(180, 60), 30); jt.text = "JUMP"; jt.alignment = TextAlignmentOptions.Center;

  var hud = go.AddComponent<DebugHUD>(); hud.player = player; hud.states = states; hud.combat = combat;
  hud.status = Text(go.transform, "DebugHUD", new Vector2(0, 1), new Vector2(30, -26), new Vector2(760, 330), 34);
  hud.help = Text(go.transform, "Controls", new Vector2(.5f, 0), new Vector2(0, 26), new Vector2(1500, 70), 19); hud.help.alignment = TextAlignmentOptions.Bottom;
  return hud;
 }
 static Image Img(Transform parent, string name, Sprite sprite, Color color) {
  var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
  var img = go.GetComponent<Image>(); img.sprite = sprite; img.color = color; return img;
 }
 static void Place(RectTransform r, Vector2 anchor, Vector2 pos, Vector2 size) { r.anchorMin = r.anchorMax = anchor; r.pivot = new Vector2(.5f, .5f); r.anchoredPosition = pos; r.sizeDelta = size; }
 static TMP_Text Text(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, int fontSize) {
  var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); go.transform.SetParent(parent, false);
  var t = go.GetComponent<TextMeshProUGUI>(); t.font = font; t.fontSize = fontSize; t.color = new Color(.92f, .96f, 1); t.raycastTarget = false;
  var r = t.rectTransform; r.anchorMin = r.anchorMax = r.pivot = anchor; r.anchoredPosition = pos; r.sizeDelta = size; return t;
 }

 // ---------- Humanoid model (section 4.2) ----------
 static GameObject HumanoidModel(Material white, Material teal, Material dark, out Dictionary<string, Transform> bones, out Avatar avatar) {
  var go = new GameObject("Model") { layer = PlayerLayer }; var map = new Dictionary<string, Transform>();
  Transform Bone(string name, Transform parent, Vector3 offset) { var t = new GameObject(name) { layer = PlayerLayer }.transform; t.SetParent(parent, false); t.localPosition = offset; map[name] = t; return t; }
  void Part(Transform bone, Vector3 pos, Vector3 size, Material m) { var p = Cube("Mesh", bone, pos, size, m, false); p.layer = PlayerLayer; }
  var hips = Bone("Hips", go.transform, new Vector3(0, .9f, 0)); Part(hips, Vector3.zero, new Vector3(.42f, .2f, .28f), dark);
  var spine = Bone("Spine", hips, new Vector3(0, .12f, 0)); Part(spine, new Vector3(0, .2f, 0), new Vector3(.5f, .45f, .3f), white); Part(spine, new Vector3(0, .25f, .16f), new Vector3(.22f, .1f, .03f), teal);
  var head = Bone("Head", spine, new Vector3(0, .55f, 0)); Part(head, new Vector3(0, .12f, 0), new Vector3(.32f, .3f, .3f), white); Part(head, new Vector3(0, .14f, .16f), new Vector3(.24f, .08f, .03f), teal);
  foreach (int sign in new[] { -1, 1 }) {
   string side = sign < 0 ? "Left" : "Right";
   var upperArm = Bone(side + "UpperArm", spine, new Vector3(sign * .32f, .38f, 0)); upperArm.localRotation = Quaternion.Euler(0, 0, sign * 90); // T-pose for the Avatar
   Part(upperArm, new Vector3(0, -.15f, 0), new Vector3(.15f, .3f, .17f), white);
   var lowerArm = Bone(side + "LowerArm", upperArm, new Vector3(0, -.3f, 0)); Part(lowerArm, new Vector3(0, -.13f, 0), new Vector3(.14f, .26f, .15f), teal);
   var hand = Bone(side + "Hand", lowerArm, new Vector3(0, -.26f, 0)); Part(hand, new Vector3(0, -.05f, 0), new Vector3(.15f, .1f, .15f), dark);
   var upperLeg = Bone(side + "UpperLeg", hips, new Vector3(sign * .13f, -.06f, 0)); Part(upperLeg, new Vector3(0, -.19f, 0), new Vector3(.19f, .38f, .21f), white);
   var lowerLeg = Bone(side + "LowerLeg", upperLeg, new Vector3(0, -.38f, 0)); Part(lowerLeg, new Vector3(0, -.19f, 0), new Vector3(.17f, .38f, .19f), dark);
   var foot = Bone(side + "Foot", lowerLeg, new Vector3(0, -.4f, 0)); Part(foot, new Vector3(0, -.03f, .06f), new Vector3(.2f, .1f, .3f), teal);
  }
  var human = map.Select(p => new HumanBone { humanName = p.Key, boneName = p.Value.name, limit = new HumanLimit { useDefaultValues = true } }).ToArray();
  var skeleton = new[] { go.transform }.Concat(map.Values).Select(t => new SkeletonBone { name = t.name, position = t.localPosition, rotation = t.localRotation, scale = t.localScale }).ToArray();
  var desc = new HumanDescription { human = human, skeleton = skeleton, upperArmTwist = .5f, lowerArmTwist = .5f, upperLegTwist = .5f, lowerLegTwist = .5f, armStretch = .05f, legStretch = .05f, feetSpacing = 0, hasTranslationDoF = false };
  avatar = AvatarBuilder.BuildHumanAvatar(go, desc); avatar.name = "Player Avatar";
  if (!avatar.isValid || !avatar.isHuman) throw new InvalidOperationException("Invalid Humanoid Avatar");
  string path = Root + "/Animations/PlayerAvatar.asset"; AssetDatabase.DeleteAsset(path); AssetDatabase.CreateAsset(avatar, path);
  bones = map; return go;
 }

 // ---------- Humanoid muscle clips (authored as bone rotations, converted with HumanPoseHandler) ----------
 // Pose convention: identity = limb hanging straight down. X: negative swings a limb forward, positive bends a knee
 // or leans the spine forward. Arm Z: Left negative / Right positive raises it sideways. Hips Y: hip height in metres.
 sealed class Pose {
  public readonly string name; public readonly float length; public readonly bool loop; public float[] hipsY = { .9f };
  public readonly List<(string bone, int axis, float[] values)> rot = new List<(string, int, float[])>();
  public (float time, string fn)[] events = new (float, string)[0];
  public Pose(string name, float length, bool loop) { this.name = name; this.length = length; this.loop = loop; }
  public Pose Y(params float[] v) { hipsY = v; return this; }
  public Pose R(string bone, char axis, params float[] v) { rot.Add((bone, axis - 'x', v)); return this; }
  public Pose Events(params (float, string)[] e) { events = e; return this; }
 }
 static float Sample(float[] v, float u) { // u in [0,1], smooth between evenly spaced keys
  if (v.Length == 1) return v[0];
  float s = Mathf.Clamp01(u) * (v.Length - 1); int i = Mathf.Min((int)s, v.Length - 2); float f = s - i; f = f * f * (3 - 2 * f);
  return Mathf.Lerp(v[i], v[i + 1], f);
 }
 static IEnumerable<Pose> Poses() {
  const string S = "Spine", Hd = "Head", LUA = "LeftUpperArm", RUA = "RightUpperArm", LLA = "LeftLowerArm", RLA = "RightLowerArm", LUL = "LeftUpperLeg", RUL = "RightUpperLeg", LLL = "LeftLowerLeg", RLL = "RightLowerLeg";
  yield return new Pose("Idle", 2.4f, true).Y(.9f, .885f, .9f).R(S, 'x', 2, 4, 2).R(Hd, 'x', 0, -3, 0).R(LUA, 'z', -6, -9, -6).R(RUA, 'z', 6, 9, 6).R(LLA, 'x', -10, -15, -10).R(RLA, 'x', -10, -15, -10);
  yield return new Pose("Walk", .9f, true).Y(.9f, .87f, .9f, .87f, .9f).R(S, 'x', 4, 6, 4, 6, 4).R(S, 'y', -5, 0, 5, 0, -5)
   .R(LUL, 'x', -25, 0, 25, 0, -25).R(RUL, 'x', 25, 0, -25, 0, 25).R(LLL, 'x', 5, 5, 10, 45, 5).R(RLL, 'x', 10, 45, 5, 5, 10)
   .R(LUA, 'x', 20, 0, -20, 0, 20).R(RUA, 'x', -20, 0, 20, 0, -20).R(LUA, 'z', -6).R(RUA, 'z', 6).R(LLA, 'x', -20).R(RLA, 'x', -20)
   .Events((.08f, "Footstep"), (.5f, "Footstep"));
  yield return new Pose("Run", .6f, true).Y(.86f, .81f, .86f, .81f, .86f).R(S, 'x', 14, 17, 14, 17, 14).R(S, 'y', -10, 0, 10, 0, -10)
   .R(LUL, 'x', -60, 0, 40, 0, -60).R(RUL, 'x', 40, 0, -60, 0, 40).R(LLL, 'x', 10, 15, 30, 95, 10).R(RLL, 'x', 30, 95, 10, 15, 30)
   .R(LUA, 'x', 45, 0, -45, 0, 45).R(RUA, 'x', -45, 0, 45, 0, -45).R(LUA, 'z', -8).R(RUA, 'z', 8).R(LLA, 'x', -80).R(RLA, 'x', -80)
   .Events((.05f, "Footstep"), (.35f, "Footstep"));
  yield return new Pose("Jump", .5f, false).Y(.9f).R(S, 'x', 10, 0).R(LUL, 'x', -10, -50).R(LLL, 'x', 20, 70).R(RUL, 'x', 0, -20).R(RLL, 'x', 10, 40)
   .R(LUA, 'x', 0, -30).R(RUA, 'x', 0, -30).R(LUA, 'z', -20, -60).R(RUA, 'z', 20, 60);
  yield return new Pose("Fall", .8f, true).Y(.9f).R(S, 'x', -5, -8, -5).R(Hd, 'x', -10).R(LUL, 'x', -20, -15, -20).R(LLL, 'x', 30, 25, 30).R(RUL, 'x', 10, 5, 10).R(RLL, 'x', 20, 25, 20)
   .R(LUA, 'z', -70, -85, -70).R(RUA, 'z', 70, 85, 70);
  yield return new Pose("Crouch", 1.6f, true).Y(.47f, .46f, .47f).R(S, 'x', 25, 28, 25).R(LUL, 'x', -70).R(RUL, 'x', -70).R(LLL, 'x', 120).R(RLL, 'x', 120)
   .R(LUA, 'x', -20).R(RUA, 'x', -20).R(LLA, 'x', -40).R(RLA, 'x', -40).R(LUA, 'z', -10).R(RUA, 'z', 10);
  yield return new Pose("CrouchWalk", 1f, true).Y(.47f, .45f, .47f, .45f, .47f).R(S, 'x', 25).R(LUL, 'x', -85, -70, -55, -70, -85).R(RUL, 'x', -55, -70, -85, -70, -55)
   .R(LLL, 'x', 120, 125, 115, 135, 120).R(RLL, 'x', 115, 135, 120, 125, 115).R(LUA, 'x', -30, -20, -10, -20, -30).R(RUA, 'x', -10, -20, -30, -20, -10).R(LLA, 'x', -40).R(RLA, 'x', -40).R(LUA, 'z', -10).R(RUA, 'z', 10);
  yield return new Pose("Attack", .85f, false).R(S, 'y', 0, -30, 35, 0).R(S, 'x', 0, 5, 10, 0).R(RUA, 'x', 0, -120, -70, 0).R(RUA, 'z', 0, 20, 10, 0).R(RLA, 'x', 0, -40, -5, 0)
   .R(LUA, 'x', 0, 25, 15, 0).R(LUA, 'z', -6).R(LLA, 'x', 0, -30, -30, 0).Events((.16f, "OpenHitbox"), (.65f, "CloseHitbox"));
 }
 static void ApplyPose(Pose p, float u, Dictionary<string, Transform> bones) {
  foreach (var t in bones.Values) t.localRotation = Quaternion.identity; // arms hang (not T-pose) at identity
  var e = new Dictionary<string, Vector3>();
  foreach (var (bone, axis, values) in p.rot) { e.TryGetValue(bone, out var v); v[axis] = Sample(values, u); e[bone] = v; }
  foreach (var kv in e) bones[kv.Key].localRotation = Quaternion.Euler(kv.Value);
  var hp = bones["Hips"].localPosition; hp.y = Sample(p.hipsY, u); bones["Hips"].localPosition = hp;
 }
 static AnimationClip WriteClip(Pose p, GameObject model, Dictionary<string, Transform> bones, Avatar avatar, AnimationClip clip, float rootScale) {
  clip.ClearCurves();
  var handler = new HumanPoseHandler(avatar, model.transform); var pose = new HumanPose();
  int frames = Mathf.Max(2, Mathf.RoundToInt(p.length * 30) + 1);
  var muscles = new Keyframe[HumanTrait.MuscleCount][]; for (int m = 0; m < muscles.Length; m++) muscles[m] = new Keyframe[frames];
  var root = new Keyframe[7][]; for (int r = 0; r < 7; r++) root[r] = new Keyframe[frames];
  for (int f = 0; f < frames; f++) {
   float u = f / (float)(frames - 1), t = u * p.length;
   ApplyPose(p, u, bones); handler.GetHumanPose(ref pose);
   for (int m = 0; m < muscles.Length; m++) muscles[m][f] = new Keyframe(t, pose.muscles[m]);
   var bp = pose.bodyPosition * rootScale; var bq = pose.bodyRotation;
   float[] rv = { bp.x, bp.y, bp.z, bq.x, bq.y, bq.z, bq.w }; for (int r = 0; r < 7; r++) root[r][f] = new Keyframe(t, rv[r]);
  }
  handler.Dispose();
  for (int m = 0; m < muscles.Length; m++) AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), HumanTrait.MuscleName[m]), new AnimationCurve(muscles[m]));
  string[] rootNames = { "RootT.x", "RootT.y", "RootT.z", "RootQ.x", "RootQ.y", "RootQ.z", "RootQ.w" };
  for (int r = 0; r < 7; r++) AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), rootNames[r]), new AnimationCurve(root[r]));
  var s = AnimationUtility.GetAnimationClipSettings(clip);
  s.loopTime = p.loop; // in place: bake root into pose, keep original height so crouch lowers the hips
  s.loopBlendOrientation = s.loopBlendPositionY = s.loopBlendPositionXZ = true; s.keepOriginalOrientation = s.keepOriginalPositionY = s.keepOriginalPositionXZ = true; s.heightFromFeet = false;
  AnimationUtility.SetAnimationClipSettings(clip, s);
  AnimationUtility.SetAnimationEvents(clip, p.events.Select(ev => new AnimationEvent { time = ev.time, functionName = ev.fn }).ToArray());
  return clip;
 }
 static Dictionary<string, AnimationClip> Clips(GameObject model, Dictionary<string, Transform> bones, Avatar avatar) {
  // HumanPose.bodyPosition and clip RootT may differ by the avatar's human scale: pick whichever reproduces the authored hip height.
  var probe = new Pose("Probe", 1, false).Y(.6f);
  var probeAnimator = model.GetComponent<Animator>();
  float best = 1, bestErr = float.MaxValue;
  foreach (float scale in new[] { 1, 1 / probeAnimator.humanScale, probeAnimator.humanScale }) {
   var c = WriteClip(probe, model, bones, avatar, new AnimationClip(), scale);
   c.SampleAnimation(model, .5f); float err = Mathf.Abs(bones["Hips"].position.y - .6f);
   Debug.Log($"ROOT_SCALE_PROBE scale {scale:0.000} hips {bones["Hips"].position.y:0.000} err {err:0.000}");
   if (err < bestErr) { bestErr = err; best = scale; }
  }
  var clips = new Dictionary<string, AnimationClip>();
  foreach (var p in Poses()) {
   string path = Root + "/Animations/Clips/" + p.name + ".anim"; var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
   if (!clip) { clip = new AnimationClip { name = p.name }; AssetDatabase.CreateAsset(clip, path); }
   WriteClip(p, model, bones, avatar, clip, best); EditorUtility.SetDirty(clip); clips[p.name] = clip;
  }
  foreach (var t in bones.Values) t.localRotation = Quaternion.identity; // back to bind pose for the scene
  bones["LeftUpperArm"].localRotation = Quaternion.Euler(0, 0, -90); bones["RightUpperArm"].localRotation = Quaternion.Euler(0, 0, 90); bones["Hips"].localPosition = new Vector3(0, .9f, 0);
  AssetDatabase.SaveAssets();
  return clips;
 }

 // ---------- Animator Controller (section 8) ----------
 static AnimatorController Controller(Dictionary<string, AnimationClip> clips) {
  string path = Root + "/Animations/Controllers/Player.controller"; AssetDatabase.DeleteAsset(path);
  var c = AnimatorController.CreateAnimatorControllerAtPath(path);
  c.AddParameter("Speed", AnimatorControllerParameterType.Float);
  c.AddParameter(new AnimatorControllerParameter { name = "IsGrounded", type = AnimatorControllerParameterType.Bool, defaultBool = true });
  c.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
  c.AddParameter("Crouch", AnimatorControllerParameterType.Bool);   // extension
  c.AddParameter("Attack", AnimatorControllerParameterType.Trigger); // extension
  var sm = c.layers[0].stateMachine;
  BlendTree Tree(string name, params (string clip, float threshold)[] children) {
   var t = new BlendTree { name = name, blendType = BlendTreeType.Simple1D, blendParameter = "Speed", useAutomaticThresholds = false }; AssetDatabase.AddObjectToAsset(t, c);
   foreach (var (clip, th) in children) t.AddChild(clips[clip], th); return t;
  }
  var loco = sm.AddState("Locomotion", new Vector3(300, 0)); loco.motion = Tree("Idle Walk Run", ("Idle", 0), ("Walk", .5f), ("Run", 1)); sm.defaultState = loco;
  var jump = sm.AddState("Jump", new Vector3(550, -80)); jump.motion = clips["Jump"];
  var fall = sm.AddState("Fall", new Vector3(550, 80)); fall.motion = clips["Fall"];
  var crouch = sm.AddState("Crouch", new Vector3(300, 160)); crouch.motion = Tree("Crouch Idle Walk", ("Crouch", 0), ("CrouchWalk", .4f));
  // Spec 8.2 table (Locomotion -> Jump listed first so it wins over -> Fall on the take-off frame).
  T(loco, jump, .05f).AddCondition(AnimatorConditionMode.If, 0, "Jump");
  var jf = T(jump, fall, .1f); jf.hasExitTime = true; jf.exitTime = .7f; // "or a short Exit Time": IsGrounded is already false on take-off
  T(loco, fall, .1f).AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");
  T(fall, loco, .1f).AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
  T(loco, crouch, .15f).AddCondition(AnimatorConditionMode.If, 0, "Crouch");
  T(crouch, loco, .15f).AddCondition(AnimatorConditionMode.IfNot, 0, "Crouch");
  T(crouch, fall, .1f).AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");

  // Upper-body layer for Attack: legs keep walking on the base layer.
  var mask = new AvatarMask { name = "Upper Body" };
  foreach (AvatarMaskBodyPart part in Enum.GetValues(typeof(AvatarMaskBodyPart))) if (part != AvatarMaskBodyPart.LastBodyPart)
   mask.SetHumanoidBodyPartActive(part, part == AvatarMaskBodyPart.Body || part == AvatarMaskBodyPart.Head || part == AvatarMaskBodyPart.LeftArm || part == AvatarMaskBodyPart.RightArm || part == AvatarMaskBodyPart.LeftFingers || part == AvatarMaskBodyPart.RightFingers);
  string maskPath = Root + "/Animations/Masks/UpperBody.mask"; AssetDatabase.DeleteAsset(maskPath); AssetDatabase.CreateAsset(mask, maskPath);
  c.AddLayer("Upper Body Action"); var layers = c.layers; layers[1].defaultWeight = 0; /* AnimatorBridge raises it during a swing */ layers[1].avatarMask = mask; layers[1].blendingMode = AnimatorLayerBlendingMode.Override; c.layers = layers;
  var action = c.layers[1].stateMachine; var empty = action.AddState("Empty"); action.defaultState = empty; empty.writeDefaultValues = false;
  var attack = action.AddState("Attack"); attack.motion = clips["Attack"];
  T(empty, attack, .05f).AddCondition(AnimatorConditionMode.If, 0, "Attack");
  var done = T(attack, empty, .1f); done.hasExitTime = true; done.exitTime = .95f;
  AssetDatabase.SaveAssets();
  return c;
 }
 static AnimatorStateTransition T(AnimatorState from, AnimatorState to, float duration) {
  var t = from.AddTransition(to); t.hasExitTime = false; t.duration = duration; t.hasFixedDuration = true; return t;
 }

 // ---------- batch entry points ----------
 [MenuItem("Training Arena/Build Linux Player")]
 public static void BuildLinux() {
  var r = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/TrainingArena/TrainingArena", target = BuildTarget.StandaloneLinux64, options = BuildOptions.None });
  Debug.Log("ARENA_BUILD: " + r.summary.result);
  if (Application.isBatchMode) EditorApplication.Exit(r.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
 }
 public static void BuildSceneBatch() { try { Build(); EditorApplication.Exit(0); } catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); } }
 public static void BuildAll() { try { Build(); BuildLinux(); } catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); } }
}}
