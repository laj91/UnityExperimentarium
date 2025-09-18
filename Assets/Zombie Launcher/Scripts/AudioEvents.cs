using System;

public static class AudioEvents
{
    // Crane
    public static event Action<float> CraneMove; // magnitude 0..1
    public static void RaiseCraneMove(float magnitude) => CraneMove?.Invoke(magnitude);

    // Launcher charge
    public static event Action LaunchChargeStarted;
    public static event Action<float> LaunchChargeProgress; // t 0..1
    public static event Action LaunchChargeMaxed;
    public static event Action LaunchChargeEnded;

    public static void RaiseLaunchChargeStarted() => LaunchChargeStarted?.Invoke();
    public static void RaiseLaunchChargeProgress(float t) => LaunchChargeProgress?.Invoke(t);
    public static void RaiseLaunchChargeMaxed() => LaunchChargeMaxed?.Invoke();
    public static void RaiseLaunchChargeEnded() => LaunchChargeEnded?.Invoke();

    // Launcher fire
    public static event Action<float> LaunchFired; // pass force for evt. dynamik
    public static void RaiseLaunchFired(float force) => LaunchFired?.Invoke(force);
}