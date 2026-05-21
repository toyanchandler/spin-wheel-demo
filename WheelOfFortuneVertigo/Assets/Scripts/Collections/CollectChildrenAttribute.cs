using System;
using UnityEngine;

namespace Vertigo.Collections
{
    public enum ChildCollectionScope
    {
        DirectChildren = 0,
        Descendants = 1
    }

    /// <summary>
    /// Editor-only metadata on a serialized array field. Runtime uses the filled array; no collection logic on the behaviour.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CollectChildrenAttribute : PropertyAttribute
    {
        public string RootProperty { get; }
        public ChildCollectionScope Scope { get; }
        public bool IncludeInactive { get; }
        public bool CollectOnPreprocessBuild { get; }

        public CollectChildrenAttribute(
            string rootProperty = null,
            ChildCollectionScope scope = ChildCollectionScope.DirectChildren,
            bool includeInactive = true,
            bool collectOnPreprocessBuild = true)
        {
            RootProperty = rootProperty;
            Scope = scope;
            IncludeInactive = includeInactive;
            CollectOnPreprocessBuild = collectOnPreprocessBuild;
        }
    }
}
