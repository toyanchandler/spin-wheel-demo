using System;
using System.Reflection;
using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    public static class WheelInjector
    {
        private const BindingFlags MemberFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        public static void Inject(MonoBehaviour target, WheelEventBus eventBus, WheelViewContainer views)
        {
            InjectMembers(target, target.GetType(), eventBus, views);
            Type baseType = target.GetType().BaseType;
            while (baseType != null && baseType != typeof(MonoBehaviour))
            {
                InjectMembers(target, baseType, eventBus, views);
                baseType = baseType.BaseType;
            }

            InvokeLifecycleMethods(target, typeof(WheelAfterInjectAttribute));
        }

        public static void Uninject(MonoBehaviour target)
        {
            InvokeLifecycleMethods(target, typeof(WheelBeforeUnbindAttribute));
        }

        private static void InjectMembers(
            MonoBehaviour target,
            Type type,
            WheelEventBus eventBus,
            WheelViewContainer views)
        {
            FieldInfo[] fields = type.GetFields(MemberFlags);
            for (int i = 0; i < fields.Length; i++)
            {
                InjectField(target, fields[i], eventBus, views);
            }
        }

        private static void InjectField(
            MonoBehaviour target,
            FieldInfo field,
            WheelEventBus eventBus,
            WheelViewContainer views)
        {
            WheelInjectAttribute injectAttribute = field.GetCustomAttribute<WheelInjectAttribute>();
            if (injectAttribute == null)
            {
                return;
            }

            object value = Resolve(field.FieldType, eventBus, views, target, field.Name);
            field.SetValue(target, value);
        }

        private static object Resolve(
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

            if (!typeof(Component).IsAssignableFrom(fieldType))
            {
                throw new InvalidOperationException(
                    target.name + "." + fieldName + " can only inject WheelEventBus or Component types.");
            }

            return views.Resolve(fieldType);
        }

        private static void InvokeLifecycleMethods(MonoBehaviour target, Type attributeType)
        {
            Type type = target.GetType();
            while (type != null && type != typeof(MonoBehaviour))
            {
                InvokeLifecycleMethodsOnType(target, type, attributeType);
                type = type.BaseType;
            }
        }

        private static void InvokeLifecycleMethodsOnType(MonoBehaviour target, Type type, Type attributeType)
        {
            MethodInfo[] methods = type.GetMethods(MemberFlags);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.GetCustomAttribute(attributeType) == null)
                {
                    continue;
                }

                if (method.GetParameters().Length != 0)
                {
                    throw new InvalidOperationException(
                        target.name + "." + method.Name + " lifecycle methods cannot take parameters.");
                }

                method.Invoke(target, null);
            }
        }
    }
}
