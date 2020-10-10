using KN_Loader;
using UnityEngine;

namespace KN_Core {
  public static class Skin {
    private static Texture2D texTachBg_;
    private static Texture2D texTachOutline_;
    public static GUISkin TachBg;
    public static GUISkin TachRedBg;
    public static GUISkin TachGearBg;
    public static GUISkin TachFill;
    public static GUISkin TachFillRed;
    public static GUISkin TachOutline;

    public static Color SeparatorColor;

    public static KnSkin PlusSkin;

    public static KnSkin ModPanelSkin;
    public static KnSkin ModPanelBackSkin;
    public static KnSkin DummyIconSkin;
    public static KnSkin GearSkin;
    public static KnSkin DiscordSkin;

    public static KnSkin ButtonSkin;
    public static KnSkin SliderSkin;
    public static KnSkin ScrollSkin;
    public static KnSkin BackgroundSkin;
    public static KnSkin BoxLeftSkin;
    public static KnSkin BoxSkin;

    public static KnSkin ModTabSkin;
    public static KnSkin WarningSkin;

    private static Font fontVersion_;
    private static Font fontLight_;
    private static Font fontTabs_;
    private static Font fontTach_;
    private static Font fontGear_;

    private static bool initialized_;

    public static void LoadAll() {
      if (initialized_) {
        return;
      }
      initialized_ = true;

      Log.Write("[KN_Core::Skin]: Loading skin ...");

      SeparatorColor = new Color32(0xee, 0xee, 0xee, 0xff);

      fontVersion_ = Font.CreateDynamicFontFromOSFont("Consolas", 9);
      fontTabs_ = Font.CreateDynamicFontFromOSFont("Consolas Bold", 12);
      fontLight_ = Font.CreateDynamicFontFromOSFont("Consolas", 12);

      fontTach_ = Font.CreateDynamicFontFromOSFont("Consolas Bold", 16);
      fontGear_ = Font.CreateDynamicFontFromOSFont("Consolas Bold", 32);

      PlusSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0xff, 0xff, 0xc6, 0xff)),
        new KnSkin.SkinState(new Color32(0x64, 0xff, 0xee, 0xff)),
        new KnSkin.SkinState(new Color32(0xff, 0xff, 0xff, 0xff)),
        "GUI.plus.png", TextAnchor.MiddleCenter, fontLight_);

      ModPanelSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        "GUI.base.png", TextAnchor.MiddleLeft, fontVersion_);

      ModPanelBackSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0x00, 0x00, 0x00, 0x00)),
        new KnSkin.SkinState(new Color32(0x50, 0x50, 0x50, 0xff)),
        new KnSkin.SkinState(new Color32(0x65, 0x65, 0x65, 0xff)),
        "GUI.base.png", TextAnchor.MiddleCenter, fontLight_);

      DummyIconSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0xff, 0xff, 0xc6, 0xff)),
        new KnSkin.SkinState(new Color32(0x64, 0xff, 0xee, 0xff)),
        new KnSkin.SkinState(new Color32(0xff, 0xff, 0xff, 0xff)),
        "GUI.base.png", TextAnchor.MiddleCenter, fontLight_);

      GearSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0x5d, 0x0f, 0xc6, 0xff)),
        new KnSkin.SkinState(new Color32(0x64, 0xff, 0xd6, 0xff)),
        new KnSkin.SkinState(new Color32(0x31, 0xff, 0xff, 0xff)),
        "GUI.gear.png", TextAnchor.MiddleCenter, fontLight_);

      DiscordSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0xff, 0xff, 0xc6, 0xff)),
        new KnSkin.SkinState(new Color32(0x64, 0xff, 0xee, 0xff)),
        new KnSkin.SkinState(new Color32(0xff, 0xff, 0xff, 0xff)),
        "GUI.discord.png", TextAnchor.MiddleCenter, fontLight_);

      ButtonSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x80, 0x80, 0x80, 0xff)),
        new KnSkin.SkinState(new Color32(0x43, 0x43, 0x43, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x53, 0x53, 0x53, 0xff), new Color32(0xff, 0xff, 0xff, 0xff)),
        "GUI.base.png", TextAnchor.MiddleCenter, fontLight_);

      BackgroundSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xa3)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xa3)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xa3)),
        "GUI.base.png", TextAnchor.MiddleCenter, fontLight_);

      BoxLeftSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xed), new Color32(0x15, 0x15, 0x15, 0xff)),
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xed), new Color32(0x15, 0x15, 0x15, 0xff)),
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xed), new Color32(0x15, 0x15, 0x15, 0xff)),
        "GUI.base.png", TextAnchor.MiddleLeft, fontLight_);

      BoxSkin = new KnSkin(KnSkin.Type.Box,
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xed), new Color32(0x15, 0x15, 0x15, 0xff)),
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xed), new Color32(0x15, 0x15, 0x15, 0xff)),
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xed), new Color32(0x15, 0x15, 0x15, 0xff)),
        "GUI.base.png", TextAnchor.MiddleCenter, fontLight_);

      ModTabSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff), new Color32(0x90, 0x90, 0x90, 0xff)),
        new KnSkin.SkinState(new Color32(0x53, 0x53, 0x53, 0xff), new Color32(0xff, 0xff, 0xff, 0xff)),
        "GUI.base.png", TextAnchor.MiddleCenter, fontTabs_);

      WarningSkin = new KnSkin(KnSkin.Type.Button,
        new KnSkin.SkinState(new Color32(0x9f, 0x40, 0x40, 0xff), new Color32(0xff, 0xff, 0xff, 0xff)),
        new KnSkin.SkinState(new Color32(0x9f, 0x40, 0x40, 0xff), new Color32(0xff, 0xff, 0xff, 0xff)),
        new KnSkin.SkinState(new Color32(0x9f, 0x40, 0x40, 0xff), new Color32(0xff, 0xff, 0xff, 0xff)),
        "GUI.base.png", TextAnchor.MiddleCenter, fontLight_);

      SliderSkin = new KnSkin(KnSkin.Type.Slider,
        new KnSkin.SkinState(new Color32(0xee, 0xee, 0xee, 0xff), new Color32(0x15, 0x15, 0x15, 0xff)),
        new KnSkin.SkinState(new Color32(0x33, 0x33, 0x33, 0xff)),
        new KnSkin.SkinState(new Color32(0x53, 0x53, 0x53, 0xff)),
        "GUI.base.png", TextAnchor.MiddleCenter, fontLight_);

      ScrollSkin = new KnSkin(KnSkin.Type.Scroll,
        new KnSkin.SkinState(new Color32(0xee, 0xee, 0xee, 0xff), new Color32(0xde, 0xde, 0xde, 0xff)),
        new KnSkin.SkinState(new Color32(0x53, 0x53, 0x53, 0xff)),
        new KnSkin.SkinState(new Color32(0xde, 0xde, 0xde, 0xff)),
        "GUI.base.png", TextAnchor.MiddleCenter, fontLight_);

      MakeTachStyle();

      Log.Write("[KN_Core::Skin]: Skin loaded");
    }


    private static void MakeTachStyle() {
      texTachBg_ = Embedded.LoadEmbeddedTexture("TachBg.png");

      TachBg = ScriptableObject.CreateInstance<GUISkin>();
      TachBg.box.normal.background = texTachBg_;
      // TachBg.box.normal.textColor = TextColorInv;
      TachBg.box.alignment = TextAnchor.MiddleRight;
      TachBg.box.font = fontTach_;
      TachBg.box.padding = new RectOffset(5, 5, 0, 5);

      TachGearBg = ScriptableObject.CreateInstance<GUISkin>();
      TachGearBg.box.normal.background = texTachBg_;
      // TachGearBg.box.normal.textColor = TextColorInv;
      TachGearBg.box.alignment = TextAnchor.MiddleCenter;
      TachGearBg.box.font = fontGear_;
      TachGearBg.box.padding = new RectOffset(1, 0, 0, 8);

      TachRedBg = ScriptableObject.CreateInstance<GUISkin>();
      // TachRedBg.box.normal.background = texRedButtonA_;

      TachFill = ScriptableObject.CreateInstance<GUISkin>();
      // TachFill.box.normal.background = texMainDark_; //white

      TachFillRed = ScriptableObject.CreateInstance<GUISkin>();
      // TachFillRed.box.normal.background = texRedButtonH_;

      texTachOutline_ = Embedded.LoadEmbeddedTexture("TachoOutline.png");
      texTachOutline_.filterMode = FilterMode.Point;
      TachOutline = ScriptableObject.CreateInstance<GUISkin>();
      TachOutline.box.normal.background = texTachOutline_;
      // TachOutline.box.normal.textColor = TextColor;
      TachOutline.box.alignment = TextAnchor.UpperCenter;
      TachOutline.box.font = fontLight_;
      TachOutline.box.border = new RectOffset(2, 2, 2, 2);
    }
  }
}