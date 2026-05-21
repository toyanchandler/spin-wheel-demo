# Vertigo.Collections (editor helper)

Runtime footprint: **`[CollectChildren]` attribute only** on serialized array fields. All collection logic lives under `Editor/`.

## On a view (typical)

```csharp
[SerializeField] private Transform _slicePoolRoot;

[CollectChildren(nameof(_slicePoolRoot))]
[SerializeField] private WheelSliceView[] _sliceViews;
```

- Play mode reads `_sliceViews` only.
- Inspector shows the array with a **Collect Children** button (property drawer).
- Optional `Transform` root when pooled objects are not direct children of the view.

## Scene builder / batch

```csharp
WheelEditorWiring.SetReference(view, "_slicePoolRoot", sliceRoot.transform);
WheelEditorWiring.CollectChildren(view, "_sliceViews", sliceRoot.transform);
```

`FinalizeCollect` runs inside the editor service (`SetDirty` + `MarkSceneDirty`).

## Custom struct bindings

No extra MonoBehaviour: add a **view-specific** `CustomEditor` with a collect button (see `WheelZoneProgressViewEditor`).

## Ordering

Sibling order under the collection root (`GetChild` order). Names are not parsed.

## Preprocess build

Fields with `[CollectChildren(collectOnPreprocessBuild: true)]` are refreshed automatically before builds.
