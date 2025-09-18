using UnityEngine;

public class RemySpawner : MonoBehaviour
{
    public GameObject objectToSpawn;   // Objektet der skal instantiere
    public Transform spawnLocation;   // Det tomme GameObject bruges som position

    public void Spawn()
    {
        Instantiate(objectToSpawn, spawnLocation.position, spawnLocation.rotation);
    }

}
