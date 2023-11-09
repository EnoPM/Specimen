using System;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Widgets;
using UniverseLib.UI.Widgets.ScrollView;
using UF = UniverseLib.UI.UIFactory;

namespace AmongUsSpecimen.UI;

public static class UiFactory
{
    public static GameObject CreateUIObject(
        string name,
        GameObject parent,
        Vector2 sizeDelta = default
    ) => UF.CreateUIObject(name, parent, sizeDelta);

    public static LayoutElement SetLayoutElement(
        GameObject gameObject,
        int? minWidth = null,
        int? minHeight = null,
        int? flexibleWidth = null,
        int? flexibleHeight = null,
        int? preferredWidth = null,
        int? preferredHeight = null,
        bool? ignoreLayout = null
    ) => UF.SetLayoutElement(gameObject, minWidth, minHeight, flexibleWidth, flexibleHeight, preferredWidth,
        preferredHeight, ignoreLayout);

    public static T SetLayoutGroup<T>(
        GameObject gameObject,
        bool? forceWidth = null,
        bool? forceHeight = null,
        bool? childControlWidth = null,
        bool? childControlHeight = null,
        int? spacing = null,
        int? padTop = null,
        int? padBottom = null,
        int? padLeft = null,
        int? padRight = null,
        TextAnchor? childAlignment = null
    ) where T : HorizontalOrVerticalLayoutGroup => UF.SetLayoutGroup<T>(gameObject, forceWidth, forceHeight,
        childControlWidth, childControlHeight, spacing, padTop, padBottom, padLeft, padRight, childAlignment);

    public static T SetLayoutGroup<T>(
        T group,
        bool? forceWidth = null,
        bool? forceHeight = null,
        bool? childControlWidth = null,
        bool? childControlHeight = null,
        int? spacing = null,
        int? padTop = null,
        int? padBottom = null,
        int? padLeft = null,
        int? padRight = null,
        TextAnchor? childAlignment = null
    ) where T : HorizontalOrVerticalLayoutGroup => UF.SetLayoutGroup(group, forceWidth, forceHeight,
        childControlWidth, childControlHeight, spacing, padTop, padBottom, padLeft, padRight, childAlignment);

    public static GameObject CreatePanel(
        string name,
        GameObject parent,
        out GameObject contentHolder,
        Color? bgColor = null
    ) => UF.CreatePanel(name, parent, out contentHolder, bgColor);

    public static GameObject CreateVerticalGroup(
        GameObject parent,
        string name,
        bool forceWidth,
        bool forceHeight,
        bool childControlWidth,
        bool childControlHeight,
        int spacing = 0,
        Vector4 padding = default,
        Color bgColor = default,
        TextAnchor? childAlignment = null
    ) => UF.CreateVerticalGroup(parent, name, forceWidth, forceHeight, childControlWidth, childControlHeight, spacing,
        padding, bgColor, childAlignment);

    public static GameObject CreateHorizontalGroup(
        GameObject parent,
        string name,
        bool forceExpandWidth,
        bool forceExpandHeight,
        bool childControlWidth,
        bool childControlHeight,
        int spacing = 0,
        Vector4 padding = default,
        Color bgColor = default,
        TextAnchor? childAlignment = null
    ) => UF.CreateHorizontalGroup(parent, name, forceExpandWidth, forceExpandHeight, childControlWidth,
        childControlHeight, spacing, padding, bgColor, childAlignment);

    public static GameObject CreateGridGroup(
        GameObject parent,
        string name,
        Vector2 cellSize,
        Vector2 spacing,
        Color bgColor = default
    ) => UF.CreateGridGroup(parent, name, cellSize, spacing, bgColor);

    public static Text CreateLabel(
        GameObject parent,
        string name,
        string defaultText,
        TextAnchor alignment = TextAnchor.MiddleLeft,
        Color color = default,
        bool supportRichText = true,
        int fontSize = 14
    ) => UF.CreateLabel(parent, name, defaultText, alignment, color, supportRichText, fontSize);

    public static Components.ButtonRef CreateButton(
        GameObject parent,
        string name,
        string text,
        Color? normalColor = null
    ) => new(UF.CreateButton(parent, name, text, normalColor));

    public static ButtonRef CreateButton(
        GameObject parent,
        string name,
        string text,
        ColorBlock colors
    ) => UF.CreateButton(parent, name, text, colors);

    public static GameObject CreateSlider(
        GameObject parent,
        string name,
        out Slider slider
    ) => UF.CreateSlider(parent, name, out slider);

    public static GameObject CreateScrollbar(
        GameObject parent,
        string name,
        out UnityEngine.UI.Scrollbar scrollbar
    ) => UF.CreateScrollbar(parent, name, out scrollbar);

    public static GameObject CreateToggle(
        GameObject parent,
        string name,
        out Toggle toggle,
        out Text text,
        Color bgColor = default,
        int checkWidth = 20,
        int checkHeight = 20
    ) => UF.CreateToggle(parent, name, out toggle, out text, bgColor, checkWidth, checkHeight);

    public static InputFieldRef CreateInputField(
        GameObject parent,
        string name,
        string placeHolderText
    ) => UF.CreateInputField(parent, name, placeHolderText);

    public static GameObject CreateDropdown(
        GameObject parent,
        string name,
        out Dropdown dropdown,
        string defaultItemText,
        int itemFontSize,
        Action<int> onValueChanged,
        string[] defaultOptions = null
    ) => UF.CreateDropdown(parent, name, out dropdown, defaultItemText, itemFontSize, onValueChanged, defaultOptions);

    public static ScrollPool<T> CreateScrollPool<T>(
        GameObject parent,
        string name,
        out GameObject uiRoot,
        out GameObject content,
        Color? bgColor = null
    ) where T : ICell => UF.CreateScrollPool<T>(parent, name, out uiRoot, out content, bgColor);

    public static GameObject CreateSliderScrollbar(
        GameObject parent,
        out Slider slider
    ) => UF.CreateSliderScrollbar(parent, out slider);

    public static GameObject CreateScrollView(
        GameObject parent,
        string name,
        out GameObject content,
        out AutoSliderScrollbar autoScrollbar,
        Color color = default
    ) => UF.CreateScrollView(parent, name, out content, out autoScrollbar, color);

    public static GameObject CreateScrollInputField(
        GameObject parent,
        string name,
        string placeHolderText,
        out InputFieldScroller inputScroll,
        int fontSize = 14,
        Color color = default
    ) => UF.CreateScrollInputField(parent, name, placeHolderText, out inputScroll, fontSize, color);
}