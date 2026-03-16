using Solitaire.Domain;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Solitaire.Presentation.UIToolkit
{
    /// <summary>
    /// A custom VisualElement representing a single playing card in UI Toolkit.
    /// Handles rendering (front/back), click, and drag interactions.
    /// Replaces the old uGUI CardView for visual representation.
    /// </summary>
    public class CardElement : VisualElement
    {
        public Card Model { get; private set; }

        // Child elements
        private VisualElement _frontFace;
        private VisualElement _backFace;
        private Label _rankLabel;
        private VisualElement _suitSmall;
        private VisualElement _suitCenter;

        // State
        private bool _canInteract = true;
        private bool _isDragging;
        private Vector2 _dragStartPointer;
        private PileElement _parentPile;

        // Aspect ratio: 5:7 card proportions
        public const float CardAspectRatio = 7f / 5f;

        // Events
        public event Action<CardElement> OnClicked;
        public event Action<CardElement> OnDragStarted;
        public event Action<CardElement, Vector2> OnDragging;
        public event Action<CardElement, Vector2> OnDragEnded;
        public event Action<CardElement> OnMoveCompleted;
        public event Action<CardElement> OnFlipCompleted;

        public PileElement ParentPile
        {
            get => _parentPile;
            set => _parentPile = value;
        }

        public CardElement()
        {
            BuildVisualTree();
            RegisterCallbacks();
        }

        public CardElement(Card model) : this()
        {
            Initialize(model);
        }

        private void BuildVisualTree()
        {
            AddToClassList("card");

            // Front face
            _frontFace = new VisualElement();
            _frontFace.AddToClassList("card-front");

            _rankLabel = new Label();
            _rankLabel.AddToClassList("card-rank-label");
            _frontFace.Add(_rankLabel);

            _suitSmall = new VisualElement();
            _suitSmall.AddToClassList("card-suit-small");
            _frontFace.Add(_suitSmall);

            var centerContainer = new VisualElement();
            centerContainer.AddToClassList("card-suit-center");
            _suitCenter = new VisualElement();
            _suitCenter.AddToClassList("card-suit-center-img");
            centerContainer.Add(_suitCenter);
            _frontFace.Add(centerContainer);

            Add(_frontFace);

            // Back face
            _backFace = new VisualElement();
            _backFace.AddToClassList("card-back");
            var backInner = new VisualElement();
            backInner.AddToClassList("card-back-inner");
            _backFace.Add(backInner);
            Add(_backFace);

            // Enforce aspect ratio: when width changes, set height = width * 1.4
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            float w = resolvedStyle.width;
            if (w > 0 && !_isDragging)
            {
                float targetH = w * CardAspectRatio;
                float currentH = resolvedStyle.height;
                if (Mathf.Abs(currentH - targetH) > 1f)
                {
                    style.height = targetH;
                }
            }
        }

        /// <summary>
        /// Set explicit size (used when dragging to preserve size outside pile).
        /// </summary>
        public void SetExplicitSize(float width, float height)
        {
            style.width = width;
            style.height = height;
        }

        /// <summary>
        /// Reset to percentage-based width with auto aspect ratio.
        /// </summary>
        public void ResetToFlowSize()
        {
            style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            // Height will be set by GeometryChangedEvent
            style.height = StyleKeyword.Auto;
        }

        public void Initialize(Card model)
        {
            Model = model;
            name = $"Card_{model.Rank}_{model.Suit}";

            // Load suit sprites
            Sprite suitSprite = Resources.Load<Sprite>($"Suits/{model.Suit}");
            if (suitSprite != null)
            {
                _suitSmall.style.backgroundImage = new StyleBackground(suitSprite);
                _suitCenter.style.backgroundImage = new StyleBackground(suitSprite);
            }

            // Load face card art for J/Q/K
            string colorFolder = model.IsRed ? "Red" : "Black";
            switch (model.Rank)
            {
                case Rank.Ace:
                    _rankLabel.text = "A";
                    break;
                case Rank.Jack:
                    _rankLabel.text = "J";
                    _suitCenter.style.backgroundImage = new StyleBackground(
                        Resources.Load<Sprite>($"Ranks/{colorFolder}/{model.Rank}"));
                    break;
                case Rank.Queen:
                    _rankLabel.text = "Q";
                    _suitCenter.style.backgroundImage = new StyleBackground(
                        Resources.Load<Sprite>($"Ranks/{colorFolder}/{model.Rank}"));
                    break;
                case Rank.King:
                    _rankLabel.text = "K";
                    _suitCenter.style.backgroundImage = new StyleBackground(
                        Resources.Load<Sprite>($"Ranks/{colorFolder}/{model.Rank}"));
                    break;
                default:
                    _rankLabel.text = ((int)model.Rank).ToString();
                    break;
            }

            // Color
            if (model.IsRed)
            {
                _rankLabel.AddToClassList("card-red");
                _rankLabel.RemoveFromClassList("card-black");
            }
            else
            {
                _rankLabel.AddToClassList("card-black");
                _rankLabel.RemoveFromClassList("card-red");
            }

            UpdateFaceUpStatus();
        }

        public void UpdateFaceUpStatus()
        {
            bool isFaceUp = Model?.IsFaceUp ?? false;
            _frontFace.style.display = isFaceUp ? DisplayStyle.Flex : DisplayStyle.None;
            _backFace.style.display = isFaceUp ? DisplayStyle.None : DisplayStyle.Flex;
        }

        public void SetInteractable(bool interactable)
        {
            _canInteract = interactable;
        }

        // --- Input Callbacks ---

        private void RegisterCallbacks()
        {
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!_canInteract) return;
            if (evt.button != 0) return;

            _dragStartPointer = evt.position;
            _isDragging = false;

            this.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_canInteract) return;
            if (!this.HasPointerCapture(evt.pointerId)) return;

            Vector2 delta = (Vector2)evt.position - _dragStartPointer;

            // Threshold before starting drag (5px)
            if (!_isDragging && delta.magnitude > 5f)
            {
                if (Model == null || !Model.IsFaceUp) return;

                _isDragging = true;
                AddToClassList("card--dragging");
                OnDragStarted?.Invoke(this);
            }

            if (_isDragging)
            {
                OnDragging?.Invoke(this, (Vector2)evt.position);
                evt.StopPropagation();
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!this.HasPointerCapture(evt.pointerId)) return;
            this.ReleasePointer(evt.pointerId);

            if (!_canInteract) return;

            if (_isDragging)
            {
                _isDragging = false;
                RemoveFromClassList("card--dragging");
                OnDragEnded?.Invoke(this, (Vector2)evt.position);
            }
            else
            {
                // It was a click (no significant drag)
                OnClicked?.Invoke(this);
            }

            evt.StopPropagation();
        }

        // --- Animation helpers ---

        public void SetPositionY(float y)
        {
            style.top = y;
        }

        public float GetPositionY()
        {
            return resolvedStyle.top;
        }

        public void NotifyMoveCompleted()
        {
            OnMoveCompleted?.Invoke(this);
        }

        public void NotifyFlipCompleted()
        {
            OnFlipCompleted?.Invoke(this);
        }
    }
}
