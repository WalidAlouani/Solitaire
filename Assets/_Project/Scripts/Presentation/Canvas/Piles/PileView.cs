using Solitaire.Domain;
using Solitaire.Domain.Piles;
using UnityEngine;

namespace Solitaire.Presentation.Canvas
{
    [RequireComponent(typeof(RectTransform))]
    public class PileView : MonoBehaviour
    {
        [SerializeField] private RectTransform _pileVisual;
        [SerializeField] private RectTransform _cardsHolder;

        private float _faceUpOffset;
        private float _faceDownOffset;

        public CardPile Model { get; private set; }
        public RectTransform CardsHolder => _cardsHolder;

        public void Initialize(CardPile model, float faceUpOffset, float faceDownOffset)
        {
            Model = model;
            _faceUpOffset = faceUpOffset;
            _faceDownOffset = faceDownOffset;
            gameObject.name = model.GetType().Name;
        }

        public void ParentToPile(CardView cardView)
        {
            cardView.transform.SetParent(_cardsHolder);
        }

        /// <summary>
        /// Calculates the next card's UI position based on pile count.
        /// This is what makes the cards stack.
        /// </summary>
        public float GetNextCardPosition()
        {
            float position = 0;

            for (int i = 0; i < _cardsHolder.childCount; i++)
            {
                var cardView = _cardsHolder.GetChild(i).GetComponent<CardView>();
                if (cardView == null)
                {
                    Debug.LogWarning("PileView.GetNextCardPosition found a child without CardView!");
                    continue;
                }

                position += cardView.Model.IsFaceUp ? _faceUpOffset : _faceDownOffset;
            }

            return position;
        }

        public float GetCardPosition(Card targetCard)
        {
            var cards = Model.GetCardsReverse();
            if (!cards.Contains(targetCard))
                return 0;

            float position = 0;

            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                if (card == targetCard)
                    break;
                position += card.IsFaceUp ? _faceUpOffset : _faceDownOffset;
            }

            return position;
        }

        /// <summary>
        /// Scales only the visual children (Image, DottedOutline) without affecting CardsHolder.
        /// Used by pile punch animation to avoid scaling cards.
        /// </summary>
        public void SetVisualsScale(float scale)
        {
            _pileVisual.localScale = new Vector3(scale, scale, 1f);
        }

        /// <summary>
        /// Resets all visual children to scale (1,1,1).
        /// </summary>
        public void ResetVisualsScale()
        {

            _pileVisual.localScale = Vector3.one;
        }
    }
}
