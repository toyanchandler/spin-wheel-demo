using NUnit.Framework;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelGameplaySessionTests
    {
        private WheelTestObjectScope _scope;

        [TearDown]
        public void TearDown()
        {
            WheelRuntimeLocator.Clear();
            _scope?.DestroyAll();
        }

        [Test]
        public void TryGet_ReturnsFalse_WhenLocatorNotReady()
        {
            Assert.IsFalse(WheelGameplaySession.TryGet(out WheelGameplaySession session));
            Assert.IsFalse(session.IsValid);
        }

        [Test]
        public void TryGet_ReturnsPopulatedSession_WhenLocatorReady()
        {
            _scope = new WheelTestObjectScope();
            var eventBus = new WheelEventBus();
            WheelGameSettings settings = WheelTestFixtures.CreateSettings(_scope);
            var stateObject = _scope.Track(new GameObject("WheelGameStateSessionTest"));
            WheelGameState state = stateObject.AddComponent<WheelGameState>();
            state.InitializeRuntime(settings);
            var publisherObject = _scope.Track(new GameObject("WheelStatePublisherSessionTest"));
            WheelStatePublisher publisher = publisherObject.AddComponent<WheelStatePublisher>();
            var flowObject = _scope.Track(new GameObject("WheelGameFlowControllerSessionTest"));
            WheelGameFlowController flow = flowObject.AddComponent<WheelGameFlowController>();
            flow.Bind(eventBus, state, publisher, null);
            var spinnerObject = _scope.Track(new GameObject("WheelSpinnerLocatorTest"));
            WheelSpinner spinner = spinnerObject.AddComponent<WheelSpinner>();

            WheelRuntimeLocator.RegisterSession(eventBus, settings, state, publisher, flow, spinner);
            WheelRuntimeLocator.NotifyRuntimeReady();

            Assert.IsTrue(WheelGameplaySession.TryGet(out WheelGameplaySession session));
            Assert.IsTrue(session.IsValid);
            Assert.IsTrue(WheelRuntimeLocator.TryGetSession(out WheelGameplaySession viaLocator));
            Assert.AreSame(session.EventBus, viaLocator.EventBus);
            Assert.AreSame(eventBus, session.EventBus);
            Assert.AreSame(state, session.State);
            Assert.AreSame(flow, session.Flow);
            Assert.AreSame(spinner, session.Spinner);
        }
    }
}
