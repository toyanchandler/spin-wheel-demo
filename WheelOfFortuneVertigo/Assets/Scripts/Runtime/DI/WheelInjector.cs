using System;
using System.Reflection;
using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    public static class WheelInjector
    {
        internal const BindingFlags MemberFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        public static void Inject(MonoBehaviour target, WheelEventBus eventBus, WheelViewContainer views)
        {
            WheelInjectorTypeCache cache = WheelInjectorTypeCache.Get(target.GetType());
            cache.InjectFields(target, eventBus, views);
            cache.InvokeAfterInject(target);
        }

        public static void Uninject(MonoBehaviour target)
        {
            WheelInjectorTypeCache.Get(target.GetType()).InvokeBeforeUnbind(target);
        }

        internal static object Resolve(
            Type fieldType,
            WheelEventBus eventBus,
            WheelViewContainer views,
            MonoBehaviour target,
            string fieldName)
        {
            if (fieldType == typeof(WheelEventBus))
            {
                return eventBus;
            }

            if (fieldType == typeof(IWheelUiIntentPublisher)
                || fieldType == typeof(IWheelUiIntentSubscriber)
                || fieldType == typeof(IWheelSnapshotPublisher)
                || fieldType == typeof(IWheelSnapshotSubscriber))
            {
                return eventBus;
            }

            if (!typeof(Component).IsAssignableFrom(fieldType))
            {
                throw new InvalidOperationException(
                    target.name + "." + fieldName + " can only inject WheelEventBus, event bus capability interfaces, or Component types.");
            }

            return views.Resolve(fieldType);
        }
    }
}
