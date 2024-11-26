using UnityEngine;

namespace Angry_Girls
{
    public class InputManager : MonoBehaviour
    {
        public bool GetMouseButtonUp = false;
        public bool GetMouseButtonDown = false;
        public bool GetMouseButton = false;
        public bool test = false;
        private void Update()
        {
            test = Input.GetKey(KeyCode.Mouse0);

            //отпустил
            if (Input.GetMouseButtonUp(0))
            {
                GetMouseButtonUp = Input.GetMouseButtonUp(0);
                Debug.Log("Pressed left click.");
            }

            //нажал
            if (Input.GetMouseButtonDown(0))
                GetMouseButtonDown = Input.GetMouseButtonDown(0);
            Debug.Log("Pressed left-click.");

            //держу
            if (Input.GetMouseButton(0))
            {
                GetMouseButton = Input.GetMouseButton(0);
                Debug.Log("The middle mouse button is being held down.");
            }
        }
    }
}