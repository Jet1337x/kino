using KN_Loader;
using UnityEngine;

namespace KN_Core {
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

    public static KnSkin PlusSkin;

    public static KnSkin ModPanelSkin;
    public static KnSkin ModPanelBackSkin;
    public static KnSkin DummyIconSkin;
    public static KnSkin GearSkin;
    public static KnSkin DiscordSkin;

    public static KnSkin ButtonSkin;
    public static KnSkin BoxLeftSkin;

    public static KnSkin ModTabSkin;
    public static KnSkin WarningSkin;

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

      PlusSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0xff, 0xff, 0xc6, 0xff)),
        new KnSkin.SkinState(new Color32(0x64, 0xff, 0xee, 0xff)),
        new KnSkin.SkinState(new Color32(0xff, 0xff, 0xff, 0xff)),
        "GUI.plus.png", TextAnchor.MiddleCenter, FontLight);

      ModPanelSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        "GUI.base.png", TextAnchor.MiddleLeft, FontVersion);

      ModPanelBackSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0x00, 0x00, 0x00, 0x00)),
        new KnSkin.SkinState(new Color32(0x50, 0x50, 0x50, 0xff)),
        new KnSkin.SkinState(new Color32(0x65, 0x65, 0x65, 0xff)),
        "GUI.base.png", TextAnchor.MiddleCenter, FontLight);

      DummyIconSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0xff, 0xff, 0xc6, 0xff)),
        new KnSkin.SkinState(new Color32(0x64, 0xff, 0xee, 0xff)),
        new KnSkin.SkinState(new Color32(0xff, 0xff, 0xff, 0xff)),
        "GUI.base.png", TextAnchor.MiddleCenter, FontLight);

      GearSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0x5d, 0x0f, 0xc6, 0xff)),
        new KnSkin.SkinState(new Color32(0x64, 0xff, 0xd6, 0xff)),
        new KnSkin.SkinState(new Color32(0x31, 0xff, 0xff, 0xff)),
        "GUI.gear.png", TextAnchor.MiddleCenter, FontLight);

      DiscordSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0xff, 0xff, 0xc6, 0xff)),
        new KnSkin.SkinState(new Color32(0x64, 0xff, 0xee, 0xff)),
        new KnSkin.SkinState(new Color32(0xff, 0xff, 0xff, 0xff)),
        "GUI.discord.png", TextAnchor.MiddleCenter, FontLight);

      ButtonSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x80, 0x80, 0x80, 0xff)),
        new KnSkin.SkinState(new Color32(0x43, 0x43, 0x43, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x53, 0x53, 0x53, 0xff), new Color32(0xff, 0xff, 0xff, 0xff)),
        "GUI.base.png", TextAnchor.MiddleCenter, FontLight);

      BoxLeftSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0x80), new Color32(0x15, 0x15, 0x15, 0xff)),
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0x80), new Color32(0x15, 0x15, 0x15, 0xff)),
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0x80), new Color32(0x15, 0x15, 0x15, 0xff)),
        "GUI.base.png", TextAnchor.MiddleLeft, FontLight);

      ModTabSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x53, 0x53, 0x53, 0xff), new Color32(0xff, 0xff, 0xff, 0xff)),
        "GUI.base.png", TextAnchor.MiddleCenter, FontTabs);

      WarningSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0x9f, 0x40, 0x40, 0xff), new Color32(0xff, 0xff, 0xff, 0xff)),
        new KnSkin.SkinState(new Color32(0x9f, 0x40, 0x40, 0xff), new Color32(0xff, 0xff, 0xff, 0xff)),
        new KnSkin.SkinState(new Color32(0x9f, 0x40, 0x40, 0xff), new Color32(0xff, 0xff, 0xff, 0xff)),
        "GUI.base.png", TextAnchor.MiddleCenter, FontLight);

      MakeMainContainerStyle();
      MakeButtonsStyle();
      MakeSliderStyle();
      MakeScrollViewStyle();
      MakeTextFieldStyle();
      MakeTimelineSlider();
      MakeTachStyle();

      Log.Write("[KN_Core::Skin]: Skin loaded");
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