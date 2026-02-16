using UnityEngine;

[ExecuteAlways]

public class GetGlobalMatrixLite : MonoBehaviour
{

    void Update()
    {
        Shader.SetGlobalMatrix("_LightMatrix", Matrix4x4.Rotate(transform.rotation));
    }
}
