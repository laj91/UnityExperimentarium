using UnityEngine;

public class CubeCollisionHandler : MonoBehaviour
{
    private CubeWallGenerator wallGenerator;
    private RandomSoundPlayer soundPlayer;
    void Start()
    {
        // Find CubeWallGenerator komponenten p� parent objektet
        wallGenerator = GetComponentInParent<CubeWallGenerator>();
        soundPlayer = GetComponentInParent<RandomSoundPlayer>();
        if (wallGenerator == null)
        {
            Debug.LogError("CubeCollisionHandler kunne ikke finde CubeWallGenerator p� parent objekt!", this);
        }
        if (soundPlayer == null)
        {
            Debug.LogError("soundPlayer kunne ikke finde CubeWallGenerator p� parent objekt!", this);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Tjek om det kollisionsobjekt har tagget "RagdollBullet"
        if (collision.gameObject.CompareTag("RagdollBullet"))
        {
            Debug.Log("RagdollBullet ramte cube: " + gameObject.name);
            
            // Aktiver fysik p� alle cubes
            if (wallGenerator != null)
            {
                wallGenerator.ActivatePhysics();
            }
        }

        // Spil en tilf�ldig lyd ved kollision
        if (soundPlayer != null)
        {
            soundPlayer.PlayRandomSound();
        }
    }
}