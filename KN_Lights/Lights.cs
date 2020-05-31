using System.Reflection;
using KN_Core;
using UnityEngine;

namespace KN_Lights {
  public class Lights : BaseMod {
    public const string LightsConfigFile = "kn_lights.knl";
    public const string NwLightsConfigFile = "kn_nwlights.knl";

    private LightsConfig lightsConfig_;
    private NwLightsConfig nwLightsConfig_;

    private Renderer renderer_;

    public static Texture2D LightMask;

    private CarLights activeLights_;

    public Lights(Core core) : base(core, "LIGHTS", 6) {
      var front = new GameObject("KN_LightsFront");
      renderer_ = front.GetComponent<Renderer>();
    }

    public override bool WantsCaptureInput() {
      return true;
    }

    public override bool LockCameraRotation() {
      return true;
    }

    public override void OnStart() {
      var assembly = Assembly.GetExecutingAssembly();

      LightMask = Core.LoadTexture(assembly, "KN_Lights", "HeadLightMask.png");

      if (LightsConfigSerializer.Deserialize(LightsConfigFile, out var lights)) {
        lightsConfig_ = new LightsConfig(lights);
      }
      else {
        lightsConfig_ = new LightsConfig();
        //todo: load default
      }

    }

    public override void OnStop() {
      if (!LightsConfigSerializer.Serialize(lightsConfig_, LightsConfigFile)) { }
      // if (!LcBase.Serialize(nwLightsConfig_, NwLightsConfigFile)) { }
    }

    public override void OnGUI(int id, Gui gui, ref float x, ref float y) {
      if (id != Id) {
        return;
      }

      const float width = Gui.Width * 2.0f;
      const float height = Gui.Height;

      if (gui.Button(ref x, ref y, width, height, "SPAWN LIGHTS", Skin.Button)) {
        var l = lightsConfig_.GetLights(Core.PlayerCar.Id);
        if (l == null) {
          l = new CarLights {
            Pitch = 0.0f,
            PitchTail = 0.0f,
            HeadLightBrightness = 3000.0f,
            HeadLightAngle = 100.0f,
            TailLightBrightness = 500.0f,
            TailLightAngle = 170.0f,
            IsHeadLightLeftEnabled = true,
            IsHeadLightRightEnabled = true,
            HeadlightOffset = new Vector3(0.6f, 0.6f, 1.9f),
            IsTailLightLeftEnabled = true,
            IsTailLightRightEnabled = true,
            TailLightOffset = new Vector3(0.6f, 0.6f, -1.6f)
          };
          l.Attach(Core.PlayerCar, "own_car");
          lightsConfig_.AddLights(l);
          Log.Write($"[KN_Lights]: New car lights created for '{l.CarId}'");
        }
        else {
          l.Attach(Core.PlayerCar, "own_car");
          Log.Write($"[KN_Lights]: Car lights for '{l.CarId}' attached");
        }

        activeLights_ = l;
      }

      GuiHeadLights(gui, ref x, ref y, width, height);

      gui.Line(x, y, Core.GuiTabsWidth - Gui.OffsetSmall * 2.0f, 1.0f, Skin.SeparatorColor);
      y += Gui.OffsetY;

      GuiTailLights(gui, ref x, ref y, width, height);
    }

    public override void Update(int id) {
      if (id != Id) {
        return;
      }

      if (Core.PlayerCar == null || Core.PlayerCar.Base == null) {
        activeLights_ = null;
      }
    }

    public override void LateUpdate(int id) {
      if (id != Id) {
        return;
      }
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
      if (gui.SliderH(ref x, ref y, width, ref brightness, 100.0f, 1000.0f, $"TAILLIGHTS BRIGHTNESS: {brightness:F1}")) {
        if (activeLights_ != null) {
          activeLights_.TailLightBrightness = brightness;
        }
      }

      float angle = activeLights_?.TailLightAngle ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref angle, 50.0f, 160.0f, $"TAILLIGHTS ANGLE: {angle:F1}")) {
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
  }
}