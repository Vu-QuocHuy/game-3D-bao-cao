using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace TrainingArena {
// End-to-end acceptance run for spec D01–D12. Drives a virtual keyboard/mouse through the real input path
// and reads back motor, FSM, Animator, camera and HUD state. Start a build with --verify-demo.
public sealed class DemoVerifier : MonoBehaviour {
    [Serializable] public class CheckResult { public string name; public bool passed; public string detail; }
    [Serializable] public class Report { public string scene; public int passed, failed; public List<CheckResult> checks = new List<CheckResult>(); }
    public string reportPath = "/tmp/training-arena-validation.json";
    public string shotDir; // --verification-shots <dir>: PNG of the game camera at key moments (needs a graphics device, not -nographics)
    bool quitAfterReport;

    readonly Report report = new Report();
    Keyboard kb; Mouse mouse;
    PlayerBrain brain; PlayerMotor motor; Health health; Animator animator; CameraCoordinator cam; DebugOverlay hud; CinemachineBrain cmBrain; Transform spawn;
    readonly HashSet<string> seenAnims = new HashSet<string>();
    static readonly string[] AnimStates = { "Locomotion", "Crouch", "Jump", "Fall", "Land", "Hit", "Dead" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void StartFromCommandLine() {
        var args = Environment.GetCommandLineArgs();
        if (Array.IndexOf(args, "--verify-demo") < 0) return;
        var verifier = new GameObject("Standalone Verification").AddComponent<DemoVerifier>();
        verifier.quitAfterReport = true;
        int index = Array.IndexOf(args, "--verification-report");
        if (index >= 0 && index + 1 < args.Length) verifier.reportPath = args[index + 1];
        index = Array.IndexOf(args, "--verification-shots");
        if (index >= 0 && index + 1 < args.Length) verifier.shotDir = args[index + 1];
    }

    void Check(string name, bool ok, string detail) {
        report.checks.Add(new CheckResult { name = name, passed = ok, detail = detail });
        Debug.Log($"VERIFY {(ok ? "PASS" : "FAIL")} {name}: {detail}");
    }

    void Keys(params Key[] keys) => InputSystem.QueueStateEvent(kb, new KeyboardState(keys));
    void Click(bool down) => InputSystem.QueueStateEvent(mouse, down ? new MouseState().WithButton(MouseButton.Left) : new MouseState());
    IEnumerator TapClick() { Click(true); yield return null; yield return null; Click(false); yield return null; }
    void MouseDelta(Vector2 d) { Cursor.lockState = CursorLockMode.Locked; InputSystem.QueueStateEvent(mouse, new MouseState { delta = d }); }
    IEnumerator Frames(int n) { for (int i = 0; i < n; i++) yield return null; }
    IEnumerator Wait(float s) { float end = Time.time + s; while (Time.time < end) yield return null; }
    IEnumerator Tap(params Key[] keys) { Keys(keys); yield return null; yield return null; Keys(); yield return null; }
    IEnumerator ResetDemo() { yield return Tap(Key.R); yield return Wait(.4f); }

    string Anim() {
        var info = animator.IsInTransition(0) ? animator.GetNextAnimatorStateInfo(0) : animator.GetCurrentAnimatorStateInfo(0);
        return AnimStates.FirstOrDefault(n => info.IsName(n)) ?? "?";
    }
    Vector3 Flat(Vector3 v) { v.y = 0; return v; }
    Vector3 CamForward => Flat(Camera.main.transform.forward).normalized;
    string V(Vector3 v) => $"({v.x:0.00},{v.y:0.00},{v.z:0.00})";

    void LateUpdate() { if (animator) seenAnims.Add(Anim()); }

    IEnumerator Start() {
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        kb = InputSystem.AddDevice<Keyboard>("VerifierKeyboard"); kb.MakeCurrent();
        mouse = InputSystem.AddDevice<Mouse>("VerifierMouse"); mouse.MakeCurrent();
        Cursor.lockState = CursorLockMode.Locked;
        PlayerInputHandler.IgnoreCursorLock = Cursor.lockState != CursorLockMode.Locked;
        yield return Wait(1f);
        report.scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (PlayerInputHandler.IgnoreCursorLock) Debug.Log("VERIFY NOTE cursor cannot lock in this player (batchmode); mouse Look read without lock gate");

        var brains = FindObjectsByType<PlayerBrain>(FindObjectsSortMode.None);
        brain = brains.FirstOrDefault(); motor = brain.GetComponent<PlayerMotor>(); health = brain.GetComponent<Health>();
        animator = brain.GetComponentInChildren<PlayerAnimator>().animator; cam = FindAnyObjectByType<CameraCoordinator>();
        hud = FindAnyObjectByType<DebugOverlay>(); cmBrain = Camera.main.GetComponent<CinemachineBrain>(); spawn = brain.spawn;

        yield return Run(D01);
        yield return Run(D09Bones);
        yield return Run(D02D03);
        yield return Run(D03CameraRelative);
        yield return Run(D05Pitch);
        yield return Run(D02JumpHold);
        yield return Run(D04NoDoubleJump);
        yield return Run(D04Platform);
        yield return Run(D04Wall);
        yield return Run(D07Occlusion);
        yield return Run(D06Wide);
        yield return Run(D10Hit);
        yield return Run(D10HitPriority);
        yield return Run(D11Dead);
        yield return Run(D12Hud);
        yield return Run(Crouch);
        yield return Run(Attack);
        Check("D09 Animator visited all FSM states", AnimStates.All(seenAnims.Contains), "seen: " + string.Join(",", seenAnims));

        report.passed = report.checks.Count(c => c.passed); report.failed = report.checks.Count - report.passed;
        File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
        Debug.Log($"VERIFY SUMMARY {report.scene}: {report.passed}/{report.checks.Count} passed -> {reportPath}");
        if (quitAfterReport) Application.Quit(report.failed == 0 ? 0 : 1);
        Destroy(this);
    }

    // One failing check must not abort the run: exceptions become FAIL entries.
    IEnumerator Run(Func<IEnumerator> test) {
        var e = test();
        while (true) {
            object current;
            try { if (!e.MoveNext()) break; current = e.Current; }
            catch (Exception ex) { Check(test.Method.Name + " crashed", false, ex.ToString()); break; }
            yield return current;
        }
        Keys(); yield return null;
    }

    IEnumerator D01() {
        yield return ResetDemo();
        Check("D01 single player in scene", FindObjectsByType<PlayerBrain>(FindObjectsSortMode.None).Length == 1, "PlayerBrain count");
        float d = Vector3.Distance(Flat(brain.transform.position), Flat(spawn.position));
        Check("D01 reset puts player at spawn A", d < .2f && brain.CurrentState == PlayerBrain.StateMode.Grounded && health.Current == 100,
              $"dist {d:0.00}, state {brain.CurrentState}, HP {health.Current}");
        var clips = animator.runtimeAnimatorController.animationClips;
        var names = clips.Where(c => c).Select(c => c.name).ToList();
        bool allClips = new[] { "Idle", "Walk", "Run", "Jump", "Fall", "Land", "Hit", "Dead" }.All(names.Contains) && clips.All(c => c);
        Check("D09 controller has every clip, none missing", allClips, string.Join(",", names.Distinct()));
        bool param = new[] { "Mode", "Speed", "VerticalSpeed" }.All(p => animator.parameters.Any(x => x.name == p));
        Check("D09 Animator parameters Mode/Speed/VerticalSpeed", param, string.Join(",", animator.parameters.Select(p => p.name)));
        Check("D09 Root Motion off", !animator.applyRootMotion, "applyRootMotion=" + animator.applyRootMotion);
        Check("D09 idle plays Locomotion blend tree", Anim() == "Locomotion" && animator.GetFloat("Speed") < .1f, $"anim {Anim()}, Speed {animator.GetFloat("Speed"):0.00}");
    }

    void Shot(string name) {
        if (string.IsNullOrEmpty(shotDir) || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        Directory.CreateDirectory(shotDir);
        var cam = Camera.main; var rt = RenderTexture.GetTemporary(1280, 720, 24);
        cam.targetTexture = rt; cam.Render(); cam.targetTexture = null;
        var prev = RenderTexture.active; RenderTexture.active = rt;
        var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply();
        RenderTexture.active = prev; RenderTexture.ReleaseTemporary(rt);
        File.WriteAllBytes(Path.Combine(shotDir, $"{report.scene}-{name}.png"), tex.EncodeToPNG()); Destroy(tex);
    }

    float Angle(Transform t) => Mathf.DeltaAngle(0, t.localEulerAngles.x);
    // Samples a bone for `seconds` and returns how far it moved: proves the clip really drives the rig (no frozen/T-pose).
    IEnumerator Range(Func<float> sample, float seconds, float[] result) {
        float lo = float.MaxValue, hi = float.MinValue;
        for (float t = 0; t < seconds; t += Time.deltaTime) { float v = sample(); lo = Mathf.Min(lo, v); hi = Mathf.Max(hi, v); yield return null; }
        result[0] = hi - lo;
    }

    IEnumerator D09Bones() {
        yield return ResetDemo();
        var animators = brain.GetComponentsInChildren<Animator>();
        Check("D09 exactly one Animator drives the player", animators.Length == 1, $"Animators on player: {animators.Length}");
        Transform hips = animator.transform.Find("Hips"), leg = animator.transform.Find("Hips/LeftLeg"), shin = animator.transform.Find("Hips/LeftLeg/LeftShin");
        var r = new float[1];
        yield return Range(() => hips.localPosition.y, 2.5f, r);
        Shot("01-idle");
        Check("D09 Idle clip moves the rig (breathing)", r[0] > .008f, $"hips y range {r[0]:0.000} m");
        Keys(Key.W); yield return Wait(.4f);
        yield return Range(() => Angle(leg), 1f, r); float walkLeg = r[0]; Shot("02-walk");
        yield return Range(() => Angle(shin), 1f, r); float walkKnee = r[0];
        yield return Range(() => Angle(animator.transform.Find("Hips/UpperBody/LeftArm")), 1f, r); float walkArm = r[0];
        Check("D09 Walk swings arms (upper-body layer not freezing them)", walkArm > 25, $"arm {walkArm:0} deg");
        Keys(Key.W, Key.LeftShift); yield return Wait(.5f);
        yield return Range(() => Angle(leg), .8f, r); float runLeg = r[0]; Shot("03-run");
        Keys();
        Check("D09 Walk swings legs and bends knees", walkLeg > 35 && walkKnee > 25, $"thigh {walkLeg:0} deg, knee {walkKnee:0} deg");
        Check("D09 Run swings wider than Walk", runLeg > walkLeg + 15, $"walk {walkLeg:0} deg, run {runLeg:0} deg");
        yield return ResetDemo();
        yield return Tap(Key.K); yield return Wait(1.5f);
        float tilt = Mathf.DeltaAngle(0, hips.localEulerAngles.x); Shot("08-dead");
        Check("D09 Dead pose lies down and holds", tilt < -80 && hips.localPosition.y < .4f, $"hips tilt {tilt:0} deg, height {hips.localPosition.y:0.00}");
    }

    IEnumerator Crouch() {
        yield return ResetDemo();
        Transform hips = animator.transform.Find("Hips");
        Keys(Key.LeftCtrl); yield return Wait(.5f);
        Check("Crouch: Ctrl crouches inside Grounded", motor.Crouched && brain.CurrentState == PlayerBrain.StateMode.Grounded && Anim() == "Crouch",
              $"crouched {motor.Crouched}, state {brain.CurrentState}, anim {Anim()}");
        Shot("09-crouch");
        Check("Crouch: pose lowers hips", hips.localPosition.y < .6f, $"hips y {hips.localPosition.y:0.00}");
        yield return Tap(Key.LeftCtrl, Key.Space); Keys(Key.LeftCtrl); yield return Wait(.2f);
        Check("Crouch: Jump blocked while crouched", motor.VerticalVelocity <= 0 && brain.BlockReason.Contains("crouched"), $"vy {motor.VerticalVelocity:0.0}, reason '{brain.BlockReason}'");
        Keys(Key.LeftCtrl, Key.W, Key.LeftShift); yield return Wait(.6f);
        Check("Crouch: moves slower, Shift ignored", motor.Speed > 1 && motor.Speed < 1.7f, $"speed {motor.Speed:0.00}");
        Keys(); yield return Wait(.4f);
        Check("Crouch: release Ctrl in open stands up", !motor.Crouched && Anim() == "Locomotion", $"crouched {motor.Crouched}, anim {Anim()}");

        // Crouch beam spans x -8..-4, z -3..-1, underside 1.25 m. Walk in crouched, release Ctrl underneath.
        yield return ResetDemo();
        motor.ResetAt(new Vector3(-6, .05f, -6));
        Keys(Key.W); yield return Wait(1.6f); Keys();
        float blockedZ = brain.transform.position.z;
        Check("Crouch: standing player cannot pass under beam", blockedZ < -3f + .05f, $"z {blockedZ:0.00}, beam starts -3");
        Keys(Key.LeftCtrl, Key.W); yield return Wait(1f); Keys(); yield return Wait(.15f);
        float underZ = brain.transform.position.z; Shot("11-under-beam");
        Vector3 camPos = Camera.main.transform.position, head = brain.transform.position + Vector3.up * .95f;
        Check("Crouch: camera stays out of beam with clear view", !Physics.CheckSphere(camPos, .05f, 1, QueryTriggerInteraction.Ignore) && !Physics.Linecast(camPos, head, 1, QueryTriggerInteraction.Ignore), $"camera {V(camPos)}");
        Check("Crouch: under beam with Ctrl released stays crouched", underZ > -3 && underZ < -1 && motor.Crouched && hud.status.text.Contains("Ceiling"),
              $"z {underZ:0.00}, crouched {motor.Crouched}");
        Keys(Key.W); yield return Wait(2f); Keys(); yield return Wait(.3f);
        Check("Crouch: stands automatically after leaving beam", brain.transform.position.z > -1 && !motor.Crouched, $"z {brain.transform.position.z:0.00}, crouched {motor.Crouched}");
    }

    IEnumerator Attack() {
        yield return ResetDemo();
        var combat = brain.GetComponent<PlayerCombat>();
        var dummy = FindObjectsByType<Health>(FindObjectsSortMode.None).First(h => h != health);
        Transform arm = animator.transform.Find("Hips/UpperBody/RightArm");
        motor.ResetAt(dummy.transform.position + new Vector3(0, .05f, -1.3f)); // facing +Z toward the dummy
        yield return Wait(.3f);
        yield return TapClick();
        bool started = combat.Attacking; float minArm = 0; bool layerAttack = false, windowSeen = false;
        for (float t = 0; t < 1f; t += Time.deltaTime) {
            if (Angle(arm) < -80 && minArm >= -80) Shot("13-attack");
            minArm = Mathf.Min(minArm, Angle(arm)); windowSeen |= combat.WindowOpen;
            layerAttack |= animator.GetCurrentAnimatorStateInfo(1).IsName("Attack"); yield return null;
        }
        Check("Attack: LMB starts swing on upper-body layer", started && layerAttack, $"attacking {started}, layer state Attack {layerAttack}");
        Check("Attack: arm swings overhead", minArm < -80, $"right arm min {minArm:0} deg");
        Check("Attack: Animation Events open hitbox, dummy takes 25 once", windowSeen && dummy.Current == 75 && !combat.WindowOpen, $"window seen {windowSeen}, dummy HP {dummy.Current}");

        Click(true); yield return Wait(2f); Click(false); yield return null;
        Check("Attack: holding LMB swings once", dummy.Current == 50, $"dummy HP {dummy.Current}");
        yield return Wait(.2f);
        yield return TapClick(); yield return Wait(.15f); yield return TapClick();
        Check("Attack: second click inside cooldown blocked", brain.BlockReason.Contains("cooldown"), $"reason '{brain.BlockReason}'");
        yield return Wait(1f);
        Check("Attack: cooldown swing still deals one hit", dummy.Current == 25, $"dummy HP {dummy.Current}");

        yield return Wait(.3f);
        Vector3 p = brain.transform.position;
        MouseDelta(new Vector2(1500, 0)); yield return Wait(.4f); // turn away from the dummy before walking
        Keys(Key.W); yield return TapClick(); yield return Wait(.5f);
        Check("Attack: can walk while swinging", combat.Attacking && Flat(brain.transform.position - p).magnitude > .6f && Anim() == "Locomotion",
              $"moved {Flat(brain.transform.position - p).magnitude:0.00}, anim {Anim()}");
        Keys(); yield return Wait(1f);

        yield return Tap(Key.Space); yield return Wait(.1f); yield return TapClick();
        Check("Attack: blocked in air", !combat.Attacking && brain.BlockReason.Contains("Grounded"), $"reason '{brain.BlockReason}'");
        yield return Wait(1.2f);

        yield return TapClick(); yield return Wait(.1f); yield return Tap(Key.H); yield return Wait(.1f);
        Check("Attack: Hit cancels swing and closes hitbox", !combat.Attacking && !combat.WindowOpen && brain.CurrentState == PlayerBrain.StateMode.Hit && !animator.GetNextAnimatorStateInfo(1).IsName("Attack"),
              $"attacking {combat.Attacking}, state {brain.CurrentState}");
        yield return ResetDemo();
        Check("Attack: R restores dummy HP", dummy.Current == 100, $"dummy HP {dummy.Current}");
    }

    IEnumerator D02D03() {
        yield return ResetDemo();
        Vector3 start = brain.transform.position, fwd = CamForward;
        Keys(Key.W); yield return Wait(1f);
        float walk = motor.Speed, animWalk = animator.GetFloat("Speed");
        Vector3 moved = Flat(brain.transform.position - start);
        Check("D02 W moves along camera forward", moved.magnitude > 2 && Vector3.Dot(moved.normalized, fwd) > .95f, $"moved {moved.magnitude:0.00} m, dot {Vector3.Dot(moved.normalized, fwd):0.00}");
        Keys(Key.W, Key.D); yield return Wait(.6f);
        float diag = motor.Speed;
        Check("D03 W+D not faster than W", diag <= walk + .05f, $"W {walk:0.00} m/s, W+D {diag:0.00} m/s");
        Keys(Key.W, Key.LeftShift); yield return Wait(.8f);
        float run = motor.Speed, animRun = animator.GetFloat("Speed");
        Check("D02 Shift sprints", run > walk + 2, $"walk {walk:0.00}, run {run:0.00}");
        Check("D09 blend Speed follows motor (walk~3, run~6)", Mathf.Abs(animWalk - 3) < .5f && Mathf.Abs(animRun - 6) < .6f && Anim() == "Locomotion",
              $"anim Speed walk {animWalk:0.00}, run {animRun:0.00}, state {Anim()}");
        Keys(); yield return Wait(.6f);
        Check("D09 stopping blends back to Idle", animator.GetFloat("Speed") < .3f, $"Speed {animator.GetFloat("Speed"):0.00}");
    }

    IEnumerator D03CameraRelative() {
        yield return ResetDemo();
        Vector3 before = CamForward;
        MouseDelta(new Vector2(750, 0)); // yaw += 750 * 0.12 = 90 deg
        yield return Wait(.8f);
        Vector3 after = CamForward;
        float turned = Vector3.SignedAngle(before, after, Vector3.up);
        Vector3 start = brain.transform.position;
        Keys(Key.W); yield return Wait(.7f); Keys();
        Vector3 moved = Flat(brain.transform.position - start).normalized;
        Check("D03 mouse turns camera ~90 deg", Mathf.Abs(turned - 90) < 8, $"turned {turned:0.0} deg, cursor {Cursor.lockState}");
        Check("D03 W follows rotated camera", Vector3.Dot(moved, after) > .95f, $"move {V(moved)}, cam {V(after)}");
    }

    IEnumerator D05Pitch() {
        yield return ResetDemo();
        MouseDelta(new Vector2(0, -6000)); yield return Wait(.3f);
        float high = Mathf.DeltaAngle(0, cam.tps.transform.eulerAngles.x);
        MouseDelta(new Vector2(0, 6000)); yield return Wait(.3f);
        float low = Mathf.DeltaAngle(0, cam.tps.transform.eulerAngles.x);
        Check("D05 pitch reaches and holds limits [-30,70], camera never flips", Mathf.Abs(high - 70) < .1f && Mathf.Abs(low + 30) < .1f && Camera.main.transform.up.y > 0,
              $"max {high:0.0}, min {low:0.0}, up.y {Camera.main.transform.up.y:0.00}");
        yield return ResetDemo();
        yield return Wait(.6f);
        Vector3 focus = brain.transform.position + Vector3.up * 1.55f;
        float dist = Vector3.Distance(Camera.main.transform.position, focus);
        Check("D05 TPS camera follows player", dist > 3.5f && dist < 6, $"camera-player distance {dist:0.00}");
    }

    IEnumerator D02JumpHold() {
        yield return ResetDemo();
        int jumps = 0; float last = motor.VerticalVelocity; bool sawAir = false;
        Keys(Key.Space);
        for (float t = 0; t < 1.6f; t += Time.deltaTime) {
            if (motor.VerticalVelocity > 5 && last <= 5) jumps++;
            if (brain.CurrentState == PlayerBrain.StateMode.Airborne) sawAir = true;
            last = motor.VerticalVelocity; yield return null;
        }
        Keys(); yield return Wait(.3f);
        Check("D02 holding Space jumps once", jumps == 1, $"jumps {jumps}");
        Check("D08 jump goes Airborne then Grounded", sawAir && brain.CurrentState == PlayerBrain.StateMode.Grounded, $"airborne seen {sawAir}, now {brain.CurrentState}");
    }

    IEnumerator D04NoDoubleJump() {
        yield return ResetDemo();
        float y0 = brain.transform.position.y, peak = y0; int jumps = 0; bool blockedShown = false;
        string animUp = "", animDown = "";
        yield return Tap(Key.Space);
        float last = motor.VerticalVelocity; // first jump already started; count only later ones
        bool airTap = false;
        for (float t = 0; t < 1.4f; t += Time.deltaTime) {
            if (t > .3f && !airTap) { airTap = true; StartCoroutine(Tap(Key.Space)); }
            if (motor.VerticalVelocity > 5 && last <= 5) jumps++;
            if (brain.BlockReason.Contains("Jump")) blockedShown = true;
            if (motor.VerticalVelocity > 3 && animUp == "") { animUp = Anim(); Shot("04-jump"); }
            if (motor.VerticalVelocity < -3 && animDown == "") { animDown = Anim(); Shot("05-fall"); }
            peak = Mathf.Max(peak, brain.transform.position.y); last = motor.VerticalVelocity; yield return null;
        }
        Check("D04 Space in air gives no second jump", jumps == 0 && peak - y0 < 2.1f, $"extra jumps {jumps}, peak {peak - y0:0.00} m");
        Check("D08 Jump in air reported as blocked", blockedShown, "BlockReason 'Jump blocked in air'");
        Check("D09 airborne anim follows VerticalSpeed (Jump up, Fall down)", animUp == "Jump" && animDown == "Fall", $"rising {animUp}, falling {animDown}");
        Check("D04 lands stable", brain.CurrentState == PlayerBrain.StateMode.Grounded && motor.Grounded && Mathf.Abs(brain.transform.position.y - y0) < .05f,
              $"state {brain.CurrentState}, y {brain.transform.position.y:0.00}");
    }

    IEnumerator D04Platform() {
        yield return ResetDemo();
        float maxY = 0; bool sawFall = false, sawLand = false;
        Keys(Key.W, Key.LeftShift); // 4 s at 6 m/s: up the steps, across the platform, off its far edge (floor ends at z 24)
        for (float t = 0; t < 5f; t += Time.deltaTime) {
            if (t > 4f) Keys();
            maxY = Mathf.Max(maxY, brain.transform.position.y); sawFall |= Anim() == "Fall"; sawLand |= Anim() == "Land"; yield return null;
        }
        Check("D04 climbs steps onto platform", maxY > 2.9f, $"max height {maxY:0.00} m");
        Check("D04 drops off platform and lands Grounded", brain.CurrentState == PlayerBrain.StateMode.Grounded && brain.transform.position.y < .2f,
              $"state {brain.CurrentState}, y {brain.transform.position.y:0.00}");
        Check("D09 Fall then Land played after drop", sawFall && sawLand, $"Fall {sawFall}, Land {sawLand}");
    }

    IEnumerator D04Wall() {
        yield return ResetDemo();
        motor.ResetAt(new Vector3(-5, .05f, 10));
        MouseDelta(new Vector2(-750, 0)); yield return Wait(.6f); // face -X toward Corridor Right wall (x -7.75)
        Keys(Key.W, Key.LeftShift); yield return Wait(1.5f); Keys();
        float x = brain.transform.position.x;
        Check("D04 does not pass through wall", x > -7.75f + .25f, $"player x {x:0.00}, wall face -7.75");
        Check("D09 Speed uses real velocity against wall", animator.GetFloat("Speed") < 1 || motor.Speed < 1, $"motor {motor.Speed:0.00}");
    }

    IEnumerator D07Occlusion() {
        yield return ResetDemo();
        motor.ResetAt(new Vector3(-10.5f, .05f, 10));
        MouseDelta(new Vector2(750, 0)); yield return Wait(1.2f); // look +X: raw TPS position lands behind Corridor Left (x -12)
        Shot("12-corridor-deoccluder");
        Vector3 raw = cam.tps.transform.position, live = Camera.main.transform.position, focus = brain.transform.position + Vector3.up * 1.55f;
        bool rawBlocked = Physics.Linecast(focus, raw, 1, QueryTriggerInteraction.Ignore);
        bool liveInside = Physics.CheckSphere(live, .05f, 1, QueryTriggerInteraction.Ignore);
        bool liveBlocked = Physics.Linecast(live, focus, 1, QueryTriggerInteraction.Ignore);
        Check("D07 test setup puts raw TPS camera behind wall", rawBlocked, $"raw {V(raw)}");
        Check("D07 Deoccluder keeps camera out of wall with clear view", !liveInside && !liveBlocked, $"live {V(live)}, inside {liveInside}, blocked {liveBlocked}");
    }

    IEnumerator D06Wide() {
        yield return ResetDemo();
        Vector3 p = brain.transform.position;
        Keys(Key.C); yield return null; yield return null;
        bool blending = false;
        for (int i = 0; i < 10; i++) { blending |= cmBrain.IsBlending; yield return null; }
        Keys(); yield return Wait(1f);
        Shot("10-wide");
        Check("D06 C switches to wide angle with blend", cam.ModeWide && cam.topDown.Priority > cam.tps.Priority && blending, $"wide {cam.ModeWide}, blending {blending}");
        Check("D06 camera switch keeps player position", Vector3.Distance(p, brain.transform.position) < .01f, $"moved {Vector3.Distance(p, brain.transform.position):0.000}");
        Check("D06 HUD shows camera mode", hud.status.text.Contains("WIDE"), "status contains WIDE");
        yield return Tap(Key.C); yield return Wait(.8f);
        Check("D06 C returns to TPS", !cam.ModeWide && cam.tps.Priority > cam.topDown.Priority, $"wide {cam.ModeWide}");
    }

    IEnumerator D10Hit() {
        yield return ResetDemo();
        yield return Tap(Key.H);
        bool hit = brain.CurrentState == PlayerBrain.StateMode.Hit;
        Check("D10 H grounded: -25 HP and Hit", hit && health.Current == 75, $"state {brain.CurrentState}, HP {health.Current}");
        Vector3 p = brain.transform.position; float enter = Time.time;
        Keys(Key.W, Key.Space); yield return Wait(.3f);
        Check("D10 Hit locks Move/Jump", Flat(brain.transform.position - p).magnitude < .02f && motor.VerticalVelocity <= 0, $"moved {Flat(brain.transform.position - p).magnitude:0.000}");
        Shot("07-hit");
        Check("D09 Hit clip plays", Anim() == "Hit", $"anim {Anim()}");
        Check("D10 HUD shows Hit block reason", hud.status.text.Contains("Hit stun"), "status contains 'Hit stun'");
        Keys();
        while (brain.CurrentState == PlayerBrain.StateMode.Hit && Time.time - enter < 2) yield return null;
        float stun = Time.time - enter;
        Check("D10 Hit recovers to Grounded in 0.6-0.8s", brain.CurrentState == PlayerBrain.StateMode.Grounded && stun > .55f && stun < .85f, $"stun {stun:0.00}s, state {brain.CurrentState}");
        yield return Wait(.3f);
        Check("D09 Hit animation returns to Locomotion", Anim() == "Locomotion", $"anim {Anim()}");
        p = brain.transform.position; Keys(Key.W); yield return Wait(.4f); Keys();
        Check("D10 control restored after Hit", Flat(brain.transform.position - p).magnitude > .5f, "moves again");

        yield return Tap(Key.Space); yield return Wait(.12f);
        yield return Tap(Key.H);
        Check("D10 H in air: no damage, reason shown", health.Current == 75 && hud.status.text.Contains("H only works when Grounded"), $"HP {health.Current}");
        yield return Wait(1.2f);

        for (int i = 0; i < 3; i++) { yield return Tap(Key.H); yield return Wait(.9f); }
        Check("D10 fourth H (HP<=0) goes straight to Dead", brain.CurrentState == PlayerBrain.StateMode.Dead && health.Current == 0 && brain.History.Last().EndsWith("Grounded → Dead"),
              $"state {brain.CurrentState}, HP {health.Current}, last '{brain.History.Last()}'");
    }

    IEnumerator D10HitPriority() {
        yield return ResetDemo();
        yield return Tap(Key.H, Key.Space);
        yield return Wait(.1f);
        Check("D08 H+Space same frame: Hit wins, Jump cancelled", brain.CurrentState == PlayerBrain.StateMode.Hit && motor.VerticalVelocity <= 0, $"state {brain.CurrentState}, vy {motor.VerticalVelocity:0.0}");
        yield return ResetDemo();
        yield return Tap(Key.K, Key.Space);
        yield return Wait(.1f);
        Check("D08 K+Space same frame: Dead wins", brain.CurrentState == PlayerBrain.StateMode.Dead && motor.VerticalVelocity <= 0, $"state {brain.CurrentState}, vy {motor.VerticalVelocity:0.0}");
    }

    IEnumerator D11Dead() {
        yield return ResetDemo();
        Keys(Key.C); yield return null; yield return null; Keys(); yield return Wait(.2f); // leave wide mode on to prove R restores TPS
        yield return Tap(Key.K);
        Check("D11 K: HP 0 and Dead", brain.CurrentState == PlayerBrain.StateMode.Dead && health.Current == 0, $"state {brain.CurrentState}, HP {health.Current}");
        Vector3 p = brain.transform.position;
        Keys(Key.W, Key.LeftShift, Key.Space); yield return Wait(1f); Keys();
        Check("D08 Dead blocks Move/Sprint/Jump", Flat(brain.transform.position - p).magnitude < .02f && motor.VerticalVelocity <= 0, $"moved {Flat(brain.transform.position - p).magnitude:0.000}");
        Check("D12 HUD shows Dead block reason", hud.status.text.Contains("Dead blocks all input"), "status contains reason");
        var info = animator.GetCurrentAnimatorStateInfo(0);
        Check("D09 Dead clip plays once and holds", info.IsName("Dead") && info.normalizedTime > 1 && !animator.IsInTransition(0), $"anim {Anim()}, t {info.normalizedTime:0.00}");
        yield return Tap(Key.H);
        Check("D11 H while Dead ignored", health.Current == 0 && brain.CurrentState == PlayerBrain.StateMode.Dead, $"HP {health.Current}");
        yield return ResetDemo();
        float d = Vector3.Distance(Flat(brain.transform.position), Flat(spawn.position));
        Check("D11 R restores HP 100, Grounded, spawn, TPS", health.Current == 100 && brain.CurrentState == PlayerBrain.StateMode.Grounded && d < .2f && !cam.ModeWide,
              $"HP {health.Current}, state {brain.CurrentState}, dist {d:0.00}, wide {cam.ModeWide}");
        Check("D11 R resets Animator and history", Anim() == "Locomotion" && brain.History.Count == 1, $"anim {Anim()}, history {string.Join(" | ", brain.History)}");
        p = brain.transform.position; Keys(Key.W); yield return Wait(.4f); Keys();
        Check("D11 control works again after R", Flat(brain.transform.position - p).magnitude > .5f, "moves after reset");
    }

    IEnumerator D12Hud() {
        yield return ResetDemo();
        for (int i = 0; i < 4; i++) { yield return Tap(Key.Space); yield return Wait(.9f); }
        string s = hud.status.text;
        Check("D12 HUD shows State/HP/Speed/Grounded/Vertical", new[] { "State:", "HP ", "Speed:", "Grounded:", "Vertical:" }.All(s.Contains), s.Replace("\n", " | "));
        Check("D12 history keeps last 5 transitions", brain.History.Count == 5 && hud.history.text.Split('\n').Length == 6, string.Join(" | ", brain.History));
        Check("D12 help text lists spec keys", hud.help.text.Contains("H  -25 HP") && hud.help.text.Contains("R  RESET"), hud.help.text.Replace("\n", " | "));
        yield return Tap(Key.F1);
        bool hidden = !hud.status.gameObject.activeSelf;
        yield return Tap(Key.F1);
        Check("D12 F1 toggles HUD", hidden && hud.status.gameObject.activeSelf, $"hidden after F1 {hidden}");
    }
}
}
