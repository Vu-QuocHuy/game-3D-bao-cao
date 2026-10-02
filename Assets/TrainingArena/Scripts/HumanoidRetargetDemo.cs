using UnityEngine;
namespace TrainingArena {
// Both bodies share the same muscle clips and controller, but use different Avatars.
public sealed class HumanoidRetargetDemo : MonoBehaviour {
 public Animator animator;
 void Update(){animator.SetFloat("Speed",(Mathf.Sin(Time.time*.6f)+1)*3);}
}}
