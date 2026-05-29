using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.Wheel.Tests
{
    /// <summary>Tracks Unity objects created by test fixtures and destroys them in TearDown.</summary>
    internal sealed class WheelTestObjectScope
    {
        private readonly List<Object> _objects = new List<Object>();

        public T Track<T>(T instance) where T : Object
        {
            if (instance != null)
            {
                _objects.Add(instance);
            }

            return instance;
        }

        public void DestroyAll()
        {
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                Object obj = _objects[i];
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _objects.Clear();
        }
    }
}
