using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterMovement : MonoBehaviour
    {
        [Header("Repel Settings")]
        [SerializeField] private float _repelForce = 0.25f; // ������� ���� ������������
        [SerializeField] private float _repelForceDelta = 0.05f; // ������ ���������� ����
        [SerializeField] private float _maxRepelForce = 1f; // ������������ ���� ������������
        private float _currentRepelForce; // ������� ���� ������������
        private int _hangCounter; // ������� "�������" �� ������ ���������
        private bool _isHanging; // ����, ��� �������� "�����" �� ������


        public Vector3 bottomRaycastContactPoint;
        private List<Collider> _currentCollisions = new List<Collider>(); // ������ ������� ������������

        [Header("Core")]
        private Rigidbody _rigidbody;
        private CControl _control;

        public Rigidbody Rigidbody => _rigidbody;

        private void FixedUpdate()
        {
            // ���� �������� ������ �� "�����", ���������� ������� � ���� ������������
            if (!_isHanging)
            {
                _hangCounter = 0;
                _currentRepelForce = _repelForce;
            }

            // ���������� ���� "�������" ������ ����
            _isHanging = false;
        }

        public void Initialize(CControl control)
        {
            _control = control;
            _rigidbody = GetComponent<Rigidbody>();
        }

        #region Rigidbody Operations
        public void ResetVelocity()
        {
            _rigidbody.velocity = Vector3.zero;
        }
        public void ApplyRigidForce(Vector3 force, ForceMode forceMode = ForceMode.VelocityChange)
        {
            _rigidbody.AddForce(force, ForceMode.VelocityChange);
        }
        public void ApplyKnockbackFromEnemy(GameObject opponent, float knockbackForce)
        {
            if (_rigidbody.velocity.z != 0)
            {
                return;
            }

            var direction = _rigidbody.position - opponent.transform.position;

            // ���������, ��� ������� �� ��������� � ����� �����
            if (Math.Abs(direction.z) < 0.001f)
            {
                // ��������� �����������
                int randomDirection = UnityEngine.Random.Range(0, 2);
                direction = new Vector3(0, 0, randomDirection == 0 ? -1 : 1);
            }
            else
            {
                // ����������� ������ z-���������
                direction.z /= Math.Abs(direction.z);
            }

            _rigidbody.AddForce(new Vector3(0, 0, direction.z * knockbackForce), ForceMode.VelocityChange);
        }
        public void SetPosition(Vector3 position)
        {
            _rigidbody.position = position;
        }
        public void SetRotation(Quaternion rotation)
        {
            _rigidbody.MoveRotation(rotation);
        }
        public void SetVelocity(Vector3 velocity)
        {
            _rigidbody.velocity = velocity;
        }
        public void Teleport(Vector3 position)
        {
            _rigidbody.MovePosition(position);
        }
        #endregion

        #region Collision operations
        private void OnCollisionEnter(Collision collision)
        {
            // ��������� ��������� � ������ ������� ������������
            if (!_currentCollisions.Contains(collision.collider))
            {
                _currentCollisions.Add(collision.collider);
            }

            HandleCharacterCollision();
        }

        private void OnCollisionStay(Collision collision)
        {
            HandleCharacterCollision();
        }

        private void OnCollisionExit(Collision collision)
        {
            // ������� ��������� �� ������ ������� ������������
            if (_currentCollisions.Contains(collision.collider))
            {
                _currentCollisions.Remove(collision.collider);
            }
        }
        private void HandleRepel(Collider other)
        {
            // �������� Rigidbody ������� ���������
            var otherRigidbody = other.attachedRigidbody;
            if (otherRigidbody == null)
                return;

            // ����������� ������� "�������"
            _hangCounter++;
            _isHanging = true;

            // ����������� ���� ������������, �� �� ������ ������������
            _currentRepelForce = Mathf.Min(_repelForce + _hangCounter * _repelForceDelta, _maxRepelForce);

            // ��������� ������� �� ��� Z ����� ������� ���������� � ������
            var zDifference = _rigidbody.position.z - otherRigidbody.transform.position.z;

            // ���������� ����������� ������������
            Vector3 repelDirection;
            if (zDifference > 0)
            {
                // ������� �������� ������� �������, ����������� ������
                repelDirection = Vector3.forward;
            }
            else if (zDifference < 0)
            {
                // ������� �������� ����� �������, ����������� �����
                repelDirection = Vector3.back;
            }
            else
            {
                // ������� �� Z ����� ����, ����������� � ��������� �������
                repelDirection = UnityEngine.Random.Range(0, 2) == 0 ? Vector3.back : Vector3.forward;
            }

            // ������� ��������� ������, ��� ��������� ��������
            Vector3 repelOffset = repelDirection * (_currentRepelForce * Time.fixedDeltaTime);

            Teleport(_rigidbody.position + repelOffset);
            //rigidBody.MovePosition(rigidBody.position + repelOffset);
        }
        private void HandleCharacterCollision()
        {
            if (_control.isGrounded)
            {
                return;
            }

            if (_currentCollisions.Count == 0)
                return;

            // ������� ���������� ����������
            Collider nearestCollider = null;
            float nearestDistance = float.MaxValue;

            foreach (var collider in _currentCollisions)
            {
                // ���������, ��� ��� ������ �������� (���� "Character" ��� "Bot")
                var otherLayer = collider.gameObject.layer;
                if (otherLayer != LayerMask.NameToLayer("Character") && otherLayer != LayerMask.NameToLayer("Bot"))
                    continue;

                // �������� Rigidbody ������� ���������
                var otherRigidbody = collider.attachedRigidbody;
                if (otherRigidbody == null)
                    continue;

                // ���������, ��� ������� �������� �������� ����
                if (_rigidbody.velocity.y >= 0)
                    continue;

                // ���������, ��� ������ �������� ����� �� ����� ��� ����������
                var otherIsGrounded = otherRigidbody.velocity.magnitude < 0.1f; // ��������� �������
                if (!otherIsGrounded)
                    continue;

                // ��������� ���������� �� ����������
                float distance = Vector3.Distance(_rigidbody.position, otherRigidbody.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCollider = collider;
                }
            }

            // ���� ��������� ��������� ������, ������������ ������������
            if (nearestCollider != null)
            {
                HandleRepel(nearestCollider);
            }
        }
        #endregion

        // ���������� �� FixedUpdate
        public void UpdateGroundCheck() { }
    }
}