using System;

namespace Vertigo.Wheel.Runtime
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class WheelBeforeUnbindAttribute : Attribute
    {
    }
}
