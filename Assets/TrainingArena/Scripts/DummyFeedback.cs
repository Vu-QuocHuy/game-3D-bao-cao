using UnityEngine;
using TMPro;
namespace TrainingArena {
public sealed class DummyFeedback : MonoBehaviour {
 public TMP_Text label;public Renderer body;Health health;float flash;
 void Awake(){health=GetComponent<Health>();}
 void OnEnable(){health.Damaged+=Flash;health.Died+=Flash;}
 void OnDisable(){health.Damaged-=Flash;health.Died-=Flash;}
 void Flash()=>flash=.2f;
 void Update(){flash=Mathf.Max(0,flash-Time.deltaTime);label.text=health.IsDead?"DUMMY DOWN\nBACKSPACE: RESET":$"TRAINING DUMMY\n{health.Current:0} HP";body.material.color=flash>0?Color.white:health.IsDead?Color.gray:new Color(.98f,.64f,.12f);}
}}
