using System;
namespace TrainingArena {
 public interface IState { string Name {get;} void Enter(); void Tick(); void Exit(); }
 public sealed class StateMachine {
  public IState Current {get;private set;} public event Action<string,string> Changed;
  public void Change(IState next){if(next==null||next==Current)return;string old=Current?.Name??"None";Current?.Exit();Current=next;Current.Enter();Changed?.Invoke(old,next.Name);}
  public void Tick()=>Current?.Tick();
 }
 public sealed class PlayerState : IState {
  public string Name {get;} readonly Action enter,tick,exit;
  public PlayerState(string name,Action enter=null,Action tick=null,Action exit=null){Name=name;this.enter=enter;this.tick=tick;this.exit=exit;}
  public void Enter()=>enter?.Invoke();public void Tick()=>tick?.Invoke();public void Exit()=>exit?.Invoke();
 }
}
