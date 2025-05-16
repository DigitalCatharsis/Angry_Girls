using UnityEngine;
using UnityEngine.VFX;

namespace Angry_Girls
{

    public class VFXConfig
    {
        public Vector3 spawnPosition;
        public LayerMask layermask;
        public Color color;
        public VFX_Type Vfxtype;
        public GameObject originator;
        public float timeToLive;
        public bool connectToOriginator;
    }

    public class VFX : PoolObject
    {
        private VFXConfig _config;
        public void InitVfx(VFXConfig config)
        {
            _config = config;
            this.gameObject.layer = config.layermask;
        }

        public void SetColor()
        {
            var visualEffect = GetComponentInChildren<VisualEffect>();
            if (visualEffect != null)
            {
                visualEffect.SetVector4("Color", _config.color);
            }
        }

        protected override void OnDispose()
        {
            StopVisualEffects();
        }
        protected override void ReturnToPool()
        {
            if (!GameLoader.Instance.poolManager.vfxPoolDictionary[_config.Vfxtype].Contains(this))
            {
                GameLoader.Instance.poolManager.AddObject<VFX_Type>(_config.Vfxtype, GameLoader.Instance.poolManager.vfxPoolDictionary, this);
            }
        }

        private void StopVisualEffects()
        {
            var visualEffect = GetComponentInChildren<VisualEffect>();
            if (visualEffect != null)
            {
                visualEffect.Stop();
            }
        }
    }
}