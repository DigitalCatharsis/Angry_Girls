using UnityEngine;

namespace Angry_Girls
{

    public class ExtraCharacterObject_Wings : MonoBehaviour, IExtraCharacterObject
    {
        private Animator m_Animator = null;

        public void SwitchAnimation(int index)
        {
            if (!m_Animator)
                m_Animator = GetComponentInChildren<Animator>();

            if (m_Animator)
                m_Animator.SetInteger("Mode", index);
        }
        public void OnRagdollEnabled()
        {
            SwitchAnimation(1);
        }
    }
}