using UnityEngine;

namespace KN_Core {
  public class Tachometer {
    private const float Width = 250.0f;
    private const float RpmWidth = 46.0f;
    private const float GearWidth = 36.0f;
    private const float GearHeight = 45.0f;

    private readonly Core core_;
    private readonly Color tachColor_;
    private readonly Color tachColorAlpha_;

    public Tachometer(Core core) {
      core_ = core;
      tachColor_ = new Color32(0xff, 0xff, 0xff, 0xff);
      tachColorAlpha_ = new Color32(0xff, 0xff, 0xff, 0xab);
    }

    public void Update() { }

    public void OnGUI(float x, float y) {
      if (core_.PlayerCar.CarX == null) {
        return;
      }

      if (Input.GetKey(KeyCode.H)) {
        core_.PlayerCar.CarX.rpm = 15000.0f;
      }

      const float maxRpm = 10000.0f;

      int gear = core_.PlayerCar.CarX.gear;
      float rpm = core_.PlayerCar.CarX.rpm;
      float limiterBeg = core_.PlayerCar.CarX.engineRevLimiter - 1000.0f;
      float limiter = limiterBeg % 1000.0f >= 500.0f ? limiterBeg + 1000.0f - limiterBeg % 1000.0f : limiterBeg - limiterBeg % 1000.0f;
      float rpmUnderBounds = rpm > maxRpm ? maxRpm : rpm;
      float rpmLimited = rpmUnderBounds > limiter ? limiter : rpmUnderBounds;

      float boxWidth = limiter / maxRpm * Width;
      float redWidth = (maxRpm - limiter) / maxRpm * Width;

      string gearStr = gear == -1 ? "R" : gear == 0 ? "N" : $"{gear:D}";
      DrawBox(x, y, GearWidth, GearHeight, Skin.TachGearBg, tachColorAlpha_, gearStr);
      x += GearWidth + Gui.OffsetSmall;

      float rpmBoxAdd = rpm > 10000 ? 9.0f : 0.0f;
      DrawBox(x, y, RpmWidth + rpmBoxAdd, Gui.Height, Skin.TachBg, tachColorAlpha_, $"{(int) rpm:D}");
      y += Gui.Height + Gui.OffsetSmall;

      DrawBox(x, y, boxWidth, Gui.Height, Skin.TachBg, tachColorAlpha_);
      x += boxWidth;
      DrawBox(x, y, redWidth, Gui.Height, Skin.TachRedBg, tachColorAlpha_);
      x -= boxWidth;

      DrawBox(x, y, rpmLimited / limiter * boxWidth, Gui.Height, Skin.TachFill, tachColor_);
      x += boxWidth;

      if (rpm > limiter) {
        DrawBox(x, y, (rpmUnderBounds - limiter) / (maxRpm - limiter) * redWidth, Gui.Height, Skin.TachFillRed, tachColor_);
      }
    }

    private static void DrawBox(float x, float y, float width, float height, GUISkin skin, Color color, string text = "") {
      var oldColor = GUI.color;
      var old = GUI.skin;
      GUI.skin = skin;
      GUI.color = color;
      GUI.Box(new Rect(x, y, width, height), text);
      GUI.color = oldColor;
      GUI.skin = old;
    }
  }
}