# Solitaire

A Klondike Solitaire card game built in Unity 2022.3 (LTS) targeting Android. The project ships two fully independent UI implementations — Canvas/uGUI and UI Toolkit — sharing the same domain logic through a common `IGameUI` interface.

Built as a portfolio piece to demonstrate clean architecture, testability, and mobile-first design in a real Unity project.

## Gameplay

Standard Klondike rules. Build tableau columns in descending rank with alternating colors. Stack each suit from Ace to King on the four foundation piles. Tap the stock to draw cards. Undo any move. When all remaining cards are face-up, an auto-complete button appears to finish the game automatically.

## Architecture

The codebase is organized into clearly separated layers, each with a single responsibility:

```
Scripts/
  Domain/         Pure C# game rules — no Unity dependencies
  Application/    Use cases: Game orchestration, Command pattern (undo/redo)
  Core/           GameManager, IGameContext, State Machine, ApplicationBootstrap
  Presentation/   IGameUI interface + two implementations (Canvas, UIToolkit)
  Audio/          Event-driven audio system (AudioServiceSO, AudioPlayer, GameAudioHandler)
  Infrastructure/ DeckFactory
  Extensions/     ListExtensions (Fisher-Yates shuffle)
  Tests/          NUnit unit tests — domain, application, and game state layers
```

**Domain layer** is entirely framework-agnostic. `Card`, `CardPile` and its subclasses (`StockPile`, `WastePile`, `TableauPile`, `FoundationPile`), enums, and validation rules live here with zero `using UnityEngine` imports. This means the core game logic can be unit-tested with plain NUnit — no Play Mode required.

**Application layer** contains the `Game` class (the aggregate root that wires piles, manages a card-to-pile dictionary for O(1) lookups, and exposes the public API) and the Command system for undo/redo.

**Presentation layer** is where Unity lives. Two concrete presenters implement `IGameUI`: `CanvasGamePresenter` (uGUI + DOTween) and `UIToolkitGamePresenter` (USS/UXML + DOTween). Game states don't know which UI is active — they talk through the interface. The presenters raise events (`OnDealCardAnimated`, `OnInvalidMoveAttempted`, `OnAutoCompleteStepPerformed`) that the audio system subscribes to independently.

**Audio layer** follows a ScriptableObject-as-service pattern. `AudioServiceSO` acts as a decoupled bridge between game code and the `AudioPlayer` runtime. `GameAudioHandler` subscribes to both `Game` events (card moved, flipped, won) and presenter events (deal animated, invalid move) to trigger contextual sounds. A pre-allocated `AudioSource` pool in `AudioPlayer` avoids runtime allocations. `UIButtonSound` is a drop-in component for any button.

**Core layer** owns `GameManager` (which implements `IGameContext`) and `GameStateManager`, a plain C# state machine whose `Tick()` is driven by `GameManager.Update()`. States depend on the `IGameContext` interface rather than the MonoBehaviour directly, enabling pure C# unit testing with no GameObjects.

## Design Patterns

**State Machine** — Four states (`DealingState`, `PlayingState`, `AutoCompleteState`, `WinState`) control the game loop. Each state subscribes to `IGameUI` events on `Enter()` and unsubscribes on `Exit()`, keeping transitions clean and preventing stale listeners. `AutoCompleteState` runs a timer-based step loop in `Update()`, moving one card per tick to the foundations. `ChangeState<T>()` returns `bool` for safe transition handling.

**Command** — Every card move is wrapped in an `ICommand` (`MoveCommand`, `MoveReverseCommand`). A `CommandManager` with dual stacks handles undo/redo. Stock recycling is also a command, so it can be undone.

**Strategy** — Card acceptance rules are injected via `IDropRule`. Each pile type has a default rule (`TableauDropRule`, `FoundationDropRule`, etc.), but custom rules can be injected through the constructor for testing or variant game modes.

**MVP (Model-View-Presenter)** — Domain models (`Card`, `CardPile`) have no knowledge of their visual representation. Presenters map models to views (`CardView`/`PileView` for Canvas, `CardElement`/`PileElement` for UIToolkit) and coordinate animations.

**Observer** — Domain events (`OnCardMoved`, `OnCardFlipped`, `OnGameWon`) propagate state changes from the model to the presentation layer. An instance-based `ViewEventBus` replaces what was originally static state, making the system testable and avoiding global coupling. The audio system subscribes independently to both model and presenter events.

**ScriptableObject-as-Service** — `AudioServiceSO` decouples audio playback from game code. Any script references the SO asset to play sounds without knowing about the `AudioPlayer` MonoBehaviour. The player registers itself on `Awake`, persists via `DontDestroyOnLoad`, and provides a pooled SFX system with per-sound cooldowns and music crossfading.

## ScriptableObject Usage

Configuration is externalized into ScriptableObjects so designers can tune values without touching code:

- **CardThemeSO** — All suit sprites, face card art, and card colors in one asset. Swap themes by assigning a different ScriptableObject in the Inspector.
- **GameSettingsSO** — Animation timings (deal, move, flip, snap-back), pile stacking offsets for both Canvas and UIToolkit coordinate systems, drop detection thresholds, auto-complete pacing, and win screen timing. One asset controls game feel across both UI implementations.
- **AudioServiceSO** — Decoupled audio bridge. Game code calls `PlaySFX`/`PlayMusic` on the SO asset; the runtime `AudioPlayer` handles actual playback.
- **SoundSO** — Per-sound configuration: clip reference, volume range, pitch range, and cooldown. Each sound effect is a separate asset.
- **SoundLibrarySO** — Central registry mapping game events to `SoundSO` assets.

## Performance and Mobile Optimizations

- **Zero physics** — Drag-and-drop uses `RectTransform.GetWorldCorners()` overlap checks instead of Collider2D/Rigidbody2D. Physics2D simulation is disabled entirely (`SimulationMode2D.Script`).
- **Zero-allocation hot paths** — No LINQ in gameplay code. Card lookups use a `Dictionary<Card, CardPile>` instead of linear search. Pile iteration uses `Stack<T>.Enumerator` to avoid list allocation where possible.
- **Plain C# where MonoBehaviour isn't needed** — `GameStateManager`, `CommandManager`, `Game`, all pile classes, and all rules are regular C# objects. Fewer MonoBehaviours means less overhead from Unity's lifecycle callbacks.
- **Named event handlers** — All event subscriptions use named methods instead of lambdas, eliminating delegate allocations on subscribe/unsubscribe.
- **Pre-allocated audio pool** — `AudioPlayer` creates a fixed pool of `AudioSource` components at startup. SFX playback uses round-robin indexing with zero runtime allocation.
- **DOTween sequences** — Animations are pooled DOTween sequences with `OnKill` cleanup callbacks, preventing tween leaks.
- **Mobile quality level** — Custom "Mobile" quality preset: shadows disabled, no MSAA, 0 pixel lights, no anisotropic filtering. Set as Android default.
- **Target frame rate** — `ApplicationBootstrap` sets `targetFrameRate = 60` and `sleepTimeout = NeverSleep` via `[RuntimeInitializeOnLoadMethod]` before any scene loads.
- **Sprite Atlases** — Card sprites and UI sprites are packed into two `SpriteAtlas` assets with Android ASTC 6x6 compression overrides, reducing draw calls and texture memory.
- **ASTC texture compression** — All project textures have Android-specific ASTC 6x6 overrides for 4-8x memory reduction on device.
- **Optimized audio imports** — All SFX clips use ADPCM compression, 22050 Hz sample rate, DecompressOnLoad, and force-to-mono.
- **Safe area support** — Both UI implementations handle notches and rounded corners. The Canvas version uses `OnRectTransformDimensionsChange` instead of polling in `Update()`. The UIToolkit version converts screen-space insets to panel-space padding.
- **Orientation handling** — `OrientationChanger` locks to portrait on Android.

## Testing

Test suites cover domain, application, and game state logic:

- `CardTests` — Equality, hashing, color properties, flip events, redundant flip guard
- `CardPileTests` — LIFO ordering, Contains, GetCards/Reverse, SetCards, Clear
- `FoundationPileTests` — Validation rules, suit assignment, regression test for a side-effect bug in CanAddCard
- `TableauPileTests` — Placement rules, TryGetCardStack, auto-flip on remove
- `StockPileTests` — Always rejects add, forces face-down
- `WastePileTests` — Stock-origin only, forces face-up
- `CommandTests` — MoveCommand/MoveReverseCommand execute and undo, regression test for list mutation bug
- `CommandManagerTests` — CanUndo/CanRedo states, redo-clear behavior, Clear()
- `DeckFactoryTests` — 52 unique cards, all suits and ranks present, all face-down
- `GameTests` — Game-level integration tests
- `AutoCompleteTests` — Auto-complete step logic, foundation targeting, win detection
- `GameStateTests` — State machine transitions, DealingState/PlayingState/AutoCompleteState/WinState lifecycle, event wiring and cleanup. Uses pure C# mocks (`MockGameContext`, `MockGameUI`) with no MonoBehaviour dependencies.
- `ListExtensionsTests` — Fisher-Yates shuffle correctness and edge cases

All tests run against pure C# classes with no Unity dependencies (EditMode tests), so they execute in under a second.

## Project Structure

```
Assets/_Project/
  Art/              Card sprites, backgrounds, UI assets, Sprite Atlases
  Audio/            SFX clips (WAV)
  Data/             ScriptableObject instances (CardThemeSO, GameSettingsSO,
                    AudioServiceSO, SoundSO assets, SoundLibrarySO)
  Prefabs/          Card and pile prefabs for Canvas variant, AudioPlayer
  Scenes/
    Game-Canvas.unity         uGUI scene
    Game-UIToolkit.unity      UI Toolkit scene
    MainMenu.unity
  Scripts/          (see Architecture section above)
  UI/               UXML and USS files for UI Toolkit variant
```

## Setup

1. Clone the repo
2. Open in Unity 2022.3.62f2 or compatible LTS version
3. Open either `Game-Canvas` or `Game-UIToolkit` scene
4. Press Play

## Tech Stack

- **Unity** 2022.3.62f2 (LTS)
- **DOTween** — Animation tweening (sequences, easing, callbacks)
- **TextMeshPro** — Card text rendering
- **Target** — Android (portrait, safe area aware)

## License

MIT
