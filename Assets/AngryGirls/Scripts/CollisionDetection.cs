using UnityEngine;

namespace Angry_Girls
{
    public class CollisionDetection : MonoBehaviour
    {
        public static GameObject GetCollidingObject(CharacterControl control, GameObject start, Vector3 dir, float blockDistance, ref Vector3 collisionPoint)
        {
            collisionPoint = Vector3.zero;

            //draw DebugLine
            Debug.DrawRay(start.transform.position, dir * blockDistance, Color.yellow);

            //check collision
            RaycastHit hit;
            if (Physics.Raycast(start.transform.position, dir, out hit, blockDistance))
            {
                if (!IsOwnBodyPart(control, hit.collider)
                   //&& !IsIgnoringCharacter(control, hit.collider)
                   )
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


        //public static bool IsIgnoringCharacter(CharacterControl control, Collider col)
        //{
        //    if (!control.CHARACTER_MOVEMENT_DATA.isIgnoreCharacterTime)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        var blockingCharacter = CharacterManager.Instance.GetCharacter(col.transform.root.gameObject);

        //        if (blockingCharacter == null)
        //        {
        //            return false;
        //        }

        //        if (blockingCharacter == control)
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            return true;
        //        }
        //    }
        //}

        public static bool IsOwnBodyPart(CharacterControl control, Collider col)
        {
            if (col.transform.root.gameObject == control.gameObject)
            {
                return true;
            }

            return false;
        }
    }
}
