using Angry_Girls;
using UnityEngine;
public class Singleton : MonoBehaviour
{
    public static Singleton Instance { get; private set; }

    public MyExtentions myExtentions;
    public CameraManager �ameraManager;
    public LaunchManager launchManager;
    public HashManager hashManager;
    public CharacterManager characterManager;
    public SpawnManager spawnManager;
    public TurnManager turnManager;

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
        �ameraManager = GetComponentInChildren<CameraManager>();
        launchManager = GetComponentInChildren<LaunchManager>();
        hashManager = GetComponentInChildren<HashManager>();
        spawnManager = GetComponentInChildren<SpawnManager>();
        turnManager = GetComponentInChildren<TurnManager>();
    }
}
