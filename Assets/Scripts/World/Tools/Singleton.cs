using UnityEngine;

public abstract class Singleton<T> :
    Singleton where T : MonoBehaviour
{
    [SerializeField]
    protected bool _persistent = true;

    private static T _instance;
    private static readonly object _lock = new();

    public static T Instance
    {
        get
        {
            if (Quitting)
            {
                Debug.LogWarning($"[{nameof(Singleton)}<{typeof(T)}>] Instance " +
                    "might not be returned because the application is quitting.");

                return _instance;
            }

            lock (_lock)
            {
                return GetInstance();
            }
        }
    }

    private static T GetInstance()
    {
        if (_instance != null)
        {
            return _instance;

        }

        var instances = FindObjectsOfType<T>(true);
        var count = instances.Length;

        if (count > 0)
        {
            if (count == 1)
            {
                return _instance = instances[0];
            }

            Debug.LogWarning($"[{nameof(Singleton)}<{typeof(T)}>] There " +
                "should never be more than one {nameof(Singleton)} of " +
                "type {typeof(T)} in the scene, but {count} were found. " +
                "The first instance found will be used, and all others " +
                "will be destroyed.");

            for (var i = 1; i < instances.Length; ++i)
            {
                Debug.Log("Destroying object.");
                Destroy(instances[i].gameObject);
            }

            return _instance = instances[0];
        }

        Debug.Log($"[{nameof(Singleton)}<{typeof(T)}>] An instance " +
            "is needed in the scene and no existing instances were " +
            "found, so a new instance will be created.");

        return _instance =
            new GameObject($"({nameof(Singleton)}){typeof(T)}").
            AddComponent<T>();
    }

    private void Start()
    {
        if (Instance == null || (Instance != null && Instance != this))
        {
            // This singleton already exists.
            Destroy(gameObject);
        }
        else if (_persistent)
        {
            DontDestroyOnLoad(gameObject);
        }

        OnStart();
    }

    protected virtual void OnStart() {}
}

public abstract class Singleton : MonoBehaviour
{
    public static bool Quitting { get; private set; }

    private void OnApplicationQuit() => Quitting = true;

    private void Awake()
    {
        Quitting = false;
    }
}
