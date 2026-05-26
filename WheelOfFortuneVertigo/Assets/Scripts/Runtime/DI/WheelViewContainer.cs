using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelViewContainer
    {
        private readonly Dictionary<Type, List<Component>> _bindings = new Dictionary<Type, List<Component>>();

        public static WheelViewContainer Build(Transform root)
        {
            var container = new WheelViewContainer();
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                container.Register(behaviours[i]);
            }

            return container;
        }

        public void Register(Component component)
        {
            if (component == null)
            {
                return;
            }

            Type type = component.GetType();
            while (type != null && type != typeof(MonoBehaviour) && type != typeof(Component))
            {
                Add(type, component);
                type = type.BaseType;
            }
        }

        public T Resolve<T>() where T : class
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            List<Component> matches = GetMatches(type);
            if (matches.Count == 0)
            {
                throw new InvalidOperationException("No view binding for " + type.Name + ".");
            }

            if (matches.Count > 1)
            {
                throw new InvalidOperationException(
                    "Multiple view bindings for " + type.Name + ". Use a unique concrete type or aggregate binding.");
            }

            return matches[0];
        }

        private List<Component> GetMatches(Type type)
        {
            if (_bindings.TryGetValue(type, out List<Component> matches))
            {
                return matches;
            }

            return EmptyComponents;
        }

        private void Add(Type type, Component component)
        {
            if (!_bindings.TryGetValue(type, out List<Component> list))
            {
                list = new List<Component>();
                _bindings[type] = list;
            }

            if (list.Contains(component))
            {
                return;
            }

            list.Add(component);
        }

        private static readonly List<Component> EmptyComponents = new List<Component>();
    }

    public static class WheelBindDiscovery
    {
        private const BindingFlags TypeFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static MonoBehaviour[] Collect(Transform root)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            var bindables = new List<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (IsBindable(behaviour.GetType()))
                {
                    bindables.Add(behaviour);
                }
            }

            return bindables.ToArray();
        }

        private static bool IsBindable(Type type)
        {
            while (type != null && type != typeof(MonoBehaviour))
            {
                if (type.GetCustomAttribute<WheelBindAttribute>(false) != null)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}
