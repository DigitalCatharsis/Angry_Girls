// Convert the 2D position of the mouse into a
// 3D position.  Display these on the game window.

using UnityEngine;

namespace Angry_Girls
{
    public class DebugInfo : MonoBehaviour
    {
        private Camera cam;

        void Start()
        {
            cam = Camera.main;
        }

        void OnGUI()
        {
            Vector3 point = new Vector3();
            Event currentEvent = Event.current;
            Vector2 mousePos = new Vector2();

            // Get the mouse position from Event.
            // Note that the y position from Event is inverted.
            mousePos.x = currentEvent.mousePosition.x;
            mousePos.y = cam.pixelHeight - currentEvent.mousePosition.y;

            point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));

            GUILayout.BeginArea(new Rect(20, 20, 250, 600));

            //pointer position
            GUILayout.Label("==Pointer==");
            GUILayout.Label("Screen pixels: " + cam.pixelWidth + ":" + cam.pixelHeight);
            GUILayout.Label("Mouse position: " + mousePos);
            GUILayout.Label("World position: " + point.ToString("F3"));

            //Enemy:
            GUILayout.Label("==Enemy==");
            if (Singleton.Instance.characterManager.enemyCharacters.Count != 0)
            {
                GUILayout.Label("Enemy HP: " + Singleton.Instance.characterManager.enemyCharacters[0].GetComponent<EnemyControl>().subComponentProcessor.damageProcessor.currentHealth);
            }
            GUILayout.Label("==Turn Manager:==");
            GUILayout.Label("Current phase: " + Singleton.Instance.turnManager.currentPhase.ToString());
            GUILayout.Label("Current turn: " + Singleton.Instance.turnManager.currentTurn);
            GUILayout.EndArea();
        }
    }
}