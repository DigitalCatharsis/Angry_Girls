using Angry_Girls;
using UnityEngine;
public class Singleton : MonoBehaviour
{
    public static Singleton Instance { get; private set; }

    public MyExtentions myExtentions;
    public CameraManager �ameraManager;
    public LaunchManager launchManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        myExtentions = GetComponentInChildren<MyExtentions>();
        �ameraManager = GetComponentInChildren<CameraManager>();
        launchManager = GetComponentInChildren<LaunchManager>();
    }
}
