using System;
using UnityEngine;

namespace Angry_Girls
{
    [Serializable]
    public class BackGround : MonoBehaviour
    {
        private Vector3 _startPos;
        private float _repeatWidth;

        [SerializeField] float _speed = 30.0f;

        void Start()
        {
            _startPos = transform.position;
            _repeatWidth = GetComponent<BoxCollider>().bounds.extents.z;
        }

        private void Update()
        {
            transform.Translate(Vector3.left * Time.deltaTime * _speed);

            if (transform.position.z < _startPos.z - _repeatWidth)
            {
                transform.position = _startPos;
            }
        }
    }
}