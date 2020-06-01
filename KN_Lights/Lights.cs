using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KN_Core;
using UnityEngine;

namespace KN_Lights {
  public class Lights : BaseMod {
    public static Texture2D LightMask;

    public const string LightsConfigFile = "kn_lights.knl";
    public const string NwLightsConfigFile = "kn_nwlights.knl";
#if KN_DEV_TOOLS
    public const string LightsDevConfigFile = "kn_lights_dev.knl";
#endif

    private LightsConfig lightsConfig_;
    private NwLightsConfig nwLightsConfig_;
#if KN_DEV_TOOLS
    private LightsConfig carLightsDev_;
#endif

    private CarLights activeLights_;
    private readonly List<CarLights> carLights_;
    private readonly List<CarLights> carLightsToRemove_;

    private bool allowPick_;
    private float clListScrollH_;
    private Vector2 clListScroll_;

    private bool hlTabActive_ = true;
    private bool slTabActive_;

    private bool pickingColor_;

    public Lights(Core core) : base(core, "LIGHTS", 4) {
      carLights_ = new List<CarLights>();
      carLightsToRemove_ = new List<CarLights>();
    }

    public override void ResetState() {
      pickingColor_ = false;
      Core.ColorPicker.Reset();
    }

    public override void OnStart() {
      var assembly = Assembly.GetExecutingAssembly();

      LightMask = Core.LoadTexture(assembly, "KN_Lights", "HeadLightMask.png");

      if (LightsConfigSerializer.Deserialize(LightsConfigFile, out var lights)) {
        lightsConfig_ = new LightsConfig(lights);
      }
      else {
        lightsConfig_ = new LightsConfig();
        //todo(trbflxr): load default
      }
      nwLightsConfig_ = LightsConfigSerializer.Deserialize(NwLightsConfigFile, out var nwLights) ? new NwLightsConfig(nwLights) : new NwLightsConfig();
#if KN_DEV_TOOLS
      carLightsDev_ = LightsConfigSerializer.Deserialize(LightsDevConfigFile, out var devLights) ? new LightsConfig(devLights) : new LightsConfig();
#endif
    }

    public override void OnStop() {
      if (!LightsConfigSerializer.Serialize(lightsConfig_, LightsConfigFile)) { }
      if (!LightsConfigSerializer.Serialize(nwLightsConfig_, NwLightsConfigFile)) { }

#if KN_DEV_TOOLS
      LightsConfigSerializer.Serialize(carLightsDev_, LightsDevConfigFile);
#endif
    }

    public override void Update(int id) {
      if (id != Id) {
        return;
      }

      if (Core.PickedCar != null && allowPick_) {
        if (Core.PickedCar != Core.PlayerCar) {
          EnableLightsOn(Core.PickedCar);
        }
        Core.PickedCar = null;
        allowPick_ = false;
      }

      if (pickingColor_) {
        if (Core.ColorPicker.PickedColor != Color.white) {
          Log.Write(Core.ColorPicker.PickedColor.ToString());
        }
        if (Core.ColorPicker.IsForceClosed) {
          Core.ColorPicker.IsForceClosed = false;
          pickingColor_ = false;
        }
      }
    }

    public override void LateUpdate(int id) {
      if (id != Id) {
        return;
      }

      foreach (var cl in carLights_) {
        if (cl.Car == null || cl.Car.Base == null) {
          carLightsToRemove_.Add(cl);
          continue;
        }
        cl.LateUpdate();
      }

      if (carLightsToRemove_.Count > 0) {
        foreach (var cl in carLightsToRemove_) {
          if (activeLights_ == cl) {
            activeLights_ = null;
          }
          carLights_.Remove(cl);
        }
        carLightsToRemove_.Clear();
      }
    }

    public override void OnGUI(int id, Gui gui, ref float x, ref float y) {
      if (id != Id) {
        return;
      }

      GuiSideBar(gui, ref x, ref y);

      x += Gui.OffsetGuiX;
      gui.Line(x, y, 1.0f, Core.GuiTabsHeight - Gui.OffsetY * 2.0f, Skin.SeparatorColor);
      x += Gui.OffsetGuiX;

      if (hlTabActive_) {
        GuiHeadLightsTab(gui, ref x, ref y);
      }
      else if (slTabActive_) {
        GuiSunTab(gui, ref x, ref y);
      }
    }

    private void GuiSideBar(Gui gui, ref float x, ref float y) {
      float yBegin = y;

      x += Gui.OffsetSmall;
      if (gui.ImageButton(ref x, ref y, hlTabActive_ ? Skin.IconHeadlightsActive : Skin.IconHeadlights)) {
        hlTabActive_ = true;
        slTabActive_ = false;
        Core.ShowCars = false;
      }

      if (gui.ImageButton(ref x, ref y, slTabActive_ ? Skin.IconSunActive : Skin.IconSun)) {
        hlTabActive_ = false;
        slTabActive_ = true;
        Core.ShowCars = false;
      }

      x += Gui.IconSize;
      y = yBegin;
    }

    private void GuiHeadLightsTab(Gui gui, ref float x, ref float y) {
      float yBegin = y;

      const float width = Gui.Width * 2.0f;
      const float height = Gui.Height;

      bool guiEnabled = GUI.enabled;
      GUI.enabled = Core.PlayerCar != null;

      if (gui.Button(ref x, ref y, width, height, "ADD LIGHTS", Skin.Button)) {
        EnableLightsOnOnwCar();
      }

      GUI.enabled = activeLights_ != null;

#if KN_DEV_TOOLS
      if (gui.Button(ref x, ref y, width, height, "DEV SAVE", Skin.Button)) {
        carLightsDev_.AddLights(activeLights_);
        Log.Write($"[KN_Lights]: Dev save / saved for '{activeLights_?.CarId ?? 0}'");
      }
#endif

      bool debugObjects = activeLights_?.IsDebugObjectsEnabled ?? false;
      if (gui.Button(ref x, ref y, width, height, "DEBUG OBJECTS", debugObjects ? Skin.ButtonActive : Skin.Button)) {
        if (activeLights_ != null) {
          activeLights_.IsDebugObjectsEnabled = !activeLights_.IsDebugObjectsEnabled;
        }
      }

      GuiHeadLights(gui, ref x, ref y, width, height);

      gui.Line(x, y, width, 1.0f, Skin.SeparatorColor);
      y += Gui.OffsetY;

      GuiTailLights(gui, ref x, ref y, width, height);

      y = yBegin;
      x += width;

      x += Gui.OffsetGuiX;
      gui.Line(x, y, 1.0f, Core.GuiTabsHeight - Gui.OffsetY * 2.0f, Skin.SeparatorColor);
      x += Gui.OffsetGuiX;

      GUI.enabled = guiEnabled;

      GuiLightsList(gui, ref x, ref y);
    }

    private void GuiSunTab(Gui gui, ref float x, ref float y) {
      if (gui.Button(ref x, ref y, "DUMMY", Skin.Button)) { }
    }

    private void GuiHeadLights(Gui gui, ref float x, ref float y, float width, float height) {
      float xBegin = x;
      float widthLight = width / 2.0f - Gui.OffsetSmall;

      bool lh = activeLights_?.IsHeadLightLeftEnabled ?? false;
      if (gui.Button(ref x, ref y, widthLight, height, "LEFT HEADLIGHT", lh ? Skin.ButtonActive : Skin.Button)) {
        if (activeLights_ != null) {
          activeLights_.IsHeadLightLeftEnabled = !activeLights_.IsHeadLightLeftEnabled;
        }
      }
      y -= Gui.OffsetY + height;
      x += widthLight + Gui.OffsetSmall * 2.0f;

      bool rh = activeLights_?.IsHeadLightRightEnabled ?? false;
      if (gui.Button(ref x, ref y, widthLight, height, "RIGHT HEADLIGHT", rh ? Skin.ButtonActive : Skin.Button)) {
        if (activeLights_ != null) {
          activeLights_.IsHeadLightRightEnabled = !activeLights_.IsHeadLightRightEnabled;
        }
      }
      x = xBegin;

      if (gui.Button(ref x, ref y, width, height, "COLOR", Skin.Button)) {
        pickingColor_ = !pickingColor_;
        if (pickingColor_) {
          Core.ColorPicker.Pick(Color.white);
        }
        else {
          Core.ColorPicker.Reset();
        }
      }

      float brightness = activeLights_?.HeadLightBrightness ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref brightness, 100.0f, 20000.0f, $"HEADLIGHTS BRIGHTNESS: {brightness:F1}")) {
        if (activeLights_ != null) {
          activeLights_.HeadLightBrightness = brightness;
        }
      }

      float angle = activeLights_?.HeadLightAngle ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref angle, 50.0f, 160.0f, $"HEADLIGHTS ANGLE: {angle:F1}")) {
        if (activeLights_ != null) {
          activeLights_.HeadLightAngle = angle;
        }
      }

      float hlPitch = activeLights_?.Pitch ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref hlPitch, -20.0f, 20.0f, $"HEADLIGHTS PITCH: {hlPitch:F}")) {
        if (activeLights_ != null) {
          activeLights_.Pitch = hlPitch;
        }
      }

      var offset = activeLights_?.HeadlightOffset ?? Vector3.zero;
      if (gui.SliderH(ref x, ref y, width, ref offset.x, 0.0f, 3.0f, $"X: {offset.x:F}")) {
        if (activeLights_ != null) {
          activeLights_.HeadlightOffset = offset;
        }
      }

      if (gui.SliderH(ref x, ref y, width, ref offset.y, 0.0f, 3.0f, $"Y: {offset.y:F}")) {
        if (activeLights_ != null) {
          activeLights_.HeadlightOffset = offset;
        }
      }

      if (gui.SliderH(ref x, ref y, width, ref offset.z, 0.0f, 3.0f, $"Z: {offset.z:F}")) {
        if (activeLights_ != null) {
          activeLights_.HeadlightOffset = offset;
        }
      }
    }

    private void GuiTailLights(Gui gui, ref float x, ref float y, float width, float height) {
      float xBegin = x;
      float widthLight = width / 2.0f - Gui.OffsetSmall;

      bool lt = activeLights_?.IsTailLightLeftEnabled ?? false;
      if (gui.Button(ref x, ref y, widthLight, height, "LEFT TAILLIGHT", lt ? Skin.ButtonActive : Skin.Button)) {
        if (activeLights_ != null) {
          activeLights_.IsTailLightLeftEnabled = !activeLights_.IsTailLightLeftEnabled;
        }
      }
      y -= Gui.OffsetY + height;
      x += widthLight + Gui.OffsetSmall * 2.0f;

      bool rt = activeLights_?.IsTailLightRightEnabled ?? false;
      if (gui.Button(ref x, ref y, widthLight, height, "RIGHT TAILLIGHT", rt ? Skin.ButtonActive : Skin.Button)) {
        if (activeLights_ != null) {
          activeLights_.IsTailLightRightEnabled = !activeLights_.IsTailLightRightEnabled;
        }
      }
      x = xBegin;

      float brightness = activeLights_?.TailLightBrightness ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref brightness, 50.0f, 500.0f, $"TAILLIGHTS BRIGHTNESS: {brightness:F1}")) {
        if (activeLights_ != null) {
          activeLights_.TailLightBrightness = brightness;
        }
      }

      float angle = activeLights_?.TailLightAngle ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref angle, 50.0f, 170.0f, $"TAILLIGHTS ANGLE: {angle:F1}")) {
        if (activeLights_ != null) {
          activeLights_.TailLightAngle = angle;
        }
      }

      float tlPitch = activeLights_?.PitchTail ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref tlPitch, -20.0f, 20.0f, $"TAILLIGHTS PITCH: {tlPitch:F1}")) {
        if (activeLights_ != null) {
          activeLights_.PitchTail = tlPitch;
        }
      }

      var offset = activeLights_?.TailLightOffset ?? Vector3.zero;
      if (gui.SliderH(ref x, ref y, width, ref offset.x, 0.0f, 3.0f, $"X: {offset.x:F}")) {
        if (activeLights_ != null) {
          activeLights_.TailLightOffset = offset;
        }
      }

      if (gui.SliderH(ref x, ref y, width, ref offset.y, 0.0f, 3.0f, $"Y: {offset.y:F}")) {
        if (activeLights_ != null) {
          activeLights_.TailLightOffset = offset;
        }
      }

      if (gui.SliderH(ref x, ref y, width, ref offset.z, 0.0f, -3.0f, $"Z: {offset.z:F}")) {
        if (activeLights_ != null) {
          activeLights_.TailLightOffset = offset;
        }
      }
    }

    private void GuiLightsList(Gui gui, ref float x, ref float y) {
      const float listHeight = 210.0f;

      bool guiEnabled = GUI.enabled;
      GUI.enabled = !Core.IsInGarage;

      if (gui.Button(ref x, ref y, "ADD LIGHTS TO", Skin.Button)) {
        allowPick_ = !allowPick_;
        Core.ShowCars = allowPick_;
      }

      gui.BeginScrollV(ref x, ref y, listHeight, clListScrollH_, ref clListScroll_, $"LIGHTS {carLights_.Count}");

      float sx = x;
      float sy = y;
      const float offset = Gui.ScrollBarWidth / 2.0f;
      bool scrollVisible = clListScrollH_ > listHeight;
      float width = scrollVisible ? Gui.WidthScroll - offset : Gui.WidthScroll + offset;
      foreach (var cl in carLights_) {
        if (cl != null) {
          bool active = activeLights_ == cl;
          if (gui.ScrollViewButton(ref sx, ref sy, width, Gui.Height, $"{cl.UserName}", out bool delPressed, active ? Skin.ButtonActive : Skin.Button, Skin.RedSkin)) {
            if (delPressed) {
              if (cl == activeLights_) {
                activeLights_ = null;
              }
              cl.Dispose();
              carLights_.Remove(cl);
              break;
            }
            activeLights_ = cl;
          }
        }
      }

      clListScrollH_ = gui.EndScrollV(ref x, ref y, sx, sy);
      y += Gui.OffsetSmall;

      GUI.enabled = guiEnabled;
    }

    private void EnableLightsOnOnwCar() {
      var l = lightsConfig_.GetLights(Core.PlayerCar.Id);
      if (l == null) {
        l = CreateLights(Core.PlayerCar, lightsConfig_);
      }
      else {
        l.Attach(Core.PlayerCar, "OWN_CAR");
        Log.Write($"[KN_Lights]: Car lights for own car '{l.CarId}' attached");
      }

      int index = carLights_.FindIndex(cl => cl.Car == Core.PlayerCar);
      if (index != -1) {
        carLights_[index] = l;
      }
      else {
        carLights_.Add(l);
      }
      activeLights_ = l;
    }

    private void EnableLightsOn(TFCar car) {
      var lights = nwLightsConfig_.GetLights(car.Id, car.Name);
      if (lights == null) {
        lights = CreateLights(car, nwLightsConfig_);
      }
      else {
        lights.Attach(car, car.Name);
        Log.Write($"[KN_Lights]: Car lights for '{lights.CarId}' attached");
      }

      int index = carLights_.FindIndex(cl => cl.Car == car);
      if (index != -1) {
        carLights_[index] = lights;
      }
      else {
        carLights_.Add(lights);
      }
      activeLights_ = lights;
    }

    private static CarLights CreateLights(TFCar car, LightsConfigBase config) {
      var light = new CarLights {
        Pitch = 0.0f,
        PitchTail = 0.0f,
        HeadLightBrightness = 1500.0f,
        HeadLightAngle = 100.0f,
        TailLightBrightness = 80.0f,
        TailLightAngle = 170.0f,
        IsHeadLightLeftEnabled = true,
        IsHeadLightRightEnabled = true,
        HeadlightOffset = new Vector3(0.6f, 0.6f, 1.9f),
        IsTailLightLeftEnabled = true,
        IsTailLightRightEnabled = true,
        TailLightOffset = new Vector3(0.6f, 0.6f, -1.6f)
      };

      light.Attach(car, car.Name);
      config.AddLights(light);
      Log.Write($"[KN_Lights]: New car lights created for '{light.CarId}'");

      return light;
    }
  }
}