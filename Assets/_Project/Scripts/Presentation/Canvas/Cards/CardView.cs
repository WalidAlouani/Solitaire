using DG.Tweening;
using Solitaire.Domain;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Solitaire.Presentation.Canvas
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class CardView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Card Model { get; private set; }

        [Header("UI References")]
        [SerializeField] private Image _frontImage;
        [SerializeField] private Image _backImage;
        [SerializeField] private Image _suitImage;
        [SerializeField] private Image _suitImageSmall;
        [SerializeField] private TMP_Text _rankText;
        [SerializeField] private Transform _cardsHolder;

        // Cached components
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;

        // Injected event bus (replaces static GameEvents)
        private ViewEventBus _eventBus;

        // Reference to spawner for pile overlap queries
        private CardSpawner _cardSpawner;

        // State
        private Transform _originalParent;
        private Vector2 _dragOffset;
        private bool _isDragging;
        private bool _canInteract = false;

        // Active tweens (for cleanup)
        private Sequence _moveSequence;
        private Sequence _flipSequence;
        private Sequence _shakeSequence;

        // Cached resting position for _frontImage so shake can always reset cleanly
        private Vector2 _frontImageRestPosition;
        private bool _frontImageRestCached;

        // This must be set by the spawner on creation
        public Transform TopLevelCanvasTransform { get; set; }

        public event Action<CardView> OnCardMoveCompleted;
        public event Action<CardView> OnCardFlipCompleted;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnDestroy()
        {
            KillActiveTweens();
        }

        /// <summary>
        /// Injects the instance-based event bus used for view → presenter communication.
        /// Must be called by CardSpawner immediately after instantiation.
        /// </summary>
        public void SetEventBus(ViewEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        /// <summary>
        /// Injects the CardSpawner reference for pile overlap queries during drag.
        /// Must be called by CardSpawner immediately after instantiation.
        /// </summary>
        public void SetCardSpawner(CardSpawner spawner)
        {
            _cardSpawner = spawner;
        }

        public void Initialize(Card model, CardThemeSO theme)
        {
            Model = model;
            gameObject.name = $"{model.Rank} of {model.Suit}";

            _suitImageSmall.sprite = theme.GetSuitSprite(model.Suit);
            _suitImage.sprite = theme.GetCenterSprite(model.Rank, model.Suit, model.IsRed);
            _rankText.text = theme.GetRankDisplayText(model.Rank);
            _rankText.color = theme.GetCardColor(model.IsRed);

            UpdateFaceUpStatus();
        }

        public void UpdateFaceUpStatus()
        {
            bool isFaceUp = Model?.IsFaceUp ?? false;
            _frontImage.gameObject.SetActive(isFaceUp);
            _backImage.gameObject.SetActive(!isFaceUp);
        }

        public void SetInteractable(bool interactable)
        {
            _canInteract = interactable;
        }

        // --- Input Reporting ---

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_canInteract) return;

            if (_isDragging)
            {
                _isDragging = false;
                return;
            }

            _eventBus.RaiseCardClicked(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_canInteract) return;
            if (!Model.IsFaceUp) return;

            _isDragging = true;

            _originalParent = transform.parent;

            // Gather all cards in the stack being dragged
            int thisIndex = transform.GetSiblingIndex();
            int childCount = _originalParent.childCount;
            for (int i = thisIndex + 1; i < childCount; i++)
            {
                var cardView = _originalParent.GetChild(thisIndex + 1).GetComponent<CardView>();
                if (cardView != null)
                    cardView.transform.SetParent(_cardsHolder);
            }

            transform.SetParent(TopLevelCanvasTransform);
            transform.SetAsLastSibling();

            _canvasGroup.blocksRaycasts = false;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out _dragOffset
            );
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                TopLevelCanvasTransform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
            {
                _rectTransform.localPosition = localPoint - _dragOffset;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            _canvasGroup.blocksRaycasts = true;

            // Find overlapping piles via RectTransform bounds check (no physics needed)
            var piles = FindOverlappingPiles();

            _eventBus.RaiseCardDroppedOnPiles(this, piles);

            _isDragging = false;
        }

        // --- RectTransform Overlap (replaces Physics2D triggers) ---

        /// <summary>
        /// Checks which PileViews the card currently overlaps by comparing
        /// world-space RectTransform bounds. No Collider2D or Rigidbody2D needed.
        /// </summary>
        private List<PileView> FindOverlappingPiles()
        {
            var result = new List<PileView>(4);
            var cardCorners = new Vector3[4];
            _rectTransform.GetWorldCorners(cardCorners);
            var cardRect = WorldCornersToRect(cardCorners);

            var pileCorners = new Vector3[4];
            foreach (var pileView in _cardSpawner.AllPileViews)
            {
                pileView.CardsHolder.GetWorldCorners(pileCorners);
                var pileRect = WorldCornersToRect(pileCorners);

                if (cardRect.Overlaps(pileRect))
                    result.Add(pileView);
            }

            return result;
        }

        /// <summary>
        /// Converts the 4 world corners from GetWorldCorners() into a Rect.
        /// Corners are: [0]=bottomLeft, [1]=topLeft, [2]=topRight, [3]=bottomRight.
        /// </summary>
        private static Rect WorldCornersToRect(Vector3[] corners)
        {
            float xMin = corners[0].x;
            float yMin = corners[0].y;
            float xMax = corners[2].x;
            float yMax = corners[2].y;
            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        // --- Animation ---

        public void AnimateMove(PileView newParent, float duration = 0.2f)
        {
            _moveSequence?.Kill();

            Vector2 target = new Vector2(0, newParent.GetCardPosition(Model));

            transform.SetParent(TopLevelCanvasTransform);

            RectTransform newParentCardsHolder = newParent.CardsHolder;
            Vector3 worldTargetPos = newParentCardsHolder.TransformPoint(new Vector3(target.x, target.y, 0));
            Vector3 canvasSpacePos = TopLevelCanvasTransform.InverseTransformPoint(worldTargetPos);
            Vector2 targetCanvasPos = new Vector2(canvasSpacePos.x, canvasSpacePos.y);
            Vector2 startPos = _rectTransform.anchoredPosition;

            _moveSequence = DOTween.Sequence();

            float progress = 0f;
            _moveSequence.Append(
                DOTween.To(
                    () => progress,
                    t =>
                    {
                        progress = t;
                        _rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetCanvasPos, t);
                    },
                    1f, duration
                ).SetEase(Ease.OutQuad)
            );

            _moveSequence.OnComplete(() =>
            {
                newParent.ParentToPile(this);
                _rectTransform.anchoredPosition = target;

                // Re-parent any child cards to the new pile as well
                int childCount = _cardsHolder.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    var cardView = _cardsHolder.GetChild(0).GetComponent<CardView>();
                    if (cardView == null) continue;
                    newParent.ParentToPile(cardView);
                }

                OnCardMoveCompleted?.Invoke(this);
            });
        }

        public void AnimateFlip(float duration = 0.2f)
        {
            _flipSequence?.Kill();

            float halfDuration = duration / 2f;
            Vector3 startScale = transform.localScale;
            float scaleX = startScale.x;

            _flipSequence = DOTween.Sequence();
            _flipSequence.Append(
                DOTween.To(
                    () => scaleX,
                    x =>
                    {
                        scaleX = x;
                        transform.localScale = new Vector3(x, startScale.y, startScale.z);
                    },
                    0f, halfDuration
                ).SetEase(Ease.InQuad)
            );
            _flipSequence.AppendCallback(() => UpdateFaceUpStatus());
            _flipSequence.Append(
                DOTween.To(
                    () => scaleX,
                    x =>
                    {
                        scaleX = x;
                        transform.localScale = new Vector3(x, startScale.y, startScale.z);
                    },
                    startScale.x, halfDuration
                ).SetEase(Ease.OutQuad)
            );
            _flipSequence.OnComplete(() =>
            {
                transform.localScale = startScale;
                OnCardFlipCompleted?.Invoke(this);
            });
        }

        /// <summary>
        /// Quick horizontal shake when the player clicks a card with no valid move.
        /// </summary>
        public void AnimateShake(float duration = 0.3f, float strength = 15f)
        {
            var rectTransform = _frontImage.rectTransform;

            // Cache the resting position once, then always reset to it before starting
            if (!_frontImageRestCached)
            {
                _frontImageRestPosition = rectTransform.anchoredPosition;
                _frontImageRestCached = true;
            }

            // Kill any in-progress shake and force-reset to resting position
            _shakeSequence?.Kill();
            rectTransform.anchoredPosition = _frontImageRestPosition;

            float shakeStep = duration / 6f;
            Vector2 origin = _frontImageRestPosition;

            _shakeSequence = DOTween.Sequence();

            float offsetX = 0f;
            _shakeSequence.Append(DOTween.To(() => offsetX, x => { offsetX = x; rectTransform.anchoredPosition = new Vector2(origin.x + x, origin.y); }, strength, shakeStep).SetEase(Ease.OutQuad));
            _shakeSequence.Append(DOTween.To(() => offsetX, x => { offsetX = x; rectTransform.anchoredPosition = new Vector2(origin.x + x, origin.y); }, -strength, shakeStep).SetEase(Ease.InOutQuad));
            _shakeSequence.Append(DOTween.To(() => offsetX, x => { offsetX = x; rectTransform.anchoredPosition = new Vector2(origin.x + x, origin.y); }, strength * 0.5f, shakeStep).SetEase(Ease.InOutQuad));
            _shakeSequence.Append(DOTween.To(() => offsetX, x => { offsetX = x; rectTransform.anchoredPosition = new Vector2(origin.x + x, origin.y); }, -strength * 0.5f, shakeStep).SetEase(Ease.InOutQuad));
            _shakeSequence.Append(DOTween.To(() => offsetX, x => { offsetX = x; rectTransform.anchoredPosition = new Vector2(origin.x + x, origin.y); }, strength * 0.2f, shakeStep).SetEase(Ease.InOutQuad));
            _shakeSequence.Append(DOTween.To(() => offsetX, x => { offsetX = x; rectTransform.anchoredPosition = new Vector2(origin.x + x, origin.y); }, 0f, shakeStep).SetEase(Ease.InQuad));

            _shakeSequence.OnComplete(() => rectTransform.anchoredPosition = origin);
        }

        private void KillActiveTweens()
        {
            _moveSequence?.Kill();
            _flipSequence?.Kill();
            _shakeSequence?.Kill();

            // Ensure front image is reset if shake was active
            if (_frontImageRestCached && _frontImage != null)
                _frontImage.rectTransform.anchoredPosition = _frontImageRestPosition;
        }
    }
}
