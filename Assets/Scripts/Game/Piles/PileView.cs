using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PileView : MonoBehaviour
{
    [Header("Stacking (Tableau)")]
    [SerializeField] private RectTransform _cardsHolder;
    [SerializeField] private float _faceUpOffset = 0;
    [SerializeField] private float _faceDownOffset = 0;

    public CardPile Model { get; private set; }
    public RectTransform CardsHolder => _cardsHolder;

    public void Initialize(CardPile model)
    {
        Model = model;
        gameObject.name = model.GetType().Name;
    }

    public void ParentToPile(CardView cardView)
    {
        cardView.transform.SetParent(_cardsHolder);
        cardView.transform.SetAsLastSibling();
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
        {
            Debug.LogWarning("PileView.GetCardPosition called for a card not in the pile!");
            return 0;
        }

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
}
