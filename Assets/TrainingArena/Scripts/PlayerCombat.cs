using System.Collections.Generic;
using UnityEngine;
namespace TrainingArena {
public sealed class PlayerCombat : MonoBehaviour {
 PlayerBrain brain;readonly HashSet<Health> hits=new HashSet<Health>();bool windowOpen;
 public bool WindowOpen=>windowOpen;public int HitCount=>hits.Count;
 void Awake()=>brain=GetComponent<PlayerBrain>();
 public void SetWindow(bool value){if(value&&!windowOpen)hits.Clear();windowOpen=value;}
 void Update(){if(!brain.Attacking||brain.Priority!="Normal"){windowOpen=false;return;}if(!windowOpen)return;foreach(var c in Physics.OverlapSphere(transform.position+Vector3.up+transform.forward*.9f,.65f)){var target=c.GetComponentInParent<Health>();if(target&&target.gameObject!=gameObject&&hits.Add(target))target.Damage(25);}}
 void OnDrawGizmosSelected(){Gizmos.color=Color.red;Gizmos.DrawWireSphere(transform.position+Vector3.up+transform.forward*.9f,.65f);}
}
}
