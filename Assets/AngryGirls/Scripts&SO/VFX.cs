using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;

namespace Angry_Girls
{
    public class VFX : PoolObject
    {
        [SerializeField] private VFX_Type _vfxType;

        [Header("Setup")]
        [SerializeField] private float _timeToLive = 1f;
        [SerializeField] private bool _isTimeToLiveIsNormilizedTime;

        [Space(10)]
        [SerializeField] private ParticleSystem _particleSystem;
        [ShowOnly] public float projectileDamage = 0f;
        [ShowOnly] public float projectileKnockBack = 0f;
        [SerializeField] private bool _destroyOnCollision;
        [SerializeField] private bool _destroyOnCharacterTrigger;

        [Space(5)]
        public GameObject vfxOwner;

        protected override void Dispose(bool disposing)
        {
            var visualEffect = GetComponentInChildren<VisualEffect>();
            if (visualEffect != null)
            {
                visualEffect.Stop();
            }

            base.Dispose(disposing);
        }

        protected override void ReturnToPool() //«апихать в дочерний класс
        {
            var vfx = GetComponent<VFX>().GetVFXType();
            if (!GameLoader.Instance.poolManager.vfxPoolDictionary[vfx].Contains(this))
            {
                GameLoader.Instance.poolManager.AddObject(vfx, GameLoader.Instance.poolManager.vfxPoolDictionary, this);
            }
        }

        private IEnumerator VFXLiving_Routine()
        {
            yield return new WaitForSeconds(_timeToLive);
            Dispose();
        }

        private void ShowDamage(float damageAmount)
        {
            GetComponentInChildren<TextMeshPro>().text = damageAmount.ToString();
            transform.DOMove(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), 1);
        }

        public void InitAndRunVFX_ByAbility(AttackAbility ability, CControl control)
        {
            InitAndRunVFX_ByCustom(timeToLive: ability.timeToLive,
                    isTimeToLiveIsNormilizedTime: ability.isTimeToLiveIsNormilizedTime,
                    destroyOnCollision: ability.destroyOnCollision,
                    destroyOnCharacterCollision: ability.destroyOnCharacterCollision,
                    damage: ability.attackDamage,
                    knockbackValue: ability.knockback,
                    enableCollider: ability.enableCollider,
                    enableTrigger: ability.enableTrigger,
                    owner: control.gameObject);
        }

        public void InitAndRunVFX_ByCustom(float timeToLive, bool isTimeToLiveIsNormilizedTime, bool destroyOnCollision, bool destroyOnCharacterCollision, float damage, float knockbackValue, bool enableCollider, bool enableTrigger, GameObject owner)
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
            _destroyOnCharacterTrigger = destroyOnCharacterCollision;

            //Set VfxDamage and knockback value
            projectileDamage = damage;
            projectileKnockBack = knockbackValue;

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

        private void OnTriggerEnter(Collider other)
        {
            if (_destroyOnCharacterTrigger)
            {
                var control = other.transform.root.gameObject.GetComponent<CControl>();

                //if not ally or self
                if (control != null
                    && control.playerOrAi != vfxOwner.GetComponent<CControl>().playerOrAi)
                {
                    SpawnSmashVFXAndDestroyThis();
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            //do not collide with VFX
            if (collision.gameObject.GetComponent<VFX>() != null)
            {
                return;
            }

            if (_destroyOnCollision)
            {
                SpawnSmashVFXAndDestroyThis();
            }
        }

        private void SpawnSmashVFXAndDestroyThis()
        {
            var vfx = GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_Shouryken, transform.position, Quaternion.identity, vfxOwner);
            vfx.GetComponent<VFX>().InitAndRunVFX_ByCustom(timeToLive: 1, isTimeToLiveIsNormilizedTime: true, destroyOnCollision: false, destroyOnCharacterCollision: false, damage: 0, knockbackValue: 0, enableCollider: false, enableTrigger: false, owner: vfxOwner);
            Dispose();
        }

        public VFX_Type GetVFXType()
        {
            return _vfxType;
        }
    }
}