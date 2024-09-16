using System;
using UnityEngine;

namespace Angry_Girls
{
    public abstract class PoolObject : MonoBehaviour, IDisposable
    {
        [SerializeField][ShowOnly] private bool _isDisposed = false;

        // реализация интерфейса IDisposable.
        public void Dispose()
        {
            // освобождаем неуправляемые ресурсы
            Dispose(true);
        }
        private void OnEnable()
        {
            _isDisposed = false;
            //int LayerIgnoreRaycast = LayerMask.NameToLayer("Projectile");
            //gameObject.layer = LayerIgnoreRaycast;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                // Освобождаем управляемые ресурсы

                this.gameObject.SetActive(false);
                ResetRigidbody();
                ResetTransform();
                ReturnToPool();
            }
            // освобождаем неуправляемые объекты
            _isDisposed = true;
        }
        protected abstract void ReturnToPool();

        public virtual void Init_PoolObject()
        {
            _isDisposed = false;
        }

        private void ResetRigidbody()
        {
            var rigid = gameObject.GetComponent<Rigidbody>();
            if (rigid != null)
            {
                rigid.velocity = Vector3.zero;
                //gameObject.GetComponent<Rigidbody>().rotation = Quaternion.identity;
                rigid.angularVelocity = Vector3.zero;
            }
        }

        private void ResetTransform()
        {
            this.transform.parent = null;
            this.transform.position = Vector3.zero;
            this.transform.rotation = Quaternion.identity;
        }
    }
}
