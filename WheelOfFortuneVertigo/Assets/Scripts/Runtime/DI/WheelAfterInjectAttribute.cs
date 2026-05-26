using System;

namespace Vertigo.Wheel.Runtime
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class WheelAfterInjectAttribute : Attribute
    {
    }
}
