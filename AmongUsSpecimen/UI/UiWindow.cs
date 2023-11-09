using AmongUsSpecimen.UI.Components;
using AmongUsSpecimen.UI.Patches;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Panels;

namespace AmongUsSpecimen.UI;

public abstract class UiWindow : PanelBase
{
    protected UiWindow() : base(UiManager.UIBase)
    {
        UiManager.Windows.Add(this);
    }

    protected virtual bool DisplayByDefault => false;
    protected virtual bool DisplayTitleBar => true;
    public virtual bool HasOverlay => false;
    protected virtual Color BackgroundColor => UIPalette.Primary;
    protected virtual string Title => Name;
    protected virtual Positions Position => Positions.MiddleCenter;
    protected virtual Vector4 Paddings => Vector4.zero;
    protected virtual int Spacing => 0;
    public virtual bool DisableClickThroughWindow => true;
    protected WindowHeader Header { get; set; }
    
    public float InnerWidth => MinWidth - Paddings.y - Paddings.w;
    public float InnerHeight => MinHeight - Paddings.x - Paddings.z;
    
    public override Vector2 DefaultAnchorMin => Vector2.zero;
    public override Vector2 DefaultAnchorMax => Vector2.zero;
    public override bool CanDragAndResize => false;
    
    public override void ConstructUI()
    {
        base.ConstructUI();
        UIRoot.GetComponent<Image>().color = UIPalette.Transparent;
        SetBackgroundColor(BackgroundColor);
        TitleBar.SetActive(false);
        var layout = ContentRoot.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 0f;
        layout.padding.top = 0;
        layout.padding.right = 0;
        layout.padding.bottom = 0;
        layout.padding.left = 0;
    }

    protected abstract void ConstructWindowContent();

    protected virtual void SetBackgroundColor(Color color)
    {
        ContentRoot.GetComponent<Image>().color = color;
    }
    
    protected override void ConstructPanelContent()
    {
        Header = new WindowHeader(this, Title);
        TitleBar.SetActive(false);
        Header.SetActive(DisplayTitleBar);
        ConstructWindowContent();
        SetActive(DisplayByDefault);
    }

    public virtual void TriggerClose()
    {
        SetActive(false);
    }
    
    public override void EnsureValidPosition()
    {
        var screenHeight = Screen.height;
        var screenWidth = Screen.width;

        switch (Position)
        {
            case Positions.MiddleCenter:
                Rect.position = new Vector3(screenWidth / 2f - MinWidth / 2f, screenHeight / 2f + MinHeight / 2f,
                    Rect.position.z);
                break;
            case Positions.TopLeft:
                Rect.position = new Vector3(0f, screenHeight, Rect.position.z);
                break;
            case Positions.TopCenter:
                Rect.position = new Vector3(screenWidth / 2f - MinWidth / 2f, screenHeight, Rect.position.z);
                break;
            case Positions.MiddleLeft:
                Rect.position = new Vector3(0f, screenHeight / 2f + MinHeight / 2f);
                break;
            case Positions.TopRight:
                Rect.position = new Vector3(screenWidth - MinWidth, screenHeight, Rect.position.z);
                break;
            case Positions.MiddleRight:
                Rect.position = new Vector3(screenWidth - MinWidth, screenHeight / 2f + MinHeight / 2f, Rect.position.z);
                break;
            case Positions.BottomLeft:
                Rect.position = new Vector3(0f, MinHeight, Rect.position.z);
                break;
            case Positions.BottomCenter:
                Rect.position = new Vector3(screenWidth / 2f - MinWidth / 2f, MinHeight, Rect.position.z);
                break;
            case Positions.BottomRight:
                Rect.position = new Vector3(screenWidth - MinWidth, MinHeight, Rect.position.z);
                break;
        }
        
        Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MinWidth);
        Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, MinHeight);
    }
    
    public override void SetActive(bool active)
    {
        SetActive(active, true);
    }
    
    public void SetActive(bool active, bool updateOverlay)
    {
        base.SetActive(active);
        if (HasOverlay && updateOverlay)
        {
            UiManager.UpdateOverlayState();
        }
        PassiveButtonManagerPatches.UpdateState();
    }
    
    protected enum Positions
    {
        TopLeft,
        TopRight,
        TopCenter,
        MiddleCenter,
        MiddleRight,
        MiddleLeft,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
}