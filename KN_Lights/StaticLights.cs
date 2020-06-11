using System.Collections.Generic;
using KN_Core;
using UnityEngine;

namespace KN_Lights {
  public class StaticLights {
    private readonly Core core_;

    private readonly ColorPicker colorPicker_;
    private readonly CarPicker carPicker_;

    private float clListScrollH_;
    private Vector2 clListScroll_;
    private int lightId_;
    private StaticLightData activeLight_;
    private readonly List<StaticLightData> lights_;

    public StaticLights(Core core) {
      core_ = core;

      colorPicker_ = new ColorPicker();
      carPicker_ = new CarPicker(core);

      lights_ = new List<StaticLightData>();
    }

    public void ResetPickers() {
      colorPicker_.Reset();
      carPicker_.Reset();
    }

    public void OnStart() { }

    public void OnStop() { }

    public void Update() {
      if (carPicker_.IsPicking && !TFCar.IsNull(carPicker_.PickedCar)) {
        SpawnLight(carPicker_.PickedCar);
        carPicker_.Reset();
      }

      if (colorPicker_.IsPicking) {
        if (activeLight_ != null && colorPicker_.PickedColor != activeLight_.Color) {
          activeLight_.Color = colorPicker_.PickedColor;
        }
      }
    }

    public void GuiPickers(Gui gui, ref float x, ref float y) {
      carPicker_.OnGUI(gui, ref x, ref y);

      if (colorPicker_.IsPicking) {
        if (carPicker_.IsPicking) {
          x += Gui.OffsetGuiX;
        }
        colorPicker_.OnGui(gui, ref x, ref y);
      }
    }

    public void OnGUI(Gui gui, ref float x, ref float y) {
      float yBegin = y;

      const float width = Gui.Width * 2.0f;
      const float height = Gui.Height;

      GuiProps(gui, ref x, ref y, width, height);

      y = yBegin;
      x += width;

      x += Gui.OffsetGuiX;
      gui.Line(x, y, 1.0f, core_.GuiTabsHeight - Gui.OffsetY * 2.0f, Skin.SeparatorColor);
      x += Gui.OffsetGuiX;

      GuiList(gui, ref x, ref y);
    }

    private void GuiProps(Gui gui, ref float x, ref float y, float width, float height) {
      bool guiEnabled = GUI.enabled;
      GUI.enabled = activeLight_ != null;

      bool lightEnabled = activeLight_?.Enabled ?? false;
      if (gui.Button(ref x, ref y, width, height, lightEnabled ? "DISABLE" : "ENABLE", lightEnabled ? Skin.ButtonActive : Skin.Button)) {
        if (activeLight_ != null) {
          activeLight_.Enabled = !activeLight_.Enabled;
        }
      }

      var type = activeLight_?.Type ?? LightType.Spot;
      string typeStr = type == LightType.Point ? "POINT" : "SPOT";
      if (gui.Button(ref x, ref y, width, height, $"TYPE: {typeStr}", Skin.Button)) {
        if (activeLight_ != null) {
          activeLight_.Type = activeLight_.Type == LightType.Spot ? LightType.Point : LightType.Spot;
        }
      }

      if (gui.Button(ref x, ref y, width, height, "COLOR", Skin.Button)) {
        if (activeLight_ != null) {
          colorPicker_.Toggle(activeLight_.Color, false);
        }
      }

      if (type == LightType.Spot) {
        float angle = activeLight_?.Angle ?? 0.0f;
        if (gui.SliderH(ref x, ref y, width, ref angle, 10.0f, 180.0f, $"ANGLE: {angle:F1}")) {
          if (activeLight_ != null) {
            activeLight_.Angle = angle;
          }
        }
      }

      float maxBrightness = type == LightType.Spot ? 5000.0f : 100.0f;
      float brightness = activeLight_?.Brightness ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref brightness, 0.1f, maxBrightness, $"BRIGHTNESS: {brightness:F1}")) {
        if (activeLight_ != null) {
          activeLight_.Brightness = brightness;
        }
      }

      float maxRange = type == LightType.Spot ? 1000.0f : 30.0f;
      float range = activeLight_?.Range ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref range, 0.1f, maxRange, $"RANGE: {range:F1}")) {
        if (activeLight_ != null) {
          activeLight_.Range = range;
        }
      }

      var offset = activeLight_?.Position ?? Vector3.zero;
      if (gui.SliderH(ref x, ref y, width, ref offset.x, 0.0f, 3.0f, $"X: {offset.x:F}")) {
        if (activeLight_ != null) {
          activeLight_.Position = offset;
        }
      }

      if (gui.SliderH(ref x, ref y, width, ref offset.y, 0.0f, 3.0f, $"Y: {offset.y:F}")) {
        if (activeLight_ != null) {
          activeLight_.Position = offset;
        }
      }

      if (gui.SliderH(ref x, ref y, width, ref offset.z, 0.0f, 3.0f, $"Z: {offset.z:F}")) {
        if (activeLight_ != null) {
          activeLight_.Position = offset;
        }
      }

      GUI.enabled = guiEnabled;
    }

    private void GuiList(Gui gui, ref float x, ref float y) {
      const float listHeight = 320.0f;
      const float widthScale = 1.2f;
      const float buttonWidth = Gui.Width * widthScale;

      bool guiEnabled = GUI.enabled;
      //todo:
      // GUI.enabled = !core_.IsInGarage;

      if (gui.Button(ref x, ref y, buttonWidth, Gui.Height, "ADD LIGHT", Skin.Button)) {
        SpawnLight();
      }

      if (gui.Button(ref x, ref y, buttonWidth, Gui.Height, "ADD LIGHT TO", Skin.Button)) {
        carPicker_.Toggle();
        colorPicker_.Reset();
      }
      GUI.enabled = guiEnabled;

      gui.BeginScrollV(ref x, ref y, buttonWidth, listHeight, clListScrollH_, ref clListScroll_, $"LIGHTS {lights_.Count}");

      float sx = x;
      float sy = y;
      const float offset = Gui.ScrollBarWidth;
      bool scrollVisible = clListScrollH_ > listHeight;
      float width = scrollVisible ? Gui.WidthScroll * widthScale - offset : Gui.WidthScroll * widthScale + offset;
      foreach (var light in lights_) {
        if (light != null) {
          bool active = activeLight_ == light;
          if (gui.ScrollViewButton(ref sx, ref sy, width, Gui.Height, $"{light.Name}", out bool delPressed, active ? Skin.ButtonActive : Skin.Button, Skin.RedSkin)) {
            if (delPressed) {
              if (light == activeLight_) {
                activeLight_ = null;
              }
              light.Dispose();
              lights_.Remove(light);
              break;
            }
            activeLight_ = light;
            if (colorPicker_.IsPicking) {
              colorPicker_.Pick(activeLight_.Color, false);
            }
          }
        }
      }

      clListScrollH_ = gui.EndScrollV(ref x, ref y, sx, sy);
      y += Gui.OffsetSmall;
    }

    private void SpawnLight(TFCar parent = null) {
      //todo:
      // if (core_.IsInGarage) {
      //   return;
      // }

      var light = new StaticLightData(LightType.Point, $"Light_{lightId_}", core_.ActiveCamera.transform);

      activeLight_ = light;
      lights_.Add(light);
      if (parent != null) {
        light.Attach(parent);
      }
      ++lightId_;
    }
  }
}