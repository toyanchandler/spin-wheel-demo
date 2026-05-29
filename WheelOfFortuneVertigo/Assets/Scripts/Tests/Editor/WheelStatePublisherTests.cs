using NUnit.Framework;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelStatePublisherTests
    {
        private WheelTestObjectScope _scope;
        private WheelStatePublisher _publisher;
        private WheelGameState _state;
        private WheelGameSettings _settings;
        private WheelEventBus _eventBus;
        private int _hudPublishCount;

        [SetUp]
        public void SetUp()
        {
            _scope = new WheelTestObjectScope();
            _eventBus = new WheelEventBus();
            _eventBus.HudStateChanged += _ => _hudPublishCount++;
            _state = WheelTestFixtures.CreateState(_scope, out _settings);
            var publisherObject = _scope.Track(new GameObject("WheelStatePublisherTest"));
            _publisher = publisherObject.AddComponent<WheelStatePublisher>();
            _publisher.Bind(_eventBus, _state);
            _hudPublishCount = 0;
        }

        [TearDown]
        public void TearDown()
        {
            WheelRuntimeLocator.Clear();
            _scope.DestroyAll();
        }

        [Test]
        public void PublishHud_AfterUnbind_IsNoOp()
        {
            _publisher.Unbind();

            Assert.DoesNotThrow(() => _publisher.PublishHud());
            Assert.AreEqual(0, _hudPublishCount);
        }

        [Test]
        public void PublishHud_WhenBound_RaisesHudStateChanged()
        {
            _publisher.PublishHud();

            Assert.AreEqual(1, _hudPublishCount);
        }
    }
}
