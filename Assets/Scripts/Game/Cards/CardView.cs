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
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Collider2D _collider;

    // State
    private Transform originalParent;
    private Vector2 dragOffset;
    private bool isDragging = false;

    // This must be set by the Presenter on spawn!
    public Transform TopLevelCanvasTransform { get; set; }

    public Action OnCardMoveCompleted;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
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

    // --- Input Reporting (IPointer interfaces) ---

    public void OnPointerClick(PointerEventData eventData)
    {
        // Don't register click if it was the end of a drag
        if (isDragging)
        {
            isDragging = false; // Reset flag
            return;
        }

        GameEvents.RaiseCardClicked(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!Model.IsFaceUp) return; // Can't drag face-down cards

        isDragging = true;
        GameEvents.WasDropSuccessfulThisFrame = false; // Reset flag

        // Store original parent (its PileView) to return to if drag fails
        originalParent = transform.parent;

        // Gather all cards in the stack being dragged
        int thisIndex = transform.GetSiblingIndex();
        int childCount = originalParent.childCount;
        for (int i = thisIndex + 1; i < childCount; i++)
        {
            var cardView = originalParent.GetChild(thisIndex + 1).GetComponent<CardView>();
            if (cardView != null)
            {
                cardView.transform.SetParent(_cardsHolder);
            }
        }

        // Re-parent to top-level canvas to render "above" all other UI
        transform.SetParent(TopLevelCanvasTransform);
        transform.SetAsLastSibling(); // Ensure it's on top

        // Block raycasts so we can detect drops on Piles underneath
        canvasGroup.blocksRaycasts = false;
        _collider.enabled = true;

        // Calculate offset from card pivot to mouse position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out dragOffset
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // Follow mouse
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            TopLevelCanvasTransform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            rectTransform.localPosition = localPoint - dragOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // Re-enable raycasts
        canvasGroup.blocksRaycasts = true;

        // Check overlapping drop zones
        var piles = overlappingZones.Select(el=> el.PileView).ToList();

        GameEvents.RaiseCardDroppedOnPiles(this, piles);

        _collider.enabled = false;
        isDragging = false;
    }

    // --- Public methods for Presenter to call ---

    public void AnimateMove(PileView newParent, Vector2 targetAnchoredPosition, float duration = 0.15f)
    {
        // Start animation
        StartCoroutine(MoveCoroutine(newParent, targetAnchoredPosition, duration));
    }

    private IEnumerator MoveCoroutine(PileView newParent, Vector2 target, float duration)
    {
        newParent.ParentToPile(this);

        Vector2 startPos = rectTransform.anchoredPosition;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, target, time / duration);
            yield return null;
        }
        rectTransform.anchoredPosition = target;

        // Re-parent any child cards to the new parent as well
        int childCount = _cardsHolder.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var cardView = _cardsHolder.GetChild(0).GetComponent<CardView>();
            if (cardView != null)
            {
                newParent.ParentToPile(cardView);
            }
        }

        OnCardMoveCompleted?.Invoke();
    }

    public void AnimateFlip(float duration = 0.2f)
    {
        StartCoroutine(FlipCoroutine(duration));
    }

    private IEnumerator FlipCoroutine(float duration)
    {
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
            Debug.Log($"CardView exited DropZone: {zone.gameObject.name}");
            // It is! Remove it from our list.
            overlappingZones.Remove(zone);
        }
    }
}
