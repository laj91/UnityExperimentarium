using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip craneMovement;
    [SerializeField] private AudioClip launcherPower;
    [SerializeField] private AudioClip launcherFire;
    [SerializeField] private AudioClip[] ambience;

    [Header("Mix")]
    [SerializeField] private float craneVolume = 0.8f;
    [SerializeField] private float chargeVolume = 0.9f;
    [SerializeField] private float sfxVolume = 1.0f;
    [SerializeField] private float ambienceVolume = 0.5f;

    private AudioSource craneSource;
    private AudioSource chargeSource;
    private AudioSource sfxSource;
    private AudioSource ambienceSource;

    private void Awake()
    {
        craneSource = gameObject.AddComponent<AudioSource>();
        craneSource.playOnAwake = false;
        craneSource.loop = true;
        craneSource.spatialBlend = 0f;

        chargeSource = gameObject.AddComponent<AudioSource>();
        chargeSource.playOnAwake = false;
        chargeSource.loop = true;
        chargeSource.spatialBlend = 0f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;

        ambienceSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.playOnAwake = false;
        ambienceSource.loop = false; // vi looper manuelt gennem playlisten
        ambienceSource.spatialBlend = 0f;
        ambienceSource.volume = ambienceVolume;
    }

    private void OnEnable()
    {
        AudioEvents.CraneMove += OnCraneMove;
        AudioEvents.LaunchChargeStarted += OnLaunchChargeStarted;
        AudioEvents.LaunchChargeProgress += OnLaunchChargeProgress;
        AudioEvents.LaunchChargeMaxed += OnLaunchChargeStopped;
        AudioEvents.LaunchChargeEnded += OnLaunchChargeStopped;
        AudioEvents.LaunchFired += OnLaunchFired;
    }

    private void OnDisable()
    {
        AudioEvents.CraneMove -= OnCraneMove;
        AudioEvents.LaunchChargeStarted -= OnLaunchChargeStarted;
        AudioEvents.LaunchChargeProgress -= OnLaunchChargeProgress;
        AudioEvents.LaunchChargeMaxed -= OnLaunchChargeStopped;
        AudioEvents.LaunchChargeEnded -= OnLaunchChargeStopped;
        AudioEvents.LaunchFired -= OnLaunchFired;
    }

    private void Start()
    {
        if (ambience != null && ambience.Length > 0)
            StartCoroutine(PlayAmbienceLoop());
    }

    // Crane bevægelse: start/stop + let pitch/volume-dynamik efter inputstyrke
    private void OnCraneMove(float magnitude)
    {
        if (craneMovement == null) return;

        if (magnitude > 0.001f)
        {
            if (!craneSource.isPlaying)
            {
                craneSource.clip = craneMovement;
                craneSource.volume = craneVolume;
                craneSource.pitch = 1f;
                craneSource.Play();
            }

            // Dynamik: lidt pitch/volume respons
            craneSource.pitch = Mathf.Lerp(0.9f, 1.2f, Mathf.Clamp01(magnitude));
            craneSource.volume = craneVolume * Mathf.Lerp(0.6f, 1f, Mathf.Clamp01(magnitude));
        }
        else
        {
            if (craneSource.isPlaying)
                craneSource.Stop();
        }
    }

    // Launcher opladning
    private void OnLaunchChargeStarted()
    {
        if (launcherPower == null) return;

        chargeSource.clip = launcherPower;
        chargeSource.volume = chargeVolume;
        chargeSource.pitch = 1f;
        //chargeSource.loop = true;
        chargeSource.Play();
    }

    private void OnLaunchChargeProgress(float t)
    {
        if (!chargeSource.isPlaying) return;

        t = Mathf.Clamp01(t);
        // Lidt “spændings”-kurve: stigende pitch og volume
        chargeSource.pitch = Mathf.Lerp(0.9f, 1.3f, t);
        chargeSource.volume = Mathf.Lerp(chargeVolume * 0.7f, chargeVolume, t);
    }

    private void OnLaunchChargeStopped()
    {
        if (chargeSource.isPlaying)
            chargeSource.Stop();
    }

    // Affyring (one-shot)
    private void OnLaunchFired(float force)
    {
        if (launcherFire == null) return;
        sfxSource.PlayOneShot(launcherFire, sfxVolume);
    }

    // Ambience: loop sekventielt gennem listen
    private IEnumerator PlayAmbienceLoop()
    {
        var i = 0;
        while (true)
        {
            if (ambience == null || ambience.Length == 0)
                yield break;

            var clip = ambience[i];
            i = (i + 1) % ambience.Length;

            if (clip != null)
            {
                ambienceSource.clip = clip;
                ambienceSource.volume = ambienceVolume;
                ambienceSource.loop = false;
                ambienceSource.Play();

                while (ambienceSource.isPlaying)
                    yield return null;
            }
            else
            {
                // Spring null over
                yield return null;
            }
        }
    }
}
