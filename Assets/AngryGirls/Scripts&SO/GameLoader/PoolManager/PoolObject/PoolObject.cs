using System;
using UnityEngine;

namespace Angry_Girls
{
    public class PoolObject : MonoBehaviour, IDisposable
    {
        [SerializeField][ShowOnly] private bool _isDisposedBase = false;

        // реализация интерфейса IDisposable.
        public void Dispose()
        {
            // освобождаем неуправляемые ресурсы
            Dispose(true);
            // подавляем финализацию
            GC.SuppressFinalize(this);
        }
        private void OnEnable()
        {
            _isDisposedBase = false;
            //int LayerIgnoreRaycast = LayerMask.NameToLayer("Projectile");
            //gameObject.layer = LayerIgnoreRaycast;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposedBase) return;
            if (disposing)
            {
                // Освобождаем управляемые ресурсы

                this.gameObject.SetActive(false);
                ResetTransform();
            }
            // освобождаем неуправляемые объекты
            _isDisposedBase = true;
        }

        private void ResetTransform()
        {
            this.transform.parent = null;
            this.transform.position = Vector3.zero;
            this.transform.rotation = Quaternion.identity;
        }

        // Деструктор
        ~PoolObject()
        {
            Dispose(false);
        }
    }
}
