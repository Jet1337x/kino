using KN_Loader;
using UnityEngine;

namespace KN_Core {
  public class KnSkin {
    public enum Type {
      Button,
      Box
    }

    public class SkinState {
      public Color32 TextColor { get; }
      public Texture2D Texture { get; private set; }

      private readonly Color32 elementColor_;

      public SkinState(Color32 elementColor) {
        TextColor = new Color32(0xff, 0xff, 0xff, 0xff);
        elementColor_ = elementColor;
      }

      public SkinState(Color32 elementColor, Color32 textColor) {
        TextColor = textColor;
        elementColor_ = elementColor;
      }

      public void Load(string texturePath) {
        Texture = Embedded.LoadEmbeddedTexture(texturePath, elementColor_);
      }
    }

    public GUISkin Normal { get; }
    public GUISkin Active { get; }

    private readonly SkinState normal_;
    private readonly SkinState hover_;
    private readonly SkinState active_;

    private readonly string texturePath_;

    private readonly Type type_;
    private readonly TextAnchor alignment_;
    private readonly Font font_;

    public KnSkin(Type type, SkinState normal, SkinState hover, SkinState active, string texturePath, TextAnchor alignment, Font font) {
      Normal = ScriptableObject.CreateInstance<GUISkin>();
      Active = ScriptableObject.CreateInstance<GUISkin>();

      type_ = type;

      normal_ = normal;
      hover_ = hover;
      active_ = active;

      texturePath_ = texturePath;
      alignment_ = alignment;
      font_ = font;

      switch (type_) {
        case Type.Button: {
          MakeButton();
          break;
        }
        case Type.Box: {
          MakeBox();
          break;
        }
      }
    }

    private void MakeButton() {
      normal_.Load(texturePath_);
      hover_.Load(texturePath_);
      active_.Load(texturePath_);

      Normal.button.normal.textColor = normal_.TextColor;
      Normal.button.normal.background = normal_.Texture;
      Normal.button.hover.textColor = hover_.TextColor;
      Normal.button.hover.background = hover_.Texture;
      Normal.button.active.textColor = active_.TextColor;
      Normal.button.active.background = active_.Texture;
      Normal.button.alignment = alignment_;
      Normal.button.font = font_;

      Active.button.normal.textColor = active_.TextColor;
      Active.button.normal.background = active_.Texture;
      Active.button.hover.textColor = active_.TextColor;
      Active.button.hover.background = active_.Texture;
      Active.button.active.textColor = active_.TextColor;
      Active.button.active.background = active_.Texture;
      Active.button.alignment = alignment_;
      Active.button.font = font_;
    }

    private void MakeBox() {
      normal_.Load(texturePath_);
      hover_.Load(texturePath_);
      active_.Load(texturePath_);

      Normal.box.normal.textColor = normal_.TextColor;
      Normal.box.normal.background = normal_.Texture;
      Normal.box.hover.textColor = hover_.TextColor;
      Normal.box.hover.background = hover_.Texture;
      Normal.box.active.textColor = active_.TextColor;
      Normal.box.active.background = active_.Texture;
      Normal.box.alignment = alignment_;
      Normal.box.font = font_;
      Normal.box.padding = new RectOffset(5, 5, 0, 0);

      Active.box.normal.textColor = active_.TextColor;
      Active.box.normal.background = active_.Texture;
      Active.box.hover.textColor = active_.TextColor;
      Active.box.hover.background = active_.Texture;
      Active.box.active.textColor = active_.TextColor;
      Active.box.active.background = active_.Texture;
      Active.box.alignment = alignment_;
      Active.box.font = font_;
      Normal.box.padding = new RectOffset(5, 5, 0, 0);
    }
  }

  public static class Skin {
    public static GUISkin MainContainer;
    public static GUISkin MainContainerLeft;
    public static GUISkin MainContainerRed;
    private static Texture2D texMain_;

    public static GUISkin MainContainerDark;
    private static Texture2D texMainDark_;

    public static GUISkin OutlineDark;
    private static Texture2D texOutlineDark_;

    private static Texture2D texTachBg_;
    private static Texture2D texTachOutline_;
    public static GUISkin TachBg;
    public static GUISkin TachRedBg;
    public static GUISkin TachGearBg;
    public static GUISkin TachFill;
    public static GUISkin TachFillRed;
    public static GUISkin TachOutline;

    public static GUISkin IconCam;
    public static GUISkin IconCamActive;
    private static Texture2D texCamN_;
    private static Texture2D texCamH_;
    private static Texture2D texCamA_;

    public static GUISkin IconAnim;
    public static GUISkin IconAnimActive;
    private static Texture2D texAnimN_;
    private static Texture2D texAnimH_;
    private static Texture2D texAnimA_;

    public static GUISkin IconReplay;
    public static GUISkin IconReplayActive;
    private static Texture2D texReplayN_;
    private static Texture2D texReplayH_;
    private static Texture2D texReplayA_;

    public static GUISkin IconPlayPause;
    public static GUISkin IconPlayPauseActive;
    private static Texture2D texPPN_;
    private static Texture2D texPPH_;
    private static Texture2D texPPA_;

    public static GUISkin IconStop;
    public static GUISkin IconStopActive;
    private static Texture2D texStopN_;
    private static Texture2D texStopH_;
    private static Texture2D texStopA_;

    public static GUISkin IconSpeedLeft;
    public static GUISkin IconSpeedLeftActive;
    private static Texture2D texSpdLN_;
    private static Texture2D texSpdLH_;
    private static Texture2D texSpdLA_;

    public static GUISkin IconSpeedRight;
    public static GUISkin IconSpeedRightActive;
    private static Texture2D texSpdRN_;
    private static Texture2D texSpdRH_;
    private static Texture2D texSpdRA_;

    public static GUISkin IconKeyframe;
    public static GUISkin IconKeyframeActive;
    private static Texture2D texKeyframeN_;
    private static Texture2D texKeyframeH_;
    private static Texture2D texKeyframeA_;

    public static GUISkin IconGear;
    public static GUISkin IconGearActive;
    private static Texture2D texGearN_;
    private static Texture2D texGearH_;
    private static Texture2D texGearA_;

    public static GUISkin IconLoop;
    public static GUISkin IconLoopActive;
    private static Texture2D texLoopN_;
    private static Texture2D texLoopH_;
    private static Texture2D texLoopA_;

    public static GUISkin IconMinus;
    public static GUISkin IconMinusActive;
    private static Texture2D texMinusN_;
    private static Texture2D texMinusH_;
    private static Texture2D texMinusA_;

    public static GUISkin IconPlus;
    public static GUISkin IconPlusActive;
    private static Texture2D texPlusN_;
    private static Texture2D texPlusH_;
    private static Texture2D texPlusA_;

    public static GUISkin IconSun;
    public static GUISkin IconSunActive;
    private static Texture2D texSunN_;
    private static Texture2D texSunH_;
    private static Texture2D texSunA_;

    public static GUISkin IconHeadlights;
    public static GUISkin IconHeadlightsActive;
    private static Texture2D texHeadlightsN_;
    private static Texture2D texHeadlightsH_;
    private static Texture2D texHeadlightsA_;

    public static GUISkin IconProjector;
    public static GUISkin IconProjectorActive;
    private static Texture2D texProjectorN_;
    private static Texture2D texProjectorH_;
    private static Texture2D texProjectorA_;

    public static GUISkin Button;
    public static GUISkin ButtonActive;
    public static GUISkin ButtonTab;
    public static GUISkin ButtonActiveTab;
    public static GUISkin ButtonDisabled;
    public static GUISkin ButtonDummy;
    public static GUISkin ButtonDummyRed;
    private static Texture2D texButtonN_;
    private static Texture2D texButtonH_;
    private static Texture2D texButtonA_;
    private static Texture2D texButtonAT_;
    private static Texture2D texButtonAD_;
    private static Texture2D texButtonD_;

    public static GUISkin RedSkin;
    private static Texture2D texRedButtonN_;
    private static Texture2D texRedButtonH_;
    private static Texture2D texRedButtonA_;

    public static GUISkin Slider;
    private static Texture2D texSliderN_;
    private static Texture2D texSliderH_;
    private static Texture2D texSliderBG_;

    public static GUISkin ScrollView;
    public static GUISkin TextField;
    public static GUISkin Label;

    public static GUISkin TimelineSliderLow;
    public static GUISkin TimelineSliderMid;
    public static GUISkin TimelineSliderHigh;
    private static Texture2D texTimelineLow_;
    private static Texture2D texTimelineMid_;
    private static Texture2D texTimelineHigh_;

    public static Color TextColor;
    public static Color TextColorRed;
    public static Color TextColorInv;
    public static Color SeparatorColor;

    public static Color ContainerAlpha;
    public static Color ContainerAlphaLow;
    public static Color ElementAlpha;
    public static Color TextAlpha;

    public static Font FontVersion;

    public static Font FontLight;
    public static Font FontTabs;
    public static Font FontTach;
    public static Font FontGear;

    public static KnSkin GearSkin;
    public static KnSkin PlusSkin;
    public static KnSkin ModPanelSkin;
    public static KnSkin ModPanelBackSkin;

    private static bool initialized_;

    public static void LoadAll() {
      if (initialized_) {
        return;
      }
      initialized_ = true;

      Log.Write("[KN_Core::Skin]: Loading skin ...");

      TextColor = new Color32(0x30, 0x30, 0x30, 0xff);
      TextColorRed = new Color32(0xff, 0x30, 0x30, 0xff);
      TextColorInv = new Color32(0xee, 0xee, 0xee, 0xff);
      SeparatorColor = new Color32(0xee, 0xee, 0xee, 0xff);

      ContainerAlpha = new Color(1.0f, 1.0f, 1.0f, 1.0f);
      ContainerAlphaLow = new Color(1.0f, 1.0f, 1.0f, 1.0f);
      ElementAlpha = new Color(1.0f, 1.0f, 1.0f, 1.0f);
      TextAlpha = new Color(1.0f, 1.0f, 1.0f, 1.0f);

      FontVersion = Font.CreateDynamicFontFromOSFont("Consolas", 9);

      FontTabs = Font.CreateDynamicFontFromOSFont("Consolas Bold", 12);
      FontLight = Font.CreateDynamicFontFromOSFont("Consolas", 12);
      FontTach = Font.CreateDynamicFontFromOSFont("Consolas Bold", 16);
      FontGear = Font.CreateDynamicFontFromOSFont("Consolas Bold", 32);

      GearSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0x5d, 0x0f, 0xc6, 0xff)),
        new KnSkin.SkinState(new Color32(0x64, 0xff, 0xd6, 0xff)),
        new KnSkin.SkinState(new Color32(0x31, 0xff, 0xff, 0xff)),
        "GUI.gear.png", TextAnchor.MiddleCenter, FontLight);

      PlusSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0xff, 0xff, 0xc6, 0xff)),
        new KnSkin.SkinState(new Color32(0x64, 0xff, 0xee, 0xff)),
        new KnSkin.SkinState(new Color32(0xff, 0xff, 0xff, 0xff)),
        "GUI.plus.png", TextAnchor.MiddleCenter, FontLight);

      ModPanelSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff),new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff),new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff),new Color32(0x90, 0x90, 0x90, 0xff)),
        "GUI.base.png", TextAnchor.MiddleLeft, FontVersion);

      ModPanelBackSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0x00, 0x00, 0x00, 0x00)),
        new KnSkin.SkinState(new Color32(0x50, 0x50, 0x50, 0xff)),
        new KnSkin.SkinState(new Color32(0x65, 0x65, 0x65, 0xff)),
        "GUI.base.png", TextAnchor.MiddleCenter, FontLight);

      LoadButtonTex(out texCamN_, out texCamH_, out texCamA_, out IconCam, out IconCamActive, "Camera");
      LoadButtonTex(out texAnimN_, out texAnimH_, out texAnimA_, out IconAnim, out IconAnimActive, "Animation");
      LoadButtonTex(out texReplayN_, out texReplayH_, out texReplayA_, out IconReplay, out IconReplayActive, "Replay");

      LoadButtonTex(out texPPN_, out texPPH_, out texPPA_, out IconPlayPause, out IconPlayPauseActive, "PlayPause");
      LoadButtonTex(out texStopN_, out texStopH_, out texStopA_, out IconStop, out IconStopActive, "Stop");
      LoadButtonTex(out texSpdLN_, out texSpdLH_, out texSpdLA_, out IconSpeedLeft, out IconSpeedLeftActive, "SpeedLeft");
      LoadButtonTex(out texSpdRN_, out texSpdRH_, out texSpdRA_, out IconSpeedRight, out IconSpeedRightActive, "SpeedRight");
      LoadButtonTex(out texKeyframeN_, out texKeyframeH_, out texKeyframeA_, out IconKeyframe, out IconKeyframeActive, "Keyframe");
      LoadButtonTex(out texGearN_, out texGearH_, out texGearA_, out IconGear, out IconGearActive, "Gear");
      LoadButtonTex(out texLoopN_, out texLoopH_, out texLoopA_, out IconLoop, out IconLoopActive, "Loop");
      LoadButtonTex(out texMinusN_, out texMinusH_, out texMinusA_, out IconMinus, out IconMinusActive, "Minus");
      LoadButtonTex(out texPlusN_, out texPlusH_, out texPlusA_, out IconPlus, out IconPlusActive, "Plus");

      LoadButtonTex(out texSunN_, out texSunH_, out texSunA_, out IconSun, out IconSunActive, "Sunlight");
      LoadButtonTex(out texHeadlightsN_, out texHeadlightsH_, out texHeadlightsA_, out IconHeadlights, out IconHeadlightsActive, "Headlights");
      LoadButtonTex(out texProjectorN_, out texProjectorH_, out texProjectorA_, out IconProjector, out IconProjectorActive, "Projector");

      MakeMainContainerStyle();
      MakeButtonsStyle();
      MakeSliderStyle();
      MakeScrollViewStyle();
      MakeTextFieldStyle();
      MakeTimelineSlider();
      MakeTachStyle();

      Log.Write("[KN_Core::Skin]: Skin loaded");
    }

    private static void LoadButtonTex(out Texture2D normal, out Texture2D hover, out Texture2D active, out GUISkin skin, out GUISkin skinActive, string name) {
      normal = Embedded.LoadEmbeddedTexture(name + "Normal.png");
      hover = Embedded.LoadEmbeddedTexture(name + "Hover.png");
      active = Embedded.LoadEmbeddedTexture(name + "Active.png");

      skin = ScriptableObject.CreateInstance<GUISkin>();
      skin.button.normal.background = normal;
      skin.button.hover.background = hover;
      skin.button.active.background = active;

      skinActive = ScriptableObject.CreateInstance<GUISkin>();
      skinActive.button.normal.background = active;
      skinActive.button.hover.background = active;
      skinActive.button.active.background = active;
    }

    private static void MakeButtonsStyle() {
      var color = new Color32(0x30, 0x30, 0x30, 0xff);
      var colorActive = new Color32(0xee, 0xee, 0xee, 0xff);

      texButtonN_ = Embedded.LoadEmbeddedTexture("ButtonNormal.png");
      texButtonH_ = Embedded.LoadEmbeddedTexture("ButtonHover.png");
      texButtonA_ = Embedded.LoadEmbeddedTexture("ButtonActive.png");
      texButtonAT_ = Embedded.LoadEmbeddedTexture("Main.png");
      texButtonAD_ = Embedded.LoadEmbeddedTexture("ButtonActiveDark.png");
      texButtonD_ = Embedded.LoadEmbeddedTexture("ButtonDisabled.png");
      texRedButtonN_ = Embedded.LoadEmbeddedTexture("RedButtonNormal.png");
      texRedButtonH_ = Embedded.LoadEmbeddedTexture("RedButtonHover.png");
      texRedButtonA_ = Embedded.LoadEmbeddedTexture("RedButtonActive.png");

      Button = ScriptableObject.CreateInstance<GUISkin>();
      Button.button.normal.textColor = color;
      Button.button.normal.background = texButtonN_;
      Button.button.hover.textColor = color;
      Button.button.hover.background = texButtonH_;
      Button.button.active.textColor = color;
      Button.button.active.background = texButtonA_;
      Button.button.alignment = TextAnchor.MiddleCenter;
      Button.button.font = FontLight;

      ButtonDummy = ScriptableObject.CreateInstance<GUISkin>();
      ButtonDummy.button.normal.textColor = color;
      ButtonDummy.button.normal.background = texButtonN_;
      ButtonDummy.button.hover.textColor = color;
      ButtonDummy.button.hover.background = texButtonN_;
      ButtonDummy.button.active.textColor = color;
      ButtonDummy.button.active.background = texButtonN_;
      ButtonDummy.button.alignment = TextAnchor.MiddleCenter;
      ButtonDummy.button.font = FontLight;

      ButtonDummyRed = ScriptableObject.CreateInstance<GUISkin>();
      ButtonDummyRed.button.normal.textColor = TextColorInv;
      ButtonDummyRed.button.normal.background = texRedButtonN_;
      ButtonDummyRed.button.hover.textColor = TextColorInv;
      ButtonDummyRed.button.hover.background = texRedButtonN_;
      ButtonDummyRed.button.active.textColor = TextColorInv;
      ButtonDummyRed.button.active.background = texRedButtonN_;
      ButtonDummyRed.button.alignment = TextAnchor.MiddleCenter;
      ButtonDummyRed.button.font = FontTabs;

      ButtonTab = ScriptableObject.CreateInstance<GUISkin>();
      ButtonTab.button.normal.textColor = color;
      ButtonTab.button.normal.background = texButtonN_;
      ButtonTab.button.hover.textColor = color;
      ButtonTab.button.hover.background = texButtonH_;
      ButtonTab.button.active.textColor = color;
      ButtonTab.button.active.background = texButtonA_;
      ButtonTab.button.alignment = TextAnchor.MiddleCenter;
      ButtonTab.button.font = FontTabs;
      ButtonTab.button.padding.top = -2;

      ButtonActive = ScriptableObject.CreateInstance<GUISkin>();
      ButtonActive.button.normal.background = texButtonAD_;
      ButtonActive.button.normal.textColor = colorActive;
      ButtonActive.button.hover.background = texButtonAD_;
      ButtonActive.button.hover.textColor = colorActive;
      ButtonActive.button.active.background = texButtonAD_;
      ButtonActive.button.active.textColor = colorActive;
      ButtonActive.button.alignment = TextAnchor.MiddleCenter;
      ButtonActive.button.font = FontLight;

      ButtonActiveTab = ScriptableObject.CreateInstance<GUISkin>();
      ButtonActiveTab.button.normal.background = texButtonAT_;
      ButtonActiveTab.button.normal.textColor = TextColor;
      ButtonActiveTab.button.hover.background = texButtonAT_;
      ButtonActiveTab.button.hover.textColor = TextColor;
      ButtonActiveTab.button.active.background = texButtonAT_;
      ButtonActiveTab.button.active.textColor = TextColor;
      ButtonActiveTab.button.alignment = TextAnchor.MiddleCenter;
      ButtonActiveTab.button.font = FontTabs;

      ButtonDisabled = ScriptableObject.CreateInstance<GUISkin>();
      ButtonDisabled.button.normal.background = texButtonD_;
      ButtonDisabled.button.normal.textColor = TextColorInv;
      ButtonDisabled.button.hover.background = texButtonD_;
      ButtonDisabled.button.hover.textColor = TextColorInv;
      ButtonDisabled.button.active.background = texButtonD_;
      ButtonDisabled.button.active.textColor = TextColorInv;
      ButtonDisabled.button.alignment = TextAnchor.MiddleCenter;
      ButtonDisabled.button.font = FontLight;

      RedSkin = ScriptableObject.CreateInstance<GUISkin>();
      RedSkin.button.normal.background = texRedButtonN_;
      RedSkin.button.normal.textColor = colorActive;
      RedSkin.button.hover.background = texRedButtonH_;
      RedSkin.button.hover.textColor = colorActive;
      RedSkin.button.active.background = texRedButtonA_;
      RedSkin.button.active.textColor = colorActive;
      RedSkin.button.alignment = TextAnchor.MiddleCenter;
      RedSkin.button.font = FontLight;
      RedSkin.button.padding.top = -1;
      RedSkin.button.padding.right = -2;

      RedSkin.horizontalSlider.normal.background = texRedButtonN_;
      RedSkin.horizontalSlider.hover.background = texRedButtonN_;
      RedSkin.horizontalSlider.active.background = texRedButtonN_;
      RedSkin.horizontalSlider.fixedHeight = Gui.Height;
      RedSkin.horizontalSliderThumb.normal.background = texMain_;
      RedSkin.horizontalSliderThumb.hover.background = texMain_;
      RedSkin.horizontalSliderThumb.active.background = texMain_;
      RedSkin.horizontalSliderThumb.fixedWidth = Gui.WidthSlider;
      RedSkin.horizontalSliderThumb.fixedHeight = Gui.Height;

      RedSkin.label.normal.textColor = TextColor;
      RedSkin.label.alignment = TextAnchor.MiddleCenter;
      RedSkin.label.font = FontLight;
    }

    private static void MakeSliderStyle() {
      texSliderN_ = Embedded.LoadEmbeddedTexture("SliderThumbNormal.png");
      texSliderH_ = Embedded.LoadEmbeddedTexture("SliderThumbHover.png");
      texSliderBG_ = Embedded.LoadEmbeddedTexture("SliderBG.png");

      Slider = ScriptableObject.CreateInstance<GUISkin>();
      Slider.horizontalSlider.normal.background = texSliderBG_;
      Slider.horizontalSlider.hover.background = texSliderBG_;
      Slider.horizontalSlider.active.background = texSliderBG_;
      Slider.horizontalSlider.fixedHeight = Gui.Height;
      Slider.horizontalSliderThumb.normal.background = texSliderN_;
      Slider.horizontalSliderThumb.hover.background = texSliderH_;
      Slider.horizontalSliderThumb.active.background = texSliderH_;
      Slider.horizontalSliderThumb.fixedWidth = Gui.WidthSlider;
      Slider.horizontalSliderThumb.fixedHeight = Gui.Height;

      Slider.label.normal.textColor = TextColor;
      Slider.label.alignment = TextAnchor.MiddleCenter;
      Slider.label.font = FontLight;
    }

    private static void MakeScrollViewStyle() {
      ScrollView = ScriptableObject.CreateInstance<GUISkin>();
      ScrollView.scrollView.normal.background = texSliderBG_;

      ScrollView.verticalScrollbar.normal.background = texMainDark_;
      ScrollView.verticalScrollbar.stretchHeight = false;
      ScrollView.verticalScrollbar.stretchWidth = false;
      ScrollView.verticalScrollbar.fixedWidth = Gui.ScrollBarWidth;

      ScrollView.verticalScrollbarThumb.normal.background = texSliderH_;
      ScrollView.verticalScrollbarThumb.stretchHeight = false;
      ScrollView.verticalScrollbarThumb.stretchWidth = false;
      ScrollView.verticalScrollbarThumb.stretchWidth = false;

      ScrollView.box.normal.textColor = TextColor;
      ScrollView.box.alignment = TextAnchor.MiddleCenter;
      ScrollView.box.normal.background = texMainDark_;
      ScrollView.box.font = FontLight;
    }

    private static void MakeMainContainerStyle() {
      texMain_ = Embedded.LoadEmbeddedTexture("Main.png");
      MainContainer = ScriptableObject.CreateInstance<GUISkin>();
      MainContainer.box.normal.background = texMain_;
      MainContainer.box.normal.textColor = TextColor;
      MainContainer.box.alignment = TextAnchor.MiddleCenter;
      MainContainer.box.font = FontLight;

      MainContainerLeft = ScriptableObject.CreateInstance<GUISkin>();
      MainContainerLeft.box.normal.background = texMain_;
      MainContainerLeft.box.normal.textColor = TextColor;
      MainContainerLeft.box.alignment = TextAnchor.MiddleLeft;
      MainContainerLeft.box.font = FontLight;
      MainContainerLeft.box.padding = new RectOffset(5, 5, 0, 0);

      MainContainerRed = ScriptableObject.CreateInstance<GUISkin>();
      MainContainerRed.box.normal.background = texMain_;
      MainContainerRed.box.normal.textColor = TextColorRed;
      MainContainerRed.box.alignment = TextAnchor.MiddleLeft;
      MainContainerRed.box.font = FontLight;
      MainContainerRed.box.padding = new RectOffset(5, 5, 0, 0);

      texMainDark_ = Embedded.LoadEmbeddedTexture("MainDark.png");
      MainContainerDark = ScriptableObject.CreateInstance<GUISkin>();
      MainContainerDark.box.normal.background = texMainDark_;
      MainContainerDark.box.normal.textColor = TextColor;
      MainContainerDark.box.alignment = TextAnchor.MiddleCenter;
      MainContainerDark.box.font = FontLight;

      texOutlineDark_ = Embedded.LoadEmbeddedTexture("OutlineDark.png");
      texOutlineDark_.filterMode = FilterMode.Point;
      OutlineDark = ScriptableObject.CreateInstance<GUISkin>();
      OutlineDark.box.normal.background = texOutlineDark_;
      OutlineDark.box.normal.textColor = TextColor;
      OutlineDark.box.alignment = TextAnchor.UpperCenter;
      OutlineDark.box.font = FontLight;
      OutlineDark.box.border = new RectOffset(1, 1, 1, 1);
    }

    private static void MakeTextFieldStyle() {
      TextField = ScriptableObject.CreateInstance<GUISkin>();
      TextField.textField.normal.background = texSliderBG_;
      TextField.textField.normal.textColor = TextColor;
      TextField.textField.alignment = TextAnchor.MiddleCenter;
      TextField.textField.fixedHeight = Gui.Height;
      TextField.textField.fontSize = 13;
      TextField.textField.font = FontLight;
      TextField.textField.stretchHeight = false;
      TextField.textField.stretchWidth = false;
      TextField.textField.padding.bottom = 1;

      TextField.label.normal.textColor = TextColor;
      TextField.label.alignment = TextAnchor.MiddleCenter;
      TextField.label.font = FontLight;

      Label = ScriptableObject.CreateInstance<GUISkin>();
      Label.label.normal.textColor = TextColor;
      Label.label.alignment = TextAnchor.MiddleCenter;
      Label.label.font = FontLight;
    }

    private static void MakeTimelineSlider() {
      texTimelineLow_ = Embedded.LoadEmbeddedTexture("TimelineSliderLow.png");
      texTimelineMid_ = Embedded.LoadEmbeddedTexture("TimelineSliderMid.png");
      texTimelineHigh_ = Embedded.LoadEmbeddedTexture("TimelineSliderHigh.png");

      TimelineSliderMid = ScriptableObject.CreateInstance<GUISkin>();
      TimelineSliderMid.horizontalSlider.normal.background = texSliderBG_;
      TimelineSliderMid.horizontalSlider.hover.background = texSliderBG_;
      TimelineSliderMid.horizontalSlider.active.background = texSliderBG_;
      TimelineSliderMid.horizontalSlider.fixedHeight = Gui.Height;
      TimelineSliderMid.horizontalSliderThumb.normal.background = texTimelineMid_;
      TimelineSliderMid.horizontalSliderThumb.hover.background = texTimelineMid_;
      TimelineSliderMid.horizontalSliderThumb.active.background = texTimelineMid_;
      TimelineSliderMid.horizontalSliderThumb.fixedWidth = Gui.WidthSlider;
      TimelineSliderMid.horizontalSliderThumb.fixedHeight = Gui.Height;

      TimelineSliderMid.label.normal.textColor = TextColor;
      TimelineSliderMid.label.alignment = TextAnchor.MiddleCenter;
      TimelineSliderMid.label.font = FontLight;

      var bgTex = KnUtils.CreateTexture(Color.clear);

      //low bound
      TimelineSliderLow = ScriptableObject.CreateInstance<GUISkin>();
      TimelineSliderLow.horizontalSlider.normal.background = bgTex;
      TimelineSliderLow.horizontalSlider.hover.background = bgTex;
      TimelineSliderLow.horizontalSlider.active.background = bgTex;
      TimelineSliderLow.horizontalSlider.fixedHeight = Gui.HeightTimeline;
      TimelineSliderLow.horizontalSliderThumb.normal.background = texTimelineLow_;
      TimelineSliderLow.horizontalSliderThumb.hover.background = texTimelineLow_;
      TimelineSliderLow.horizontalSliderThumb.active.background = texTimelineLow_;
      TimelineSliderLow.horizontalSliderThumb.fixedWidth = Gui.WidthSlider;
      TimelineSliderLow.horizontalSliderThumb.fixedHeight = Gui.Height * 1.5f;

      //high bound
      TimelineSliderHigh = ScriptableObject.CreateInstance<GUISkin>();
      TimelineSliderHigh.horizontalSlider.normal.background = bgTex;
      TimelineSliderHigh.horizontalSlider.hover.background = bgTex;
      TimelineSliderHigh.horizontalSlider.active.background = bgTex;
      TimelineSliderHigh.horizontalSlider.fixedHeight = Gui.HeightTimeline;
      TimelineSliderHigh.horizontalSliderThumb.normal.background = texTimelineHigh_;
      TimelineSliderHigh.horizontalSliderThumb.hover.background = texTimelineHigh_;
      TimelineSliderHigh.horizontalSliderThumb.active.background = texTimelineHigh_;
      TimelineSliderHigh.horizontalSliderThumb.fixedWidth = Gui.WidthSlider;
      TimelineSliderHigh.horizontalSliderThumb.fixedHeight = Gui.Height * 1.5f;
    }

    private static void MakeTachStyle() {
      texTachBg_ = Embedded.LoadEmbeddedTexture("TachBg.png");

      TachBg = ScriptableObject.CreateInstance<GUISkin>();
      TachBg.box.normal.background = texTachBg_;
      TachBg.box.normal.textColor = TextColorInv;
      TachBg.box.alignment = TextAnchor.MiddleRight;
      TachBg.box.font = FontTach;
      TachBg.box.padding = new RectOffset(5, 5, 0, 5);

      TachGearBg = ScriptableObject.CreateInstance<GUISkin>();
      TachGearBg.box.normal.background = texTachBg_;
      TachGearBg.box.normal.textColor = TextColorInv;
      TachGearBg.box.alignment = TextAnchor.MiddleCenter;
      TachGearBg.box.font = FontGear;
      TachGearBg.box.padding = new RectOffset(1, 0, 0, 8);

      TachRedBg = ScriptableObject.CreateInstance<GUISkin>();
      TachRedBg.box.normal.background = texRedButtonA_;

      TachFill = ScriptableObject.CreateInstance<GUISkin>();
      TachFill.box.normal.background = texSliderBG_;

      TachFillRed = ScriptableObject.CreateInstance<GUISkin>();
      TachFillRed.box.normal.background = texRedButtonH_;

      texTachOutline_ = Embedded.LoadEmbeddedTexture("TachoOutline.png");
      texTachOutline_.filterMode = FilterMode.Point;
      TachOutline = ScriptableObject.CreateInstance<GUISkin>();
      TachOutline.box.normal.background = texTachOutline_;
      TachOutline.box.normal.textColor = TextColor;
      TachOutline.box.alignment = TextAnchor.UpperCenter;
      TachOutline.box.font = FontLight;
      TachOutline.box.border = new RectOffset(2, 2, 2, 2);
    }
  }
}