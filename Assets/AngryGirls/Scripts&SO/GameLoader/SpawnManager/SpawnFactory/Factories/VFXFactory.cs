using UnityEngine;
using UnityEngine.Rendering;

namespace Angry_Girls
{
    public enum VFX_Type
    {
        VFX_Damage_White,
        VFX_Shouryken,
        VFX_FireBall,
        VFX_Flame,
        VFX_Flame2,
        VFX_TestOnHitEffect,
    }

    public class VFXFactory : MonoBehaviour, ICoreFactory<VFX_Type>
    {
        [SerializeField] private GameObject VFX_Damage_White;
        [SerializeField] private GameObject VFX_Shouryken;
        [SerializeField] private GameObject VFX_FireBall;
        [SerializeField] private GameObject VFX_Flame;
        [SerializeField] private GameObject VFX_Flame2;
        [SerializeField] private GameObject VFX_TestOnHitEffect;

        private void Awake()
        {
            VFX_Damage_White = Resources.Load(VFX_Type.VFX_Damage_White.ToString()) as GameObject;
            VFX_Shouryken = Resources.Load(VFX_Type.VFX_Shouryken.ToString()) as GameObject;
            VFX_FireBall = Resources.Load(VFX_Type.VFX_FireBall.ToString()) as GameObject;
            VFX_Flame = Resources.Load(VFX_Type.VFX_Flame.ToString()) as GameObject;
            VFX_Flame2 = Resources.Load(VFX_Type.VFX_Flame2.ToString()) as GameObject;
            VFX_TestOnHitEffect = Resources.Load(VFX_Type.VFX_TestOnHitEffect.ToString()) as GameObject;
        }

        public PoolObject SpawnGameobject(VFX_Type VFX_Type, Vector3 position, Quaternion rotation)
        {
            switch (VFX_Type)
            {
                case VFX_Type.VFX_Damage_White:
                    {
                        return (Instantiate(VFX_Damage_White, position, rotation)).GetComponent<PoolObject>();
                    }
                case VFX_Type.VFX_Shouryken:
                    {
                        return (Instantiate(VFX_Shouryken, position, rotation)).GetComponent<PoolObject>();
                    }
                case VFX_Type.VFX_FireBall:
                    {
                        return (Instantiate(VFX_FireBall, position, rotation)).GetComponent<PoolObject>();
                    }
                case VFX_Type.VFX_Flame:
                    {
                        return (Instantiate(VFX_Flame, position, rotation)).GetComponent<PoolObject>();
                    }
                case VFX_Type.VFX_Flame2:
                    {
                        return (Instantiate(VFX_Flame2, position, rotation)).GetComponent<PoolObject>();
                    }
                case VFX_Type.VFX_TestOnHitEffect:
                    {
                        return (Instantiate(VFX_TestOnHitEffect, position, rotation)).GetComponent<PoolObject>();
                    }
                default:
                    {
                        Debug.Log("Cant instantiate " + VFX_Type);
                        return null;
                    }
            }
        }
    }
}