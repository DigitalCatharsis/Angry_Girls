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
        [SerializeField] private bool _destroyOnCollision;

        [Space(10)]
        [SerializeField] private ParticleSystem _particleSystem;
        [ShowOnly] public float projectileDamage = 0f;

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

        public void SendProjectile_Fireball__TweenMove(Vector3 startPoint, Vector3 finalRotationDegree, float damage, float moveDuration = 1.5f)
        {
            projectileDamage = damage;

            Vector3 frontDistance = new Vector3(0, 0, 2.2f);
            Vector3 gravityDistance = new Vector3(0, 2.8f, 8f);

            var waypoints = new[]
            {
                startPoint,
                new Vector3(transform.position.x, transform.position.y, transform.position.z + this.gameObject.transform.forward.z * frontDistance.z),
                new Vector3(transform.position.x, transform.position.y + Vector3.down.y * gravityDistance.y, transform.position.z + this.gameObject.transform.forward.z * gravityDistance.z),
            };


            transform.DOPath(waypoints, moveDuration, pathType: PathType.Linear, pathMode: PathMode.Full3D, resolution: 10, gizmoColor: UnityEngine.Color.green);
            transform.DORotate(endValue: new Vector3(finalRotationDegree.x, finalRotationDegree.y, finalRotationDegree.y * this.gameObject.transform.forward.z), duration: moveDuration, mode: RotateMode.Fast);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_destroyOnCollision == false)
            {
                return;
            }

            if (collision.gameObject.GetComponent<CharacterControl>()) //TODO: set for both sides 
            {
                return;
            }
            var poolManager = GameLoader.Instance.poolManager;
            poolManager.GetObject(VFX_Type.VFX_Damage_White, poolManager.vfxPoolDictionary, transform.position, Quaternion.identity);
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