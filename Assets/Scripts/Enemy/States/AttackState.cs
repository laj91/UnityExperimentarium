using UnityEngine;

public class AttackState : BaseState
{
    float losePlayerTimer;
    float moveTimer;
    float shotTimer;
    public override void Enter()
    {
        
    }

    public override void Exit()
    {
        
    }

    public override void Perform()
    {
        if (enemy.CanSeePlayer())
        {
            losePlayerTimer = 0;
            moveTimer += Time.deltaTime;
            shotTimer += Time.deltaTime;
            enemy.transform.LookAt(enemy.Player.transform);

            if(shotTimer > enemy.fireRate)
            {
                Shoot();
            }
            if (moveTimer > Random.Range(3, 7))
            {
                enemy.Agent.SetDestination(enemy.transform.position + (Random.insideUnitSphere * 5));
                moveTimer = 0;
            }
            enemy.LastKnownPos = enemy.Player.transform.position;
        }
        else
        {
            losePlayerTimer += Time.deltaTime;
            if (losePlayerTimer > 8)
            {
                // Change to the search state.
                stateMachine.ChangeState(new SearchState());
            }
        }
    }

    public void Shoot()
    {
        //store reference to the gun barrel.
        Transform gunbarrel = enemy.gunBarrel;

        //instantiate a new bullet.
        GameObject bullet = GameObject.Instantiate(Resources.Load("Prefabs/Bullet") as GameObject, gunbarrel.position, enemy.transform.rotation);
        Debug.Log($"Bullet position: {bullet.transform.position}");

        if (bullet == null)
        {
            Debug.LogError("Bullet prefab could not be loaded!");
        }


        //calculate the direction to the player.
        Vector3 shootDirection = (enemy.Player.transform.position - gunbarrel.transform.position).normalized;

        //add force rigidbody of the bullet.
        //Desto højere tal i random.range, destor lavere akurat
        bullet.GetComponent<Rigidbody>().linearVelocity = Quaternion.AngleAxis(Random.Range(-3f, 3f), Vector3.up) * shootDirection * 40;

        Debug.Log($"Bullet velocity: {bullet.GetComponent<Rigidbody>().linearVelocity}");

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Bullet Rigidbody is missing!");
        }
;

        Debug.Log("Shoot");
        shotTimer = 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
