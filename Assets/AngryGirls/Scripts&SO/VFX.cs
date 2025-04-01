using DG.Tweening;
using System;
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

        [SerializeField] private Tuple<AudioSourceType, int> _spawnSound;
        [SerializeField] private Tuple<AudioSourceType, int> _destroySound;

        [Space(5)]
        public GameObject vfxOwner;

        protected override void Dispose(bool disposing)
        {
            if (_destroySound != null && _destroySound.Item1 != AudioSourceType.None)
            {
                GameLoader.Instance.audioManager.PlayCustomSound(_destroySound.Item1, _destroySound.Item2);
            }

            _destroySound = null;
            _spawnSound = null;

            var visualEffect = GetComponentInChildren<VisualEffect>();
            if (visualEffect != null)
            {
                visualEffect.Stop();
            }

            base.Dispose(disposing);
        }

        protected override void ReturnToPool() //«апихать в дочерний класс
        {
            if (!GameLoader.Instance.poolManager.vfxPoolDictionary[_vfxType].Contains(this))
            {
                GameLoader.Instance.poolManager.AddObject(_vfxType, GameLoader.Instance.poolManager.vfxPoolDictionary, this);
            }
        }

        private IEnumerator VFXLiving_Routine()
        {
            if (_spawnSound != null && _spawnSound.Item1 != AudioSourceType.None)
            {
                GameLoader.Instance.audioManager.PlayCustomSound(_spawnSound.Item1, _spawnSound.Item2, false);
            }
            yield return new WaitForSeconds(_timeToLive);

            Dispose();
        }

        private void ShowDamage(float damageAmount)
        {
            GetComponentInChildren<TextMeshPro>().text = damageAmount.ToString();
            transform.DOMove(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), 1);
        }

        public void InitAndRunVFX_ByAbility(AttackAbilityData ability, CControl control)
        {
            InitAndRunVFX_ByCustom(timeToLive: ability.timeToLive,
                    isTimeToLiveIsNormilizedTime: ability.isTimeToLiveIsNormilizedTime,
                    destroyOnCollision: ability.destroyOnCollision,
                    destroyOnCharacterCollision: ability.destroyOnCharacterCollision,
                    damage: ability.attackDamage,
                    knockbackValue: ability.enemyKnockbackValue,
                    enableCollider: ability.enableCollider,
                    enableTrigger: ability.enableTrigger,
                    owner: control.gameObject,
                    spawnSound: new Tuple<AudioSourceType, int>(ability.spawnSourceType, ability.spawnIndex),
                    destroySound: new Tuple<AudioSourceType, int>(ability.destroySourceType, ability.destoyIndex));
        }

        public void InitAndRunVFX_ByCustom(
            float timeToLive,
            bool isTimeToLiveIsNormilizedTime,
            bool destroyOnCollision,
            bool destroyOnCharacterCollision,
            float damage,
            float knockbackValue,
            bool enableCollider,
            bool enableTrigger,
            GameObject owner = null,
            Tuple<AudioSourceType, int> spawnSound = null,
            Tuple<AudioSourceType, int> destroySound = null)
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

            _spawnSound = spawnSound;
            _destroySound = destroySound;

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
                    Dispose();
                    //SpawnSmashVFXAndDestroyThis();
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
                Dispose();
                //SpawnSmashVFXAndDestroyThis();
            }
        }

        private void SpawnSmashVFXAndDestroyThis()
        {
            var vfx = GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_Uppercut, transform.position, Quaternion.identity);
            vfx.GetComponent<VFX>().InitAndRunVFX_ByCustom(timeToLive: 1, isTimeToLiveIsNormilizedTime: true, destroyOnCollision: false, destroyOnCharacterCollision: false, damage: 0, knockbackValue: 0, enableCollider: false, enableTrigger: false, owner: vfxOwner);
            Dispose();
        }
    }
}