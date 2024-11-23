using UnityEngine;
using UnityEngine.UIElements;

namespace Angry_Girls
{
    public class Character
    {
        private readonly LaunchLogic _launchLogic;
        private readonly DamageProcessor _damageProcessor;

        public Character(LaunchLogic launchLogic, DamageProcessor damageProcessor)
        {
            _launchLogic = launchLogic;
            _damageProcessor = damageProcessor;
        }

        public void Launch()
        {
            _launchLogic.ProcessLaunch();
        }

        public void TryDamage(Collider triggerCollider)
        {
            _damageProcessor.CheckForDamage(triggerCollider);
        }

        public void FixedUpdate()
        {
            _launchLogic.OnFixedUpdate();
            _damageProcessor.OnFixedUpdate();
        }
    }
}