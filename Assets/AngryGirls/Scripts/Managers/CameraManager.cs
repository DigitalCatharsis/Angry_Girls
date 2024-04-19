using UnityEngine;

namespace Angry_Girls
{
    public class CameraManager : Singleton<CameraManager>
    {
        // Center camera on character collider center
        public void CenterCameraAgainst(Collider selectedObject)
        {
            var boundsCenter = selectedObject.bounds.center;
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, boundsCenter.y, boundsCenter.z);
        }
    }
}