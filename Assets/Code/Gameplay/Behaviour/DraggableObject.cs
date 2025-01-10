using Code.Gameplay.Services;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Code.Gameplay.Behaviour
{
    public class DragAndDropUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler
    {
        private IMouseDirectionService _directionService;
        private IFallService _fallService;

        private const float _scrollSpeed = 5f;
        private const float _scrollEdgeDistance = 100f;

        private bool _isDragging = false;
        private RectTransform _rectTransform;
        private Canvas _canvas;
        private ScrollRect _scrollRect;
        private Vector2 _offset;

        [Inject]
        public void Construct(IMouseDirectionService service, IFallService fallService)
        {
            _directionService = service;
            _fallService = fallService;
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            _scrollRect = GetComponentInParent<ScrollRect>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_scrollRect != null)
                _scrollRect.enabled = false;

            _isDragging = true;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                Input.mousePosition,
                _canvas.worldCamera,
                out Vector2 localMousePosition
            );
            _offset = _rectTransform.anchoredPosition - localMousePosition;

            _directionService.StartTracking(() => { Debug.Log("Tracking stopped"); });

            BeginDragging();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_scrollRect != null)
                _scrollRect.enabled = true;

            _isDragging = false;
            _directionService.StopTracking();
            _directionService.OnTargetPositionUpdated -= MoveObject;

            _rectTransform.DOScale(1f, 0.4f);

            _fallService.StartFall(_rectTransform, alreadyInCollider =>
            {
                if (!alreadyInCollider)
                {
                    AnimateImpact();
                }
            });
        }

        private void BeginDragging()
        {
            if (!_isDragging) return;

            _rectTransform.DOScale(1.5f, 0.4f);
            _directionService.OnTargetPositionUpdated += MoveObject;
        }

        private void MoveObject(Vector3 targetScreenPosition)
        {
            if (!_isDragging || _canvas == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                targetScreenPosition,
                _canvas.worldCamera,
                out Vector2 localMousePosition
            );

            _rectTransform.anchoredPosition = localMousePosition + _offset;

            Vector3 screenPos = _rectTransform.position;
            Vector3 screenMin = new Vector3(_scrollEdgeDistance, 0, 0);
            Vector3 screenMax = new Vector3(Screen.width - _scrollEdgeDistance, 0, 0);

            if (screenPos.x < screenMin.x)
            {
                _scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
                    _scrollRect.horizontalNormalizedPosition,
                    _scrollRect.horizontalNormalizedPosition - Time.deltaTime * _scrollSpeed,
                    0.1f
                );
            }
            else if (screenPos.x > screenMax.x)
            {
                _scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
                    _scrollRect.horizontalNormalizedPosition,
                    _scrollRect.horizontalNormalizedPosition + Time.deltaTime * _scrollSpeed,
                    0.1f
                );
            }
        }


        private void AnimateImpact()
        {
            float originalY = _rectTransform.anchoredPosition.y;

            _rectTransform.DOAnchorPosY(originalY + 30, 0.1f)
                .OnComplete(() =>
                {
                    _rectTransform.DOAnchorPosY(originalY, 0.1f)
                        .OnComplete(() =>
                        {
                            _rectTransform.DOAnchorPosY(originalY + 15, 0.1f)
                                .OnComplete(() => _rectTransform.DOAnchorPosY(originalY, 0.1f));
                        });
                });
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_scrollRect != null)
                _scrollRect.enabled = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_scrollRect != null)
                _scrollRect.enabled = true;
        }
    }
}
