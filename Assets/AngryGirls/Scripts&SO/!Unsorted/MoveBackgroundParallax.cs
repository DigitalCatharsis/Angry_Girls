using Unity.VisualScripting;
using UnityEngine;

namespace Angry_Girls
{
    public class MoveBackgroundParallax : MonoBehaviour
    {
        public float speed;
        private float _offset;
        private Vector3 _startPosition;

        // Use this for initialization
        void Start()
        {
            _startPosition = transform.position;

            _offset = GetComponent<SpriteRenderer>().bounds.size.z /4;
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + speed * Time.deltaTime);

            if (transform.position.z < _startPosition.z - _offset)
            {
                transform.position = _startPosition;
            }
        }
    }


}