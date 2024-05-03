using UnityEngine;

namespace Angry_Girls
{
    public class CharacterMovement : SubComponent
    {
        //[SerializeField] private Vector3 _atHittingObstacle_Speed = new Vector3(0, -2, 0);
        public override void OnComponentEnable()
        {
            control.subComponentProcessor.characterMovement = this;
        }

        public override void OnFixedUpdate()
        {

            if (control.subComponentProcessor.blockingManager.IsFrontBlocked() == true)
            {
                control.rigidBody.velocity = control.characterSettings.atHittingObstacle_Speed;
            }

            if (control.isGrounded == false)
            {
                SetRotation();
            }

            //if (control.subComponentProcessor.blockingManager.IsFrontBlocked() == true)
            //{
            //    control.rigidBody.velocity = _atHittingObstacle_Speed;
            //}

            //if (control.subComponentProcessor.animationProcessor.isGrounded == false)
            //{
            //    SetRotation();
            //}

        }


        public void SetRotation()
        {
            //if (control.rigidBody.velocity.z > 0f)
            //{
            //    control.transform.rotation = Quaternion.Euler(0, 0, 0);
            //    //return true;
            //}

            //if (control.rigidBody.velocity.z < 0f)
            //{
            //    control.transform.rotation = Quaternion.Euler(0, 180, 0);
            //    //return false;
            //}
            ////return false;
        }

        public override void OnLateUpdate()
        {
        }

        public override void OnStart()
        {
        }

        public override void OnUpdate()
        {
        }
        public override void OnAwake()
        {
        }

    }
}