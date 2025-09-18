using UnityEngine;

public class CubeCollisionHandler : MonoBehaviour
{
    private CubeWallGenerator wallGenerator;
    private RandomSoundPlayer soundPlayer;
    void Start()
    {
        // Find CubeWallGenerator komponenten på parent objektet
        wallGenerator = GetComponentInParent<CubeWallGenerator>();
        soundPlayer = GetComponentInParent<RandomSoundPlayer>();
        if (wallGenerator == null)
        {
            Debug.LogError("CubeCollisionHandler kunne ikke finde CubeWallGenerator på parent objekt!", this);
        }
        if (soundPlayer == null)
        {
            Debug.LogError("soundPlayer kunne ikke finde CubeWallGenerator på parent objekt!", this);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Tjek om det kollisionsobjekt har tagget "RagdollBullet"
        if (collision.gameObject.CompareTag("RagdollBullet"))
        {
            Debug.Log("RagdollBullet ramte cube: " + gameObject.name);
            
            // Aktiver fysik på alle cubes
            if (wallGenerator != null)
            {
                wallGenerator.ActivatePhysics();
            }
        }

        // Spil en tilfældig lyd ved kollision
        if (soundPlayer != null)
        {
            soundPlayer.PlayRandomSound();
        }
    }
}