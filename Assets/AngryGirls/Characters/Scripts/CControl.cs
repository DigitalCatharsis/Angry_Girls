using UnityEngine;

namespace Angry_Girls
{
    public abstract class CContol : MonoBehaviour
    {
        public bool isLanding = false;
        public bool isGrounded = false;
        public bool isAttacking = false;
        public bool isDead = false;

        public float currentHealth;

        public Animator animator;
        public SubComponentProcessor subComponentProcessor;
        public ContactPoint[] boxColliderContacts;


    }
}