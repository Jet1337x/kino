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

    private Renderer renderer_;

    private CarLights activeLights_;
    private readonly List<CarLights> carLights_;
    private readonly List<CarLights> carLightsToRemove_;

    private bool allowPick_;
    private float clListScrollH_;
    private Vector2 clListScroll_;

    public Lights(Core core) : base(core, "LIGHTS", 4) {
      var front = new GameObject("KN_LightsFront");
      renderer_ = front.GetComponent<Renderer>();

      carLights_ = new List<CarLights>();
      carLightsToRemove_ = new List<CarLights>();
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

      float yBegin = y;

      const float width = Gui.Width * 2.0f;
      const float height = Gui.Height;

#if KN_DEV_TOOLS
      if (gui.Button(ref x, ref y, width, height, "DEV SAVE", Skin.Button)) {
        carLightsDev_.AddLights(activeLights_);
        Log.Write($"[KN_Lights]: Dev save / saved for '{activeLights_.CarId}'");
      }
#endif

      if (gui.Button(ref x, ref y, width, height, "ENABLE LIGHTS", Skin.Button)) {
        EnableLightsOnOnwCar();
      }

      GuiHeadLights(gui, ref x, ref y, width, height);

      gui.Line(x, y, Core.GuiTabsWidth - Gui.OffsetSmall * 2.0f, 1.0f, Skin.SeparatorColor);
      y += Gui.OffsetY;

      GuiTailLights(gui, ref x, ref y, width, height);

      y = yBegin;
      x += width + Gui.OffsetGuiX;

      GuiLightsList(gui, ref x, ref y);
    }

    private void GuiHeadLights(Gui gui, ref float x, ref float y, float width, float height) {
      float hlPitch = activeLights_?.Pitch ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref hlPitch, -20.0f, 20.0f, $"HEADLIGHTS PITCH: {hlPitch:F}")) {
        if (activeLights_ != null) {
          activeLights_.Pitch = hlPitch;
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
      float tlPitch = activeLights_?.PitchTail ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref tlPitch, -20.0f, 20.0f, $"TAILLIGHTS PITCH: {tlPitch:F1}")) {
        if (activeLights_ != null) {
          activeLights_.PitchTail = tlPitch;
        }
      }

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
      const float listHeight = 340.0f;

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