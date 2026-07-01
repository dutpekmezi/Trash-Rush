using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TrashRush.Game
{
    public sealed class SlashDecalSpawner
        : IDisposable
    {
        private const int OcclusionRefinementIterations = 8;
        private const string SlashParentName = "SlashParent";
        private const string DecalProjectorPointName = "DecalProjectorPoint";
        private const string MeshObjectName = "Mesh";
        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static readonly int MainTextureId = Shader.PropertyToID("_MainTex");
        private static readonly int SlashTextureId = Shader.PropertyToID("Base_Map");

        private readonly Camera _camera;
        private readonly SlashConfig _config;
        private readonly HitEffectConfig _hitEffectConfig;
        private readonly GameObject _decalProjectorPrefab;
        private readonly Sprite _backgroundSlashHalfSprite;
        private readonly Sprite _backgroundSlashTopHalfSprite;
        private readonly LayerMask _hitLayerMask;
        private readonly int _hitSampleCount;
        private readonly Dictionary<Texture2D, Material> _decalMaterials = new();

        private Tween _screenShakeTween;
        private Vector3 _cameraStartLocalPosition;

        public SlashDecalSpawner(
            Camera camera,
            SlashConfig config,
            HitEffectConfig hitEffectConfig,
            GameObject decalProjectorPrefab,
            Sprite backgroundSlashHalfSprite,
            Sprite backgroundSlashTopHalfSprite,
            LayerMask hitLayerMask,
            int hitSampleCount)
        {
            _camera = camera;
            _config = config;
            _hitEffectConfig = hitEffectConfig;
            _decalProjectorPrefab = decalProjectorPrefab;
            _backgroundSlashHalfSprite = backgroundSlashHalfSprite;
            _backgroundSlashTopHalfSprite = backgroundSlashTopHalfSprite;
            _hitLayerMask = hitLayerMask;
            _hitSampleCount = Mathf.Max(2, hitSampleCount);
            _cameraStartLocalPosition = _camera != null
                ? _camera.transform.localPosition
                : Vector3.zero;
        }

        public bool TrySpawn(Vector3 startWorldPosition, Vector3 endWorldPosition)
        {
            return TrySpawn(startWorldPosition, endWorldPosition, 1, out _);
        }

        public bool TrySpawn(
            Vector3 startWorldPosition,
            Vector3 endWorldPosition,
            out SlashObject slashObject)
        {
            return TrySpawn(startWorldPosition, endWorldPosition, 1, out slashObject);
        }

        public bool TrySpawn(
            Vector3 startWorldPosition,
            Vector3 endWorldPosition,
            int streak,
            out SlashObject slashObject)
        {
            slashObject = null;

            if (_camera == null ||
                (_decalProjectorPrefab == null &&
                 _backgroundSlashHalfSprite == null &&
                 _backgroundSlashTopHalfSprite == null))
            {
                return false;
            }

            var delta = endWorldPosition - startWorldPosition;
            delta.z = 0f;

            if (delta.sqrMagnitude <= Mathf.Epsilon)
            {
                return false;
            }

            if (!TryFindHit(
                    startWorldPosition,
                    endWorldPosition,
                    out var decalPoint,
                    out var followParent,
                    out var hitRoot,
                    out slashObject))
            {
                return false;
            }

            if (slashObject == null || !slashObject.IsSlashable)
            {
                return false;
            }

            var height = GetProjectorHeight(delta.magnitude);

            TrySpawnBackgroundSprite(
                startWorldPosition,
                endWorldPosition,
                delta,
                slashObject);

            Spawn(
                _decalProjectorPrefab,
                decalPoint,
                followParent,
                GetSpawnPosition(decalPoint, startWorldPosition, endWorldPosition),
                GetSlashZRotation(delta),
                height,
                slashObject.Data.SlashTexture);

            PlayHitEffect(slashObject.RotationTarget, hitRoot, delta, height, streak);
            PlayScreenShake(streak);
            return true;
        }

        public void Dispose()
        {
            StopScreenShake();

            foreach (var material in _decalMaterials.Values)
            {
                if (material != null)
                {
                    UnityEngine.Object.Destroy(material);
                }
            }

            _decalMaterials.Clear();
        }

        private bool TryFindHit(
            Vector3 startWorldPosition,
            Vector3 endWorldPosition,
            out Transform decalPoint,
            out Transform followParent,
            out Transform hitRoot,
            out SlashObject slashObject)
        {
            var startScreenPosition = _camera.WorldToScreenPoint(startWorldPosition);
            var endScreenPosition = _camera.WorldToScreenPoint(endWorldPosition);

            for (var i = 0; i < _hitSampleCount; i++)
            {
                var t = i / (_hitSampleCount - 1f);
                var screenPosition = Vector3.Lerp(startScreenPosition, endScreenPosition, t);
                var ray = _camera.ScreenPointToRay(screenPosition);
                var hits = Physics.RaycastAll(ray, _camera.farClipPlane, _hitLayerMask, QueryTriggerInteraction.Ignore);

                if (TryGetHit(
                        hits,
                        out decalPoint,
                        out followParent,
                        out hitRoot,
                        out slashObject))
                {
                    return true;
                }
            }

            decalPoint = null;
            followParent = null;
            hitRoot = null;
            slashObject = null;
            return false;
        }

        private static bool TryGetHit(
            RaycastHit[] hits,
            out Transform decalPoint,
            out Transform followParent,
            out Transform hitRoot,
            out SlashObject slashObject)
        {
            if (hits == null || hits.Length == 0)
            {
                decalPoint = null;
                followParent = null;
                hitRoot = null;
                slashObject = null;
                return false;
            }

            Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

            for (var i = 0; i < hits.Length; i++)
            {
                if (TryFindDecalPoint(
                        hits[i].collider.transform,
                        out decalPoint,
                        out followParent,
                        out hitRoot,
                        out slashObject))
                {
                    return true;
                }
            }

            decalPoint = null;
            followParent = null;
            hitRoot = null;
            slashObject = null;
            return false;
        }

        private static bool TryFindDecalPoint(
            Transform hitTransform,
            out Transform decalPoint,
            out Transform followParent,
            out Transform hitRoot,
            out SlashObject slashObject)
        {
            for (var current = hitTransform; current != null; current = current.parent)
            {
                decalPoint = current.Find(DecalProjectorPointName);

                if (decalPoint != null)
                {
                    hitRoot = current;
                    slashObject = current.GetComponent<SlashObject>();
                    followParent = slashObject != null
                        ? slashObject.DecalFollowTarget
                        : FindFollowParent(current);
                    return true;
                }
            }

            decalPoint = null;
            followParent = null;
            hitRoot = null;
            slashObject = null;
            return false;
        }

        private static Transform FindFollowParent(Transform hitRoot)
        {
            var mesh = hitRoot.Find(MeshObjectName);
            return mesh != null ? mesh : hitRoot;
        }

        private bool TrySpawnBackgroundSprite(
            Vector3 startWorldPosition,
            Vector3 endWorldPosition,
            Vector3 slashDelta,
            SlashObject slashObject)
        {
            if (_backgroundSlashHalfSprite == null ||
                _backgroundSlashTopHalfSprite == null ||
                slashObject == null ||
                (_config != null && !_config.BackgroundSlashEnabled) ||
                !TryFindBackgroundPoint(
                    startWorldPosition,
                    endWorldPosition,
                    out var backgroundPoint,
                    out var backgroundRoot))
            {
                return false;
            }

            var center = GetSpawnPosition(
                backgroundPoint,
                startWorldPosition,
                endWorldPosition);
            var projectedStart = ProjectToPlane(backgroundPoint, startWorldPosition);
            var projectedEnd = ProjectToPlane(backgroundPoint, endWorldPosition);
            var direction = (projectedEnd - projectedStart).normalized;
            var scaleMultiplier = _config != null
                ? _config.BackgroundSlashScaleMultiplier
                : Vector2.one;
            var baseHeight = GetBackgroundSlashHeight(slashDelta.magnitude);
            var visualHeight = baseHeight * scaleMultiplier.y;

            if (direction.sqrMagnitude <= Mathf.Epsilon || visualHeight <= Mathf.Epsilon)
            {
                return false;
            }

            var visualStart = center - direction * (visualHeight * 0.5f);
            var visualEnd = center + direction * (visualHeight * 0.5f);

            if (!TryFindSlashOcclusion(
                    visualStart,
                    visualEnd,
                    slashObject,
                    out var entryT,
                    out var exitT))
            {
                return false;
            }

            var zRotation = GetSlashZRotation(slashDelta);
            var spawnedBefore = TrySpawnBackgroundSegment(
                _backgroundSlashHalfSprite,
                "BackgroundSlashHalf",
                backgroundPoint,
                backgroundRoot,
                visualStart,
                Vector3.Lerp(visualStart, visualEnd, entryT),
                zRotation,
                scaleMultiplier.y);
            var spawnedAfter = TrySpawnBackgroundSegment(
                _backgroundSlashTopHalfSprite,
                "BackgroundSlashTopHalf",
                backgroundPoint,
                backgroundRoot,
                Vector3.Lerp(visualStart, visualEnd, exitT),
                visualEnd,
                zRotation,
                scaleMultiplier.y);

            return spawnedBefore || spawnedAfter;
        }

        private bool TrySpawnBackgroundSegment(
            Sprite sprite,
            string objectName,
            Transform backgroundPoint,
            Transform backgroundRoot,
            Vector3 segmentStart,
            Vector3 segmentEnd,
            float zRotation,
            float heightScale)
        {
            var renderedHeight = Vector3.Distance(segmentStart, segmentEnd);

            if (sprite == null || renderedHeight <= Mathf.Epsilon)
            {
                return false;
            }

            var baseHeight = heightScale > Mathf.Epsilon
                ? renderedHeight / heightScale
                : renderedHeight;

            SpawnBackgroundSprite(
                sprite,
                objectName,
                backgroundPoint,
                backgroundRoot,
                Vector3.Lerp(segmentStart, segmentEnd, 0.5f),
                zRotation,
                baseHeight);
            return true;
        }

        private void SpawnBackgroundSprite(
            Sprite sprite,
            string objectName,
            Transform backgroundPoint,
            Transform backgroundRoot,
            Vector3 worldPosition,
            float zRotation,
            float baseHeight)
        {
            var spriteObject = new GameObject(objectName);
            var spriteTransform = spriteObject.transform;
            var spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            var backgroundRenderer = backgroundRoot != null
                ? backgroundRoot.GetComponent<SpriteRenderer>()
                : null;

            spriteRenderer.sprite = sprite;
            spriteRenderer.color = _config != null
                ? _config.BackgroundSlashColor
                : Color.white;

            if (backgroundRenderer != null)
            {
                spriteRenderer.sortingLayerID = backgroundRenderer.sortingLayerID;
                spriteRenderer.sortingOrder = backgroundRenderer.sortingOrder +
                    (_config != null ? _config.BackgroundSlashSortingOrderOffset : 1);
            }

            spriteTransform.SetPositionAndRotation(
                worldPosition,
                backgroundPoint.rotation * Quaternion.AngleAxis(zRotation, Vector3.forward));

            var spriteHeight = sprite.bounds.size.y;
            var uniformScale = spriteHeight > Mathf.Epsilon
                ? baseHeight / spriteHeight
                : 1f;
            var scaleMultiplier = _config != null
                ? _config.BackgroundSlashScaleMultiplier
                : Vector2.one;
            spriteTransform.localScale = new Vector3(
                uniformScale * scaleMultiplier.x,
                uniformScale * scaleMultiplier.y,
                1f);
            spriteTransform.SetParent(backgroundPoint, true);

            PlayFadeAndDestroy(spriteObject, spriteRenderer);
        }

        private bool TryFindBackgroundPoint(
            Vector3 startWorldPosition,
            Vector3 endWorldPosition,
            out Transform backgroundPoint,
            out Transform backgroundRoot)
        {
            var startScreenPosition = _camera.WorldToScreenPoint(startWorldPosition);
            var endScreenPosition = _camera.WorldToScreenPoint(endWorldPosition);

            if (TryFindBackgroundPointOnRay(
                    _camera.ScreenPointToRay(
                        Vector3.Lerp(startScreenPosition, endScreenPosition, 0.5f)),
                    out backgroundPoint,
                    out backgroundRoot))
            {
                return true;
            }

            for (var i = 0; i < _hitSampleCount; i++)
            {
                var t = i / (_hitSampleCount - 1f);
                var screenPosition = Vector3.Lerp(startScreenPosition, endScreenPosition, t);

                if (TryFindBackgroundPointOnRay(
                        _camera.ScreenPointToRay(screenPosition),
                        out backgroundPoint,
                        out backgroundRoot))
                {
                    return true;
                }
            }

            backgroundPoint = null;
            backgroundRoot = null;
            return false;
        }

        private bool TryFindBackgroundPointOnRay(
            Ray ray,
            out Transform backgroundPoint,
            out Transform backgroundRoot)
        {
            var hits = Physics.RaycastAll(
                ray,
                _camera.farClipPlane,
                _hitLayerMask,
                QueryTriggerInteraction.Ignore);

            for (var i = 0; i < hits.Length; i++)
            {
                for (var current = hits[i].collider.transform;
                     current != null;
                     current = current.parent)
                {
                    backgroundPoint = current.Find(SlashParentName);

                    if (backgroundPoint != null)
                    {
                        backgroundRoot = current;
                        return true;
                    }
                }
            }

            backgroundPoint = null;
            backgroundRoot = null;
            return false;
        }

        private bool TryFindSlashOcclusion(
            Vector3 lineStart,
            Vector3 lineEnd,
            SlashObject slashObject,
            out float entryT,
            out float exitT)
        {
            var firstHitIndex = -1;
            var lastHitIndex = -1;

            for (var i = 0; i < _hitSampleCount; i++)
            {
                var t = i / (_hitSampleCount - 1f);

                if (!IsOverSlashObject(Vector3.Lerp(lineStart, lineEnd, t), slashObject))
                {
                    continue;
                }

                if (firstHitIndex < 0)
                {
                    firstHitIndex = i;
                }

                lastHitIndex = i;
            }

            if (firstHitIndex < 0)
            {
                entryT = 0f;
                exitT = 0f;
                return false;
            }

            var firstHitT = firstHitIndex / (_hitSampleCount - 1f);
            var lastHitT = lastHitIndex / (_hitSampleCount - 1f);

            entryT = firstHitIndex == 0
                ? 0f
                : RefineOcclusionEntry(
                    lineStart,
                    lineEnd,
                    slashObject,
                    (firstHitIndex - 1) / (_hitSampleCount - 1f),
                    firstHitT);
            exitT = lastHitIndex == _hitSampleCount - 1
                ? 1f
                : RefineOcclusionExit(
                    lineStart,
                    lineEnd,
                    slashObject,
                    lastHitT,
                    (lastHitIndex + 1) / (_hitSampleCount - 1f));

            return exitT > entryT;
        }

        private float RefineOcclusionEntry(
            Vector3 lineStart,
            Vector3 lineEnd,
            SlashObject slashObject,
            float missT,
            float hitT)
        {
            for (var i = 0; i < OcclusionRefinementIterations; i++)
            {
                var midpointT = (missT + hitT) * 0.5f;

                if (IsOverSlashObject(
                        Vector3.Lerp(lineStart, lineEnd, midpointT),
                        slashObject))
                {
                    hitT = midpointT;
                }
                else
                {
                    missT = midpointT;
                }
            }

            return hitT;
        }

        private float RefineOcclusionExit(
            Vector3 lineStart,
            Vector3 lineEnd,
            SlashObject slashObject,
            float hitT,
            float missT)
        {
            for (var i = 0; i < OcclusionRefinementIterations; i++)
            {
                var midpointT = (hitT + missT) * 0.5f;

                if (IsOverSlashObject(
                        Vector3.Lerp(lineStart, lineEnd, midpointT),
                        slashObject))
                {
                    hitT = midpointT;
                }
                else
                {
                    missT = midpointT;
                }
            }

            return hitT;
        }

        private bool IsOverSlashObject(Vector3 worldPosition, SlashObject slashObject)
        {
            var ray = _camera.ScreenPointToRay(_camera.WorldToScreenPoint(worldPosition));
            var hits = Physics.RaycastAll(
                ray,
                _camera.farClipPlane,
                _hitLayerMask,
                QueryTriggerInteraction.Ignore);

            for (var i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.GetComponentInParent<SlashObject>() == slashObject)
                {
                    return true;
                }
            }

            return false;
        }

        private void Spawn(
            GameObject decalProjectorPrefab,
            Transform decalPoint,
            Transform followParent,
            Vector3 worldPosition,
            float zRotation,
            float height,
            Texture2D slashTexture)
        {
            if (decalProjectorPrefab == null)
            {
                return;
            }

            var decal = UnityEngine.Object.Instantiate(decalProjectorPrefab);
            var decalTransform = decal.transform;
            var baseLocalRotation = decalTransform.localRotation;
            var decalProjector = decal.GetComponent<DecalProjector>();

            decalTransform.SetPositionAndRotation(
                worldPosition,
                decalPoint.rotation * baseLocalRotation * Quaternion.AngleAxis(zRotation, Vector3.forward));

            decalTransform.SetParent(followParent != null ? followParent : decalPoint, true);

            ApplySlashTexture(decalProjector, slashTexture);
            ApplyHeight(decalProjector, height);
            PlayFadeAndDestroy(decal, decalProjector);
        }

        private void ApplySlashTexture(DecalProjector decalProjector, Texture2D slashTexture)
        {
            if (decalProjector == null || slashTexture == null || decalProjector.material == null)
            {
                return;
            }

            if (!_decalMaterials.TryGetValue(slashTexture, out var material) || material == null)
            {
                material = new Material(decalProjector.material)
                {
                    name = $"{decalProjector.material.name}_{slashTexture.name}"
                };

                SetTexture(material, SlashTextureId, slashTexture);
                SetTexture(material, BaseMapId, slashTexture);
                SetTexture(material, MainTextureId, slashTexture);
                _decalMaterials[slashTexture] = material;
            }

            decalProjector.material = material;
        }

        private static void SetTexture(Material material, int propertyId, Texture texture)
        {
            if (material.HasProperty(propertyId))
            {
                material.SetTexture(propertyId, texture);
            }
        }

        private static void ApplyHeight(DecalProjector decalProjector, float height)
        {
            if (decalProjector == null)
            {
                return;
            }

            var size = decalProjector.size;
            size.y = height;
            decalProjector.size = size;
        }

        private void PlayFadeAndDestroy(GameObject decal, DecalProjector decalProjector)
        {
            var lifetime = _config != null ? Mathf.Max(0f, _config.DecalProjectorLifetime) : 0f;
            var fadeTime = _config != null ? Mathf.Max(0f, _config.DecalProjectorFadeTime) : 0f;

            if (decalProjector == null)
            {
                UnityEngine.Object.Destroy(decal, lifetime + fadeTime);
                return;
            }

            var sequence = DOTween.Sequence()
                .SetLink(decal)
                .AppendInterval(lifetime);

            if (fadeTime <= 0f)
            {
                sequence.AppendCallback(() => decalProjector.fadeFactor = 0f);
            }
            else
            {
                sequence.Append(DOVirtual
                    .Float(decalProjector.fadeFactor, 0f, fadeTime, value => decalProjector.fadeFactor = value)
                    .SetEase(Ease.Linear));
            }

            sequence.AppendCallback(() => UnityEngine.Object.Destroy(decal));

            if (_config != null && _config.UseUnscaledTime)
            {
                sequence.SetUpdate(true);
            }
        }

        private void PlayFadeAndDestroy(GameObject spriteObject, SpriteRenderer spriteRenderer)
        {
            var lifetime = _config != null ? _config.BackgroundSlashLifetime : 0f;
            var fadeTime = _config != null ? _config.BackgroundSlashFadeTime : 0f;
            var sequence = DOTween.Sequence()
                .SetLink(spriteObject)
                .AppendInterval(lifetime);

            if (fadeTime <= 0f)
            {
                sequence.AppendCallback(() =>
                {
                    var color = spriteRenderer.color;
                    color.a = 0f;
                    spriteRenderer.color = color;
                });
            }
            else
            {
                sequence.Append(spriteRenderer.DOFade(0f, fadeTime).SetEase(Ease.Linear));
            }

            sequence.AppendCallback(() => UnityEngine.Object.Destroy(spriteObject));

            if (_config != null && _config.UseUnscaledTime)
            {
                sequence.SetUpdate(true);
            }
        }

        private void PlayHitEffect(
            Transform rotationTarget,
            Transform hitRoot,
            Vector3 slashDelta,
            float projectorHeight,
            int streak)
        {
            if (rotationTarget == null ||
                rotationTarget == hitRoot ||
                _hitEffectConfig == null ||
                !_hitEffectConfig.RotationEnabled ||
                _hitEffectConfig.RotationDuration <= 0f)
            {
                return;
            }

            var slashDirection = slashDelta.normalized;

            if (slashDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            var worldRotationAxis = Vector3.Cross(_camera.transform.forward, slashDirection).normalized;
            var localRotationAxis = rotationTarget.InverseTransformDirection(worldRotationAxis).normalized;
            var rotationAngle =
                GetHitRotationAngle(projectorHeight) * _hitEffectConfig.GetRotationStrengthMultiplier(streak) * -1;
            var rotationOffset = localRotationAxis * rotationAngle;
            var rotationTween = rotationTarget
                .DOBlendableLocalRotateBy(
                    rotationOffset,
                    _hitEffectConfig.RotationDuration,
                    RotateMode.LocalAxisAdd)
                .SetEase(_hitEffectConfig.RotationEase)
                .SetLink(rotationTarget.gameObject);

            if (_hitEffectConfig.UseUnscaledTime)
            {
                rotationTween.SetUpdate(true);
            }
        }

        private void PlayScreenShake(int streak)
        {
            if (_camera == null ||
                _hitEffectConfig == null ||
                !_hitEffectConfig.ScreenShakeEnabled ||
                _hitEffectConfig.ScreenShakeDuration <= 0f)
            {
                return;
            }

            var strength = _hitEffectConfig.GetScreenShakeStrength(streak);

            if (strength <= 0f)
            {
                return;
            }

            StopScreenShake();
            _cameraStartLocalPosition = _camera.transform.localPosition;

            _screenShakeTween = _camera.transform
                .DOShakePosition(
                    _hitEffectConfig.ScreenShakeDuration,
                    new Vector3(strength, strength, 0f),
                    _hitEffectConfig.ScreenShakeVibrato,
                    _hitEffectConfig.ScreenShakeRandomness,
                    false,
                    true)
                .SetLink(_camera.gameObject)
                .OnKill(RestoreCameraPosition);

            if (_hitEffectConfig.UseUnscaledTime)
            {
                _screenShakeTween.SetUpdate(true);
            }
        }

        private void StopScreenShake()
        {
            if (_screenShakeTween != null && _screenShakeTween.IsActive())
            {
                _screenShakeTween.Kill(false);
                return;
            }

            RestoreCameraPosition();
        }

        private void RestoreCameraPosition()
        {
            if (_camera != null)
            {
                _camera.transform.localPosition = _cameraStartLocalPosition;
            }

            _screenShakeTween = null;
        }

        private float GetHitRotationAngle(float projectorHeight)
        {
            var minHeight = _config != null ? Mathf.Max(0f, _config.DecalProjectorMinHeight) : 3f;
            var maxHeight = _config != null ? Mathf.Max(minHeight, _config.DecalProjectorMaxHeight) : 8f;
            var heightProgress = maxHeight > minHeight
                ? Mathf.InverseLerp(minHeight, maxHeight, projectorHeight)
                : 1f;

            return Mathf.Lerp(
                _hitEffectConfig.MinRotationAngle,
                _hitEffectConfig.MaxRotationAngle,
                heightProgress);
        }

        private static float GetSlashZRotation(Vector3 delta)
        {
            return Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90f;
        }

        private float GetProjectorHeight(float slashLength)
        {
            var minHeight = _config != null ? Mathf.Max(0f, _config.DecalProjectorMinHeight) : 3f;
            var maxHeight = _config != null ? Mathf.Max(minHeight, _config.DecalProjectorMaxHeight) : 8f;
            var minSlashLength = _config != null ? Mathf.Max(0f, _config.MinDragWorldDistance) : 0f;

            if (minSlashLength <= Mathf.Epsilon)
            {
                return Mathf.Clamp(slashLength, minHeight, maxHeight);
            }

            var height = minHeight * (slashLength / minSlashLength);
            return Mathf.Clamp(height, minHeight, maxHeight);
        }

        private float GetBackgroundSlashHeight(float slashLength)
        {
            var minHeight = _config != null ? _config.BackgroundSlashMinHeight : 2.5f;
            var maxHeight = _config != null ? _config.BackgroundSlashMaxHeight : 10f;
            var minSlashLength = _config != null ? Mathf.Max(0f, _config.MinDragWorldDistance) : 0f;

            if (minSlashLength <= Mathf.Epsilon)
            {
                return Mathf.Clamp(slashLength, minHeight, maxHeight);
            }

            var height = minHeight * (slashLength / minSlashLength);
            return Mathf.Clamp(height, minHeight, maxHeight);
        }

        private Vector3 GetSpawnPosition(Transform decalPoint, Vector3 startWorldPosition, Vector3 endWorldPosition)
        {
            var slashMidpoint = Vector3.Lerp(startWorldPosition, endWorldPosition, 0.5f);
            return ProjectToPlane(decalPoint, slashMidpoint);
        }

        private Vector3 ProjectToPlane(Transform planePoint, Vector3 worldPosition)
        {
            var screenPosition = _camera.WorldToScreenPoint(worldPosition);
            var screenRay = _camera.ScreenPointToRay(screenPosition);
            var projectorPlane = new Plane(_camera.transform.forward, planePoint.position);

            if (projectorPlane.Raycast(screenRay, out var distance))
            {
                return screenRay.GetPoint(distance);
            }

            var planeOffset = Vector3.ProjectOnPlane(
                worldPosition - planePoint.position,
                _camera.transform.forward);
            return planePoint.position + planeOffset;
        }
    }
}
