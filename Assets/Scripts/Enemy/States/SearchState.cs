using UnityEngine;

public class SearchState : BaseState
{
    private float searchTimer;
    private float moveTimer;

    public override void Enter()
    {
        enemy.Agent.SetDestination(enemy.LastKnownPos);
        // Fjenden bev�ger sig mod spillerens sidste kendte position.
    }

    public override void Perform()
    {
        if (enemy.CanSeePlayer())
            stateMachine.ChangeState(new AttackState()); // Skifter til AttackState, hvis fjenden kan se spilleren.

       
        if (enemy.Agent.remainingDistance < Mathf.Max(enemy.Agent.stoppingDistance, 0.1f))
        {
            searchTimer += Time.deltaTime; // Incrementerer s�getimeren, hvis fjenden er t�t p� m�let.
          
            moveTimer += Time.deltaTime;
            if (moveTimer > Random.Range(3, 5))
            {
                enemy.Agent.SetDestination(enemy.transform.position + (Random.insideUnitSphere * 10));
                moveTimer = 0;
            }
            if (searchTimer > 10)
            {
                stateMachine.ChangeState(new PatrolState()); // Skifter til PatrolState efter 10 sekunder.
            }
        }
    }


    public override void Exit()
    {
        
    }

    
}
