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
    public static Color TextColorDark0;
    public static Color TextColorDark1;
    public static Color SeparatorColor;
    public static Color ContrastColor;
    public static Color ContrastHoverColor;
    public static Color ContrastActiveColor;
    public static Color LightColor;
    public static Color DarkColor;
    public static Color MildColor;
    public static Color BgTabHoverColor;
    public static Color BgTabActiveColor;
    public static Color BgTabBarColor;
    public static Color BgColor;
    public static Color BgWarnColor;
    public static Color WarnColor;
    public static Color RedNormalColor;
    public static Color RedHoverColor;
    public static Color RedActiveColor;
    public static Color ButtonNormalColor;
    public static Color ButtonHoverColor;
    public static Color ButtonActiveColor;
    public static Color TabButtonNormalColor;
    public static Color TabButtonHoverColor;
    public static Color TabButtonActiveColor;

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
    public static KnSkin SliderSkin;
    public static KnSkin ScrollSkin;
    public static KnSkin BackgroundSkin;
    public static KnSkin BoxLeftSkin;
    public static KnSkin BoxSkin;
    public static KnSkin BoxDarkSkin;
    public static KnSkin BoxMildSkin;

    public static KnSkin ModTabSkin;
    public static KnSkin WarningSkin;

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

      TextColorLight0 = new Color32(0xde, 0xdf, 0xe3, 0xff);
      TextColorLight1 = new Color32(0xea, 0xeb, 0xf0, 0xff);
      TextColorDark0 = new Color32(0x4f, 0x53, 0x59, 0xff);
      TextColorDark1 = new Color32(0x29, 0x2a, 0x2f, 0xff);

      SeparatorColor = new Color32(0x29, 0x2a, 0x2f, 0xff);

      ContrastColor = new Color32(0x33, 0x99, 0xcc, 0xff);
      ContrastHoverColor = new Color32(0x4b, 0xa8, 0xd7, 0xff);
      ContrastActiveColor = new Color32(0x31, 0x85, 0xaf, 0xff);

      LightColor = new Color32(0xf7, 0xf9, 0xff, 0xff);
      DarkColor = new Color32(0x3d, 0x3e, 0x45, 0xff);
      MildColor = new Color32(0xcd, 0xcf, 0xd5, 0xff);

      BgTabHoverColor = new Color32(0x2a, 0x2d, 0x31, 0xff);
      BgTabActiveColor = new Color32(0x33, 0x36, 0x3b, 0xff);

      BgTabBarColor = new Color32(0x20, 0x22, 0x25, 0xf2);
      BgColor = new Color32(0x47, 0x49, 0x51, 0xf2);
      BgWarnColor = new Color32(0xb0, 0x4f, 0x4f, 0xf2);
      WarnColor = new Color32(0xb0, 0x4f, 0x4f, 0xff);

      ButtonNormalColor = new Color32(0x29, 0x2a, 0x2f, 0xff);
      ButtonHoverColor = new Color32(0x20, 0x22, 0x25, 0xff);
      ButtonActiveColor = new Color32(0x20, 0x22, 0x25, 0xff);

      RedNormalColor = new Color32(0xcf, 0x3f, 0x44, 0xff);
      RedHoverColor = new Color32(0xdd, 0x43, 0x49, 0xff);
      RedActiveColor = new Color32(0xbd, 0x39, 0x3e, 0xff);

      TabButtonNormalColor = new Color32(0x20, 0x22, 0x25, 0xff);
      TabButtonHoverColor = new Color32(0x1d, 0x1e, 0x21, 0xff);
      TabButtonActiveColor = new Color32(0x18, 0x1a, 0x1c, 0xff);

      fontVersion_ = Font.CreateDynamicFontFromOSFont("Consolas", 9);
      fontTabs_ = Font.CreateDynamicFontFromOSFont("Consolas Bold", 12);
      fontLight_ = Font.CreateDynamicFontFromOSFont("Consolas", 12);

      PuzzleSkin = MakeModButton("puzzle.png");
      SettingsSkin = MakeModButton("puzzle.png");
      AboutSkin = MakeModButton("puzzle.png");
      VisualsSkin = MakeModButton("puzzle.png");
      CarLightsSkin = MakeModButton("puzzle.png");
      WorldLightsSkin = MakeModButton("puzzle.png");
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
        new KnSkin.SkinState(ButtonNormalColor, TextColorLight0),
        new KnSkin.SkinState(ButtonHoverColor, TextColorLight1),
        new KnSkin.SkinState(ButtonActiveColor, ContrastColor),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      RedButtonSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(RedNormalColor, TextColorLight0),
        new KnSkin.SkinState(RedHoverColor, TextColorLight0),
        new KnSkin.SkinState(RedActiveColor, TextColorLight1),
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
        new KnSkin.SkinState(DarkColor, TextColorLight1),
        new KnSkin.SkinState(DarkColor, TextColorLight1),
        new KnSkin.SkinState(DarkColor, TextColorLight1),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      BoxMildSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(MildColor, TextColorDark1),
        new KnSkin.SkinState(MildColor, TextColorDark1),
        new KnSkin.SkinState(MildColor, TextColorDark1),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      ModTabSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(TabButtonNormalColor, TextColorLight0),
        new KnSkin.SkinState(TabButtonHoverColor, TextColorLight0),
        new KnSkin.SkinState(TabButtonActiveColor, TextColorLight1),
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