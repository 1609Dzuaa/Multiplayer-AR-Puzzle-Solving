using Unity.Netcode;
using UnityEngine;

public class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    protected static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError($"{typeof(T)} instance is not initialized!");
                GameObject gObj = new GameObject();
                gObj.AddComponent<T>();
                gObj.name = "NetworkSingleton_" + typeof(T).ToString();
                Debug.Log("Singleton created by getter: " + gObj.name);
                return gObj.GetComponent<T>();
            }
            return _instance;
        }
    }

    protected void CreateInstance()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"Multiple instances of {typeof(T)} detected. Destroying duplicate.");
            Destroy(gameObject);
        }
        else
        {
            _instance = this as T;
        }
    }

    protected virtual void Awake()
    {
        CreateInstance();
    }
}
