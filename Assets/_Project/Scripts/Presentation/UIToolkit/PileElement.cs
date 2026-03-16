using Solitaire.Domain;
using Solitaire.Domain.Piles;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Solitaire.Presentation.UIToolkit
{
    /// <summary>
    /// Represents a pile (foundation, tableau, stock, waste) in UI Toolkit.
    /// Manages card stacking offsets and acts as drop target.
    /// </summary>
    public class PileElement : VisualElement
    {
        public CardPile Model { get; private set; }

        private float _faceUpOffset;
        private float _faceDownOffset;
        private bool _isTableau;

        private readonly List<CardElement> _cardElements = new List<CardElement>();
        public IReadOnlyList<CardElement> CardElements => _cardElements;

        public PileElement()
        {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public void Initialize(CardPile model, float faceUpOffset, float faceDownOffset)
        {
            Model = model;
            _faceUpOffset = faceUpOffset;
            _faceDownOffset = faceDownOffset;
            _isTableau = model is TableauPile;

            // GeometryChanged may have already fired before Initialize was called,
            // so force the size check now.
            ApplySizeFromWidth();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            ApplySizeFromWidth();
        }

        /// <summary>
        /// Sets the pile's height based on its current width and the card aspect ratio (5:7).
        /// For non-tableau piles (foundation, stock, waste): fixes exact height = card height.
        /// For tableau piles: sets min-height = one card height so the empty placeholder
        /// matches, but the pile can still grow taller as cards are stacked.
        /// </summary>
        private void ApplySizeFromWidth()
        {
            float w = resolvedStyle.width;
            if (w <= 0) return;

            float cardH = w * CardElement.CardAspectRatio;

            if (_isTableau)
            {
                float currentMin = resolvedStyle.minHeight.value;
                if (Mathf.Abs(currentMin - cardH) > 1f)
                    style.minHeight = cardH;
            }
            else
            {
                float currentH = resolvedStyle.height;
                if (Mathf.Abs(currentH - cardH) > 1f)
                    style.height = cardH;
            }
        }

        public void AddCard(CardElement cardElement)
        {
            if (!_cardElements.Contains(cardElement))
                _cardElements.Add(cardElement);

            cardElement.ParentPile = this;

            if (cardElement.parent != this)
                Add(cardElement);

            RepositionCard(cardElement);
        }

        public void RemoveCard(CardElement cardElement)
        {
            _cardElements.Remove(cardElement);
            if (cardElement.ParentPile == this)
                cardElement.ParentPile = null;
        }

        public float GetCardPositionY(Card targetCard)
        {
            float position = 0;
            var cards = Model.GetCardsReverse();

            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] == targetCard)
                    break;
                position += cards[i].IsFaceUp ? _faceUpOffset : _faceDownOffset;
            }

            return position;
        }

        public float GetNextCardPositionY()
        {
            float position = 0;

            for (int i = 0; i < _cardElements.Count; i++)
            {
                var card = _cardElements[i].Model;
                if (card != null)
                    position += card.IsFaceUp ? _faceUpOffset : _faceDownOffset;
            }

            return position;
        }

        public void RepositionCard(CardElement cardElement)
        {
            if (cardElement.Model == null) return;

            float y = GetCardPositionY(cardElement.Model);
            cardElement.style.top = y;
            cardElement.style.left = 0;
            cardElement.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        }

        public void RepositionAllCards()
        {
            for (int i = 0; i < _cardElements.Count; i++)
                RepositionCard(_cardElements[i]);
        }

        public List<CardElement> GetCardsFromTo(CardElement startCard)
        {
            var result = new List<CardElement>();
            bool found = false;

            for (int i = 0; i < _cardElements.Count; i++)
            {
                if (_cardElements[i] == startCard)
                    found = true;
                if (found)
                    result.Add(_cardElements[i]);
            }

            return result;
        }

        public void ClearCards()
        {
            _cardElements.Clear();
        }
    }
}
