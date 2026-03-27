using UnityEngine;

namespace Solitaire.Core
{
    /// <summary>
    /// Sets application-wide settings (frame rate, sleep timeout, etc.)
    /// before any gameplay code runs. Uses [RuntimeInitializeOnLoadMethod]
    /// so no scene GameObject is needed.
    /// </summary>
    public static class ApplicationBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            UnityEngine.Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}
