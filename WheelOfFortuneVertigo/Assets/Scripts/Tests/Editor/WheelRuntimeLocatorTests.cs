using System;
using NUnit.Framework;
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelRuntimeLocatorTests
    {
        private WheelTestObjectScope _scope;

        [TearDown]
        public void TearDown()
        {
            WheelRuntimeLocator.Clear();
            _scope?.DestroyAll();
        }

        [Test]
        public void RegisterSession_ExposesServicesUntilClear()
        {
            _scope = new WheelTestObjectScope();
            var eventBus = new WheelEventBus();
            var settings = WheelTestFixtures.CreateSettings(_scope);
            var stateObject = _scope.Track(new GameObject("WheelGameStateLocatorTest"));
            WheelGameState state = stateObject.AddComponent<WheelGameState>();
            state.InitializeRuntime(settings);
            var publisherObject = _scope.Track(new GameObject("WheelStatePublisherLocatorTest"));
            WheelStatePublisher publisher = publisherObject.AddComponent<WheelStatePublisher>();
            var flowObject = _scope.Track(new GameObject("WheelGameFlowControllerLocatorTest"));
            WheelGameFlowController flow = flowObject.AddComponent<WheelGameFlowController>();
            var spinnerObject = _scope.Track(new GameObject("WheelSpinnerLocatorTest"));
            WheelSpinner spinner = spinnerObject.AddComponent<WheelSpinner>();

            WheelRuntimeLocator.RegisterSession(eventBus, settings, state, publisher, flow, spinner);
            WheelRuntimeLocator.NotifyRuntimeReady();

            Assert.IsTrue(WheelRuntimeLocator.IsReady);
            Assert.AreSame(eventBus, WheelRuntimeLocator.EventBus);
            Assert.AreSame(settings, WheelRuntimeLocator.Settings);
            Assert.AreSame(state, WheelRuntimeLocator.State);
            Assert.AreSame(publisher, WheelRuntimeLocator.Publisher);
            Assert.AreSame(flow, WheelRuntimeLocator.Flow);
            Assert.AreSame(spinner, WheelRuntimeLocator.Spinner);

            WheelRuntimeLocator.Clear();

            Assert.IsFalse(WheelRuntimeLocator.IsReady);
            Assert.IsNull(WheelRuntimeLocator.EventBus);
            Assert.IsNull(WheelRuntimeLocator.Settings);
            Assert.IsNull(WheelRuntimeLocator.State);
            Assert.IsNull(WheelRuntimeLocator.Publisher);
            Assert.IsNull(WheelRuntimeLocator.Flow);
            Assert.IsNull(WheelRuntimeLocator.Spinner);
        }

        [Test]
        public void NotifyRuntimeReady_Throws_WhenSessionNotRegistered()
        {
            Assert.Throws<InvalidOperationException>(() => WheelRuntimeLocator.NotifyRuntimeReady());
        }
    }
}
