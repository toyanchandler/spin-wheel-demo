using System;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    /// <summary>Canonical null-check validation for scene wiring, bindings, and prefab references.</summary>
    internal static class WheelWiringValidation
    {
        public static void Require(Component owner, string panelName, UnityEngine.Object value, string fieldName)
        {
            if (value != null)
            {
                return;
            }

            throw new InvalidOperationException(owner.name + " requires " + panelName + " wiring field " + fieldName + ".");
        }

        public static void Require(Component owner, UnityEngine.Object value, string fieldName, string bindingKind)
        {
            if (value != null)
            {
                return;
            }

            throw new InvalidOperationException(owner.name + " requires " + bindingKind + " binding " + fieldName + ".");
        }

        public static void Require(string hostName, string prefabLabel, UnityEngine.Object value, string fieldName)
        {
            if (value != null)
            {
                return;
            }

            throw new InvalidOperationException(hostName + " " + prefabLabel + " requires " + fieldName + ".");
        }
    }
}
