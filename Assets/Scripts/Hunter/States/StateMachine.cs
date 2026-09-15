using System;
using System.Collections.Generic;
using UnityEngine;




public class StateMachine
{
    public State currentState {  get; private set; }

    private Dictionary<Enum, State> states = new Dictionary<Enum, State>();

    public void RegisterState(Enum Key, State state )
    {
        states[Key] = state;
    }

    public void ChangeState(Enum key)
    {
        State newState = states[key];

        if (newState == currentState)
            return;
            
        

        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void Update()
    {
        currentState?.Update();
    }
}
