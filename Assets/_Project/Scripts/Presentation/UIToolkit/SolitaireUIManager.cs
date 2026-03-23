using Solitaire.Application;
using Solitaire.Domain;
using Solitaire.Domain.Piles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Solitaire.Presentation.UIToolkit
{
    /// <summary>
    /// Main UI Toolkit controller for the Solitaire game.
    /// Replaces the old uGUI-based GamePresenter + CardSpawner + PileView + DropZone system.
    /// Manages the UIDocument, spawns CardElements, handles drag/drop, and drives animations.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class SolitaireUIManager : MonoBehaviour, IGameUI
    {
        [Header("Stacking Offsets (positive = downward cascade)")]
        [SerializeField] private float _tableauFaceUpOffset = 30f;
        [SerializeField] private float _tableauFaceDownOffset = 15f;
        [SerializeField] private float _stockOffset = 0.5f;
        [SerializeField] private float _wasteOffset = 0.5f;

        [Header("Animation")]
        [SerializeField] private float _moveDuration = 0.15f;
        [SerializeField] private float _flipDuration = 0.2f;
        [SerializeField] private float _dealCardDuration = 0.1f;
        [SerializeField] private float _dealDelay = 0.04f;
        [SerializeField] private float _snapBackDuration = 0.12f;

        [Header("Drop Detection")]
        [Tooltip("Minimum overlap area (in px squared) between card and pile for a valid drop")]
        [SerializeField] private float _minOverlapArea = 500f;

        [Header("Win Screen")]
        [SerializeField] private float _winScreenFadeDuration = 0.5f;
        [SerializeField] private float _winScreenDelayBeforeShow = 0.5f;

        [Header("Scenes")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        // UI references
        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _dragLayer;

        // Win screen elements
        private VisualElement _winScreenOverlay;
        private VisualElement _winScreenPanel;

        // Pile elements
        private PileElement[] _foundationPiles = new PileElement[4];
        private PileElement[] _tableauPiles = new PileElement[7];
        private PileElement _stockPile;
        private PileElement _wastePile;

        // Model-to-view mappings
        private readonly Dictionary<Card, CardElement> _cardMap = new Dictionary<Card, CardElement>();
        private readonly Dictionary<CardPile, PileElement> _pileMap = new Dictionary<CardPile, PileElement>();

        // Game model
        private Game _game;
        private bool _canInteract;

        // Drag state
        private List<CardElement> _draggedCards = new List<CardElement>();
        private PileElement _dragOriginPile;

        // Tracked rect of the first dragged card (updated every PointerMove).
        // We track this ourselves because worldBound isn't refreshed until the
        // next layout pass, making it stale when PointerUp fires in the same frame.
        private Rect _draggedCardRect;

        // Animation queue: prevents overlapping coroutines from conflicting
        private int _activeAnimations;

        public Game Game => _game;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        // --- Public API ---

        public void StartGame()
        {
            UnsubscribeFromGame();

            _game = new Game();

            SetupUIReferences();
            BindPiles();

            _game.RecycleAndShuffleStock();
            SpawnAllCards();
            _game.PopulateTableauPiles();

            _game.OnCardMoved += HandleCardMoved;
            _game.OnCardFlipped += HandleCardFlipped;
            _game.OnGameWon += HandleGameWon;

            StartCoroutine(DealAnimation());
        }

        // --- Setup ---

        private void SetupUIReferences()
        {
            _root = _uiDocument.rootVisualElement.Q<VisualElement>("Root");

            for (int i = 0; i < 4; i++)
            {
                var placeholder = _root.Q<VisualElement>($"Foundation{i}");
                _foundationPiles[i] = ReplaceToPileElement(placeholder);
            }

            for (int i = 0; i < 7; i++)
            {
                var placeholder = _root.Q<VisualElement>($"Tableau{i}");
                _tableauPiles[i] = ReplaceToPileElement(placeholder);
            }

            _stockPile = ReplaceToPileElement(_root.Q<VisualElement>("Stock"));
            _wastePile = ReplaceToPileElement(_root.Q<VisualElement>("Waste"));

            // Drag layer overlay
            _dragLayer = new VisualElement();
            _dragLayer.name = "DragLayer";
            _dragLayer.style.position = Position.Absolute;
            _dragLayer.style.left = 0;
            _dragLayer.style.top = 0;
            _dragLayer.style.right = 0;
            _dragLayer.style.bottom = 0;
            _dragLayer.pickingMode = PickingMode.Ignore;
            _root.Add(_dragLayer);

            // Wire buttons
            var btnUndo = _root.Q<Button>("BtnUndo");
            var btnRedo = _root.Q<Button>("BtnRedo");
            if (btnUndo != null) btnUndo.clicked += HandleUndo;
            if (btnRedo != null) btnRedo.clicked += HandleRedo;

            var stockButton = _root.Q<Button>("StockButton");
            if (stockButton != null) stockButton.clicked += HandleStockClicked;
        }

        private PileElement ReplaceToPileElement(VisualElement placeholder)
        {
            var pile = new PileElement();
            pile.name = placeholder.name;

            foreach (var cls in placeholder.GetClasses())
                pile.AddToClassList(cls);

            pile.style.width = placeholder.style.width;
            pile.style.height = placeholder.style.height;
            pile.style.flexGrow = placeholder.style.flexGrow;

            // Move children (e.g. StockButton) to the new pile
            while (placeholder.childCount > 0)
            {
                var child = placeholder[0];
                placeholder.Remove(child);
                pile.Add(child);
            }

            var parent = placeholder.parent;
            int index = parent.IndexOf(placeholder);
            parent.Remove(placeholder);
            parent.Insert(index, pile);

            return pile;
        }

        private void BindPiles()
        {
            for (int i = 0; i < 4; i++)
            {
                _foundationPiles[i].Initialize(_game.Foundations[i], 0, 0);
                _pileMap[_game.Foundations[i]] = _foundationPiles[i];
            }

            for (int i = 0; i < 7; i++)
            {
                _tableauPiles[i].Initialize(_game.Tableaus[i], _tableauFaceUpOffset, _tableauFaceDownOffset);
                _pileMap[_game.Tableaus[i]] = _tableauPiles[i];
            }

            _stockPile.Initialize(_game.Stock, _stockOffset, _stockOffset);
            _pileMap[_game.Stock] = _stockPile;

            _wastePile.Initialize(_game.Waste, _wasteOffset, _wasteOffset);
            _pileMap[_game.Waste] = _wastePile;
        }

        // --- Card Spawning ---

        private void SpawnAllCards()
        {
            var stockCards = _game.Stock.GetCardsReverse();

            foreach (Card card in stockCards)
            {
                var cardElement = new CardElement(card);
                _cardMap[card] = cardElement;
                _stockPile.AddCard(cardElement);

                cardElement.OnClicked += HandleCardClicked;
                cardElement.OnDragStarted += HandleDragStarted;
                cardElement.OnDragging += HandleDragMove;
                cardElement.OnDragEnded += HandleDragEnded;
            }
        }

        // ==================================================================
        //  DEAL ANIMATION — cards fly one-by-one from stock to each tableau
        // ==================================================================

        private IEnumerator DealAnimation()
        {
            // Wait one frame so the layout pass gives us valid worldBounds
            yield return null;
            yield return null;

            Rect stockBound = _stockPile.worldBound;

            for (int col = 0; col < _game.Tableaus.Count; col++)
            {
                var cards = _game.Tableaus[col].GetCardsReverse();

                for (int row = 0; row < cards.Count; row++)
                {
                    Card card = cards[row];
                    if (!_cardMap.TryGetValue(card, out CardElement cardElement))
                        continue;

                    // Remove from stock's visual list
                    _stockPile.RemoveCard(cardElement);

                    // ------- animate card from stock to target position -------

                    // 1. Move into drag layer at stock pile position
                    _dragLayer.Add(cardElement);
                    cardElement.style.position = Position.Absolute;

                    float cardW = stockBound.width;
                    float cardH = cardW * CardElement.CardAspectRatio;
                    cardElement.SetExplicitSize(cardW, cardH);
                    cardElement.style.left = stockBound.x;
                    cardElement.style.top = stockBound.y;

                    // 2. Calculate target world position in the tableau pile
                    Rect tableauBound = _tableauPiles[col].worldBound;
                    float targetY = 0f;
                    // Sum offsets for cards already placed in this pile before this one
                    for (int k = 0; k < row; k++)
                    {
                        targetY += cards[k].IsFaceUp
                            ? _tableauFaceUpOffset
                            : _tableauFaceDownOffset;
                    }
                    Vector2 target = new Vector2(tableauBound.x, tableauBound.y + targetY);
                    Vector2 start = new Vector2(stockBound.x, stockBound.y);

                    // 3. Lerp
                    float elapsed = 0f;
                    while (elapsed < _dealCardDuration)
                    {
                        elapsed += Time.deltaTime;
                        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / _dealCardDuration));
                        cardElement.style.left = Mathf.Lerp(start.x, target.x, t);
                        cardElement.style.top = Mathf.Lerp(start.y, target.y, t);
                        yield return null;
                    }

                    // 4. Reparent into the pile element
                    if (_dragLayer.Contains(cardElement))
                        _dragLayer.Remove(cardElement);

                    cardElement.ResetToFlowSize();
                    _tableauPiles[col].AddCard(cardElement);

                    if (card.IsFaceUp)
                        cardElement.UpdateFaceUpStatus();

                    // short pause between cards
                    yield return new WaitForSeconds(_dealDelay);
                }
            }

            SetAllCardsInteraction(true);
        }

        // --- Card Click ---

        private void HandleCardClicked(CardElement cardElement)
        {
            if (!_canInteract) return;

            var pile = _game.FindPileForCard(cardElement.Model);

            if (pile is StockPile)
            {
                _game.DrawFromStock();
                return;
            }

            for (int i = 0; i < _game.Foundations.Count; i++)
            {
                if (_game.Foundations[i] == pile) continue;
                if (_game.TryMoveCard(cardElement.Model, _game.Foundations[i]))
                    return;
            }

            for (int i = 0; i < _game.Tableaus.Count; i++)
            {
                if (_game.Tableaus[i] == pile) continue;
                if (_game.TryMoveCard(cardElement.Model, _game.Tableaus[i]))
                    return;
            }
        }

        // ==================================================================
        //  DRAG & DROP
        // ==================================================================

        private void HandleDragStarted(CardElement cardElement)
        {
            if (!_canInteract) return;

            var pile = cardElement.ParentPile;
            if (pile == null) return;

            _dragOriginPile = pile;
            _draggedCards.Clear();
            _draggedCards = pile.GetCardsFromTo(cardElement);

            for (int i = 0; i < _draggedCards.Count; i++)
            {
                var card = _draggedCards[i];
                var worldPos = card.worldBound;
                pile.RemoveCard(card);
                _dragLayer.Add(card);
                card.style.position = Position.Absolute;
                card.SetExplicitSize(worldPos.width, worldPos.height);
                card.style.left = worldPos.x;
                card.style.top = worldPos.y;
            }

            // Initialize tracked rect
            _draggedCardRect = _draggedCards[0].worldBound;
        }

        private void HandleDragMove(CardElement cardElement, Vector2 pointerPos)
        {
            if (_draggedCards.Count == 0) return;

            var first = _draggedCards[0];
            float cardW = first.resolvedStyle.width;
            float cardH = first.resolvedStyle.height;
            float halfW = cardW * 0.5f;
            float halfH = cardH * 0.25f;

            float cardX = pointerPos.x - halfW;
            float cardY = pointerPos.y - halfH;

            first.style.left = cardX;
            first.style.top = cardY;

            // Track the first card's rect ourselves (worldBound lags one frame)
            _draggedCardRect = new Rect(cardX, cardY, cardW, cardH);

            float offset = _tableauFaceUpOffset;
            for (int i = 1; i < _draggedCards.Count; i++)
            {
                _draggedCards[i].style.left = cardX;
                _draggedCards[i].style.top = cardY + (offset * i);
            }
        }

        private void HandleDragEnded(CardElement cardElement, Vector2 pointerPos)
        {
            if (_draggedCards.Count == 0) return;

            // Get ALL overlapping piles sorted by overlap area (largest first).
            // Try each one until a game-legal move succeeds.
            List<PileElement> candidates = FindOverlappingPilesSorted(_draggedCardRect);
            bool success = false;

            for (int i = 0; i < candidates.Count; i++)
            {
                PileElement targetPile = candidates[i];
                if (targetPile.Model == null) continue;

                var originModel = _game.FindPileForCard(cardElement.Model);
                if (targetPile.Model == originModel) continue;

                if (_game.TryMoveCard(cardElement.Model, targetPile.Model))
                {
                    success = true;
                    break;
                }
            }

            if (!success)
            {
                // Animate snap-back to origin pile instead of instant teleport
                StartCoroutine(AnimateSnapBack());
            }
        }

        // ==================================================================
        //  DROP DETECTION — overlap-based, multi-pile fallback
        // ==================================================================

        /// <summary>
        /// Returns ALL piles that overlap with the given card rect, sorted by
        /// overlap area in descending order (largest overlap first).
        /// Only includes piles whose overlap exceeds _minOverlapArea.
        /// </summary>
        private List<PileElement> FindOverlappingPilesSorted(Rect cardRect)
        {
            var candidates = new List<(PileElement pile, float area)>();

            foreach (var kvp in _pileMap)
            {
                PileElement pile = kvp.Value;

                // Skip the origin pile
                if (pile == _dragOriginPile) continue;

                // Get the effective drop zone rect for this pile
                Rect pileRect = GetEffectiveDropRect(pile);

                // Calculate overlap area
                float overlapArea = CalculateOverlapArea(cardRect, pileRect);

                if (overlapArea > _minOverlapArea)
                {
                    candidates.Add((pile, overlapArea));
                }
            }

            // Sort by overlap area descending (largest overlap = highest priority)
            candidates.Sort((a, b) => b.area.CompareTo(a.area));

            var result = new List<PileElement>(candidates.Count);
            for (int i = 0; i < candidates.Count; i++)
                result.Add(candidates[i].pile);

            return result;
        }

        /// <summary>
        /// Returns the effective drop target rectangle for a pile.
        /// For tableau piles, this is the union of the pile's bounds and all
        /// its visible cards (which extend below the pile element itself due
        /// to absolute positioning with overflow: visible).
        /// For other piles, it's just the pile's worldBound.
        /// </summary>
        private Rect GetEffectiveDropRect(PileElement pile)
        {
            Rect pileRect = pile.worldBound;

            // For tableau piles, expand to cover all stacked cards
            if (pile.Model is TableauPile && pile.CardElements.Count > 0)
            {
                var lastCard = pile.CardElements[pile.CardElements.Count - 1];
                Rect lastCardRect = lastCard.worldBound;

                float bottom = Mathf.Max(pileRect.yMax, lastCardRect.yMax);
                pileRect.height = bottom - pileRect.y;
            }

            return pileRect;
        }

        /// <summary>
        /// Calculates the area of intersection between two rectangles.
        /// Returns 0 if they don't overlap.
        /// </summary>
        private float CalculateOverlapArea(Rect a, Rect b)
        {
            float xOverlap = Mathf.Max(0, Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin));
            float yOverlap = Mathf.Max(0, Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin));
            return xOverlap * yOverlap;
        }

        // ==================================================================
        //  SNAP-BACK ANIMATION (failed drop)
        // ==================================================================

        private IEnumerator AnimateSnapBack()
        {
            if (_draggedCards.Count == 0 || _dragOriginPile == null)
                yield break;

            SetAllCardsInteraction(false);

            var startPositions = new List<Vector2>();
            var targetPositions = new List<Vector2>();

            Rect pileBound = _dragOriginPile.worldBound;

            for (int i = 0; i < _draggedCards.Count; i++)
            {
                var card = _draggedCards[i];

                float curX = card.style.left.value.value;
                float curY = card.style.top.value.value;
                startPositions.Add(new Vector2(curX, curY));

                float targetY = pileBound.y + _dragOriginPile.GetCardPositionY(card.Model);
                targetPositions.Add(new Vector2(pileBound.x, targetY));
            }

            float elapsed = 0f;
            while (elapsed < _snapBackDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / _snapBackDuration));

                for (int i = 0; i < _draggedCards.Count; i++)
                {
                    float x = Mathf.Lerp(startPositions[i].x, targetPositions[i].x, t);
                    float y = Mathf.Lerp(startPositions[i].y, targetPositions[i].y, t);
                    _draggedCards[i].style.left = x;
                    _draggedCards[i].style.top = y;
                }

                yield return null;
            }

            for (int i = 0; i < _draggedCards.Count; i++)
            {
                var card = _draggedCards[i];
                if (_dragLayer.Contains(card))
                    _dragLayer.Remove(card);
                card.style.position = Position.Absolute;
                card.ResetToFlowSize();
                _dragOriginPile.AddCard(card);
            }

            _draggedCards.Clear();
            _dragOriginPile = null;
            SetAllCardsInteraction(true);
        }

        // ==================================================================
        //  MODEL EVENT HANDLERS — card moved / flipped
        // ==================================================================

        private void HandleCardMoved(Card card, CardPile newPile)
        {
            if (!_cardMap.TryGetValue(card, out CardElement cardElement)) return;
            if (!_pileMap.TryGetValue(newPile, out PileElement targetPileElement)) return;

            SetAllCardsInteraction(false);
            _activeAnimations++;

            Vector2 startWorldPos;
            bool wasInDragLayer = _draggedCards.Contains(cardElement);

            if (wasInDragLayer)
            {
                startWorldPos = new Vector2(
                    cardElement.style.left.value.value,
                    cardElement.style.top.value.value);

                for (int i = 0; i < _draggedCards.Count; i++)
                {
                    var dragCard = _draggedCards[i];
                    if (_dragLayer.Contains(dragCard))
                        _dragLayer.Remove(dragCard);
                    dragCard.ResetToFlowSize();
                }
            }
            else
            {
                startWorldPos = new Vector2(cardElement.worldBound.x, cardElement.worldBound.y);
            }

            if (cardElement.ParentPile != null)
                cardElement.ParentPile.RemoveCard(cardElement);

            StartCoroutine(AnimateMoveToPile(cardElement, targetPileElement, startWorldPos));
        }

        private IEnumerator AnimateMoveToPile(CardElement cardElement, PileElement targetPile, Vector2 startWorldPos)
        {
            _dragLayer.Add(cardElement);
            cardElement.style.position = Position.Absolute;

            float cardWidth = cardElement.resolvedStyle.width;
            float cardHeight = cardElement.resolvedStyle.height;

            if (cardWidth <= 0) cardWidth = targetPile.resolvedStyle.width;
            if (cardHeight <= 0) cardHeight = cardWidth * CardElement.CardAspectRatio;

            cardElement.SetExplicitSize(cardWidth, cardHeight);
            cardElement.style.left = startWorldPos.x;
            cardElement.style.top = startWorldPos.y;

            Rect targetPileBound = targetPile.worldBound;
            float targetCardY = targetPile.GetNextCardPositionY();
            Vector2 targetWorldPos = new Vector2(targetPileBound.x, targetPileBound.y + targetCardY);

            float elapsed = 0f;
            while (elapsed < _moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / _moveDuration));

                cardElement.style.left = Mathf.Lerp(startWorldPos.x, targetWorldPos.x, t);
                cardElement.style.top = Mathf.Lerp(startWorldPos.y, targetWorldPos.y, t);

                yield return null;
            }

            cardElement.style.left = targetWorldPos.x;
            cardElement.style.top = targetWorldPos.y;

            if (_dragLayer.Contains(cardElement))
                _dragLayer.Remove(cardElement);

            cardElement.style.position = Position.Absolute;
            cardElement.ResetToFlowSize();
            targetPile.AddCard(cardElement);
            cardElement.UpdateFaceUpStatus();

            _activeAnimations--;
            cardElement.NotifyMoveCompleted();

            if (_activeAnimations <= 0)
            {
                _activeAnimations = 0;
                SetAllCardsInteraction(true);
            }
        }

        private void HandleCardFlipped(Card card)
        {
            if (!_cardMap.TryGetValue(card, out CardElement cardElement)) return;

            SetAllCardsInteraction(false);
            _activeAnimations++;
            StartCoroutine(AnimateFlip(cardElement));
        }

        private IEnumerator AnimateFlip(CardElement cardElement)
        {
            float halfDuration = _flipDuration / 2f;
            float time = 0f;

            while (time < halfDuration)
            {
                time += Time.deltaTime;
                float scaleX = Mathf.Lerp(1f, 0f, time / halfDuration);
                cardElement.style.scale = new StyleScale(new Scale(new Vector3(scaleX, 1f, 1f)));
                yield return null;
            }

            cardElement.UpdateFaceUpStatus();

            time = 0f;
            while (time < halfDuration)
            {
                time += Time.deltaTime;
                float scaleX = Mathf.Lerp(0f, 1f, time / halfDuration);
                cardElement.style.scale = new StyleScale(new Scale(new Vector3(scaleX, 1f, 1f)));
                yield return null;
            }

            cardElement.style.scale = new StyleScale(new Scale(Vector3.one));
            cardElement.NotifyFlipCompleted();

            _activeAnimations--;
            if (_activeAnimations <= 0)
            {
                _activeAnimations = 0;
                SetAllCardsInteraction(true);
            }
        }

        // --- Stock / Undo / Redo ---

        private void HandleStockClicked()
        {
            if (!_canInteract) return;
            _game.DrawFromStock();
        }

        public void HandleUndo()
        {
            if (!_canInteract) return;
            _game.Undo();
        }

        public void HandleRedo()
        {
            if (!_canInteract) return;
            _game.Redo();
        }

        // --- Interaction ---

        private void SetAllCardsInteraction(bool interactable)
        {
            _canInteract = interactable;
            foreach (var kvp in _cardMap)
                kvp.Value.SetInteractable(interactable);
        }

        private void HandleGameWon()
        {
            SetAllCardsInteraction(false);
            ShowWinScreen();
        }

        // ==================================================================
        //  WIN SCREEN — programmatic VisualElement overlay
        // ==================================================================

        public void ShowWinScreen()
        {
            if (_winScreenOverlay != null) return;

            BuildWinScreenElements();
            _root.Add(_winScreenOverlay);
            StartCoroutine(AnimateWinScreenIn());
        }

        public void HideWinScreen()
        {
            if (_winScreenOverlay != null && _root.Contains(_winScreenOverlay))
            {
                _root.Remove(_winScreenOverlay);
            }
            _winScreenOverlay = null;
            _winScreenPanel = null;
        }

        private void BuildWinScreenElements()
        {
            // --- Backdrop (semi-transparent black) ---
            _winScreenOverlay = new VisualElement();
            _winScreenOverlay.name = "WinScreenOverlay";
            _winScreenOverlay.style.position = Position.Absolute;
            _winScreenOverlay.style.left = 0;
            _winScreenOverlay.style.top = 0;
            _winScreenOverlay.style.right = 0;
            _winScreenOverlay.style.bottom = 0;
            _winScreenOverlay.style.backgroundColor = new Color(0f, 0f, 0f, 0.6f);
            _winScreenOverlay.style.alignItems = Align.Center;
            _winScreenOverlay.style.justifyContent = Justify.Center;
            _winScreenOverlay.style.opacity = 0f;

            // --- Panel (centered card) ---
            _winScreenPanel = new VisualElement();
            _winScreenPanel.name = "WinScreenPanel";
            _winScreenPanel.style.width = 600;
            _winScreenPanel.style.paddingTop = 60;
            _winScreenPanel.style.paddingBottom = 60;
            _winScreenPanel.style.paddingLeft = 50;
            _winScreenPanel.style.paddingRight = 50;
            _winScreenPanel.style.backgroundColor = new Color(0.15f, 0.15f, 0.2f, 1f);
            _winScreenPanel.style.borderTopLeftRadius = 24;
            _winScreenPanel.style.borderTopRightRadius = 24;
            _winScreenPanel.style.borderBottomLeftRadius = 24;
            _winScreenPanel.style.borderBottomRightRadius = 24;
            _winScreenPanel.style.alignItems = Align.Center;
            _winScreenPanel.style.scale = new StyleScale(new Scale(new Vector3(0.85f, 0.85f, 1f)));
            _winScreenOverlay.Add(_winScreenPanel);

            // --- Title ---
            var title = new Label("You Win!");
            title.name = "WinTitle";
            title.style.fontSize = 72;
            title.style.color = new Color(1f, 0.84f, 0f, 1f); // Gold
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.marginBottom = 60;
            _winScreenPanel.Add(title);

            // --- Buttons Container ---
            var buttonsContainer = new VisualElement();
            buttonsContainer.name = "ButtonsContainer";
            buttonsContainer.style.flexDirection = FlexDirection.Row;
            buttonsContainer.style.justifyContent = Justify.Center;
            buttonsContainer.style.width = Length.Percent(100);
            _winScreenPanel.Add(buttonsContainer);

            // --- Play Again Button ---
            var playAgainBtn = new Button(HandlePlayAgainClicked);
            playAgainBtn.name = "BtnPlayAgain";
            playAgainBtn.text = "Play Again";
            StyleWinButton(playAgainBtn, new Color(0.2f, 0.7f, 0.3f, 1f));
            playAgainBtn.style.marginRight = 20;
            buttonsContainer.Add(playAgainBtn);

            // --- Main Menu Button ---
            var mainMenuBtn = new Button(HandleMainMenuClicked);
            mainMenuBtn.name = "BtnMainMenu";
            mainMenuBtn.text = "Main Menu";
            StyleWinButton(mainMenuBtn, new Color(0.4f, 0.4f, 0.45f, 1f));
            mainMenuBtn.style.marginLeft = 20;
            buttonsContainer.Add(mainMenuBtn);
        }

        private void StyleWinButton(Button button, Color bgColor)
        {
            button.style.fontSize = 36;
            button.style.color = Color.white;
            button.style.backgroundColor = bgColor;
            button.style.paddingTop = 18;
            button.style.paddingBottom = 18;
            button.style.paddingLeft = 40;
            button.style.paddingRight = 40;
            button.style.borderTopLeftRadius = 12;
            button.style.borderTopRightRadius = 12;
            button.style.borderBottomLeftRadius = 12;
            button.style.borderBottomRightRadius = 12;
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 0;
            button.style.borderRightWidth = 0;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
        }

        private IEnumerator AnimateWinScreenIn()
        {
            yield return new WaitForSeconds(_winScreenDelayBeforeShow);

            float elapsed = 0f;
            while (elapsed < _winScreenFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _winScreenFadeDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f); // ease-out cubic

                _winScreenOverlay.style.opacity = eased;

                float scale = Mathf.Lerp(0.85f, 1f, eased);
                _winScreenPanel.style.scale = new StyleScale(new Scale(new Vector3(scale, scale, 1f)));

                yield return null;
            }

            _winScreenOverlay.style.opacity = 1f;
            _winScreenPanel.style.scale = new StyleScale(new Scale(Vector3.one));
        }

        // ==================================================================
        //  WIN SCREEN BUTTON HANDLERS
        // ==================================================================

        private void HandlePlayAgainClicked()
        {
            RestartGame();
        }

        private void HandleMainMenuClicked()
        {
            UnsubscribeFromGame();
            SceneManager.LoadScene(_mainMenuSceneName);
        }

        // ==================================================================
        //  RESTART GAME — clean up everything and start fresh
        // ==================================================================

        public void RestartGame()
        {
            StopAllCoroutines();
            HideWinScreen();
            DestroyAllCards();
            StartGame();
        }

        /// <summary>
        /// Remove all CardElements from the visual tree and clear mappings.
        /// </summary>
        private void DestroyAllCards()
        {
            foreach (var kvp in _cardMap)
            {
                CardElement cardElement = kvp.Value;

                // Unsubscribe events
                cardElement.OnClicked -= HandleCardClicked;
                cardElement.OnDragStarted -= HandleDragStarted;
                cardElement.OnDragging -= HandleDragMove;
                cardElement.OnDragEnded -= HandleDragEnded;

                // Remove from whatever parent it's in
                cardElement.RemoveFromHierarchy();
            }

            _cardMap.Clear();
            _pileMap.Clear();
            _draggedCards.Clear();
            _dragOriginPile = null;
            _activeAnimations = 0;
            _canInteract = false;

            // Clear all pile elements' card lists
            for (int i = 0; i < 4; i++)
                _foundationPiles[i]?.ClearCards();
            for (int i = 0; i < 7; i++)
                _tableauPiles[i]?.ClearCards();
            _stockPile?.ClearCards();
            _wastePile?.ClearCards();

            // Remove drag layer so SetupUIReferences creates a fresh one
            if (_dragLayer != null && _root != null && _root.Contains(_dragLayer))
                _root.Remove(_dragLayer);
            _dragLayer = null;
        }

        // --- Cleanup ---

        private void UnsubscribeFromGame()
        {
            if (_game != null)
            {
                _game.OnCardMoved -= HandleCardMoved;
                _game.OnCardFlipped -= HandleCardFlipped;
                _game.OnGameWon -= HandleGameWon;
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromGame();
        }
    }
}
