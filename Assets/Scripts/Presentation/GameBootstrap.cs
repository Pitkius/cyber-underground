using UnityEngine;

namespace CyberUnderground.Presentation
{
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Object.FindAnyObjectByType<GameManager>() != null)
                return;
            new GameObject("GameManager").AddComponent<GameManager>();
        }
    }
}
