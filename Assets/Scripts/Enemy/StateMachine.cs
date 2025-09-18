using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public BaseState activeState;
   
    public void Inisialize()
    {
        
        ChangeState(new PatrolState());
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    void Update()
    {
        if (activeState != null)
        {
            Debug.Log($"StateMachine Update: Active state is {activeState.GetType().Name}");
            activeState.Perform();
        }
        else
        {
            Debug.Log("StateMachine Update: No active state.");
        }
    }

    public void ChangeState(BaseState newState)
    {
        Debug.Log($"StateMachine ChangeState: Changing from {activeState?.GetType().Name ?? "null"} to {newState?.GetType().Name ?? "null"}");

        //check activeState != null
        if (activeState != null)
        {
            Debug.Log($"StateMachine ChangeState: Exiting state {activeState.GetType().Name}");
            //run cleanup on activeState.
            activeState.Exit();
        }

        //change to a new state.
        activeState = newState;

        //fail-safe null check to make sure new state wasn't null
        if (activeState != null)
        {
            Debug.Log($"StateMachine ChangeState: Entering state {activeState.GetType().Name}");
            //Setup new state.
            activeState.stateMachine = this;
            activeState.enemy = GetComponent<Enemy>();
            //assign state enemy class.
            activeState.Enter();
        }
        else
        {
            Debug.Log("StateMachine ChangeState: New state is null.");
        }
    }

}
