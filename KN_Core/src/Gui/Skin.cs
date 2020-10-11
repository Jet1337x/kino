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

    public static Color SeparatorColor;
    public static Color WarnColor;

    public static KnSkin ModPanelSkin;
    public static KnSkin ModPanelBackSkin;
    public static KnSkin PuzzleSkin;
    public static KnSkin SettingsSkin;
    public static KnSkin AboutSkin;
    public static KnSkin VisualsSkin;
    public static KnSkin LightsSkin;
    public static KnSkin MapsSkin;
    public static KnSkin DiscordSkin;

    public static KnSkin ButtonSkin;
    public static KnSkin RedButtonSkin;
    public static KnSkin SliderSkin;
    public static KnSkin ScrollSkin;
    public static KnSkin BackgroundSkin;
    public static KnSkin BoxLeftSkin;
    public static KnSkin BoxSkin;
    public static KnSkin OutlineSkin;

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

      SeparatorColor = new Color32(0xee, 0xee, 0xee, 0xff);
      WarnColor = new Color32(0x9f, 0x40, 0x40, 0xff);

      fontVersion_ = Font.CreateDynamicFontFromOSFont("Consolas", 9);
      fontTabs_ = Font.CreateDynamicFontFromOSFont("Consolas Bold", 12);
      fontLight_ = Font.CreateDynamicFontFromOSFont("Consolas", 12);

      PuzzleSkin = MakeModButton("puzzle.png");
      SettingsSkin = MakeModButton("puzzle.png");
      AboutSkin = MakeModButton("puzzle.png");
      VisualsSkin = MakeModButton("puzzle.png");
      LightsSkin = MakeModButton("puzzle.png");
      MapsSkin = MakeModButton("puzzle.png");
      DiscordSkin = MakeModButton("discord.png");

      ModPanelSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        "base.png", TextAnchor.MiddleLeft, fontVersion_);

      ModPanelBackSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0x00, 0x00, 0x00, 0x00)),
        new KnSkin.SkinState(new Color32(0x50, 0x50, 0x50, 0xff)),
        new KnSkin.SkinState(new Color32(0x65, 0x65, 0x65, 0xff)),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      ButtonSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x80, 0x80, 0x80, 0xff)),
        new KnSkin.SkinState(new Color32(0x43, 0x43, 0x43, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x53, 0x53, 0x53, 0xff), new Color32(0xff, 0xff, 0xff, 0xff)),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      RedButtonSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(WarnColor, new Color32(0xde, 0xde, 0xde, 0xed)),
        new KnSkin.SkinState(WarnColor, new Color32(0xde, 0xde, 0xde, 0xed)),
        new KnSkin.SkinState(WarnColor, new Color32(0xff, 0xff, 0xff, 0xff)),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      BackgroundSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xa3)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xa3)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xa3)),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      BoxLeftSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xed), new Color32(0x15, 0x15, 0x15, 0xff)),
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xed), new Color32(0x15, 0x15, 0x15, 0xff)),
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xed), new Color32(0x15, 0x15, 0x15, 0xff)),
        "base.png", TextAnchor.MiddleLeft, fontLight_);

      BoxSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xed), new Color32(0x15, 0x15, 0x15, 0xff)),
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xed), new Color32(0x15, 0x15, 0x15, 0xff)),
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xed), new Color32(0x15, 0x15, 0x15, 0xff)),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      OutlineSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xed), new Color32(0x15, 0x15, 0x15, 0xff)),
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xed), new Color32(0x15, 0x15, 0x15, 0xff)),
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xed), new Color32(0x15, 0x15, 0x15, 0xff)),
        "outline.png", TextAnchor.MiddleCenter, fontLight_);

      ModTabSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x53, 0x53, 0x53, 0xff), new Color32(0xff, 0xff, 0xff, 0xff)),
        "base.png", TextAnchor.MiddleCenter, fontTabs_);

      WarningSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(WarnColor, new Color32(0xff, 0xff, 0xff, 0xff)),
        new KnSkin.SkinState(WarnColor, new Color32(0xff, 0xff, 0xff, 0xff)),
        new KnSkin.SkinState(WarnColor, new Color32(0xff, 0xff, 0xff, 0xff)),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      SliderSkin = new KnSkin(KnSkin.Type.Slider,
        new KnSkin.SkinState(new Color32(0xee, 0xee, 0xee, 0xff), new Color32(0x15, 0x15, 0x15, 0xff)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff)),
        new KnSkin.SkinState(new Color32(0x53, 0x53, 0x53, 0xff)),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      ScrollSkin = new KnSkin(KnSkin.Type.Scroll,
        new KnSkin.SkinState(new Color32(0xee, 0xee, 0xee, 0xff), new Color32(0xde, 0xde, 0xde, 0xff)),
        new KnSkin.SkinState(new Color32(0x53, 0x53, 0x53, 0xff)),
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xff)),
        "base.png", TextAnchor.MiddleCenter, fontLight_);

      MakeTachStyle();

      Log.Write("[KN_Core::Skin]: Skin loaded");
    }

    private static KnSkin MakeModButton(string texture) {
      return MakeModButton(Assembly.GetExecutingAssembly(), $"{CoreGuiPath}.{texture}");
    }

    public static KnSkin MakeModButton(Assembly assembly, string texture) {
      var button = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0xff, 0xff, 0xc6, 0xff)),
        new KnSkin.SkinState(new Color32(0x64, 0xff, 0xee, 0xff)),
        new KnSkin.SkinState(new Color32(0xff, 0xff, 0xff, 0xff)),
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