using UnityEngine;

namespace Angry_Girls
{
    public class CollisionDetection : MonoBehaviour
    {
        public static GameObject GetCollidingObject(CControl control, Vector3 start, Vector3 dir, float blockDistance, ref Vector3 collisionPoint)
        {
            // Игнор рейкастом слоя 9 (character) и 13 (projectile) 10(bot)
            //int layerMask = 1 << 13;
            int layerMask = LayerMask.GetMask("Projectile", "Pickable");

            collisionPoint = Vector3.zero;

            //draw DebugLine
            Debug.DrawRay(start, dir * blockDistance, Color.yellow);

            //check collision
            RaycastHit hit;
            if (Physics.Raycast(start, dir, out hit, blockDistance, ~layerMask))
            {
                if (!IsOwnBodyPart(control, hit.collider)                   )
                {
                    collisionPoint = hit.point;
                    return hit.collider.transform.gameObject;
                }
                else
                {
                    return null;
                }
            }
            else  //collide nothing
            {
                return null;
            }
        }

        public static bool IsOwnBodyPart(CControl control, Collider col)
        {
            if (col.transform.root.gameObject == control.gameObject)
            {
                return true;
            }

            return false;
        }
    }
}
