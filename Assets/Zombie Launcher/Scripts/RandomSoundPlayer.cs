// 30-08-2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System.Collections;
using UnityEditor;
using UnityEngine;

public class RandomSoundPlayer : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip stoneSoundClip; // Dit 5-minutters lydklip
    [SerializeField] private AudioClip[] audioClips; // Array af lydklip

    [Header("Settings")]
    [SerializeField] private int maxSimultaneousSounds = 10; // Maks antal lyde der kan afspilles samtidig
    [SerializeField] private float minClipLength = 1f; // Minimum længde af en lyd-bid
    [SerializeField] private float maxClipLength = 2f; // Maksimum længde af en lyd-bid
    [Space]
    [SerializeField] private bool isUsingMultipleClips = false; // Skift mellem at bruge et enkelt klip eller flere

    private int activeSounds = 0; // Antal aktive lyde
    private AudioSource audioSource;

    void Start()
    {
        // Tilføj en AudioSource til objektet
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = stoneSoundClip;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = 100f;
    }

    public void PlayRandomSound()
    {
        if (!isUsingMultipleClips)
        {
            if (activeSounds >= maxSimultaneousSounds || stoneSoundClip == null)
                return;
            PlaySoundUsingOneClip();
        }
        else
        {
            if (activeSounds >= maxSimultaneousSounds || audioClips.Length == 0)
                return;
            PlaySoundUsingMultipleClips();

        }
    }

    private void PlaySoundUsingMultipleClips()
    {
        // Vælg et tilfældigt klip fra arrayet
        AudioClip selectedClip = audioClips[Random.Range(0, audioClips.Length)];
        if (selectedClip == null)
            return;
        // Vælg en tilfældig starttid i det valgte klip
        float randomStartTime = Random.Range(0, selectedClip.length - maxClipLength);
        float randomDuration = Random.Range(minClipLength, maxClipLength);
        // Brug en midlertidig AudioSource til at afspille det valgte klip
        AudioSource tempAudioSource = gameObject.AddComponent<AudioSource>();
        tempAudioSource.clip = selectedClip;
        tempAudioSource.time = randomStartTime;
        tempAudioSource.loop = false;
        tempAudioSource.playOnAwake = false;
        tempAudioSource.Play();
        activeSounds++;
        StartCoroutine(StopTempAudioSource(tempAudioSource, randomDuration));
    }

    private void PlaySoundUsingOneClip()
    {
        // Vælg en tilfældig starttid i lydklippet
        float randomStartTime = Random.Range(0, stoneSoundClip.length - maxClipLength);
        float randomDuration = Random.Range(minClipLength, maxClipLength);

        // Start en coroutine for at afspille lyden
        StartCoroutine(PlaySoundCoroutine(randomStartTime, randomDuration));
    }

    private IEnumerator StopTempAudioSource(AudioSource tempAudioSource, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (tempAudioSource != null)
        {
            tempAudioSource.Stop();
            Destroy(tempAudioSource);
            activeSounds--;
        }
    }

    private IEnumerator PlaySoundCoroutine(float startTime, float duration)
    {
        activeSounds++;

        // Indstil starttidspunktet
        audioSource.time = startTime;
        audioSource.Play();

        // Vent på den tilfældige varighed
        yield return new WaitForSeconds(duration);

        // Stop lyden og reducer antallet af aktive lyde
        audioSource.Stop();
        activeSounds--;
    }
}
