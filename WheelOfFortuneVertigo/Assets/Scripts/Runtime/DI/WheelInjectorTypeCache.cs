using System;
using System.Collections.Generic;
using System.Reflection;

namespace Vertigo.Wheel.Runtime
{
    internal sealed class WheelInjectorTypeCache
    {
        private static readonly Dictionary<Type, WheelInjectorTypeCache> CacheByType = new Dictionary<Type, WheelInjectorTypeCache>();

        private readonly FieldInfo[] _injectFields;
        private readonly MethodInfo[] _afterInjectMethods;
        private readonly MethodInfo[] _beforeUnbindMethods;

        private WheelInjectorTypeCache(
            FieldInfo[] injectFields,
            MethodInfo[] afterInjectMethods,
            MethodInfo[] beforeUnbindMethods)
        {
            _injectFields = injectFields;
            _afterInjectMethods = afterInjectMethods;
            _beforeUnbindMethods = beforeUnbindMethods;
        }

        public static WheelInjectorTypeCache Get(Type type)
        {
            if (!CacheByType.TryGetValue(type, out WheelInjectorTypeCache cache))
            {
                cache = Build(type);
                CacheByType[type] = cache;
            }

            return cache;
        }

        public void InjectFields(object target, WheelEventBus eventBus, WheelViewContainer views)
        {
            for (int i = 0; i < _injectFields.Length; i++)
            {
                FieldInfo field = _injectFields[i];
                object value = WheelInjector.Resolve(field.FieldType, eventBus, views, (UnityEngine.MonoBehaviour)target, field.Name);
                field.SetValue(target, value);
            }
        }

        public void InvokeAfterInject(object target)
        {
            InvokeLifecycle(target, _afterInjectMethods);
        }

        public void InvokeBeforeUnbind(object target)
        {
            InvokeLifecycle(target, _beforeUnbindMethods);
        }

        private static WheelInjectorTypeCache Build(Type rootType)
        {
            var injectFields = new List<FieldInfo>();
            var afterInjectMethods = new List<MethodInfo>();
            var beforeUnbindMethods = new List<MethodInfo>();

            Type type = rootType;
            while (type != null && type != typeof(UnityEngine.MonoBehaviour))
            {
                CollectTypeMembers(type, injectFields, afterInjectMethods, beforeUnbindMethods);
                type = type.BaseType;
            }

            return new WheelInjectorTypeCache(
                injectFields.ToArray(),
                afterInjectMethods.ToArray(),
                beforeUnbindMethods.ToArray());
        }

        private static void CollectTypeMembers(
            Type type,
            List<FieldInfo> injectFields,
            List<MethodInfo> afterInjectMethods,
            List<MethodInfo> beforeUnbindMethods)
        {
            FieldInfo[] fields = type.GetFields(WheelInjector.MemberFlags);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (field.GetCustomAttribute<WheelInjectAttribute>() != null)
                {
                    injectFields.Add(field);
                }
            }

            MethodInfo[] methods = type.GetMethods(WheelInjector.MemberFlags);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.GetCustomAttribute<WheelAfterInjectAttribute>() != null)
                {
                    afterInjectMethods.Add(method);
                }

                if (method.GetCustomAttribute<WheelBeforeUnbindAttribute>() != null)
                {
                    beforeUnbindMethods.Add(method);
                }
            }
        }

        private static void InvokeLifecycle(object target, MethodInfo[] methods)
        {
            for (int i = 0; i < methods.Length; i++)
            {
                methods[i].Invoke(target, null);
            }
        }
    }
}
