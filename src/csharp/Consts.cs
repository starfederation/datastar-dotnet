// This is auto-generated by Datastar. DO NOT EDIT.

namespace StarFederation.Datastar;

using System;

public enum ElementPatchMode
{
    /// Morphs the element into the existing element.
    Outer,
    /// Replaces the inner HTML of the existing element.
    Inner,
    /// Removes the existing element.
    Remove,
    /// Replaces the existing element with the new element.
    Replace,
    /// Prepends the element inside to the existing element.
    Prepend,
    /// Appends the element inside the existing element.
    Append,
    /// Inserts the element before the existing element.
    Before,
    /// Inserts the element after the existing element.
    After,

}
public enum EventType
{
    /// An event for patching HTML elements into the DOM.
    PatchElements,
    /// An event for patching signals.
    PatchSignals,

}

public static class Consts
{
    public const string DatastarKey = "datastar";
    public const string Version     = "1.0.0";

    /// Default: TimeSpan.FromMilliseconds 1000
    public static readonly TimeSpan DefaultSseRetryDuration = TimeSpan.FromMilliseconds(1000);


    /// Default: outer - Morphs the element into the existing element.
    public const ElementPatchMode DefaultElementPatchMode = ElementPatchMode.Outer;

    public const bool DefaultElementsUseViewTransitions = false;
    public const bool DefaultPatchSignalsOnlyIfMissing = false;


    public const string DatastarDatalineSelector = "selector";
    public const string DatastarDatalineMode = "mode";
    public const string DatastarDatalineElements = "elements";
    public const string DatastarDatalineUseViewTransition = "useViewTransition";
    public const string DatastarDatalineSignals = "signals";
    public const string DatastarDatalineOnlyIfMissing = "onlyIfMissing";

    public static string EnumToString( ElementPatchMode enumValue ) => enumValue switch {
        ElementPatchMode.Outer => "outer",
        ElementPatchMode.Inner => "inner",
        ElementPatchMode.Remove => "remove",
        ElementPatchMode.Replace => "replace",
        ElementPatchMode.Prepend => "prepend",
        ElementPatchMode.Append => "append",
        ElementPatchMode.Before => "before",
        ElementPatchMode.After => "after",
        _ => throw new NotImplementedException($"ElementPatchMode.{enumValue}")
    };
    public static string EnumToString( EventType enumValue ) => enumValue switch {
        EventType.PatchElements => "datastar-patch-elements",
        EventType.PatchSignals => "datastar-patch-signals",
        _ => throw new NotImplementedException($"EventType.{enumValue}")
    };
}