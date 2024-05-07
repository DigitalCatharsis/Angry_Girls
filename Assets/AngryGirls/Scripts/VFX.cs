using DG.Tweening;
using System.Collections;
using System.Drawing;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace Angry_Girls
{
    public class VFX : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private float _timeToLive = 1;
        [SerializeField] private bool _isTimeToLiveIsNormilizedTime = false;
        [Space(10)]
        [SerializeField] private ParticleSystem _particleSystem;
        public float projectileDamage = 10f;


        private void OnEnable()
        {
            if (_isTimeToLiveIsNormilizedTime)
            {
                _timeToLive = _particleSystem.main.duration;
            }

            //задаем скорость _particleSystem
            //_rigidBody.DOMove(new Vector3(0, 0, 1500));
            var startPoint = transform.position;
            Vector3 frontDistance = new Vector3 (0,0,2.2f);
            Vector3 gravityDistance = new Vector3(0, 2.8f, 8f);
            float moveDuration = 1.5f;

            var waypoints = new[]
            {
                startPoint + new Vector3(0,0.7f,0),
                new Vector3(transform.position.x, transform.position.y, transform.position.z + Vector3.forward.z * frontDistance.z),
                new Vector3(transform.position.x, transform.position.y + Vector3.down.y * gravityDistance.y, transform.position.z + Vector3.forward.z * gravityDistance.z),

            };

            int LayerIgnoreRaycast = LayerMask.NameToLayer("Projectile");
            gameObject.layer = LayerIgnoreRaycast;

            transform.DOPath(waypoints, moveDuration, pathType: PathType.Linear, pathMode: PathMode.Full3D, resolution: 10, gizmoColor: UnityEngine.Color.green);
            transform.DORotate(new Vector3(Vector3.forward.z * 45f, 0, 0), duration: moveDuration, mode: RotateMode.Fast);


            StartCoroutine(VFXLiving_Routine());
        }

        private void OnCollisionEnter(Collision collision)
        { 
            if (collision.gameObject.GetComponent<CharacterControl>())
            {
                return;
            }

            DestroySelf();
        }

        private IEnumerator VFXLiving_Routine()
        {
            var time = 0f;

            while (time <= _timeToLive)
            {
                time += Time.deltaTime;
                yield return null;
            }

            DestroySelf();
        }

        private void DestroySelf()
        {
            Singleton.Instance.spawnManager.SpawnThing<VFX_Type>(VFX_Type.VFX_Damage_White, transform.position, Quaternion.identity);
            this.gameObject.SetActive(false);
        }
    }
}