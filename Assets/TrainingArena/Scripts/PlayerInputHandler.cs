using UnityEngine;
using UnityEngine.InputSystem;
namespace TrainingArena {
public sealed class PlayerInputHandler : MonoBehaviour {
 public InputActionAsset controls;InputActionAsset runtimeControls;
 public Vector2 Move {get;private set;} public Vector2 Look {get;private set;}
 public bool SprintHeld {get;private set;} public bool CrouchHeld {get;private set;}
 public bool JumpPressed {get;private set;} public bool AttackPressed {get;private set;}
 InputAction move,look,jump,sprint,crouch,attack;
 // Set only by DemoVerifier: -batchmode players cannot lock the cursor, so Look would always read zero.
 internal static bool IgnoreCursorLock;
 void Awake(){
  if(controls){runtimeControls=Instantiate(controls);move=runtimeControls.FindAction("Move",true);look=runtimeControls.FindAction("Look",true);jump=runtimeControls.FindAction("Jump",true);sprint=runtimeControls.FindAction("Sprint",true);crouch=runtimeControls.FindAction("Crouch",true);attack=runtimeControls.FindAction("Attack",true);return;}
  move=new InputAction("Move",InputActionType.Value);move.AddCompositeBinding("2DVector").With("Up","<Keyboard>/w").With("Down","<Keyboard>/s").With("Left","<Keyboard>/a").With("Right","<Keyboard>/d");move.AddBinding("<Gamepad>/leftStick");
  look=new InputAction("Look",InputActionType.Value,"<Mouse>/delta");jump=new InputAction("Jump",InputActionType.Button,"<Keyboard>/space");jump.AddBinding("<Gamepad>/buttonSouth");
  sprint=new InputAction("Sprint",InputActionType.Button,"<Keyboard>/leftShift");crouch=new InputAction("Crouch",InputActionType.Button,"<Keyboard>/leftCtrl");attack=new InputAction("Attack",InputActionType.Button,"<Mouse>/leftButton");
 }
 void OnEnable(){foreach(var a in new[]{move,look,jump,sprint,crouch,attack})a?.Enable();}
 void OnDisable(){foreach(var a in new[]{move,look,jump,sprint,crouch,attack})a?.Disable();Clear();}
 public void Sample(){Move=Vector2.ClampMagnitude(move.ReadValue<Vector2>(),1);Look=Cursor.lockState==CursorLockMode.Locked||IgnoreCursorLock?look.ReadValue<Vector2>():Vector2.zero;SprintHeld=sprint.IsPressed();CrouchHeld=crouch.IsPressed();JumpPressed=jump.WasPressedThisFrame();AttackPressed=(Cursor.lockState==CursorLockMode.Locked||IgnoreCursorLock)&&attack.WasPressedThisFrame();}
 public void Clear(){Move=Look=Vector2.zero;SprintHeld=CrouchHeld=JumpPressed=AttackPressed=false;}
 void OnDestroy(){foreach(var a in new[]{move,look,jump,sprint,crouch,attack})a?.Dispose();if(runtimeControls)Destroy(runtimeControls);}
}}
