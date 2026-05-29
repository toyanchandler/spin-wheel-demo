using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelGameFlowControllerTests
    {
        private WheelTestObjectScope _scope;
        private GameObject _flowObject;
        private GameObject _publisherObject;
        private GameObject _spinnerObject;
        private WheelGameFlowController _flow;
        private WheelGameState _state;
        private WheelGameSettings _settings;
        private WheelStatePublisher _publisher;
        private WheelSpinner _spinner;
        private WheelEventBus _eventBus;
        private readonly List<WheelSpinResult> _landedResults = new List<WheelSpinResult>();

        [SetUp]
        public void SetUp()
        {
            _scope = new WheelTestObjectScope();
            _eventBus = new WheelEventBus();
            _eventBus.SpinLanded += result => _landedResults.Add(result);
            _state = WheelTestFixtures.CreateState(_scope, out _settings);
            _publisherObject = _scope.Track(new GameObject("WheelStatePublisherFlowTest"));
            _publisher = _publisherObject.AddComponent<WheelStatePublisher>();
            _publisher.Bind(_eventBus, _state);
            _spinnerObject = _scope.Track(new GameObject("WheelSpinnerFlowTest"));
            _spinner = _spinnerObject.AddComponent<WheelSpinner>();
            _flowObject = _scope.Track(new GameObject("WheelGameFlowControllerTest"));
            _flow = _flowObject.AddComponent<WheelGameFlowController>();
            _flow.Bind(_eventBus, _state, _publisher, _spinner);
            _landedResults.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            WheelRuntimeLocator.Clear();
            _scope.DestroyAll();
        }

        [Test]
        public void LandingStarted_RaisesSpinLanded_BeforeSpinCompletedResolves()
        {
            WheelSpinResult result = WheelTestFixtures.CreateRewardResult(_scope, 0, "gold", "Gold", 1);

            _spinner.RaiseLandingStartedForTests(result);
            _spinner.RaiseSpinCompletedForTests(result);

            Assert.AreEqual(1, _landedResults.Count);
            Assert.AreEqual(result, _landedResults[0]);
            Assert.AreEqual(WheelGamePhase.Won, _state.Phase);
            Assert.AreEqual(1, _state.Inventory.Count);
        }

        [Test]
        public void ForceResolveOutcome_UsesSamePathAsCompletedSpin()
        {
            WheelSpinResult result = WheelTestFixtures.CreateRewardResult(_scope, 0, "gold", "Gold", 1);

            _flow.ForceResolveOutcome(result);

            Assert.AreEqual(1, _landedResults.Count);
            Assert.AreEqual(WheelGamePhase.Won, _state.Phase);
            Assert.AreEqual(1, _state.Inventory.Count);
        }
    }
}
