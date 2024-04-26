using UnityEngine;

namespace Angry_Girls
{
    public class BlockingManager : SubComponent
    {

        public override void OnAwake()
        {

        }

        public override void OnComponentEnable()
        {
            control.subComponentProcessor.blockingManager = this;
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
    }
}