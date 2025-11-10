using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class PileView : MonoBehaviour, IDropHandler
{

    [Header("Stacking (Tableau)")]
    [SerializeField] private RectTransform _cardsHolder;
    [SerializeField] private float _faceUpOffset = 0;
    [SerializeField] private float _faceDownOffset = 0;

    public CardPile Model { get; private set; }
    public RectTransform CardsHolder => _cardsHolder;
    public float FaceUpOffset => _faceUpOffset;
    public float FaceDownOffset => _faceDownOffset;

    public void Initialize(CardPile model)
    {
        Model = model;
        gameObject.name = model.GetType().Name;
    }

    /// <summary>
    /// Called by UGUI when a drag ends on this object.
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        // Get the CardView being dragged
        CardView cardView = eventData.pointerDrag.GetComponent<CardView>();

        if (cardView == null)
            return;

        // Tell the system a drop occurred
        // The Presenter will validate this move
        GameEvents.RaiseCardDroppedOnPile(cardView, this);

        // Mark the drop as successful
        GameEvents.WasDropSuccessfulThisFrame = true;
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
        var cards = Model.GetCards();
        if (!cards.Contains(targetCard))
        {
            Debug.LogWarning("PileView.GetCardPosition called for a card not in the pile!");
            return 0;
        }

        cards.Reverse();
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
