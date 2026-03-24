using System;
using UnityEngine;

namespace Solitaire.Presentation
{
    /// <summary>
    /// Centralized game-feel settings shared between Canvas and UIToolkit presenters.
    /// Adjust a single asset to tune animation timing and pile layout across both UI implementations.
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Solitaire/Game Settings")]
    public class GameSettingsSO : ScriptableObject
    {
        [Header("Pile Stacking — Canvas (negative Y = downward)")]
        [SerializeField] private PileOffsets _canvasTableauOffsets = new PileOffsets(-68f, -20f);
        [SerializeField] private PileOffsets _canvasStockOffsets = new PileOffsets(0f, -2f);
        [SerializeField] private PileOffsets _canvasWasteOffsets = new PileOffsets(-2f, 0f);

        [Header("Pile Stacking — UIToolkit (positive = downward)")]
        [SerializeField] private PileOffsets _uitkTableauOffsets = new PileOffsets(30f, 15f);
        [SerializeField] private PileOffsets _uitkStockOffsets = new PileOffsets(0.5f, 0.5f);
        [SerializeField] private PileOffsets _uitkWasteOffsets = new PileOffsets(0.5f, 0.5f);

        [Header("Deal Animation")]
        [Tooltip("Delay before the first card is dealt")]
        [SerializeField] private float _dealStartDelay = 1f;
        [Tooltip("Duration of each card's flight from stock to tableau")]
        [SerializeField] private float _dealCardDuration = 0.1f;
        [Tooltip("Pause between dealing consecutive cards")]
        [SerializeField] private float _dealCardDelay = 0.04f;

        [Header("Card Animation")]
        [Tooltip("Duration of a card move animation")]
        [SerializeField] private float _cardMoveDuration = 0.2f;
        [Tooltip("Duration of a card flip animation")]
        [SerializeField] private float _cardFlipDuration = 0.2f;
        [Tooltip("Duration of snap-back when a drop is invalid")]
        [SerializeField] private float _snapBackDuration = 0.12f;

        [Header("Drop Detection")]
        [Tooltip("Minimum overlap area (px squared) for a valid card drop")]
        [SerializeField] private float _minOverlapArea = 500f;

        [Header("Auto-Complete")]
        [Tooltip("Delay between each auto-complete step")]
        [SerializeField] private float _autoCompleteStepDelay = 0.15f;

        [Header("Win Screen")]
        [Tooltip("Fade-in duration for the win screen overlay")]
        [SerializeField] private float _winScreenFadeDuration = 0.5f;
        [Tooltip("Delay before showing the win screen after the last card lands")]
        [SerializeField] private float _winScreenShowDelay = 0.5f;

        // --- Pile offset accessors ---

        public PileOffsets CanvasTableauOffsets => _canvasTableauOffsets;
        public PileOffsets CanvasStockOffsets => _canvasStockOffsets;
        public PileOffsets CanvasWasteOffsets => _canvasWasteOffsets;

        public PileOffsets UITKTableauOffsets => _uitkTableauOffsets;
        public PileOffsets UITKStockOffsets => _uitkStockOffsets;
        public PileOffsets UITKWasteOffsets => _uitkWasteOffsets;

        // --- Animation accessors ---

        public float DealStartDelay => _dealStartDelay;
        public float DealCardDuration => _dealCardDuration;
        public float DealCardDelay => _dealCardDelay;

        public float CardMoveDuration => _cardMoveDuration;
        public float CardFlipDuration => _cardFlipDuration;
        public float SnapBackDuration => _snapBackDuration;

        public float MinOverlapArea => _minOverlapArea;

        public float AutoCompleteStepDelay => _autoCompleteStepDelay;

        public float WinScreenFadeDuration => _winScreenFadeDuration;
        public float WinScreenShowDelay => _winScreenShowDelay;
    }

    [Serializable]
    public struct PileOffsets
    {
        [Tooltip("Offset between face-up cards")]
        public float FaceUpOffset;
        [Tooltip("Offset between face-down cards")]
        public float FaceDownOffset;

        public PileOffsets(float faceUp, float faceDown)
        {
            FaceUpOffset = faceUp;
            FaceDownOffset = faceDown;
        }
    }
}
