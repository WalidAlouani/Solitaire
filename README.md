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
  Core/           GameManager, State Machine (Dealing → Playing → Win)
  Presentation/   IGameUI interface + two implementations (Canvas, UIToolkit)
  Infrastructure/ DeckFactory
  Extensions/     ListExtensions (Fisher-Yates shuffle)
  Tests/          NUnit unit tests against domain and application layers
```

**Domain layer** is entirely framework-agnostic. `Card`, `CardPile` and its subclasses (`StockPile`, `WastePile`, `TableauPile`, `FoundationPile`), enums, and validation rules live here with zero `using UnityEngine` imports. This means the core game logic can be unit-tested with plain NUnit — no Play Mode required.

**Application layer** contains the `Game` class (the aggregate root that wires piles, manages a card-to-pile dictionary for O(1) lookups, and exposes the public API) and the Command system for undo/redo.

**Presentation layer** is where Unity lives. Two concrete presenters implement `IGameUI`: `CanvasGamePresenter` (uGUI + DOTween) and `UIToolkitGamePresenter` (USS/UXML + DOTween). Game states don't know which UI is active — they talk through the interface.

**Core layer** owns `GameManager` (the scene root MonoBehaviour) and `GameStateManager`, a plain C# state machine whose `Tick()` is driven by `GameManager.Update()`. Keeping the state machine as a regular class avoids unnecessary MonoBehaviour overhead.

## Design Patterns

**State Machine** — Three states (`DealingState`, `PlayingState`, `WinState`) control the game loop. Each state subscribes to `IGameUI` events on `Enter()` and unsubscribes on `Exit()`, keeping transitions clean and preventing stale listeners.

**Command** — Every card move is wrapped in an `ICommand` (`MoveCommand`, `MoveReverseCommand`). A `CommandManager` with dual stacks handles undo/redo. Stock recycling is also a command, so it can be undone.

**Strategy** — Card acceptance rules are injected via `IDropRule`. Each pile type has a default rule (`TableauDropRule`, `FoundationDropRule`, etc.), but custom rules can be injected through the constructor for testing or variant game modes.

**MVP (Model-View-Presenter)** — Domain models (`Card`, `CardPile`) have no knowledge of their visual representation. Presenters map models to views (`CardView`/`PileView` for Canvas, `CardElement`/`PileElement` for UIToolkit) and coordinate animations.

**Observer** — Domain events (`OnCardMoved`, `OnCardFlipped`, `OnGameWon`) propagate state changes from the model to the presentation layer. An instance-based `ViewEventBus` replaces what was originally static state, making the system testable and avoiding global coupling.

## ScriptableObject Usage

Configuration is externalized into ScriptableObjects so designers can tune values without touching code:

- **CardThemeSO** — All suit sprites, face card art, and card colors in one asset. Swap themes by assigning a different ScriptableObject in the Inspector.
- **GameSettingsSO** — Animation timings (deal, move, flip, snap-back), pile stacking offsets for both Canvas and UIToolkit coordinate systems, drop detection thresholds, auto-complete pacing, and win screen timing. One asset controls game feel across both UI implementations.

## Performance and Mobile Considerations

- **Zero-allocation hot paths** — No LINQ in gameplay code. Card lookups use a `Dictionary<Card, CardPile>` instead of linear search. Pile iteration uses `Stack<T>.Enumerator` to avoid list allocation where possible.
- **Plain C# where MonoBehaviour isn't needed** — `GameStateManager`, `CommandManager`, `Game`, all pile classes, and all rules are regular C# objects. Fewer MonoBehaviours means less overhead from Unity's lifecycle callbacks.
- **Named event handlers** — All 52 card flip subscriptions use named methods instead of lambdas, eliminating delegate allocations on subscribe/unsubscribe.
- **DOTween sequences** — Animations are pooled DOTween sequences with `OnKill` cleanup callbacks, preventing tween leaks.
- **Safe area support** — Both UI implementations handle notches and rounded corners. The Canvas version uses `OnRectTransformDimensionsChange` instead of polling in `Update()`. The UIToolkit version converts screen-space insets to panel-space padding.
- **Orientation handling** — `OrientationChanger` locks to portrait on Android.

## Testing

10 test classes covering domain and application logic:

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

All tests run against pure C# classes with no Unity dependencies (EditMode tests), so they execute in under a second.

## Project Structure

```
Assets/_Project/
  Art/              Card sprites, backgrounds, UI assets
  Data/             ScriptableObject instances (CardThemeSO, GameSettingsSO)
  Prefabs/          Card and pile prefabs for Canvas variant
  Resources/        Runtime-loaded assets
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
