using System;
using System.Threading.Tasks;
using Code.Gameplay.Behaviour;
using UnityEngine;

namespace Code.Gameplay.Services
{
    public class FallService : IFallService
    {
        private const float _fallSpeed = 2500f;
        private const int _checkIntervalMs = 1;

        public void StartFall(RectTransform rectTransform, Action<bool> onCollisionDetected)
        {
            Collider2D[] initialColliders = CheckCollisionsBelow(rectTransform);

            if (initialColliders.Length == 1 && GetColliderByTag(initialColliders, "Floor") != null)
            {
                onCollisionDetected?.Invoke(true);
                return;
            }

            if (initialColliders.Length > 0)
            {
                Collider2D objectCollider = GetColliderByTag(initialColliders, "Object");
                if (objectCollider != null)
                {
                    InnerColliderReference handler = objectCollider.GetComponent<InnerColliderReference>();
                    if (handler != null && handler.innerCollider != null)
                    {
                        if (!IsInsideCollider(rectTransform, handler.innerCollider))
                        {
                            Vector3 closestPoint = handler.innerCollider.ClosestPoint(rectTransform.position);
                            MoveObjectSmoothlyToPosition(rectTransform, closestPoint, 0.3f);
                        }
                    }

                    onCollisionDetected?.Invoke(true);
                    return;
                }
            }

            FallRoutine(rectTransform, onCollisionDetected);
        }

        private async void FallRoutine(RectTransform rectTransform, Action<bool> onCollisionDetected)
        {
            bool isFalling = true;

            while (isFalling)
            {
                Collider2D[] collidersBelow = CheckCollisionsBelow(rectTransform);

                if (collidersBelow.Length > 0)
                {
                    Collider2D objectCollider = GetColliderByTag(collidersBelow, "Object");
                    if (objectCollider == null)
                    {
                        Collider2D floorCollider = GetColliderByTag(collidersBelow, "Floor");
                        if (floorCollider != null)
                        {
                            isFalling = false;
                            onCollisionDetected?.Invoke(false);
                        }
                    }
                }

                if (isFalling)
                {
                    rectTransform.anchoredPosition += Vector2.down * _fallSpeed * Time.deltaTime;
                }

                await Task.Delay(_checkIntervalMs);
            }
        }

        private Collider2D[] CheckCollisionsBelow(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            Vector3 bottomLeft = corners[0];
            Vector3 bottomRight = corners[3];

            return Physics2D.OverlapAreaAll(bottomLeft, bottomRight);
        }

        private Collider2D GetColliderByTag(Collider2D[] colliders, string tag)
        {
            foreach (var collider in colliders)
            {
                if (collider.CompareTag(tag))
                {
                    return collider;
                }
            }
            return null;
        }

        private bool IsInsideCollider(RectTransform rectTransform, Collider2D collider)
        {
            return collider.OverlapPoint(rectTransform.position);
        }

        private async void MoveObjectSmoothlyToPosition(RectTransform rectTransform, Vector3 targetPosition, float duration)
        {
            Vector3 startPosition = rectTransform.position;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                rectTransform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                await Task.Yield();
            }

            rectTransform.position = targetPosition;
        }
    }
}
