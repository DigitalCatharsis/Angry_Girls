using UnityEngine;

namespace Angry_Girls
{
    public class CharacterMovement : SubComponent
    {
        public override void OnComponentEnable()
        {
            control.subComponentProcessor.characterMovement = this;
        }

        public override void OnFixedUpdate()
        {
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