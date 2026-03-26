using NUnit.Framework;
using Solitaire.Application;
using Solitaire.Core;
using Solitaire.Core.StateMachine;
using Solitaire.Presentation;
using System;

namespace Solitaire.Tests
{
    // =========================================================================
    //  Mock IGameUI — pure C#, tracks all calls for assertion
    // =========================================================================

    public class MockGameUI : IGameUI
    {
        public event Action OnDealingComplete;
        public event Action OnPlayAgainRequested;
        public event Action OnMainMenuRequested;
        public event Action OnAutoCompleteRequested;

        public GameSettingsSO GameSettings { get; set; }

        public bool CleanupCalled { get; private set; }
        public bool StartGameCalled { get; private set; }
        public bool ShowWinScreenCalled { get; private set; }
        public bool ShowAutoCompleteCalled { get; private set; }
        public bool HideAutoCompleteCalled { get; private set; }
        public bool? LastSetInteractable { get; private set; }
        public Game LastStartedGame { get; private set; }

        public void Cleanup() => CleanupCalled = true;

        public void StartGame(Game game)
        {
            StartGameCalled = true;
            LastStartedGame = game;
        }

        public void ShowWinScreen() => ShowWinScreenCalled = true;
        public void ShowAutoCompleteButton() => ShowAutoCompleteCalled = true;
        public void HideAutoCompleteButton() => HideAutoCompleteCalled = true;

        public void SetInteractable(bool interactable)
        {
            LastSetInteractable = interactable;
        }

        // --- Test helpers to raise events ---

        public void RaiseDealingComplete() => OnDealingComplete?.Invoke();
        public void RaisePlayAgainRequested() => OnPlayAgainRequested?.Invoke();
        public void RaiseMainMenuRequested() => OnMainMenuRequested?.Invoke();
        public void RaiseAutoCompleteRequested() => OnAutoCompleteRequested?.Invoke();

        public void Reset()
        {
            CleanupCalled = false;
            StartGameCalled = false;
            ShowWinScreenCalled = false;
            ShowAutoCompleteCalled = false;
            HideAutoCompleteCalled = false;
            LastSetInteractable = null;
            LastStartedGame = null;
        }
    }

    // =========================================================================
    //  Mock IGameContext — pure C#, no MonoBehaviour
    // =========================================================================

    public class MockGameContext : IGameContext
    {
        public GameStateManager StateManager { get; set; }
        public IGameUI GameUI { get; set; }
        public Game Game { get; set; }
        public string MainMenuSceneName { get; set; } = "MainMenu";

        public bool CreateNewGameCalled { get; private set; }

        public Game CreateNewGame()
        {
            CreateNewGameCalled = true;
            Game = new Game();
            Game.RecycleAndShuffleStock();
            return Game;
        }

        public void Reset()
        {
            CreateNewGameCalled = false;
        }
    }

    // =========================================================================
    //  GameStateManager Tests (pure C#)
    // =========================================================================

    [TestFixture]
    public class GameStateManagerTests
    {
        private GameStateManager _sm;
        private MockState _stateA;

        private class MockState : IGameState
        {
            public int EnterCount;
            public int UpdateCount;
            public int ExitCount;

            public void Enter() => EnterCount++;
            public void Update() => UpdateCount++;
            public void Exit() => ExitCount++;
        }

        private class MockStateB : IGameState
        {
            public int EnterCount;
            public int UpdateCount;
            public int ExitCount;

            public void Enter() => EnterCount++;
            public void Update() => UpdateCount++;
            public void Exit() => ExitCount++;
        }

        [SetUp]
        public void SetUp()
        {
            _sm = new GameStateManager();
            _stateA = new MockState();
        }

        [Test]
        public void CurrentState_Initially_IsNull()
        {
            Assert.IsNull(_sm.CurrentState);
        }

        [Test]
        public void ChangeState_RegisteredState_CallsEnter()
        {
            _sm.RegisterState(_stateA);
            _sm.ChangeState<MockState>();

            Assert.AreEqual(1, _stateA.EnterCount);
        }

        [Test]
        public void ChangeState_RegisteredState_ReturnsTrue()
        {
            _sm.RegisterState(_stateA);

            Assert.IsTrue(_sm.ChangeState<MockState>());
        }

        [Test]
        public void ChangeState_SetsCurrentState()
        {
            _sm.RegisterState(_stateA);
            _sm.ChangeState<MockState>();

            Assert.AreSame(_stateA, _sm.CurrentState);
        }

        [Test]
        public void ChangeState_FromOneToAnother_CallsExitThenEnter()
        {
            var stateB = new MockStateB();
            _sm.RegisterState(_stateA);
            _sm.RegisterState(stateB);

            _sm.ChangeState<MockState>();
            _sm.ChangeState<MockStateB>();

            Assert.AreEqual(1, _stateA.ExitCount);
            Assert.AreEqual(1, stateB.EnterCount);
        }

        [Test]
        public void Tick_CallsUpdateOnCurrentState()
        {
            _sm.RegisterState(_stateA);
            _sm.ChangeState<MockState>();

            _sm.Tick();
            _sm.Tick();
            _sm.Tick();

            Assert.AreEqual(3, _stateA.UpdateCount);
        }

        [Test]
        public void Tick_NoCurrentState_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _sm.Tick());
        }

        [Test]
        public void IsInState_ReturnsTrue_WhenInThatState()
        {
            _sm.RegisterState(_stateA);
            _sm.ChangeState<MockState>();

            Assert.IsTrue(_sm.IsInState<MockState>());
        }

        [Test]
        public void IsInState_ReturnsFalse_WhenInDifferentState()
        {
            var stateB = new MockStateB();
            _sm.RegisterState(_stateA);
            _sm.RegisterState(stateB);
            _sm.ChangeState<MockState>();

            Assert.IsFalse(_sm.IsInState<MockStateB>());
        }

        [Test]
        public void ChangeState_UnregisteredState_ReturnsFalse()
        {
            Assert.IsFalse(_sm.ChangeState<MockStateB>());
        }

        [Test]
        public void ChangeState_UnregisteredState_KeepsCurrentState()
        {
            _sm.RegisterState(_stateA);
            _sm.ChangeState<MockState>();

            _sm.ChangeState<MockStateB>();

            Assert.AreSame(_stateA, _sm.CurrentState);
        }

        [Test]
        public void ChangeState_UnregisteredState_DoesNotCallExit()
        {
            _sm.RegisterState(_stateA);
            _sm.ChangeState<MockState>();

            _sm.ChangeState<MockStateB>();

            Assert.AreEqual(0, _stateA.ExitCount);
        }

        [Test]
        public void RegisterState_OverwritesPreviousOfSameType()
        {
            var stateA2 = new MockState();
            _sm.RegisterState(_stateA);
            _sm.RegisterState(stateA2);

            _sm.ChangeState<MockState>();

            Assert.AreEqual(0, _stateA.EnterCount);
            Assert.AreEqual(1, stateA2.EnterCount);
        }
    }

    // =========================================================================
    //  Shared test rig builder — pure C#
    // =========================================================================

    internal static class TestRig
    {
        public static (MockGameContext ctx, MockGameUI ui, GameStateManager sm) Create()
        {
            var ui = new MockGameUI();
            var sm = new GameStateManager();
            var ctx = new MockGameContext
            {
                GameUI = ui,
                StateManager = sm
            };
            return (ctx, ui, sm);
        }

        /// <summary>
        /// Creates a rig with all 4 states registered and a Game ready.
        /// </summary>
        public static (MockGameContext ctx, MockGameUI ui, GameStateManager sm) CreateFull()
        {
            var (ctx, ui, sm) = Create();

            sm.RegisterState(new DealingState(ctx));
            sm.RegisterState(new PlayingState(ctx));
            sm.RegisterState(new AutoCompleteState(ctx));
            sm.RegisterState(new WinState(ctx));

            ctx.CreateNewGame();

            return (ctx, ui, sm);
        }
    }

    // =========================================================================
    //  DealingState Tests
    // =========================================================================

    [TestFixture]
    public class DealingStateTests
    {
        private MockGameContext _ctx;
        private MockGameUI _ui;
        private GameStateManager _sm;

        [SetUp]
        public void SetUp()
        {
            (_ctx, _ui, _sm) = TestRig.Create();

            _sm.RegisterState(new DealingState(_ctx));
            _sm.RegisterState(new PlayingState(_ctx));
        }

        [Test]
        public void Enter_CallsCleanup()
        {
            _sm.ChangeState<DealingState>();
            Assert.IsTrue(_ui.CleanupCalled);
        }

        [Test]
        public void Enter_CreatesNewGame()
        {
            _sm.ChangeState<DealingState>();
            Assert.IsTrue(_ctx.CreateNewGameCalled);
            Assert.IsNotNull(_ctx.Game);
        }

        [Test]
        public void Enter_CallsStartGame()
        {
            _sm.ChangeState<DealingState>();
            Assert.IsTrue(_ui.StartGameCalled);
        }

        [Test]
        public void Enter_PassesCreatedGameToStartGame()
        {
            _sm.ChangeState<DealingState>();
            Assert.AreSame(_ctx.Game, _ui.LastStartedGame);
        }

        [Test]
        public void OnDealingComplete_TransitionsToPlayingState()
        {
            _sm.ChangeState<DealingState>();
            _ui.RaiseDealingComplete();

            Assert.IsTrue(_sm.IsInState<PlayingState>());
        }

        [Test]
        public void Exit_UnsubscribesFromDealingComplete()
        {
            _sm.ChangeState<DealingState>();
            _sm.ChangeState<PlayingState>(); // triggers Exit

            // Raising event after exit should NOT cause another transition
            Assert.DoesNotThrow(() => _ui.RaiseDealingComplete());
        }
    }

    // =========================================================================
    //  PlayingState Tests
    // =========================================================================

    [TestFixture]
    public class PlayingStateTests
    {
        private MockGameContext _ctx;
        private MockGameUI _ui;
        private GameStateManager _sm;

        [SetUp]
        public void SetUp()
        {
            (_ctx, _ui, _sm) = TestRig.CreateFull();
        }

        [Test]
        public void Enter_EnablesInteraction()
        {
            _sm.ChangeState<PlayingState>();
            Assert.AreEqual(true, _ui.LastSetInteractable);
        }

        [Test]
        public void Exit_DisablesInteraction()
        {
            _sm.ChangeState<PlayingState>();
            _ui.Reset();

            _sm.ChangeState<WinState>(); // triggers Exit

            Assert.AreEqual(false, _ui.LastSetInteractable);
        }

        [Test]
        public void Exit_HidesAutoCompleteButton()
        {
            _sm.ChangeState<PlayingState>();
            _ui.Reset();

            _sm.ChangeState<WinState>();

            Assert.IsTrue(_ui.HideAutoCompleteCalled);
        }

        [Test]
        public void OnAutoCompleteRequested_TransitionsToAutoCompleteState()
        {
            _sm.ChangeState<PlayingState>();

            _ui.RaiseAutoCompleteRequested();

            Assert.IsTrue(_sm.IsInState<AutoCompleteState>());
        }

        [Test]
        public void Exit_UnsubscribesFromAutoCompleteRequested()
        {
            _sm.ChangeState<PlayingState>();
            _sm.ChangeState<WinState>(); // triggers Exit

            // Raising after exit should not cause transition
            Assert.DoesNotThrow(() => _ui.RaiseAutoCompleteRequested());
            Assert.IsTrue(_sm.IsInState<WinState>());
        }
    }

    // =========================================================================
    //  WinState Tests
    // =========================================================================

    [TestFixture]
    public class WinStateTests
    {
        private MockGameContext _ctx;
        private MockGameUI _ui;
        private GameStateManager _sm;

        [SetUp]
        public void SetUp()
        {
            (_ctx, _ui, _sm) = TestRig.CreateFull();
        }

        [Test]
        public void Enter_ShowsWinScreen()
        {
            _sm.ChangeState<WinState>();
            Assert.IsTrue(_ui.ShowWinScreenCalled);
        }

        [Test]
        public void OnPlayAgainRequested_TransitionsToDealingState()
        {
            _sm.ChangeState<WinState>();

            _ui.RaisePlayAgainRequested();

            Assert.IsTrue(_sm.IsInState<DealingState>());
        }

        [Test]
        public void Exit_UnsubscribesFromEvents()
        {
            _sm.ChangeState<WinState>();
            _sm.ChangeState<PlayingState>(); // triggers Exit on WinState

            // Raising after exit should not cause transition
            Assert.DoesNotThrow(() => _ui.RaisePlayAgainRequested());
            Assert.IsTrue(_sm.IsInState<PlayingState>());
        }
    }

    // =========================================================================
    //  AutoCompleteState Tests
    // =========================================================================

    [TestFixture]
    public class AutoCompleteStateTests
    {
        private MockGameContext _ctx;
        private MockGameUI _ui;
        private GameStateManager _sm;

        [SetUp]
        public void SetUp()
        {
            (_ctx, _ui, _sm) = TestRig.CreateFull();
        }

        [Test]
        public void Enter_DisablesInteraction()
        {
            _sm.ChangeState<AutoCompleteState>();
            Assert.AreEqual(false, _ui.LastSetInteractable);
        }

        [Test]
        public void Enter_HidesAutoCompleteButton()
        {
            _sm.ChangeState<AutoCompleteState>();
            Assert.IsTrue(_ui.HideAutoCompleteCalled);
        }
    }
}
