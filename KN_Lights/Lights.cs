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

    public override void OnStart() {
      LightMask = Core.LoadCoreTexture("HeadLightMask.png");

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
            HeadlightOffsetLeft = new Vector3(0.6f, 0.6f, 1.9f),
            IsHeadLightRightEnabled = true,
            HeadlightOffsetRight = new Vector3(-0.6f, 0.6f, 1.9f),
            IsTailLightLeftEnabled = true,
            TaillightOffsetLeft = new Vector3(0.6f, 0.6f, -1.6f),
            IsTailLightRightEnabled = true,
            TaillightOffsetRight = new Vector3(-0.6f, 0.6f, -1.6f)
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

      float hlPitch = activeLights_?.Pitch ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref hlPitch, -20.0f, 20.0f, $"HEADLIGHTS PITCH: {hlPitch:F1}")) {
        if (activeLights_ != null) {
          activeLights_.Pitch = hlPitch;
        }
      }

      // if (gui.SliderH(ref x, ref y, width, ref offsetX_, -3.0f, 3.0f, $"X: {offsetX_:F1}")) {
      //   var posLeft = headLightLeft_.transform.localPosition;
      //   var posRight = headLightRight_.transform.localPosition;
      //
      //   posLeft.x = offsetX_;
      //   posRight.x = -offsetX_;
      //
      //   headLightLeft_.transform.localPosition = posLeft;
      //   headLightRight_.transform.localPosition = posRight;
      // }
      //
      // if (gui.SliderH(ref x, ref y, width, ref offsetY_, -3.0f, 3.0f, $"Y: {offsetY_:F1}")) {
      //   var posLeft = headLightLeft_.transform.localPosition;
      //   var posRight = headLightRight_.transform.localPosition;
      //
      //   posLeft.y = offsetY_;
      //   posRight.y = offsetY_;
      //
      //   headLightLeft_.transform.localPosition = posLeft;
      //   headLightRight_.transform.localPosition = posRight;
      // }
      //
      // if (gui.SliderH(ref x, ref y, width, ref offsetZ_, -3.0f, 3.0f, $"Z: {offsetZ_:F1}")) {
      //   var posLeft = headLightLeft_.transform.localPosition;
      //   var posRight = headLightRight_.transform.localPosition;
      //
      //   posLeft.z = offsetZ_;
      //   posRight.z = offsetZ_;
      //
      //   headLightLeft_.transform.localPosition = posLeft;
      //   headLightRight_.transform.localPosition = posRight;
      // }
      //
      // if (gui.SliderH(ref x, ref y, width, ref offsetHeading_, -20.0f, 20.0f, $"HEADING: {offsetHeading_:F1}")) {
      //   var rotation = Core.PlayerCar.Transform.rotation;
      //   headLightLeft_.transform.rotation = rotation * Quaternion.AngleAxis(offsetHeading_, Vector3.right);
      //   headLightRight_.transform.rotation = rotation * Quaternion.AngleAxis(offsetHeading_, Vector3.right);
      // }
      //
      // if (gui.SliderH(ref x, ref y, width, ref brightness_, 1000.0f, 20000.0f, $"BRIGHTNESS: {brightness_:F1}")) {
      //   var lightLeft = headLightLeft_.GetComponent<Light>();
      //   var lightRight = headLightRight_.GetComponent<Light>();
      //
      //   lightLeft.intensity = brightness_;
      //   lightRight.intensity = brightness_;
      // }
      //
      // if (gui.SliderH(ref x, ref y, width, ref spotAngle_, 80.0f, 150.0f, $"ANGLE: {spotAngle_:F1}")) {
      //   var lightLeft = headLightLeft_.GetComponent<Light>();
      //   var lightRight = headLightRight_.GetComponent<Light>();
      //
      //   lightLeft.spotAngle = spotAngle_;
      //   lightRight.spotAngle = spotAngle_;
      // }
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
  }
}