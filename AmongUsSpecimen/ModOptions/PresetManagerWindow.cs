using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.UI.Components;
using AmongUsSpecimen.UI.Extensions;
using AmongUsSpecimen.Utils;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using ButtonRef = AmongUsSpecimen.UI.Components.ButtonRef;

namespace AmongUsSpecimen.ModOptions;

public class PresetManagerWindow : UiWindow
{
    public override string Name => "Specimen Updater";
    public override bool HasOverlay => true;
    protected override bool DisplayByDefault => false;
    public override bool AlwaysOnTop => true;
    protected override string Title => $"<b><color=#5925b3>Specimen</color> Preset Manager</b>";
    
    public override int MinWidth => 400;
    public override int MinHeight => 400;
    
    protected override Color BackgroundColor => UIPalette.Dark;
    protected override Positions Position => Positions.MiddleCenter;

    private ButtonRef _copyButton;
    private ButtonRef _pasteButton;
    private InputFieldRef _createInput;
    private ButtonRef _createButton;
    private readonly List<GameObject> _presetItems = [];
    private GameObject _presetsContainer;
    
    protected override void ConstructWindowContent()
    {
        var mainButtonContainer = UiFactory.CreateHorizontalGroup(ContentRoot, "Buttons", false, false, true, true,
            bgColor: UIPalette.LightDanger, childAlignment: TextAnchor.MiddleCenter, spacing: 15, padding: new Vector4(15f,  0f, 0f, 0f));
        mainButtonContainer.GetComponent<Image>().enabled = false;
        UiFactory.SetLayoutElement(mainButtonContainer, MinWidth, 50, 0, 0, 0, 0);
        _copyButton = UiFactory.CreateButton(mainButtonContainer, "CopyButton", "Copy preset");
        _copyButton.Component.SetColorsAuto(UIPalette.Primary);
        _copyButton.ButtonText.fontSize = 20;
        _copyButton.ButtonText.fontStyle = FontStyle.Bold;
        _copyButton.OnClick += OnCopyButtonClick;
        UiFactory.SetLayoutElement(_copyButton.GameObject, 185, 35, 0, 0, 0, 0);
        _pasteButton = UiFactory.CreateButton(mainButtonContainer, "PasteButton", "Paste preset");
        _pasteButton.Component.SetColorsAuto(UIPalette.Primary);
        _pasteButton.ButtonText.fontSize = 20;
        _pasteButton.ButtonText.fontStyle = FontStyle.Bold;
        _pasteButton.OnClick += OnPasteButtonClick;
        UiFactory.SetLayoutElement(_pasteButton.GameObject, 170, 35, 0, 0, 0, 0);

        var creationContainer = UiFactory.CreateVerticalGroup(ContentRoot, "CreatePreset", false, false, true, true, 5, childAlignment: TextAnchor.UpperCenter, bgColor: UIPalette.LightDanger, padding: new Vector4(15f, 0f, 0f, 0f));
        creationContainer.GetComponent<Image>().enabled = false;
        UiFactory.SetLayoutElement(creationContainer, MinWidth, 90, 0, 0, 0, 0);
        _createInput = UiFactory.CreateInputField(creationContainer, "PresetName", "Preset name");
        UiFactory.SetLayoutElement(_createInput.GameObject, MinWidth - 20, 30, 0, 0, 0, 0);
        _createInput.OnValueChanged += OnCreateInputValueChanged;
        _createButton = UiFactory.CreateButton(creationContainer, "CreateButton", "Create preset");
        _createButton.Component.SetColorsAuto(UIPalette.Success);
        _createButton.ButtonText.fontSize = 16;
        _createButton.ButtonText.fontStyle = FontStyle.Bold;
        _createButton.OnClick += OnCreateButtonClick;
        _createButton.Enabled = true;
        UiFactory.SetLayoutElement(_createButton.GameObject, MinWidth - 20, 30, 0, 0, 0, 0);

        var presetList = UiFactory.CreateScrollView(ContentRoot, "PresetList", out _presetsContainer, out _, minHeight: 200, minWidth: MinWidth, color: UIPalette.Dark, contentAlignment: TextAnchor.UpperCenter);
        UiFactory.SetLayoutElement(presetList, MinWidth, 200, 0, 0, 0, 0);
        foreach (var preset in OptionStorage.Current.Presets)
        {
            CreatePresetItem(preset);
        }
    }

    private void CreatePresetItem(ModOptionPreset preset)
    {
        var presetContainer = UiFactory.CreateHorizontalGroup(_presetsContainer, "PresetItem", false, false, true, true, childAlignment: TextAnchor.MiddleCenter, bgColor: UIPalette.Primary);
        UiFactory.SetLayoutElement(presetContainer, MinWidth - 50, 40, 0, 0, 0, 0);
        var label = UiFactory.CreateLabel(presetContainer, "PresetLabel", preset.Name, TextAnchor.MiddleLeft, UIPalette.Secondary, true, 18);
        UiFactory.SetLayoutElement(label.gameObject, MinWidth - 95, 40, 0, 0, 0, 0);
        var buttonContainer = UiFactory.CreateVerticalGroup(presetContainer, "PresetDeleteButton", false, false, true, true, bgColor: Palette.EnabledColor);
        UiFactory.SetLayoutElement(buttonContainer, 40, 40, 0, 0, 0, 0);
        buttonContainer.GetComponent<Image>().sprite = SpecimenSprites.ErrorIcon;
        var btn = buttonContainer.AddComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            UnityEngine.Object.Destroy(presetContainer);
            OptionStorage.Current.Presets.Remove(preset);
            _presetItems.Remove(presetContainer);
        });
        _presetItems.Add(presetContainer);
    }
    
    internal const string SerializedPrefix = "%%SPECIMEN_SERIALIZED_PRESET%%";

    private void OnCreateInputValueChanged(string value)
    {
        var isEnabled = value.Trim() != string.Empty && !string.Equals(value, OptionStorage.Current.OnlinePreset.Name, StringComparison.InvariantCultureIgnoreCase) &&
                        !OptionStorage.Current.Presets.Any(x => string.Equals(x.Name, value, StringComparison.InvariantCultureIgnoreCase));
        _createButton.Component.SetColorsAuto(isEnabled ? UIPalette.Success : UIPalette.Danger);
    }

    private void OnCopyButtonClick()
    {
        _copyButton.Component.StartCoroutine(CoCopyPreset());
    }
    
    private void OnPasteButtonClick()
    {
        _pasteButton.Component.StartCoroutine(CoPastePreset());
    }

    private void OnCreateButtonClick()
    {
        var value = _createInput.Text;
        if (value.Trim() == string.Empty) return;
        if (value == OptionStorage.Current.OnlinePreset.Name || OptionStorage.Current.Presets.Any(x => x.Name == value))
        {
            DisplayError($"A preset with the name \"{value}\" already exists.");
            return;
        }

        _createInput.Text = string.Empty;
        var preset = new ModOptionPreset
        {
            IsSharable = true,
            Name = value,
            Values = new Dictionary<int, int>()
        };
        OptionStorage.SavePreset(preset);
        CreatePresetItem(preset);
    }

    private static void DisplayError(string message)
    {
        NotificationManager.AddNotification(new BasicNotification(new BasicNotificationConfig
        {
            Type = NotificationTypes.Danger,
            Title = ColorHelpers.Colorize(Color.red, "Preset error"),
            Description = message
        }, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10))));
    }

    private IEnumerator CoCopyPreset()
    {
        var currentPreset = JsonSerializer.Serialize(OptionStorage.Current.GetCurrentPreset());
        GUIUtility.systemCopyBuffer = $"{SerializedPrefix}{currentPreset}";
        _copyButton.Component.SetColorsAuto(UIPalette.Success);
        _copyButton.ButtonText.text = "Preset copied!";
        yield return new WaitForSeconds(1f);
        _copyButton.Component.SetColorsAuto(UIPalette.Primary);
        _copyButton.ButtonText.text = "Copy preset";
    }
    
    private IEnumerator CoPastePreset()
    {
        var isSuccess = false;
        var buffer = GUIUtility.systemCopyBuffer;
        if (buffer != null && buffer.StartsWith(SerializedPrefix))
        {
            var raw = buffer[SerializedPrefix.Length..];
            try
            {
                var tempPreset = JsonSerializer.Deserialize<ModOptionPreset>(raw);
                var current = OptionStorage.Current.GetCurrentPreset();
                current.VanillaOptions = tempPreset.VanillaOptions;
                current.Values = tempPreset.Values;
                OptionStorage.Persist();
                OptionStorage.ApplyCurrentPreset();
                isSuccess = true;
            }
            catch
            {
                // ignored
            }
        }
        _pasteButton.Component.SetColorsAuto(isSuccess ? UIPalette.Success : UIPalette.Danger);
        _pasteButton.ButtonText.text = isSuccess ? "Preset paste!" : "Unknown preset";
        yield return new WaitForSeconds(1f);
        _pasteButton.Component.SetColorsAuto(UIPalette.Primary);
        _pasteButton.ButtonText.text = "Paste preset";
    }
}