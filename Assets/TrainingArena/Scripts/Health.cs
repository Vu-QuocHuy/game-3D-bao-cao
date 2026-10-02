using System;
using UnityEngine;
namespace TrainingArena {
public sealed class Health : MonoBehaviour {
 public float maxHealth=100;public float Current {get;private set;} public bool IsDead=>Current<=0;
 public event Action Damaged,Died;
 void Awake()=>Restore();
 public void Restore()=>Current=maxHealth;
 public void Damage(float amount){if(IsDead||amount<=0)return;Current=Mathf.Max(0,Current-amount);if(IsDead)Died?.Invoke();else Damaged?.Invoke();}
}}
