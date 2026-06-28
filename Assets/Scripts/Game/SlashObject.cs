using System;
using DG.Tweening;
using UnityEngine;

namespace TrashRush.Game
{
    public sealed class SlashObject : MonoBehaviour
    {
        [SerializeField] private Transform _rotationTarget;
        [SerializeField] private Transform _decalFollowTarget;
        [SerializeField] private Transform _mesh;

        private SlashObjectData _data;
        private int _currentSlash;
        private bool _isResolved;
        private bool _isFractured;

        public event Action<SlashObject> Exploded;

        public SlashObjectData Data => _data;
        public int CurrentSlash => _currentSlash;
        public int TotalSlash => _data != null ? _data.TotalSlash : 0;
        public bool IsSlashable => _data != null && !_isResolved;
        public Transform RotationTarget => _rotationTarget != null ? _rotationTarget : transform;
        public Transform DecalFollowTarget =>
            _decalFollowTarget != null ? _decalFollowTarget : RotationTarget;

        public void Initialize(SlashObjectData data)
        {
            _data = data;
            _currentSlash = 0;
            _isResolved = false;
            _isFractured = false;

            if (_data != null && !string.IsNullOrWhiteSpace(_data.DisplayName))
            {
                gameObject.name = _data.DisplayName;
            }
        }

        public bool TryRegisterSlash(out bool completed)
        {
            completed = false;

            if (!IsSlashable)
            {
                return false;
            }

            _currentSlash = Mathf.Min(_currentSlash + 1, TotalSlash);
            completed = _currentSlash >= TotalSlash;

            if (completed)
            {
                _isResolved = true;
            }

            return true;
        }

        public void MarkEscaped()
        {
            _isResolved = true;
        }

        public void Explode()
        {
            if (gameObject == null || _isFractured)
            {
                return;
            }

            _isResolved = true;
            _isFractured = true;
            StopObjectMotion();
            Exploded?.Invoke(this);

            DisableOriginalMesh();

            if (_data == null || _data.CellFractured == null)
            {
                Destroy(gameObject);
                return;
            }

            var fracturedObject = Instantiate(_data.CellFractured, transform.parent);
            var fracturedTransform = fracturedObject.transform;
            fracturedTransform.localScale = _mesh != null ? _mesh.localScale : Vector3.one;
            fracturedTransform.SetPositionAndRotation(
                DecalFollowTarget.position,
                DecalFollowTarget.rotation);

            ApplyFractureForce(fracturedObject);

            Destroy(fracturedObject, _data.CellFracturedDestroyDelay);
            Destroy(gameObject, _data.CellFractureDestroyDelay);
        }

        private void StopObjectMotion()
        {
            transform.DOKill(false);
            var floatingAnimators = GetComponentsInChildren<FloatingMeshAnimator>(true);

            for (var i = 0; i < floatingAnimators.Length; i++)
            {
                floatingAnimators[i].Stop();
            }
        }

        private void DisableOriginalMesh()
        {
            var renderers = DecalFollowTarget.GetComponentsInChildren<Renderer>(true);

            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }

            var colliders = DecalFollowTarget.GetComponentsInChildren<Collider>(true);

            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
        }

        private void ApplyFractureForce(GameObject fracturedObject)
        {
            var renderers = fracturedObject.GetComponentsInChildren<MeshRenderer>(true);

            if (renderers.Length == 0)
            {
                return;
            }

            var bounds = renderers[0].bounds;

            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var explosionCenter = bounds.center;
            var force = _data != null ? _data.CellFractureForce : 0f;

            for (var i = 0; i < renderers.Length; i++)
            {
                var fragment = renderers[i].gameObject;
                var rigidbody = fragment.GetComponent<Rigidbody>();

                if (rigidbody == null)
                {
                    rigidbody = fragment.AddComponent<Rigidbody>();
                }

                rigidbody.isKinematic = false;
                rigidbody.useGravity = false;
                rigidbody.linearVelocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;

                var direction = rigidbody.worldCenterOfMass - explosionCenter;

                if (direction.sqrMagnitude <= Mathf.Epsilon)
                {
                    direction = UnityEngine.Random.onUnitSphere;
                }

                rigidbody.AddForce(direction.normalized * force, ForceMode.Impulse);
            }
        }
    }
}
