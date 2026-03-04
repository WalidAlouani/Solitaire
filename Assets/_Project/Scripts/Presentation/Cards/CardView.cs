using Solitaire.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Solitaire.Presentation
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
        private Collider2D _collider;

        // State
        private Transform _originalParent;
        private Vector2 _dragOffset;
        private bool _isDragging;
        private bool _canInteract = true;

        // This must be set by the spawner on creation
        public Transform TopLevelCanvasTransform { get; set; }

        public event Action<CardView> OnCardMoveCompleted;
        public event Action<CardView> OnCardFlipCompleted;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _collider = GetComponent<Collider2D>();
        }

        public void Initialize(Card model)
        {
            Model = model;
            gameObject.name = $"{model.Rank} of {model.Suit}";

            _suitImageSmall.sprite = Resources.Load<Sprite>($"Suits/{model.Suit}");

            string colorFolder = model.IsRed ? "Red" : "Black";

            switch (model.Rank)
            {
                case Rank.Ace:
                    _rankText.text = "A";
                    _suitImage.sprite = Resources.Load<Sprite>($"Suits/{model.Suit}");
                    break;
                case Rank.Jack:
                case Rank.Queen:
                case Rank.King:
                    _rankText.text = model.Rank == Rank.Jack ? "J" : model.Rank == Rank.Queen ? "Q" : "K";
                    _suitImage.sprite = Resources.Load<Sprite>($"Ranks/{colorFolder}/{model.Rank}");
                    break;
                default:
                    _rankText.text = ((int)model.Rank).ToString();
                    _suitImage.sprite = Resources.Load<Sprite>($"Suits/{model.Suit}");
                    break;
            }

            _rankText.color = model.IsRed ? Color.red : Color.black;

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

            GameEvents.RaiseCardClicked(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_canInteract) return;
            if (!Model.IsFaceUp) return;

            _isDragging = true;
            GameEvents.WasDropSuccessfulThisFrame = false;

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
            _collider.enabled = true;

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

            // Collect overlapping pile views without LINQ
            var piles = new List<PileView>(_overlappingZones.Count);
            for (int i = 0; i < _overlappingZones.Count; i++)
                piles.Add(_overlappingZones[i].PileView);

            GameEvents.RaiseCardDroppedOnPiles(this, piles);

            _collider.enabled = false;
            _isDragging = false;
        }

        // --- Animation ---

        public void AnimateMove(PileView newParent, float duration = 0.2f)
        {
            StartCoroutine(MoveCoroutine(newParent, duration));
        }

        private IEnumerator MoveCoroutine(PileView newParent, float duration)
        {
            Vector2 target = new Vector2(0, newParent.GetCardPosition(Model));

            transform.SetParent(TopLevelCanvasTransform);

            RectTransform newParentCardsHolder = newParent.CardsHolder;
            Vector3 worldTargetPos = newParentCardsHolder.TransformPoint(new Vector3(target.x, target.y, 0));
            Vector3 canvasSpacePos = TopLevelCanvasTransform.InverseTransformPoint(worldTargetPos);
            Vector2 targetCanvasPos = new Vector2(canvasSpacePos.x, canvasSpacePos.y);

            Vector2 startPos = _rectTransform.anchoredPosition;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                _rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetCanvasPos, time / duration);
                yield return null;
            }

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
        }

        public void AnimateFlip(float duration = 0.2f)
        {
            StartCoroutine(FlipCoroutine(duration));
        }

        private IEnumerator FlipCoroutine(float duration)
        {
            float time = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 halfScale = new Vector3(0, startScale.y, startScale.z);

            while (time < duration / 2)
            {
                time += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, halfScale, time / (duration / 2));
                yield return null;
            }

            UpdateFaceUpStatus();

            time = 0f;
            while (time < duration / 2)
            {
                time += Time.deltaTime;
                transform.localScale = Vector3.Lerp(halfScale, startScale, time / (duration / 2));
                yield return null;
            }
            transform.localScale = startScale;

            OnCardFlipCompleted?.Invoke(this);
        }

        // --- Physics Trigger Events ---

        private readonly List<DropZone> _overlappingZones = new List<DropZone>();

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out DropZone zone))
            {
                if (!_overlappingZones.Contains(zone))
                    _overlappingZones.Add(zone);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out DropZone zone))
                _overlappingZones.Remove(zone);
        }
    }
}