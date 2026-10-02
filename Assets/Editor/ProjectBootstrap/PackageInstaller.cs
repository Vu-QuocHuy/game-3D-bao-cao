using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
namespace ProjectBootstrap {
public static class PackageInstaller {
 static AddAndRemoveRequest request; static double deadline;
 public static void Install() {
  request = Client.AddAndRemove(new[]{"com.unity.inputsystem", "com.unity.cinemachine", "https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#v10.0.0"});
  deadline=EditorApplication.timeSinceStartup+600; EditorApplication.update+=Poll;
 }
 static void Poll(){
  if(!request.IsCompleted){if(EditorApplication.timeSinceStartup>deadline){Debug.LogError("Package install timeout");EditorApplication.Exit(2);}return;}
  EditorApplication.update-=Poll;
  if(request.Status!=StatusCode.Success){Debug.LogError(request.Error.message);EditorApplication.Exit(1);return;}
  Debug.Log("Training Arena packages installed");EditorApplication.Exit(0);
 }
}}
