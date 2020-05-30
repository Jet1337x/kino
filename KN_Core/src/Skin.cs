using System.IO;
using System.Reflection;
using UnityEngine;

namespace KN_Core {
  public static class Skin {
    public static GUISkin MainContainer;
    public static GUISkin MainContainerLeft;
    private static Texture2D texMain_;

    public static GUISkin MainContainerDark;
    private static Texture2D texMainDark_;

    public static GUISkin OutlineDark;
    private static Texture2D texOutlineDark_;

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

    public static GUISkin Button;
    public static GUISkin ButtonActive;
    public static GUISkin ButtonTab;
    public static GUISkin ButtonActiveTab;
    public static GUISkin ButtonDisabled;
    public static GUISkin ButtonDummy;
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

    public static Color TextColor;
    public static Color TextColorInv;
    public static Color SeparatorColor;

    public static Color ContainerAlpha;
    public static Color ContainerAlphaLow;
    public static Color ElementAlpha;
    public static Color TextAlpha;

    public static Font FontLight;
    public static Font FontTabs;

    private static bool initialized_;

    public static Texture2D SpotMask;

    public static void LoadAll() {
      if (initialized_) {
        return;
      }
      initialized_ = true;

      TextColor = new Color32(0x30, 0x30, 0x30, 0xff);
      TextColorInv = new Color32(0xee, 0xee, 0xee, 0xff);
      SeparatorColor = new Color32(0xee, 0xee, 0xee, 0xff);

      ContainerAlpha = new Color(1.0f, 1.0f, 1.0f, 0.7f);
      ContainerAlphaLow = new Color(1.0f, 1.0f, 1.0f, 0.95f);
      ElementAlpha = new Color(1.0f, 1.0f, 1.0f, 0.95f);
      TextAlpha = new Color(1.0f, 1.0f, 1.0f, 1.0f);

      var assembly = Assembly.GetExecutingAssembly();

      FontTabs = Font.CreateDynamicFontFromOSFont("Consolas Bold", 12);
      FontLight = Font.CreateDynamicFontFromOSFont("Consolas", 12);

      LoadButtonTex(out texCamN_, out texCamH_, out texCamA_, out IconCam, out IconCamActive, "Camera", assembly);
      LoadButtonTex(out texAnimN_, out texAnimH_, out texAnimA_, out IconAnim, out IconAnimActive, "Animation", assembly);
      LoadButtonTex(out texReplayN_, out texReplayH_, out texReplayA_, out IconReplay, out IconReplayActive, "Replay", assembly);

      LoadButtonTex(out texPPN_, out texPPH_, out texPPA_, out IconPlayPause, out IconPlayPauseActive, "PlayPause", assembly);
      LoadButtonTex(out texStopN_, out texStopH_, out texStopA_, out IconStop, out IconStopActive, "Stop", assembly);
      LoadButtonTex(out texSpdLN_, out texSpdLH_, out texSpdLA_, out IconSpeedLeft, out IconSpeedLeftActive, "SpeedLeft", assembly);
      LoadButtonTex(out texSpdRN_, out texSpdRH_, out texSpdRA_, out IconSpeedRight, out IconSpeedRightActive, "SpeedRight", assembly);
      LoadButtonTex(out texKeyframeN_, out texKeyframeH_, out texKeyframeA_, out IconKeyframe, out IconKeyframeActive, "Keyframe", assembly);
      LoadButtonTex(out texGearN_, out texGearH_, out texGearA_, out IconGear, out IconGearActive, "Gear", assembly);
      LoadButtonTex(out texLoopN_, out texLoopH_, out texLoopA_, out IconLoop, out IconLoopActive, "Loop", assembly);
      LoadButtonTex(out texMinusN_, out texMinusH_, out texMinusA_, out IconMinus, out IconMinusActive, "Minus", assembly);
      LoadButtonTex(out texPlusN_, out texPlusH_, out texPlusA_, out IconPlus, out IconPlusActive, "Plus", assembly);

      MakeMainContainerStyle(assembly);
      MakeButtonsStyle(assembly);
      MakeSliderStyle(assembly);
      MakeScrollViewStyle();
      MakeTextFieldStyle();

      SpotMask = LoadTexture(assembly, "HeadLightMask.png");
    }

    private static void LoadButtonTex(out Texture2D normal, out Texture2D hover, out Texture2D active,
      out GUISkin skin, out GUISkin skinActive, string name, Assembly assembly) {
      normal = LoadTexture(assembly, name + "Normal.png");
      hover = LoadTexture(assembly, name + "Hover.png");
      active = LoadTexture(assembly, name + "Active.png");

      skin = ScriptableObject.CreateInstance<GUISkin>();
      skin.button.normal.background = normal;
      skin.button.hover.background = hover;
      skin.button.active.background = active;

      skinActive = ScriptableObject.CreateInstance<GUISkin>();
      skinActive.button.normal.background = active;
      skinActive.button.hover.background = active;
      skinActive.button.active.background = active;
    }

    private static void MakeButtonsStyle(Assembly assembly) {
      var color = new Color32(0x30, 0x30, 0x30, 0xff);
      var colorActive = new Color32(0xee, 0xee, 0xee, 0xff);

      texButtonN_ = LoadTexture(assembly, "ButtonNormal.png");
      texButtonH_ = LoadTexture(assembly, "ButtonHover.png");
      texButtonA_ = LoadTexture(assembly, "ButtonActive.png");
      texButtonAT_ = LoadTexture(assembly, "Main.png");
      texButtonAD_ = LoadTexture(assembly, "ButtonActiveDark.png");
      texButtonD_ = LoadTexture(assembly, "ButtonDisabled.png");
      texRedButtonN_ = LoadTexture(assembly, "RedButtonNormal.png");
      texRedButtonH_ = LoadTexture(assembly, "RedButtonHover.png");
      texRedButtonA_ = LoadTexture(assembly, "RedButtonActive.png");

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

      ButtonTab = ScriptableObject.CreateInstance<GUISkin>();
      ButtonTab.button.normal.textColor = color;
      ButtonTab.button.normal.background = texButtonN_;
      ButtonTab.button.hover.textColor = color;
      ButtonTab.button.hover.background = texButtonH_;
      ButtonTab.button.active.textColor = color;
      ButtonTab.button.active.background = texButtonA_;
      ButtonTab.button.alignment = TextAnchor.MiddleCenter;
      ButtonTab.button.font = FontTabs;

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

    private static void MakeSliderStyle(Assembly assembly) {
      texSliderN_ = LoadTexture(assembly, "SliderThumbNormal.png");
      texSliderH_ = LoadTexture(assembly, "SliderThumbHover.png");
      texSliderBG_ = LoadTexture(assembly, "SliderBG.png");

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

    private static void MakeMainContainerStyle(Assembly assembly) {
      texMain_ = LoadTexture(assembly, "Main.png");
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

      texMainDark_ = LoadTexture(assembly, "MainDark.png");
      MainContainerDark = ScriptableObject.CreateInstance<GUISkin>();
      MainContainerDark.box.normal.background = texMainDark_;
      MainContainerDark.box.normal.textColor = TextColor;
      MainContainerDark.box.alignment = TextAnchor.MiddleCenter;
      MainContainerDark.box.font = FontLight;

      texOutlineDark_ = LoadTexture(assembly, "OutlineDark.png");
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

    private static Texture2D LoadTexture(Assembly assembly, string name) {
      var tex = new Texture2D(4, 4);

      using (var stream = assembly.GetManifestResourceStream("KN_Core.Resources." + name)) {
        using (var memoryStream = new MemoryStream()) {
          if (stream != null) {
            stream.CopyTo(memoryStream);
            tex.LoadImage(memoryStream.ToArray());
          }
          else {
            tex = Texture2D.grayTexture;
          }
        }
      }

      return tex;
    }
  }
}