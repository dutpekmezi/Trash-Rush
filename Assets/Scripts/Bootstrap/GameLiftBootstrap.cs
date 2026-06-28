using GameLift.Installer;
using UnityEngine;

namespace TrashRush.Bootstrap
{
    /// <summary>
    /// Creates the configured GameLift root prefab before the Base Scene loads.
    /// The Base Scene therefore remains free of serialized MonoBehaviours.
    /// </summary>
    public static class GameLiftBootstrap
    {
        private const string RootPrefabPath = "GameLift_LifetimeScope";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateRootScope()
        {
            if (Object.FindFirstObjectByType<GameLiftLifetimeScope>() != null)
            {
                return;
            }

            var rootPrefab = Resources.Load<GameObject>(RootPrefabPath);
            if (rootPrefab == null)
            {
                Debug.LogError($"[Trash Rush] Missing Resources/{RootPrefabPath} prefab.");
                return;
            }

            var rootObject = Object.Instantiate(rootPrefab);
            rootObject.name = "GameLift_LifetimeScope";
            Object.DontDestroyOnLoad(rootObject);
        }
    }
}
