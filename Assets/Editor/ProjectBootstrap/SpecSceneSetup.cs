using System.Linq;
using TMPro;
using TrainingArena;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace ProjectBootstrap {
// Brings both demo scenes in line with the shared spec (zones A/B/C/D, one Animator on the player,
// no attack dummy / damage trap / fixed-camera zone) without regenerating them. Safe to run repeatedly.
public static class SpecSceneSetup {
 static readonly string[] Scenes={"Assets/TrainingArena/Scenes/TrainingArena.unity","Assets/TrainingArena/Scenes/TrainingArena_T3.unity"};
 static readonly string[] Remove={"Fixed Camera Zone","Fixed Camera Anchor","CM FPS","CM Fixed Zone","06  CAMERA BLEND"};
 static readonly (string from,string text)[] Labels={("01  MOVEMENT + JUMP","B  JUMP + STEPS"),("02  CAMERA COLLISION","C  CAMERA CORRIDOR"),("03  CROUCH","CROUCH  (CTRL)"),("05  HIT / DEAD","D  STATE TEST  H / K"),("04  ATTACK","ATTACK  (LMB)")};

 [MenuItem("Training Arena/Apply Spec Scene Setup")]
 public static void Apply(){
  AnimatorWiring.WireMenu();
  foreach(var path in Scenes){
   var scene=EditorSceneManager.OpenScene(path);
   var all=scene.GetRootGameObjects();
   GameObject Find(string n)=>all.FirstOrDefault(g=>g&&g.name==n);

   // Cloned TMP labels keep their RectTransform anchoredPosition; set it explicitly or the label stays at the source spot.
   void Place(string n,Vector3 p){var g=GameObject.Find(n);if(g)((RectTransform)g.transform).anchoredPosition3D=p;}
   var player=Find("Player");
   // A second Animator on the player root fights the robot's own Animator and freezes the pose.
   var rootAnimator=player.GetComponent<Animator>();if(rootAnimator)Object.DestroyImmediate(rootAnimator);

   foreach(var n in Remove){var g=Find(n);if(g)Object.DestroyImmediate(g);}
   var zoneD=Find("Damage Zone");
   if(zoneD){
    Object.DestroyImmediate(zoneD.GetComponent<DamageZone>());Object.DestroyImmediate(zoneD.GetComponent<Rigidbody>());
    foreach(var c in zoneD.GetComponents<BoxCollider>())Object.DestroyImmediate(c);
    // Spec zone D is flat floor with a contrasting colour, not a step: flatten the old trap pad into a marker.
    zoneD.transform.position=new Vector3(11,.01f,-4);zoneD.transform.localScale=new Vector3(4,.02f,4);
    zoneD.name="Zone D Floor";
   }
   foreach(var(from,text)in Labels){var g=Find(from);if(!g)continue;g.name=text;g.GetComponent<TextMeshPro>().text=text;}
   if(!Find("A  START")){var src=all.First(g=>g&&g.name=="B  JUMP + STEPS");var a=Object.Instantiate(src);a.name="A  START";a.GetComponent<TextMeshPro>().text="A  START";}
   Place("A  START",new Vector3(0,.025f,-12.5f));

   // Attack target. Generate Demo Scene already makes one; scenes cleaned by an older version of this tool lost it.
   if(!Find("Training Dummy")){
    var dummy=new GameObject("Training Dummy");dummy.transform.position=new Vector3(5,0,-4);dummy.AddComponent<Health>();
    var body=GameObject.CreatePrimitive(PrimitiveType.Cube);body.name="Body";body.transform.SetParent(dummy.transform,false);body.transform.localPosition=new Vector3(0,1,0);body.transform.localScale=new Vector3(.9f,1.8f,.7f);
    body.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/TrainingArena/Art/Materials/Amber.mat");
    var label=Object.Instantiate(all.First(g=>g&&g.name=="HUMANOID SOURCE"));label.name="Dummy Label";
    var feedback=dummy.AddComponent<DummyFeedback>();feedback.body=body.GetComponent<Renderer>();feedback.label=label.GetComponent<TextMeshPro>();feedback.label.text="TRAINING DUMMY\n100 HP";feedback.label.color=Color.white;
   }
   if(!Find("ATTACK  (LMB)")){var a=Object.Instantiate(GameObject.Find("A  START"));a.name="ATTACK  (LMB)";a.GetComponent<TextMeshPro>().text="ATTACK  (LMB)";}
   Place("ATTACK  (LMB)",new Vector3(5,.025f,-7));Place("Dummy Label",new Vector3(5,2.4f,-4));
   var bridge=player.GetComponentInChildren<PlayerAnimator>();bridge.combat=player.GetComponent<PlayerCombat>();EditorUtility.SetDirty(bridge);

   var demo=Object.FindAnyObjectByType<DemoController>();demo.overlay=Object.FindAnyObjectByType<DebugOverlay>();EditorUtility.SetDirty(demo);
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
   Debug.Log("SPEC_SCENE_APPLIED: "+path);
  }
 }
 public static void ApplyBatch(){Apply();EditorApplication.Exit(0);}
}}
