using System;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelBindingValidation
    {
        public static void Require(Component owner, UnityEngine.Object value, string fieldName, string bindingKind)
        {
            if (value == null)
            {
                throw new InvalidOperationException(owner.name + " requires " + bindingKind + " binding " + fieldName + ".");
            }
        }
    }
}
