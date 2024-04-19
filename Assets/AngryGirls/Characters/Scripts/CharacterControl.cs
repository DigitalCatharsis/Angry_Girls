using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    public Rigidbody rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }
}
