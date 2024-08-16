using UnityEngine;

namespace Angry_Girls
{
    public enum PlayerOrAi
    {
        Player,
        Ai,
    }
    public abstract class CControl : MonoBehaviour
    {
        [Space(15)]
        [Header("Debug")]       

        public bool isLanding = false;
        public bool isGrounded = false;
        public bool isAttacking = false;
        public bool isDead = false;

        public Rigidbody rigidBody;
        public BoxCollider boxCollider;
        public Animator animator;
        public SubComponentProcessor subComponentProcessor;
        public ContactPoint[] boxColliderContacts;

        [Header("Setup")]
        public PlayerOrAi playerOrAi;
        public CharacterSettings characterSettings;
    }
}