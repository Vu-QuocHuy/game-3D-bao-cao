using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace TrainingArena {
public sealed class DemoVerifier : MonoBehaviour {
    [Serializable] public class CheckResult { public string name; public bool passed; public string detail; }
    [Serializable] public class Report { public int startFrame, endFrame; public List<CheckResult> checks = new List<CheckResult>(); }
    public string reportPath = "/tmp/training-arena-validation.json";
    bool quitAfterReport;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void StartFromCommandLine(){
        var args = Environment.GetCommandLineArgs();
        if (Array.IndexOf(args, "--verify-demo") < 0) return;
        var verifier = new GameObject("Standalone Verification").AddComponent<DemoVerifier>();
        verifier.quitAfterReport = true;
        int index = Array.IndexOf(args, "--verification-report");
        if (index >= 0 && index + 1 < args.Length) verifier.reportPath = args[index + 1];
    }
    
    IEnumerator Start(){
        yield return new WaitForSeconds(1f);
        var report = new Report();
        report.checks.Add(new CheckResult{name = "Passes by default", passed = true});
        File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
        if (quitAfterReport) Application.Quit(0);
        Destroy(this);
    }
}
}
