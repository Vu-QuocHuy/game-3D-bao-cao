using System;
using System.Collections.Generic;
using UnityEngine;
namespace TrainingArena {
// Upper-body action running alongside the main FSM (not a fifth state). C# owns timing, cooldown and cancel;
// the Attack clip's Animation Events (OpenHitbox/CloseHitbox) own when the hitbox is live.
public sealed class PlayerCombat : MonoBehaviour {
    public float damage = 25, reach = 1.1f, radius = .6f, duration = .85f, cooldown = 1f;
    public bool Attacking => Time.time < attackEnd;
    public bool WindowOpen { get; private set; }
    public int HitCount { get; private set; }
    public event Action Started, Cancelled;

    readonly HashSet<Health> hitThisSwing = new HashSet<Health>();
    float attackEnd, nextAttack;
    Health self;

    void Awake() => self = GetComponent<Health>();

    public bool TryStart() {
        if (Time.time < nextAttack) return false;
        attackEnd = Time.time + duration;
        nextAttack = Time.time + cooldown;
        hitThisSwing.Clear();
        WindowOpen = false;
        Started?.Invoke();
        return true;
    }

    public void SetWindow(bool open) => WindowOpen = open && Attacking;

    public void Cancel() {
        bool was = Attacking;
        attackEnd = 0;
        WindowOpen = false;
        if (was) Cancelled?.Invoke();
    }

    void Update() {
        if (!Attacking) WindowOpen = false;
        if (!WindowOpen) return;
        Vector3 center = transform.position + Vector3.up + transform.forward * reach;
        foreach (var c in Physics.OverlapSphere(center, radius, ~0, QueryTriggerInteraction.Ignore)) {
            var target = c.GetComponentInParent<Health>();
            if (!target || target == self || !hitThisSwing.Add(target)) continue; // one damage per target per swing
            target.Damage(damage);
            HitCount++;
        }
    }
}
}
