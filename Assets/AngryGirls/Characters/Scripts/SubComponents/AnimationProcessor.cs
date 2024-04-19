using UnityEngine;

namespace Angry_Girls
{
    public enum MainParameterType
    {
        IsAirboned
    }

    public class AnimationProcessor : SubComponent
    {
        private void Update()
        {
            if (Control.SubComponentProcessor.GroundDetector.IsAirboned)
            {
                SetTurn();
                Control.Animator.SetBool(MainParameterType.IsAirboned.ToString(), true);
            }
            else
            {
                Control.Animator.SetBool(MainParameterType.IsAirboned.ToString(), false);
            }
        }

        private void SetTurn()
        {
            if (Control.RigidBody.velocity.z > 0.1f)
            {
                Control.transform.rotation = Quaternion.Euler(0,0,0);
            }
            else if (Control.RigidBody.velocity.z < 0.1f)
            {
                Control.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }
}