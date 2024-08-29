using DG.Tweening;
using System.Buffers;
using System.Collections;
using UnityEngine;

namespace Angry_Girls
{
    public class VFX : MonoBehaviour
    {
        //[Header("Setup")]
        [SerializeField] private float _timeToLive = 1f;
        [SerializeField] private bool _isTimeToLiveIsNormilizedTime;

        [Space(10)]
        [SerializeField] private ParticleSystem _particleSystem;
        [ShowOnly] public float projectileDamage = 0f;
        [SerializeField] private bool _destroyOnCollision;

        [Space(5)]
        public GameObject vfxOwner;


        private void OnEnable()
        {
            int LayerIgnoreRaycast = LayerMask.NameToLayer("Projectile");
            gameObject.layer = LayerIgnoreRaycast;

        }

        public void InitAndRunVFX(float timeToLive, bool isTimeToLiveIsNormilizedTime, bool destroyOnCollision, float damage, bool enableCollider, bool enableTrigger, GameObject owner = null)
        {
            //set owner for future trigger logic
            vfxOwner = owner;

            //Set lifetime
            _isTimeToLiveIsNormilizedTime = isTimeToLiveIsNormilizedTime;

            if (_isTimeToLiveIsNormilizedTime)
            {
                //TODO: не работает :c. Duration = 0
                _timeToLive = _particleSystem.main.duration;
            }
            else
            {
                _timeToLive = timeToLive;
            }
            
            _destroyOnCollision = destroyOnCollision;

            //Set VfxDamage
            projectileDamage = damage;

            //Set Triggers and colliders
            GetComponent<Collider>().enabled = enableCollider;

            foreach (var collider in (GetComponents<Collider>()))
            {
                if (collider.isTrigger == true)
                {
                    collider.enabled = enableTrigger;
                }
            }

            //Start Living Routine
            StartCoroutine(VFXLiving_Routine());
        }

        public void InitAndRunVFX(AttackAbility attackAbility, GameObject owner = null)
        {
            //set owner for future trigger logic
            vfxOwner = owner;

            //Set lifetime
            _isTimeToLiveIsNormilizedTime = attackAbility.isTimeToLiveIsNormilizedTime;

            if (_isTimeToLiveIsNormilizedTime)
            {
                //TODO: не работает :c. Duration = 0
                _timeToLive = _particleSystem.main.duration;
            }
            else
            {
                _timeToLive = attackAbility.timeToLive;
            }

            _destroyOnCollision = attackAbility.destroyOnCollision;

            //Set VfxDamage
            projectileDamage = attackAbility.attackDamage;

            //Set Triggers and colliders
            GetComponent<Collider>().enabled = attackAbility.enableCollider;

            foreach (var collider in (GetComponents<Collider>()))
            {
                if (collider.isTrigger == true)
                {
                    collider.enabled = attackAbility.enableTrigger;
                }
            }

            //Start Living Routine
            StartCoroutine(VFXLiving_Routine());
        }
        public void InitAndRunVFX(StaticAttackAbility attackAbility, GameObject owner = null)
        {
            //set owner for future trigger logic
            vfxOwner = owner;

            //Set lifetime
            _isTimeToLiveIsNormilizedTime = attackAbility.isTimeToLiveIsNormilizedTime;

            if (_isTimeToLiveIsNormilizedTime)
            {
                //TODO: не работает :c. Duration = 0
                _timeToLive = _particleSystem.main.duration;
            }
            else
            {
                _timeToLive = attackAbility.timeToLive;
            }

            _destroyOnCollision = attackAbility.destroyOnCollision;

            //Set VfxDamage
            projectileDamage = attackAbility.attackDamage;

            //Set Triggers and colliders
            GetComponent<Collider>().enabled = attackAbility.enableCollider;

            foreach (var collider in (GetComponents<Collider>()))
            {
                if (collider.isTrigger == true)
                {
                    collider.enabled = attackAbility.enableTrigger;
                }
            }

            //Start Living Routine
            StartCoroutine(VFXLiving_Routine());
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_destroyOnCollision == false)
            {
                return;
            }

            var control = collision.gameObject.GetComponent<CControl>();

            if (control != null &&
                control.playerOrAi == vfxOwner.GetComponent<CControl>().playerOrAi) 
            {
                return;
            }

            GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_Damage_White, transform.position, Quaternion.identity);
            this.gameObject.SetActive(false);
        }

        private IEnumerator VFXLiving_Routine()
        {
            yield return new WaitForSeconds(_timeToLive);

            this.gameObject.SetActive(false);
        }

        public VFX_Type GetVFXType()
        {
            return GetComponent<VFXPoolObject>().poolObjectType;
        }
    }
}