using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    public Rigidbody rigidBody;
    public BoxCollider boxcollider;
    public bool isLaunched;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        boxcollider = gameObject.GetComponent<BoxCollider>();
    }

    private void LateUpdate()
    {
        if (isLaunched)
        {
                Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, boxcollider.bounds.center.y, boxcollider.bounds.center.z);
        }
    }
}
