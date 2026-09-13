using UnityEngine;

public abstract class State 
{
    protected StateMachine stateMachine;
    protected State(StateMachine _stateMachine)
    {
        stateMachine = _stateMachine;
    }
   public virtual void Enter() { }
   public virtual void Update() { }
   public virtual void Exit() { }
}
