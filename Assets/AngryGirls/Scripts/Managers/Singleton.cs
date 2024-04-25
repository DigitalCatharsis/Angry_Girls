using Angry_Girls;
using UnityEngine;
public class Singleton : MonoBehaviour
{
    public static Singleton Instance { get; private set; }

    public MyExtentions myExtentions;
    public CameraManager ñameraManager;
    public LaunchManager launchManager;
    public HashManager hashManager;
    public CharacterManager characterManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        characterManager = GetComponentInChildren<CharacterManager>();
        myExtentions = GetComponentInChildren<MyExtentions>();
        ñameraManager = GetComponentInChildren<CameraManager>();
        launchManager = GetComponentInChildren<LaunchManager>();
        hashManager = GetComponentInChildren<HashManager>();
    }
}
