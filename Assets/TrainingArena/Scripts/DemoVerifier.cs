using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.OnScreen;
namespace TrainingArena {
// Acceptance run for dac-ta-game-demo-3d.md (section 11 checklist + setup tables) plus the crouch/attack extensions.
// Drives a virtual keyboard/mouse and the on-screen controls, then reads back controller, FSM, Animator, camera and HUD.
// Start a build with: --verify-demo [--verification-report file.json] [--verification-shots dir]
public sealed class DemoVerifier : MonoBehaviour {
    [Serializable] public class CheckResult { public string name; public bool passed; public string detail; }
    [Serializable] public class Report { public string scene; public int passed, failed; public List<CheckResult> checks = new List<CheckResult>(); }
    public string reportPath = "/tmp/training-arena-validation.json", shotDir;
    bool quitAfterReport;

    readonly Report report = new Report();
    Keyboard kb; Mouse mouse;
    PlayerController player; PlayerStateMachine states; PlayerCombat combat; AnimatorBridge bridge; Animator animator; DebugHUD hud;
    CinemachineOrbitalFollow orbit; CinemachineDeoccluder deoccluder; Health dummy; Transform spawn;
    readonly HashSet<string> seenStates = new HashSet<string>();
    static readonly string[] AnimStates = { "Locomotion", "Jump", "Fall", "Crouch" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void StartFromCommandLine() {
        var args = Environment.GetCommandLineArgs();
        if (Array.IndexOf(args, "--verify-demo") < 0) return;
        var v = new GameObject("Standalone Verification").AddComponent<DemoVerifier>();
        v.quitAfterReport = true;
        int i = Array.IndexOf(args, "--verification-report"); if (i >= 0 && i + 1 < args.Length) v.reportPath = args[i + 1];
        i = Array.IndexOf(args, "--verification-shots"); if (i >= 0 && i + 1 < args.Length) v.shotDir = args[i + 1];
    }

    // ---------- helpers ----------
    void Check(string name, bool ok, string detail) {
        report.checks.Add(new CheckResult { name = name, passed = ok, detail = detail });
        Debug.Log($"VERIFY {(ok ? "PASS" : "FAIL")} {name}: {detail}");
    }
    void Keys(params Key[] keys) => InputSystem.QueueStateEvent(kb, new KeyboardState(keys));
    void Look(Vector2 pixels) => InputSystem.QueueStateEvent(mouse, new MouseState { delta = pixels });
    void Click(bool down) => InputSystem.QueueStateEvent(mouse, down ? new MouseState().WithButton(MouseButton.Left) : new MouseState());
    IEnumerator Wait(float s) { float end = Time.time + s; while (Time.time < end) yield return null; }
    IEnumerator Tap(params Key[] keys) { Keys(keys); yield return null; yield return null; Keys(); yield return null; }
    IEnumerator TapClick() { Click(true); yield return null; yield return null; Click(false); yield return null; }
    IEnumerator ResetDemo() { yield return Tap(Key.R); yield return Wait(.5f); }
    IEnumerator Place(Vector3 pos, float yaw) { player.ResetTo(pos, Quaternion.identity); orbit.HorizontalAxis.Value = yaw; yield return Wait(.6f); }
    IEnumerator Yaw(float degrees) { Look(new Vector2(degrees / .12f, 0)); yield return Wait(.6f); } // CameraOrbitInput: 0.12 deg per pixel

    string Anim(int layer = 0) {
        var info = animator.IsInTransition(layer) ? animator.GetNextAnimatorStateInfo(layer) : animator.GetCurrentAnimatorStateInfo(layer);
        return (layer == 0 ? AnimStates : new[] { "Empty", "Attack" }).FirstOrDefault(n => info.IsName(n)) ?? "?";
    }
    Transform Bone(HumanBodyBones b) => animator.GetBoneTransform(b);
    static Vector3 Flat(Vector3 v) { v.y = 0; return v; }
    Vector3 CamForward => Flat(Camera.main.transform.forward).normalized;
    Vector3 Pos => player.transform.position;
    static string V(Vector3 v) => $"({v.x:0.00},{v.y:0.00},{v.z:0.00})";
    // Largest rotation a bone makes away from where it started, over `seconds`.
    IEnumerator Swing(Transform bone, float seconds, float[] result) {
        var q0 = bone.localRotation; float max = 0;
        for (float t = 0; t < seconds; t += Time.deltaTime) { max = Mathf.Max(max, Quaternion.Angle(q0, bone.localRotation)); yield return null; }
        result[0] = max;
    }
    void Shot(string name) {
        if (string.IsNullOrEmpty(shotDir) || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        Directory.CreateDirectory(shotDir);
        var cam = Camera.main; var rt = RenderTexture.GetTemporary(1280, 720, 24);
        cam.targetTexture = rt; cam.Render(); cam.targetTexture = null;
        var prev = RenderTexture.active; RenderTexture.active = rt;
        var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply();
        RenderTexture.active = prev; RenderTexture.ReleaseTemporary(rt);
        File.WriteAllBytes(Path.Combine(shotDir, name + ".png"), tex.EncodeToPNG()); Destroy(tex);
    }
    void LateUpdate() { if (states) seenStates.Add(states.CurrentName); }

    IEnumerator Start() {
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        kb = InputSystem.AddDevice<Keyboard>("VerifierKeyboard"); kb.MakeCurrent();
        mouse = InputSystem.AddDevice<Mouse>("VerifierMouse"); mouse.MakeCurrent();
        Cursor.lockState = CursorLockMode.Locked;
        PlayerController.IgnoreCursorLock = Cursor.lockState != CursorLockMode.Locked;
        yield return Wait(1f);
        report.scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        player = FindAnyObjectByType<PlayerController>(); states = player.GetComponent<PlayerStateMachine>(); combat = player.GetComponent<PlayerCombat>();
        bridge = player.GetComponentInChildren<AnimatorBridge>(); animator = bridge.animator; hud = FindAnyObjectByType<DebugHUD>();
        orbit = FindAnyObjectByType<CinemachineOrbitalFollow>(); deoccluder = FindAnyObjectByType<CinemachineDeoccluder>();
        dummy = FindAnyObjectByType<Health>(); spawn = player.spawn;
        if (PlayerController.IgnoreCursorLock) Debug.Log("VERIFY NOTE cursor cannot lock in batchmode; mouse look/attack read without the lock gate");

        foreach (var test in new Func<IEnumerator>[] { Setup, CameraRelative, WorldMode, Jumping, Ledge, Stairs, StepAndSlopes, Corridor,
                                                      BlendTree, StateNames, HumanoidMotion, AnimatorOff, Crouch, Attack, Joystick, ResetAndHud })
            yield return Run(test);
        Check("FSM visited Idle/Move/Jump/Fall/Crouch/Attack", new[] { "Idle", "Move", "Jump", "Fall", "Crouch", "Attack" }.All(seenStates.Contains), string.Join(",", seenStates));

        report.passed = report.checks.Count(c => c.passed); report.failed = report.checks.Count - report.passed;
        File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
        Debug.Log($"VERIFY SUMMARY {report.scene}: {report.passed}/{report.checks.Count} passed -> {reportPath}");
        if (quitAfterReport) Application.Quit(report.failed == 0 ? 0 : 1);
        Destroy(this);
    }

    // One failing test must not abort the run: exceptions become FAIL entries.
    IEnumerator Run(Func<IEnumerator> test) {
        var e = test();
        while (true) {
            object current;
            try { if (!e.MoveNext()) break; current = e.Current; }
            catch (Exception ex) { Check(test.Method.Name + " crashed", false, ex.ToString()); break; }
            yield return current;
        }
        Keys(); Click(false); yield return null;
    }

    // ---------- section 2, 4, 5, 7, 8 setup ----------
    IEnumerator Setup() {
        yield return ResetDemo();
        var cc = player.GetComponent<CharacterController>();
        Check("4.3 Character Controller values", Mathf.Approximately(cc.height, 1.8f) && Mathf.Approximately(cc.radius, .3f) && cc.center == new Vector3(0, .9f, 0)
              && Mathf.Approximately(cc.stepOffset, .3f) && Mathf.Approximately(cc.slopeLimit, 45) && Mathf.Approximately(cc.skinWidth, .08f),
              $"h {cc.height} r {cc.radius} c {cc.center} step {cc.stepOffset} slope {cc.slopeLimit} skin {cc.skinWidth}");
        var pi = player.GetComponent<PlayerInput>();
        Check("5 Player Input with Move/Look/Jump/Walk/ToggleWorldMove", pi && new[] { "Move", "Look", "Jump", "Walk", "ToggleWorldMove" }.All(a => pi.actions.FindAction(a) != null),
              pi ? string.Join(",", pi.actions.Select(a => a.name)) : "missing");
        Check("4.2 Humanoid model, valid Avatar, Root Motion off", animator.isHuman && animator.avatar.isValid && !animator.applyRootMotion,
              $"human {animator.isHuman}, valid {animator.avatar.isValid}, root motion {animator.applyRootMotion}");
        Check("4.1 Hierarchy Player/Model + Player/CameraTarget (y 1.5)", animator.transform.parent == player.transform && animator.name == "Model"
              && player.cameraTarget && player.cameraTarget.parent == player.transform && Mathf.Abs(player.cameraTarget.localPosition.y - 1.5f) < .01f, "");
        var loops = animator.runtimeAnimatorController.animationClips.GroupBy(c => c.name).ToDictionary(g => g.Key, g => g.First().isLooping);
        Check("4.2 Clips Idle/Walk/Run/Fall loop, Jump does not", new[] { "Idle", "Walk", "Run", "Fall" }.All(n => loops.TryGetValue(n, out var l) && l) && loops.TryGetValue("Jump", out var j) && !j,
              string.Join(",", loops.Select(kv => kv.Key + ":" + kv.Value)));
        Check("8.1 Parameters Speed/IsGrounded/Jump", new[] { ("Speed", AnimatorControllerParameterType.Float), ("IsGrounded", AnimatorControllerParameterType.Bool), ("Jump", AnimatorControllerParameterType.Trigger) }
              .All(p => animator.parameters.Any(x => x.name == p.Item1 && x.type == p.Item2)), string.Join(",", animator.parameters.Select(p => p.name + ":" + p.type)));
        Check("8.2 States Locomotion/Jump/Fall, Locomotion default", new[] { "Locomotion", "Jump", "Fall" }.All(s => animator.HasState(0, Animator.StringToHash(s))) && Anim() == "Locomotion", $"current {Anim()}");
        int env = LayerMask.NameToLayer("Environment");
        Check("3 Environment layer used by ground and Deoccluder", env >= 0 && GameObject.Find("Floor 40x40").layer == env && deoccluder.CollideAgainst == 1 << env && player.groundMask == 1 << env,
              $"layer {env}, deoccluder mask {deoccluder.CollideAgainst.value}");
        Check("7 Third-person camera: Orbital Follow 4 m, damping, Deoccluder radius 0.2", Mathf.Approximately(orbit.Radius, 4) && orbit.TrackerSettings.PositionDamping.x is >= .1f and <= .3f
              && Mathf.Approximately(deoccluder.AvoidObstacles.CameraRadius, .2f) && orbit.GetComponent<CinemachineCamera>().Follow == player.cameraTarget,
              $"radius {orbit.Radius}, damping {orbit.TrackerSettings.PositionDamping.x}, cam radius {deoccluder.AvoidObstacles.CameraRadius}");
        Check("7 Main Camera has Cinemachine Brain", Camera.main.GetComponent<CinemachineBrain>(), "");
        Check("3 Directional Light with shadows", FindObjectsByType<Light>(FindObjectsSortMode.None).Any(l => l.type == LightType.Directional && l.shadows != LightShadows.None), "");
    }

    // ---------- section 11 checks ----------
    IEnumerator CameraRelative() {
        yield return ResetDemo();
        Vector3 start = Pos, fwd = CamForward;
        Keys(Key.W); yield return Wait(.8f); Keys();
        Vector3 moved = Flat(Pos - start);
        Check("11 W walks toward the screen (camera forward)", moved.magnitude > 2 && Vector3.Dot(moved.normalized, fwd) > .95f, $"moved {moved.magnitude:0.00}, dot {Vector3.Dot(moved.normalized, fwd):0.00}");
        Check("6.1 Run speed 5 m/s", Mathf.Abs(moved.magnitude / .8f - 5) < .6f, $"{moved.magnitude / .8f:0.00} m/s");
        yield return ResetDemo();
        Vector3 before = CamForward; yield return Yaw(90);
        float turned = Vector3.SignedAngle(before, CamForward, Vector3.up);
        start = Pos; Keys(Key.W); yield return Wait(.6f); Keys();
        moved = Flat(Pos - start).normalized;
        Check("11 W still forward after turning camera 90 deg", Mathf.Abs(turned - 90) < 8 && Vector3.Dot(moved, CamForward) > .95f, $"turned {turned:0}, dot {Vector3.Dot(moved, CamForward):0.00}");
        yield return Place(new Vector3(0, 0, -14), 0); // open floor: nothing to slow the measurement
        Keys(Key.W); yield return Wait(.5f); float w = player.HorizontalSpeed;
        Keys(Key.W, Key.D); yield return Wait(.5f); float wd = player.HorizontalSpeed; Keys();
        Check("6.1 Diagonal input not faster (and not slower)", wd <= w + .05f && wd >= w - .3f && w > 4.5f, $"W {w:0.00}, W+D {wd:0.00}");
        Check("6.1 Character turns toward movement", Vector3.Angle(player.transform.forward, (CamForward + Flat(Camera.main.transform.right).normalized).normalized) < 10, $"forward {V(player.transform.forward)}");
    }

    IEnumerator WorldMode() {
        yield return ResetDemo();
        yield return Yaw(90);
        yield return Tap(Key.T);
        bool hudWorld = hud.status.text.Contains("Move mode: World axes");
        Vector3 start = Pos; Keys(Key.W); yield return Wait(.6f); Keys();
        Vector3 moved = Flat(Pos - start).normalized;
        Check("11 T switches to world axes, HUD follows", !player.moveRelativeToCamera && hudWorld && Vector3.Dot(moved, Vector3.forward) > .95f, $"move {V(moved)}, camera fwd {V(CamForward)}");
        yield return Tap(Key.T);
        Check("11 T switches back to camera-relative", player.moveRelativeToCamera && hud.status.text.Contains("Move mode: Camera-relative"), "");
    }

    IEnumerator Jumping() {
        yield return ResetDemo();
        float y0 = Pos.y, peak = y0; int jumps = 0; float last = player.VelocityY;
        Keys(Key.Space);
        for (float t = 0; t < 1.4f; t += Time.deltaTime) { if (player.VelocityY > 4 && last <= 4) jumps++; last = player.VelocityY; peak = Mathf.Max(peak, Pos.y); yield return null; }
        Keys(); yield return Wait(.3f);
        Check("11 Holding Space jumps once", jumps == 1, $"jumps {jumps}");
        Check("6.1 Jump height ~1.2 m (gravity -20)", Mathf.Abs(peak - y0 - 1.2f) < .15f, $"peak {peak - y0:0.00} m");
        yield return Tap(Key.Space); yield return Wait(.15f);
        float vy = player.VelocityY; yield return Tap(Key.Space); yield return null;
        Check("11 No jump while airborne", player.VelocityY < vy && player.BlockReason.Contains("grounded"), $"vy before {vy:0.0}, after {player.VelocityY:0.0}, '{player.BlockReason}'");
        yield return Wait(1f);
        Check("6.1 Grounded keeps VelocityY = -2", player.IsGrounded && Mathf.Abs(player.VelocityY - (-2 + player.gravity * Time.deltaTime)) < .2f, $"vy {player.VelocityY:0.00}");
    }

    IEnumerator Ledge() {
        yield return Place(new Vector3(-7.5f, 1.6f, 0), 0); // on the 1.5 m platform, camera looking +Z
        Check("3 Ledge platform is 1.5 m", Mathf.Abs(Pos.y - 1.5f) < .12f, $"standing at y {Pos.y:0.00}");
        bool fallState = false, fallAnim = false, jumpSeen = false; float minY = Pos.y;
        Keys(Key.W);
        for (float t = 0; t < 1.6f; t += Time.deltaTime) {
            if (states.CurrentName == "Fall") { if (!fallState) Shot("05-fall-off-ledge"); fallState = true; }
            jumpSeen |= states.CurrentName == "Jump"; fallAnim |= Anim() == "Fall"; minY = Mathf.Min(minY, Pos.y); yield return null;
        }
        Keys(); yield return Wait(.4f);
        Check("11 Stepping off the ledge enters Fall", fallState && !jumpSeen && fallAnim && minY < .1f, $"Fall state {fallState}, Fall anim {fallAnim}, jump {jumpSeen}");
        Check("6.2 Landing returns to Idle", states.CurrentName == "Idle" && player.IsGrounded, $"state {states.CurrentName}");
    }

    IEnumerator Stairs() {
        yield return ResetDemo();
        yield return Yaw(-90); // face -X toward the stairs
        float maxY = 0; bool fell = false; Keys(Key.W);
        for (float t = 0; t < 1.6f; t += Time.deltaTime) { maxY = Mathf.Max(maxY, Pos.y); fell |= states.CurrentName == "Fall"; yield return null; }
        Shot("06-stairs");
        Check("11 0.2 m stairs climbable", maxY > .95f, $"max height {maxY:0.00} m (top step 1.0)");
        Check("6.2 Climbing stairs does not flicker into Fall", !fell, "Fall not entered while climbing");
        yield return Tap(Key.W, Key.Space); Keys(Key.W); yield return Wait(.8f); Keys(); yield return Wait(.4f);
        Check("3 Jump from the stairs onto the 1.5 m ledge", Pos.y > 1.45f, $"y {Pos.y:0.00}");
    }

    IEnumerator StepAndSlopes() {
        yield return ResetDemo();
        yield return Yaw(90); // face +X toward the 0.6 m step
        Keys(Key.W); yield return Wait(1.4f); Keys();
        Shot("07-step-0.6-blocked");
        Check("11 0.6 m step blocks the character", Pos.y < .1f && Pos.x < 4, $"pos {V(Pos)}");
        yield return Place(new Vector3(-3, 0, 2.6f), 0);
        float maxY = 0; Keys(Key.W);
        for (float t = 0; t < 2f; t += Time.deltaTime) { maxY = Mathf.Max(maxY, Pos.y); yield return null; }
        Keys(); Shot("08-slope-30");
        Check("11 30 deg slope climbable", maxY > 1.45f, $"max height {maxY:0.00} m (top 1.6)");
        yield return Place(new Vector3(3, 0, 2.6f), 0);
        maxY = 0; Keys(Key.W);
        for (float t = 0; t < 2f; t += Time.deltaTime) { maxY = Mathf.Max(maxY, Pos.y); yield return null; }
        Keys(); Shot("09-slope-60-blocked");
        Check("11 60 deg slope blocks the character", maxY < .6f, $"max height {maxY:0.00} m (top 1.73)");
    }

    IEnumerator Corridor() {
        yield return Place(new Vector3(0, 0, 14), 90); // camera orbit to -X: raw position behind the left wall
        yield return Wait(.8f);
        Vector3 cam = Camera.main.transform.position, target = player.cameraTarget.position;
        Vector3 raw = target + Quaternion.Euler(orbit.VerticalAxis.Value, orbit.HorizontalAxis.Value, 0) * Vector3.back * orbit.Radius;
        bool rawBlocked = Physics.Linecast(target, raw, 1 << 6, QueryTriggerInteraction.Ignore);
        bool inside = Physics.CheckSphere(cam, .05f, 1 << 6, QueryTriggerInteraction.Ignore), blocked = Physics.Linecast(cam, target, 1 << 6, QueryTriggerInteraction.Ignore);
        float dist = Vector3.Distance(cam, target);
        Shot("10-corridor-deoccluder");
        Check("11 Camera moves closer when a wall is in the way", rawBlocked && !inside && !blocked && dist < orbit.Radius - .5f, $"raw blocked {rawBlocked}, camera {V(cam)}, distance {dist:0.00}");
    }

    IEnumerator BlendTree() {
        yield return ResetDemo();
        Check("8 Idle: Speed 0, Locomotion", animator.GetFloat("Speed") < .02f && Anim() == "Locomotion", $"Speed {animator.GetFloat("Speed"):0.00}");
        Shot("01-idle");
        Keys(Key.W, Key.LeftCtrl); yield return Wait(.8f);
        float walk = animator.GetFloat("Speed"), walkMs = player.HorizontalSpeed; Shot("02-walk");
        var samples = new List<float>(); Keys(Key.W);
        for (float t = 0; t < .8f; t += Time.deltaTime) { samples.Add(animator.GetFloat("Speed")); yield return null; }
        float run = animator.GetFloat("Speed"); Shot("03-run"); Keys();
        Check("6.1 Ctrl walks at 2 m/s", Mathf.Abs(walkMs - 2) < .1f, $"{walkMs:0.00} m/s");
        Check("8.1 Speed normalized: walk 0.5, run 1", Mathf.Abs(walk - .5f) < .05f && Mathf.Abs(run - 1) < .05f, $"walk {walk:0.00}, run {run:0.00}");
        bool monotonic = samples.Zip(samples.Skip(1), (a, b) => b >= a - .001f).All(x => x);
        int between = samples.Count(s => s > .55f && s < .95f);
        Check("11 Blend Tree: Speed rises smoothly Walk -> Run", monotonic && between >= 3, $"{between} intermediate samples, monotonic {monotonic}");
    }

    IEnumerator StateNames() {
        yield return ResetDemo();
        var names = new List<string>(); bool hudOk = true;
        void Sample() { string s = states.CurrentName; if (names.Count == 0 || names[names.Count - 1] != s) names.Add(s); hudOk &= hud.status.text.StartsWith("State: " + s); }
        Sample();
        Keys(Key.W); for (int i = 0; i < 20; i++) { yield return null; Sample(); }
        Keys(Key.W, Key.Space); yield return null; Keys(Key.W);
        for (float t = 0; t < 1.3f; t += Time.deltaTime) { yield return null; Sample(); if (states.CurrentName == "Jump" && player.VelocityY > 3) Shot("04-jump"); }
        Keys(); for (int i = 0; i < 20; i++) { yield return null; Sample(); }
        string seq = string.Join(" > ", names);
        Check("6.2 FSM Idle > Move > Jump > Fall > Move/Idle", seq.StartsWith("Idle > Move > Jump > Fall > ") && names.Last() == "Idle", seq);
        Check("11 HUD shows the current state on every frame", hudOk, "HUD 'State:' matched the FSM each sample");
        Check("6.2 History logged", states.History.Count > 0, string.Join(" | ", states.History));
    }

    IEnumerator HumanoidMotion() {
        yield return ResetDemo();
        var r = new float[1];
        yield return Swing(Bone(HumanBodyBones.Spine), 2.4f, r);
        Check("4.2 Idle clip moves the rig (breathing)", r[0] > 1, $"spine {r[0]:0.0} deg");
        Keys(Key.W, Key.LeftCtrl); yield return Wait(.4f);
        yield return Swing(Bone(HumanBodyBones.LeftUpperLeg), .9f, r); float leg = r[0];
        yield return Swing(Bone(HumanBodyBones.LeftLowerLeg), .9f, r); float knee = r[0];
        yield return Swing(Bone(HumanBodyBones.LeftUpperArm), .9f, r); float arm = r[0];
        Keys(Key.W); yield return Wait(.4f);
        yield return Swing(Bone(HumanBodyBones.LeftUpperLeg), .6f, r); float runLeg = r[0]; Keys();
        Check("4.2 Walk swings legs, bends knees, swings arms", leg > 30 && knee > 20 && arm > 15, $"thigh {leg:0}, knee {knee:0}, arm {arm:0} deg");
        Check("4.2 Run swings wider than Walk", runLeg > leg + 10, $"walk {leg:0}, run {runLeg:0} deg");
        yield return Wait(.5f);
        float foot = Mathf.Min(Bone(HumanBodyBones.LeftFoot).position.y, Bone(HumanBodyBones.RightFoot).position.y) - Pos.y;
        Check("4.2 Feet on the floor (not floating or sunk)", foot > -.05f && foot < .15f, $"lowest ankle {foot:0.00} m above the controller base");
    }

    IEnumerator AnimatorOff() {
        yield return ResetDemo();
        animator.enabled = false;
        Vector3 start = Pos; var leg = Bone(HumanBodyBones.LeftUpperLeg); var r = new float[1];
        Keys(Key.W); yield return Swing(leg, .8f, r); Keys();
        Shot("11-animator-off-sliding");
        Check("9 Animator off: character slides without moving its legs", Flat(Pos - start).magnitude > 2 && r[0] < .5f, $"moved {Flat(Pos - start).magnitude:0.00} m, leg {r[0]:0.0} deg");
        animator.enabled = true; yield return ResetDemo();
    }

    IEnumerator Crouch() {
        yield return ResetDemo();
        Keys(Key.C); yield return Wait(.5f);
        float hips = Bone(HumanBodyBones.Hips).position.y - Pos.y;
        Shot("12-crouch");
        Check("Crouch: C crouches (state + animation)", player.Crouched && states.CurrentName == "Crouch" && Anim() == "Crouch", $"state {states.CurrentName}, anim {Anim()}");
        Check("Crouch: hips lowered", hips < .6f, $"hips {hips:0.00} m");
        yield return Tap(Key.C, Key.Space); Keys(Key.C); yield return Wait(.2f);
        Check("Crouch: jump blocked", player.VelocityY <= 0 && player.BlockReason.Contains("crouched"), $"'{player.BlockReason}'");
        Keys(Key.C, Key.W); yield return Wait(.6f);
        Check("Crouch: moves at 1.6 m/s", Mathf.Abs(player.HorizontalSpeed - 1.6f) < .1f, $"{player.HorizontalSpeed:0.00} m/s");
        Keys(); yield return Wait(.4f);
        Check("Crouch: releasing C in the open stands up", !player.Crouched && states.CurrentName == "Idle", $"state {states.CurrentName}");

        yield return Place(new Vector3(-6, 0, -9.6f), 0); // beam spans z -7..-5, underside 1.25 m
        Keys(Key.W); yield return Wait(1.2f); Keys();
        float blocked = Pos.z;
        Check("Crouch: standing player blocked by the beam", blocked < -7, $"z {blocked:0.00}");
        Keys(Key.C, Key.W); yield return Wait(1.1f); Keys(); yield return Wait(.2f);
        float under = Pos.z; Shot("13-under-beam");
        Vector3 cam = Camera.main.transform.position;
        Check("Crouch: under the beam with C released stays crouched", under > -7 && under < -5 && player.Crouched && hud.status.text.Contains("Ceiling"), $"z {under:0.00}, crouched {player.Crouched}");
        Check("Crouch: camera clear of the beam", !Physics.CheckSphere(cam, .05f, 1 << 6) && !Physics.Linecast(cam, player.cameraTarget.position, 1 << 6), $"camera {V(cam)}");
        Keys(Key.W); yield return Wait(1.6f); Keys(); yield return Wait(.3f);
        Check("Crouch: stands up after leaving the beam", Pos.z > -5 && !player.Crouched, $"z {Pos.z:0.00}");
    }

    IEnumerator Attack() {
        yield return ResetDemo();
        yield return Place(dummy.transform.position + new Vector3(0, 0, -1.3f), 0);
        var arm = Bone(HumanBodyBones.RightUpperArm); var q0 = arm.localRotation;
        yield return TapClick();
        bool state = states.CurrentName == "Attack", layer = false, window = false; float swing = 0;
        for (float t = 0; t < 1f; t += Time.deltaTime) {
            float a = Quaternion.Angle(q0, arm.localRotation); if (a > 70 && swing <= 70) Shot("14-attack"); swing = Mathf.Max(swing, a);
            layer |= Anim(1) == "Attack"; window |= combat.WindowOpen; yield return null;
        }
        Check("Attack: LMB enters Attack state + upper-body layer", state && layer, $"state {state}, layer {layer}");
        Check("Attack: arm swings", swing > 70, $"{swing:0} deg");
        Check("Attack: Animation Events open the hitbox, dummy -25 once", window && dummy.Current == 75 && !combat.WindowOpen, $"dummy {dummy.Current}");
        Check("Attack: state returns to Idle", states.CurrentName == "Idle", states.CurrentName);
        Click(true); yield return Wait(1.8f); Click(false); yield return null;
        Check("Attack: holding LMB swings once", dummy.Current == 50, $"dummy {dummy.Current}");
        yield return TapClick(); yield return Wait(.15f); yield return TapClick();
        Check("Attack: cooldown blocks a second click", player.BlockReason.Contains("cooldown"), $"'{player.BlockReason}'");
        yield return Wait(1.2f);
        yield return Yaw(180);
        var r = new float[1]; Keys(Key.W); yield return TapClick();
        yield return Swing(Bone(HumanBodyBones.LeftUpperLeg), .5f, r); Keys();
        Check("Attack: legs keep walking during the swing (upper-body mask)", r[0] > 20 && Anim() == "Locomotion", $"leg {r[0]:0} deg, base {Anim()}");
        yield return Wait(1f);
        yield return Tap(Key.Space); yield return Wait(.1f); yield return TapClick();
        Check("Attack: blocked in the air", !combat.Attacking && player.BlockReason.Contains("grounded"), $"'{player.BlockReason}'");
        yield return Wait(1f);
    }

    IEnumerator Joystick() {
        yield return ResetDemo();
        var sticks = FindObjectsByType<OnScreenStick>(FindObjectsSortMode.None);
        var move = sticks.FirstOrDefault(s => s.controlPath == "<Gamepad>/leftStick");
        var look = sticks.FirstOrDefault(s => s.controlPath == "<Gamepad>/rightStick");
        var jump = FindAnyObjectByType<OnScreenButton>();
        Check("5 Canvas has joystick, jump button, look drag area, EventSystem", move && look && jump && jump.controlPath == "<Gamepad>/buttonSouth" && EventSystem.current, "");
        if (!move || !jump || !look) yield break;

        PointerEventData Press(Component c) {
            var d = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, c.transform.position), button = PointerEventData.InputButton.Left };
            d.pressPosition = d.position; return d;
        }
        float k = move.GetComponentInParent<Canvas>().scaleFactor; // movementRange is in canvas units, drags are in pixels
        var p = Press(move); move.OnPointerDown(p);
        p.position += new Vector2(0, 150 * k); move.OnDrag(p);
        Vector3 start = Pos; yield return Wait(.6f);
        Shot("15-joystick");
        Check("5 Joystick (On-Screen Stick) moves the character forward", Vector3.Dot(Flat(Pos - start), CamForward) > 1.5f && states.CurrentName == "Move", $"moved {V(Pos - start)}");
        p.position -= new Vector2(0, 100 * k); move.OnDrag(p); yield return Wait(.4f); // half deflection
        Check("10 Light joystick push walks (Speed < 1)", animator.GetFloat("Speed") < .8f && player.HorizontalSpeed < 3, $"Speed {animator.GetFloat("Speed"):0.00}, {player.HorizontalSpeed:0.00} m/s");
        move.OnPointerUp(p); yield return Wait(.3f);

        var b = Press(jump); jump.OnPointerDown(b); yield return Wait(.15f);
        bool jumped = player.VelocityY > 1; jump.OnPointerUp(b); yield return Wait(1f);
        Check("5 On-Screen Button jumps", jumped, $"vy {player.VelocityY:0.0}");

        float yaw0 = orbit.HorizontalAxis.Value; var l = Press(look); look.OnPointerDown(l);
        l.position += new Vector2(160 * k, 0); look.OnDrag(l); yield return Wait(.5f); look.OnPointerUp(l);
        Check("5 Dragging the right side turns the camera", Mathf.Abs(Mathf.DeltaAngle(yaw0, orbit.HorizontalAxis.Value)) > 20, $"yaw {yaw0:0} -> {orbit.HorizontalAxis.Value:0}");
    }

    IEnumerator ResetAndHud() {
        Keys(Key.W, Key.C); yield return Wait(.5f); Keys(); yield return Tap(Key.T);
        dummy.Damage(25);
        yield return ResetDemo();
        Check("9 R resets position, state, move mode, dummy", Flat(Pos - spawn.position).magnitude < .1f && states.CurrentName == "Idle" && player.moveRelativeToCamera && !player.Crouched && dummy.Current == 100,
              $"pos {V(Pos)}, state {states.CurrentName}, dummy {dummy.Current}");
        string s = hud.status.text;
        Check("6.4 HUD lines State/Speed/Grounded/VelocityY/Move mode", new[] { "State:", "Speed:", "Grounded:", "VelocityY:", "Move mode:" }.All(s.Contains), s.Replace("\n", " | "));
        Check("6.4 HUD font readable on screen share", hud.status.fontSize >= 28, $"font {hud.status.fontSize}");
        yield return Tap(Key.F1); bool hidden = !hud.Visible; yield return Tap(Key.F1);
        Check("6.4 F1 hides/shows the HUD", hidden && hud.Visible, "");
    }
}
}
