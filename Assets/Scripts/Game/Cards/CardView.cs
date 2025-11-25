using Solitaire.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class CardView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Card Model { get; private set; }

    // create scriptable object for card data
    [Header("UI References")]
    [SerializeField] private Image frontImage;
    [SerializeField] private Image backImage;
    [SerializeField] private Image suitImage;
    [SerializeField] private Image suitImageSmall;
    [SerializeField] private TMP_Text _rankText;

    [SerializeField] private Transform _cardsHolder;

    // Components
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Collider2D _collider;

    // State
    private Transform _originalParent;
    private Vector2 _dragOffset;
    private bool _isDragging = false;
    private bool _canInteract = false;

    // This must be set by the Presenter on spawn!
    public Transform TopLevelCanvasTransform { get; set; }

    public Action<CardView> OnCardMoveStarted;
    public Action<CardView> OnCardMoveCompleted;
    public Action<CardView> OnCardFlipStarted;
    public Action<CardView> OnCardFlipCompleted;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _collider = GetComponent<Collider2D>();
    }

    public void Initialize(Card model)
    {
        Model = model;
        gameObject.name = $"{model.Rank} of {model.Suit}";

        suitImageSmall.sprite = Resources.Load<Sprite>($"Suits/{model.Suit}");

        switch (model.Rank)
        {
            case Rank.Ace:
                _rankText.text = "A";
                var color1 = model.IsRed ? "Red" : "Black";
                suitImage.sprite = Resources.Load<Sprite>($"Suits/{model.Suit}");
                break;
            case Rank.Jack:
                _rankText.text = "J";
                var color2 = model.IsRed ? "Red" : "Black";
                suitImage.sprite = Resources.Load<Sprite>($"Ranks/{color2}/{model.Rank}");
                break;
            case Rank.Queen:
                _rankText.text = "Q";
                var color3 = model.IsRed ? "Red" : "Black";
                suitImage.sprite = Resources.Load<Sprite>($"Ranks/{color3}/{model.Rank}");
                break;
            case Rank.King:
                _rankText.text = "K";
                var color4 = model.IsRed ? "Red" : "Black";
                suitImage.sprite = Resources.Load<Sprite>($"Ranks/{color4}/{model.Rank}");
                break;
            default:
                _rankText.text = ((int)model.Rank).ToString();
                suitImage.sprite = Resources.Load<Sprite>($"Suits/{model.Suit}");
                break;
        }

        _rankText.color = model.IsRed ? Color.red : Color.black;

        UpdateFaceUpStatus();
    }

    public void UpdateFaceUpStatus()
    {
        bool isFaceUp = Model?.IsFaceUp ?? false;
        frontImage.gameObject.SetActive(isFaceUp);
        backImage.gameObject.SetActive(!isFaceUp);
    }

    public void SetInteractable(bool interactable)
    {
        _canInteract = interactable;
    }

    // --- Input Reporting (IPointer interfaces) ---

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_canInteract) return;

        // Don't register click if it was the end of a drag
        if (_isDragging)
        {
            _isDragging = false; // Reset flag
            return;
        }

        GameEvents.RaiseCardClicked(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_canInteract) return;

        if (!Model.IsFaceUp) return; // Can't drag face-down cards

        _isDragging = true;
        GameEvents.WasDropSuccessfulThisFrame = false; // Reset flag

        // Store original parent (its PileView) to return to if drag fails
        _originalParent = transform.parent;

        // Gather all cards in the stack being dragged
        int thisIndex = transform.GetSiblingIndex();
        int childCount = _originalParent.childCount;
        for (int i = thisIndex + 1; i < childCount; i++)
        {
            var cardView = _originalParent.GetChild(thisIndex + 1).GetComponent<CardView>();
            if (cardView != null)
            {
                cardView.transform.SetParent(_cardsHolder);
            }
        }

        // Re-parent to top-level canvas to render "above" all other UI
        transform.SetParent(TopLevelCanvasTransform);
        transform.SetAsLastSibling(); // Ensure it's on top

        // Block raycasts so we can detect drops on Piles underneath
        _canvasGroup.blocksRaycasts = false;
        _collider.enabled = true;

        // Calculate offset from card pivot to mouse position
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

        // Follow mouse
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

        // Re-enable raycasts
        _canvasGroup.blocksRaycasts = true;

        // Check overlapping drop zones
        var piles = overlappingZones.Select(el => el.PileView).ToList();

        GameEvents.RaiseCardDroppedOnPiles(this, piles);

        _collider.enabled = false;
        _isDragging = false;
    }

    // --- Public methods for Presenter to call ---

    public void AnimateMove(PileView newParent, float duration = 0.2f)
    {
        // Start animation
        StartCoroutine(MoveCoroutine(newParent, duration));
    }

    private IEnumerator MoveCoroutine(PileView newParent, float duration)
    {
        OnCardMoveStarted?.Invoke(this);

        Vector2 target = new Vector2(0, newParent.GetCardPosition(Model));

        // Ensure card is parented to top-level canvas for animation
        transform.SetParent(TopLevelCanvasTransform);

        // Convert the local position in the destination pile to world space
        RectTransform newParentCardsHolder = newParent.CardsHolder;
        Vector3 worldTargetPos = newParentCardsHolder.TransformPoint(new Vector3(target.x, target.y, 0));

        // Convert world position to canvas-space anchored position
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
        _rectTransform.anchoredPosition = targetCanvasPos;

        // Now parent the card to the destination pile
        newParent.ParentToPile(this);

        // Re-parent any child cards to the new parent as well
        int childCount = _cardsHolder.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var cardView = _cardsHolder.GetChild(0).GetComponent<CardView>();
            if (cardView == null)
                continue;

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
        OnCardFlipStarted?.Invoke(this);

        // Simple flip: just scale X
        float time = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 halfScale = new Vector3(0, startScale.y, startScale.z);

        // Scale down
        while (time < duration / 2)
        {
            time += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, halfScale, time / (duration / 2));
            yield return null;
        }

        // --- The Flip Point ---
        UpdateFaceUpStatus(); // Update visuals
        // ---

        // Scale back up
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


    // --- PHYSICS TRIGGER EVENTS ---

    private List<DropZone> overlappingZones = new List<DropZone>();

    /// <summary>
    /// This event fires when our collider (attached to a Rigidbody)
    /// enters another collider that is set to 'Is Trigger'.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object we hit is a DropZone
        if (other.TryGetComponent(out DropZone zone))
        {
            // It is! Add it to our list of overlapped zones.
            if (!overlappingZones.Contains(zone))
            {
                overlappingZones.Add(zone);
            }
        }
    }

    /// <summary>
    /// This event fires when our collider leaves the trigger collider.
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the object we're leaving is a DropZone
        if (other.TryGetComponent(out DropZone zone))
        {
            // It is! Remove it from our list.
            overlappingZones.Remove(zone);
        }
    }
}
