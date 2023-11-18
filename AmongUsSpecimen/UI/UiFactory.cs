using System;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
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
        out Slider slider,
        bool transparent = false
    )
    {
        if (!transparent) return UF.CreateSliderScrollbar(parent, out slider);
        var uiObject1 = UF.CreateUIObject("SliderScrollbar", parent, new Vector2(25f, 25f));
        uiObject1.AddComponent<Mask>().showMaskGraphic = false;
        uiObject1.AddComponent<Image>().color = UIPalette.Transparent;
        var uiObject2 = UIFactory.CreateUIObject("Background", uiObject1);
        var uiObject3 = UIFactory.CreateUIObject("Handle Slide Area", uiObject1);
        var uiObject4 = UIFactory.CreateUIObject("Handle", uiObject3);
        var image1 = uiObject2.AddComponent<Image>();
        image1.type = Image.Type.Sliced;
        image1.color = UIPalette.Transparent;
        uiObject2.AddComponent<Mask>();
        var component1 = uiObject2.GetComponent<RectTransform>();
        component1.pivot = new Vector2(0.0f, 1f);
        component1.anchorMin = Vector2.zero;
        component1.anchorMax = Vector2.one;
        component1.sizeDelta = Vector2.zero;
        component1.offsetMax = new Vector2(0.0f, 0.0f);
        var component2 = uiObject3.GetComponent<RectTransform>();
        component2.anchorMin = Vector2.zero;
        component2.anchorMax = Vector2.one;
        component2.pivot = new Vector2(0.5f, 0.5f);
        var image2 = uiObject4.AddComponent<Image>();
        image2.color = UIPalette.Transparent;
        uiObject4.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        var gameObject1 = uiObject4;
        SetLayoutElement(gameObject1, 21);
        var layoutElement = uiObject1.AddComponent<LayoutElement>();
        layoutElement.minWidth = 25f;
        layoutElement.flexibleWidth = 0.0f;
        layoutElement.minHeight = 30f;
        layoutElement.flexibleHeight = 9999f;
        slider = uiObject1.AddComponent<Slider>();
        slider.handleRect = uiObject4.GetComponent<RectTransform>();
        slider.targetGraphic = image2;
        slider.direction = Slider.Direction.TopToBottom;
        SetLayoutElement(uiObject1, 25, null, 0, 9999);
        RuntimeHelper.SetColorBlock(slider, UIPalette.Transparent,
        UIPalette.Transparent, UIPalette.Transparent,
        UIPalette.Transparent);
        return uiObject1;
    }

    public static GameObject CreateScrollView(
        GameObject parent,
        string name,
        out GameObject content,
        out AutoSliderScrollbar autoScrollbar,
        Color color = default,
        int? minWidth = null,
        int? minHeight = null,
        int spacing = 5
    )
    {
        var uiObject1 = CreateUIObject(name, parent);
        var component1 = uiObject1.GetComponent<RectTransform>();
        component1.anchorMin = Vector2.zero;
        component1.anchorMax = Vector2.one;
        var image = uiObject1.AddComponent<Image>();
        image.type = Image.Type.Filled;
        image.color = color == default ? new Color(0.3f, 0.3f, 0.3f, 1f) : color;
        image.enabled = color != default;
        SetLayoutElement(uiObject1, null, null, 9999, 9999);
        var uiObject2 = CreateUIObject("Viewport", uiObject1);
        var component2 = uiObject2.GetComponent<RectTransform>();
        component2.anchorMin = Vector2.zero;
        component2.anchorMax = Vector2.one;
        component2.pivot = new Vector2(0.0f, 1f);
        component2.offsetMax = new Vector2(-28f, 0.0f);
        if (color == default)
        {
            uiObject2.AddComponent<Image>().enabled = false;
        }
        else
        {
            uiObject2.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);
        }

        uiObject2.AddComponent<Mask>().showMaskGraphic = false;
        content = CreateUIObject("Content", uiObject2);
        var gameObject2 = content;
        SetLayoutGroup<VerticalLayoutGroup>(gameObject2, true, false, true, true, spacing, 30, null, 10, null,
            TextAnchor.MiddleLeft);
        var gameObject3 = content;
        SetLayoutElement(gameObject3, minWidth, minHeight);
        var component3 = content.GetComponent<RectTransform>();
        component3.anchorMin = Vector2.zero;
        component3.anchorMax = Vector2.one;
        component3.pivot = new Vector2(0.0f, 1f);
        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var uiObject3 = CreateUIObject("AutoSliderScrollbar", uiObject1);
        var component4 = uiObject3.GetComponent<RectTransform>();
        component4.anchorMin = new Vector2(1f, 0.0f);
        component4.anchorMax = Vector2.one;
        component4.offsetMin = new Vector2(-25f, 0.0f);
        SetLayoutGroup<VerticalLayoutGroup>(uiObject3, false, true, true, true);
        if (color == default)
        {
            uiObject3.AddComponent<Image>().enabled = false;
        }
        else
        {
            uiObject3.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);
        }

        uiObject3.AddComponent<Mask>().showMaskGraphic = false;
        var scrollbar2 = CreateScrollbar(uiObject3, "HiddenScrollviewScroller", out var scrollbar1);
        scrollbar1.SetDirection(UnityEngine.UI.Scrollbar.Direction.BottomToTop, true);
        for (var index = 0; index < scrollbar2.transform.childCount; ++index)
        {
            scrollbar2.transform.GetChild(index).gameObject.SetActive(false);
        }

        CreateSliderScrollbar(uiObject3, out var slider, color == default);
        autoScrollbar = new AutoSliderScrollbar(scrollbar1, slider, component3, component2);
        var scrollRect = uiObject1.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.verticalScrollbar = scrollbar1;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 35f;
        scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        scrollRect.viewport = component2;
        scrollRect.content = component3;
        return uiObject1;
    }

    public static GameObject CreateScrollInputField(
        GameObject parent,
        string name,
        string placeHolderText,
        out InputFieldScroller inputScroll,
        int fontSize = 14,
        Color color = default
    ) => UF.CreateScrollInputField(parent, name, placeHolderText, out inputScroll, fontSize, color);
}