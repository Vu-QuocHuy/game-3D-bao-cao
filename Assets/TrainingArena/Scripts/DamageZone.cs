using System.Collections.Generic;
using UnityEngine;
namespace TrainingArena {
public sealed class DamageZone : MonoBehaviour {
 public float damage=25,interval=1;readonly Dictionary<Health,float> nextHit=new Dictionary<Health,float>();
 void OnTriggerStay(Collider other){var health=other.GetComponentInParent<Health>();if(!health||!health.GetComponent<PlayerBrain>()||!health.GetComponent<PlayerBrain>().topic5)return;if(nextHit.TryGetValue(health,out float t)&&Time.time<t)return;nextHit[health]=Time.time+interval;health.Damage(damage);}
 void OnTriggerExit(Collider other){var h=other.GetComponentInParent<Health>();if(h)nextHit.Remove(h);}
 public void Clear()=>nextHit.Clear();
}}
