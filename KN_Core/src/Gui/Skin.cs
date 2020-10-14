using System.Reflection;
using KN_Loader;
using UnityEngine;

namespace KN_Core {
  public static class Skin {
    public const string CoreGuiPath = "KN_Core.Resources.GUI";

    public static GUISkin TachBg;
    public static GUISkin TachRedBg;
    public static GUISkin TachGearBg;
    public static GUISkin TachFill;
    public static GUISkin TachFillRed;
    public static GUISkin TachOutline;

    public static Color TextColorLight0;
    public static Color TextColorLight1;
    public static Color TextColorLight2;
    public static Color TextColorLight3;
    public static Color TextColorDark0;
    public static Color TextColorDark1;
    public static Color SeparatorColor;
    public static Color ContrastColor;
    public static Color ContrastHoverColor;
    public static Color ContrastActiveColor;
    public static Color LightColor;
    public static Color DarkColor;
    public static Color MildColor;
    public static Color BgTabBarColor;
    public static Color BgTabHoverColor;
    public static Color BgTabActiveColor;
    public static Color BgColor;
    public static Color BgWarnColor;
    public static Color TooltipColor;
    public static Color RedNormalColor;
    public static Color RedHoverColor;
    public static Color RedActiveColor;
    public static Color ButtonNormalColor;
    public static Color ButtonHoverColor;
    public static Color ButtonActiveColor;
    public static Color ListButtonNormalColor;
    public static Color ListButtonHoverColor;
    public static Color ListButtonActiveColor;

    public static KnSkin ModPanelSkin;
    public static KnSkin ModPanelBackSkin;
    public static KnSkin PuzzleSkin;
    public static KnSkin SettingsSkin;
    public static KnSkin AboutSkin;
    public static KnSkin VisualsSkin;
    public static KnSkin CarLightsSkin;
    public static KnSkin WorldLightsSkin;
    public static KnSkin MapsSkin;
    public static KnSkin DiscordSkin;

    public static KnSkin HelpSkin;
    public static KnSkin HelpBackSkin;

    public static KnSkin ButtonSkin;
    public static KnSkin RedButtonSkin;
    public static KnSkin ListButtonSkin;
    public static KnSkin SliderSkin;
    public static KnSkin ScrollSkin;
    public static KnSkin BackgroundSkin;
    public static KnSkin BoxLeftSkin;
    public static KnSkin BoxSkin;
    public static KnSkin BoxDarkSkin;
    public static KnSkin BoxMildSkin;

    public static KnSkin ModTabSkin;
    public static KnSkin ModTabSingleSkin;
    public static KnSkin WarningSkin;

    public static KnSkin TooltipSkin;

    private static Font fontVersion_;
    private static Font fontLight_;
    private static Font fontTabs_;
    private static Font fontTach_;
    private static Font fontGear_;

    private static Color tachTextColor_;
    private static Color tachTextColorAlt_;
    private static Texture2D texTach_;
    private static Texture2D texTachBg_;
    private static Texture2D texTachLimiter_;
    private static Texture2D texTachLimiterBg_;
    private static Texture2D texTachOutline_;

    private static bool initialized_;

    public static void LoadAll() {
      if (initialized_) {
        return;
      }
      initialized_ = true;

      Log.Write("[KN_Core::Skin]: Loading skin ...");

      TextColorLight0 = new Color32(0xe5, 0xea, 0xf2, 0xff);
      TextColorLight1 = new Color32(0xf1, 0xf7, 0xff, 0xff);
      TextColorLight2 = new Color32(0xbf, 0xc2, 0xcc, 0xff);
      TextColorLight3 = new Color32(0xdf, 0xe4, 0xec, 0xff);
      TextColorDark0 = new Color32(0x76, 0x7a, 0x80, 0xff);
      TextColorDark1 = new Color32(0x29, 0x2a, 0x2f, 0xff);

      SeparatorColor = new Color32(0xad, 0xaf, 0xb6, 0xff);

      ContrastColor = new Color32(0x33, 0x99, 0xcc, 0xff);
      ContrastHoverColor = new Color32(0x4b, 0xa8, 0xd7, 0xff);
      ContrastActiveColor = new Color32(0x31, 0x85, 0xaf, 0xff);

      LightColor = new Color32(0xf7, 0xf9, 0xff, 0xff);
      MildColor = new Color32(0xc3, 0xc5, 0xce, 0xff);
      DarkColor = new Color32(0xb0, 0xb2, 0xbb, 0xff);

      BgTabBarColor = new Color32(0x20, 0x21, 0x27, 0xf2);
      BgTabHoverColor = new Color32(0x30, 0x34, 0x39, 0xf2);
      BgTabActiveColor = new Color32(0x3a, 0x3d, 0x42, 0xf2);

      BgColor = new Color32(0xcf, 0xd1, 0xd8, 0xf2);
      BgWarnColor = new Color32(0xb0, 0x4f, 0x4f, 0xf2);

      TooltipColor = new Color32(0x22, 0x25, 0x2a, 0xf2);

      ButtonNormalColor = new Color32(0x4c, 0x5a, 0x6f, 0xff);
      ButtonHoverColor = new Color32(0x65, 0x77, 0x92, 0xff);
      ButtonActiveColor = new Color32(0x38, 0x42, 0x51, 0xff);

      ListButtonNormalColor = new Color32(0x65, 0x73, 0x8a, 0xff);
      ListButtonHoverColor = new Color32(0x73, 0x8d, 0xa4, 0xff);
      ListButtonActiveColor = new Color32(0x30, 0x48, 0x60, 0xff);

      RedNormalColor = new Color32(0xcf, 0x3f, 0x44, 0xff);
      RedHoverColor = new Color32(0xdd, 0x43, 0x49, 0xff);
      RedActiveColor = new Color32(0xbd, 0x39, 0x3e, 0xff);

      fontVersion_ = Font.CreateDynamicFontFromOSFont("Carlito", 9);
      fontTabs_ = Font.CreateDynamicFontFromOSFont("Carlito Bold", 12);
      fontLight_ = Font.CreateDynamicFontFromOSFont("Carlito", 12);

      PuzzleSkin = MakeModButton("puzzle.png");
      SettingsSkin = MakeModButton("gear.png");
      AboutSkin = MakeModButton("about.png");
      VisualsSkin = MakeModButton("visuals.png");
      CarLightsSkin = MakeModButton("lights.png");
      WorldLightsSkin = MakeModButton("weather.png");
      MapsSkin = MakeModButton("puzzle.png");
      DiscordSkin = MakeModButton("discord.png");

      BackgroundSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(BgColor),
        new KnSkin.SkinState(BgColor),
        new KnSkin.SkinState(BgColor),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      ModPanelSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(BgTabBarColor, TextColorDark0),
        new KnSkin.SkinState(BgTabBarColor, TextColorDark0),
        new KnSkin.SkinState(BgTabBarColor, TextColorDark0),
        "base.png", TextAnchor.MiddleLeft, fontVersion_);

      ModPanelBackSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(Color.clear),
        new KnSkin.SkinState(BgTabHoverColor),
        new KnSkin.SkinState(BgTabActiveColor),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      ButtonSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(ButtonNormalColor, TextColorLight3),
        new KnSkin.SkinState(ButtonHoverColor, TextColorLight3),
        new KnSkin.SkinState(ButtonActiveColor, TextColorLight1),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      RedButtonSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(RedNormalColor, TextColorLight0),
        new KnSkin.SkinState(RedHoverColor, TextColorLight0),
        new KnSkin.SkinState(RedActiveColor, TextColorLight1),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      ListButtonSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(ListButtonNormalColor, TextColorLight3),
        new KnSkin.SkinState(ListButtonHoverColor, TextColorLight0),
        new KnSkin.SkinState(ListButtonActiveColor, TextColorLight1),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      BoxLeftSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(LightColor, TextColorDark1),
        new KnSkin.SkinState(LightColor, TextColorDark1),
        new KnSkin.SkinState(LightColor, TextColorDark1),
        "base.png", TextAnchor.MiddleLeft, fontLight_);

      BoxSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(LightColor, TextColorDark1),
        new KnSkin.SkinState(LightColor, TextColorDark1),
        new KnSkin.SkinState(LightColor, TextColorDark1),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      BoxDarkSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(DarkColor, TextColorDark1),
        new KnSkin.SkinState(DarkColor, TextColorDark1),
        new KnSkin.SkinState(DarkColor, TextColorDark1),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      BoxMildSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(MildColor, TextColorDark1),
        new KnSkin.SkinState(MildColor, TextColorDark1),
        new KnSkin.SkinState(MildColor, TextColorDark1),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      ModTabSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(BgTabBarColor, TextColorLight2),
        new KnSkin.SkinState(BgTabHoverColor, TextColorLight0),
        new KnSkin.SkinState(BgTabActiveColor, TextColorLight1),
        "base.png", TextAnchor.MiddleCenter, fontTabs_);

      ModTabSingleSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(BgTabBarColor, TextColorLight2),
        new KnSkin.SkinState(BgTabBarColor, TextColorLight2),
        new KnSkin.SkinState(BgTabBarColor, TextColorLight2),
        "base.png", TextAnchor.MiddleCenter, fontTabs_);

      WarningSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(BgWarnColor, TextColorLight0),
        new KnSkin.SkinState(BgWarnColor, TextColorLight0),
        new KnSkin.SkinState(BgWarnColor, TextColorLight0),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      SliderSkin = new KnSkin(KnSkin.Type.Slider,
        new KnSkin.SkinState(LightColor, TextColorDark1),
        new KnSkin.SkinState(ContrastColor),
        new KnSkin.SkinState(ContrastHoverColor),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      ScrollSkin = new KnSkin(KnSkin.Type.Scroll,
        new KnSkin.SkinState(LightColor, TextColorDark1),
        new KnSkin.SkinState(MildColor),
        new KnSkin.SkinState(ContrastColor),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      HelpSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(TextColorLight1),
        new KnSkin.SkinState(TextColorLight1),
        new KnSkin.SkinState(TextColorLight1),
        "help.png", TextAnchor.MiddleCenter, fontLight_);

      HelpBackSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(ContrastColor),
        new KnSkin.SkinState(ContrastHoverColor),
        new KnSkin.SkinState(ContrastActiveColor),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      TooltipSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(TooltipColor, TextColorLight0),
        new KnSkin.SkinState(TooltipColor, TextColorLight0),
        new KnSkin.SkinState(TooltipColor, TextColorLight0),
        "tooltip.png", TextAnchor.MiddleCenter, fontLight_);

      MakeTachStyle();

      Log.Write("[KN_Core::Skin]: Skin loaded");
    }

    private static KnSkin MakeModButton(string texture) {
      return MakeModButton(Assembly.GetExecutingAssembly(), $"{CoreGuiPath}.{texture}");
    }

    public static KnSkin MakeModButton(Assembly assembly, string texture) {
      var button = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0xea, 0xeb, 0xf0, 0xff)),
        new KnSkin.SkinState(new Color32(0xf7, 0xf9, 0xff, 0xff)),
        new KnSkin.SkinState(ContrastColor),
        assembly, texture, TextAnchor.MiddleCenter, fontLight_);

      return button;
    }

    private static void MakeTachStyle() {
      tachTextColor_ = new Color32(0xee, 0xee, 0xee, 0xff);
      tachTextColorAlt_ = new Color32(0x30, 0x30, 0x30, 0xff);

      fontTach_ = Font.CreateDynamicFontFromOSFont("Consolas Bold", 16);
      fontGear_ = Font.CreateDynamicFontFromOSFont("Consolas Bold", 32);

      texTach_ = Embedded.LoadEmbeddedTexture("GUI.base.png", new Color32(0xff, 0xff, 0xff, 0xff));
      texTachBg_ = Embedded.LoadEmbeddedTexture("GUI.base.png", new Color32(0x3b, 0x3b, 0x3b, 0xff));
      texTachOutline_ = Embedded.LoadEmbeddedTexture("GUI.outline.png", new Color32(0x20, 0x20, 0x20, 0xff));
      texTachLimiter_ = Embedded.LoadEmbeddedTexture("GUI.base.png", new Color32(0xd1, 0x06, 0x28, 0xff));
      texTachLimiterBg_ = Embedded.LoadEmbeddedTexture("GUI.base.png", new Color32(0x8d, 0x09, 0x17, 0xff));

      TachBg = ScriptableObject.CreateInstance<GUISkin>();
      TachBg.box.normal.background = texTachBg_;
      TachBg.box.normal.textColor = tachTextColor_;
      TachBg.box.alignment = TextAnchor.MiddleRight;
      TachBg.box.font = fontTach_;
      TachBg.box.padding = new RectOffset(5, 5, 0, 5);

      TachGearBg = ScriptableObject.CreateInstance<GUISkin>();
      TachGearBg.box.normal.background = texTachBg_;
      TachGearBg.box.normal.textColor = tachTextColor_;
      TachGearBg.box.alignment = TextAnchor.MiddleCenter;
      TachGearBg.box.font = fontGear_;
      TachGearBg.box.padding = new RectOffset(1, 0, 0, 8);

      TachRedBg = ScriptableObject.CreateInstance<GUISkin>();
      TachRedBg.box.normal.background = texTachLimiterBg_;

      TachFill = ScriptableObject.CreateInstance<GUISkin>();
      TachFill.box.normal.background = texTach_;

      TachFillRed = ScriptableObject.CreateInstance<GUISkin>();
      TachFillRed.box.normal.background = texTachLimiter_;

      texTachOutline_.filterMode = FilterMode.Point;
      TachOutline = ScriptableObject.CreateInstance<GUISkin>();
      TachOutline.box.normal.background = texTachOutline_;
      TachOutline.box.normal.textColor = tachTextColorAlt_;
      TachOutline.box.alignment = TextAnchor.UpperCenter;
      TachOutline.box.font = fontLight_;
      TachOutline.box.border = new RectOffset(2, 2, 2, 2);
    }
  }
}