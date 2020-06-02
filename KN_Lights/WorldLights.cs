using System.Reflection;
using KN_Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;

namespace KN_Lights {
  public class WorldLights {
    private readonly Core core_;

    private GameObject map_;
    private Volume volume_;
    private HDRISky sky_;

    private bool fogEnabled_;
    private Fog fog_;
    private float fogDistance_;
    private float fogDistanceDefault_;

    private float sunBrightness_;
    private float sunBrightnessDefault_;
    private float skyExposure_;
    private float skyExposureDefault_;
    private GameObject sun_;
    private Light sunLight_;
    private float sunTemp_;
    private HDAdditionalLightData sunLightHd_;

    private float ambientLight_;
    private float ambientLightDefault_;
    private SkySettings staticSky_;
    private MonoBehaviour staticSkyBeh_;

    private bool defaultLoaded_;

    public WorldLights(Core core) {
      core_ = core;
    }

    public void ResetState() { }

    public void ResetPickers() { }

    public void OnStop() { }

    public void OnStart() { }

    public void Update() {
      if (!core_.IsInGarage) {
        UpdateMap();
      }
      else {
        volume_ = null;
        sky_ = null;
        fog_ = null;
        sun_ = null;
        sunLight_ = null;
        sunLightHd_ = null;
        staticSky_ = null;
        staticSkyBeh_ = null;
        fogEnabled_ = false;
      }
    }

    public void OnGUI(Gui gui, ref float x, ref float y) {
      const float width = Gui.Width * 2.0f;
      const float height = Gui.Height;

      bool guiEnabled = GUI.enabled;

      bool fogOk = fog_ != null;
      bool sunOk = sunLight_ != null;
      bool skyOk = sky_ != null;
      bool staticSkyOk = staticSky_ != null;
      bool hdLightOk = sunLightHd_ != null;

      if (gui.Button(ref x, ref y, width, height, "RESET TO DEFAULT", Skin.Button)) {
        if (fogOk) {
          fog_.meanFreePath.Override(fogDistanceDefault_);
        }
        if (sunOk) {
          sunLight_.intensity = sunBrightnessDefault_;
        }
        if (skyOk) {
          sky_.exposure.Override(skyExposureDefault_);
        }
        if (staticSkyOk) {
          staticSky_.exposure.Override(ambientLightDefault_);
        }
        if (hdLightOk) {
          sunLightHd_.EnableColorTemperature(false);
        }

        fogEnabled_ = false;
      }

      GUI.enabled = fogOk;
      if (gui.Button(ref x, ref y, width, height, "FOG", fogEnabled_ ? Skin.ButtonActive : Skin.Button)) {
        fogEnabled_ = !fogEnabled_;
        fog_.meanFreePath.Override(fogEnabled_ ? fogDistance_ : fogDistanceDefault_);
      }

      GUI.enabled = fogEnabled_;
      if (gui.SliderH(ref x, ref y, width, ref fogDistance_, 5.0f, fogDistanceDefault_, $"FOG DISTANCE: {fogDistance_:F1}")) {
        if (fogOk) {
          fog_.meanFreePath.Override(fogDistance_);
        }
      }
      GUI.enabled = guiEnabled;

      GUI.enabled = sunOk;
      sunBrightness_ = sunOk ? sunLight_.intensity : 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref sunBrightness_, 0.0f, 50.0f, $"SUNLIGHT BRIGHTNESS: {sunBrightness_:F1}")) {
        if (sunOk) {
          sunLight_.intensity = sunBrightness_;
        }
      }

      GUI.enabled = skyOk;
      skyExposure_ = sunOk ? sky_.exposure.value : 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref skyExposure_, 0.0f, 10.0f, $"SKYBOX EXPOSURE: {skyExposure_:F1}")) {
        if (skyOk) {
          sky_.exposure.Override(skyExposure_);
        }
      }

      GUI.enabled = staticSkyOk;
      ambientLight_ = staticSkyOk ? staticSky_.exposure.value : 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref ambientLight_, 0.0f, 5.0f, $"AMBIENT LIGHT: {ambientLight_:F1}")) {
        if (staticSkyOk) {
          staticSky_.exposure.Override(ambientLight_);
          typeof(SkySettings).GetField("m_SkySettings", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(staticSkyBeh_, staticSky_);
        }
      }

      GUI.enabled = hdLightOk;
      if (gui.SliderH(ref x, ref y, width, ref sunTemp_, 100.0f, 20000.0f, $"COLOR TEMPERATURE: {sunTemp_:F1}")) {
        if (hdLightOk) {
          sunLightHd_.EnableColorTemperature(true);
          sunLightHd_.SetColor(Color.white, sunTemp_);
        }
      }

      GUI.enabled = guiEnabled;
    }

    private void UpdateMap() {
      if (map_ == null) {
        switch (SceneManager.GetActiveScene().name) {
          case "Silverstone":
            map_ = GameObject.Find("silverstone");
            break;
          case "Bathurst":
            map_ = GameObject.Find("bathurst");
            break;
          case "Airfield":
            map_ = GameObject.Find("airfield");
            break;
          case "Fiorano2":
            map_ = GameObject.Find("fiorano");
            break;
          case "Parking":
            map_ = GameObject.Find("parking");
            break;
          case "Japan":
            map_ = GameObject.Find("japan");
            break;
          case "Winterfell":
            map_ = GameObject.Find("winterfell");
            break;
          case "LosAngeles":
            map_ = GameObject.Find("losAngeles");
            break;
          case "Ebisu":
            map_ = GameObject.Find("ebisu");
            break;
          case "Petersburg":
            map_ = GameObject.Find("petersburg");
            break;
          case "RedRing":
            map_ = GameObject.Find("redring");
            break;
          case "RedRock":
            map_ = GameObject.Find("redrock");
            break;
          case "Irwindale":
            map_ = GameObject.Find("irwindale");
            break;
          case "RedRing_winter":
            map_ = GameObject.Find("winterfell");
            break;
          default: // Fiorano2
            map_ = GameObject.Find("fiorano");
            break;
        }

        if (map_ != null) {
          volume_ = map_.GetComponent<Volume>();
          volume_.profile.TryGet(out sky_);
          volume_.profile.TryGet(out fog_);
          if (fog_ != null) {
            fogDistance_ = fog_.meanFreePath.value;
          }
        }

        defaultLoaded_ = false;
      }

      if (sun_ == null) {
        sun_ = GameObject.Find("sunlight");
        if (sun_ != null) {
          sunLight_ = sun_.GetComponent<Light>();
          sunLightHd_ = sun_.GetComponent<HDAdditionalLightData>();
        }

        defaultLoaded_ = false;
      }

      if (staticSky_ == null) {
        foreach (var c in map_.GetComponents<MonoBehaviour>()) {
          var type = c.GetType();
          if (type.Name == "StaticLightingSky") {
            staticSkyBeh_ = c;
            staticSky_ = type.GetField("m_SkySettings", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(c) as SkySettings;
            break;
          }
        }

        defaultLoaded_ = false;
      }

      SaveDefault();
    }

    private void SaveDefault() {
      if (defaultLoaded_) {
        return;
      }

      if (fog_ != null) {
        fogDistanceDefault_ = fog_.meanFreePath.value;
      }
      if (sunLight_ != null) {
        sunBrightnessDefault_ = sunLight_.intensity;
      }
      if (sky_ != null) {
        skyExposureDefault_ = sky_.exposure.value;
      }
      if (staticSky_ != null) {
        ambientLightDefault_ = staticSky_.exposure.value;
      }

      defaultLoaded_ = true;
    }
  }
}