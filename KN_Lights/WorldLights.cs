using System.Collections.Generic;
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

    private bool enabled_;

    private bool fogEnabled_;
    private Fog fog_;

    private GameObject sun_;
    private Light sunLight_;
    private HDAdditionalLightData sunLightHd_;

    private SkySettings staticSky_;
    private MonoBehaviour staticSkyBeh_;

    private bool defaultLoaded_;
    private float fogDistanceDefault_;
    private float fogDepthDefault_;
    private float sunBrightnessDefault_;
    private float skyExposureDefault_;
    private float ambientLightDefault_;

    private bool dataLoaded_;
    private WorldLightsData data_;
    private readonly WorldLightsData defaultData_;
    private readonly List<WorldLightsData> allData_;

    public WorldLights(Core core) {
      core_ = core;
      allData_ = new List<WorldLightsData>();
      data_ = new WorldLightsData();
      defaultData_ = new WorldLightsData();
    }

    public void OnStart() {
      if (WorldLightsDataSerializer.Deserialize(WorldLightsData.ConfigFile, out var data)) {
        Log.Write($"[KN_Lights]: World lights loaded {data.Count} items");
        allData_.AddRange(data);
      }
    }

    public void OnStop() {
      WorldLightsDataSerializer.Serialize(allData_, WorldLightsData.ConfigFile);
    }

    public void Update() {
      if (!core_.IsInGarage) {
        UpdateMap();
      }
      else {
        enabled_ = false;
        volume_ = null;
        sky_ = null;
        fog_ = null;
        sun_ = null;
        sunLight_ = null;
        sunLightHd_ = null;
        staticSky_ = null;
        staticSkyBeh_ = null;
        fogEnabled_ = false;
        data_ = defaultData_;
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

      string text = enabled_ ? "DISABLE" : "ENABLE";
      if (gui.Button(ref x, ref y, width, height, text, enabled_ ? Skin.ButtonActive : Skin.Button)) {
        ToggleLights();
      }

      GUI.enabled = fogOk && enabled_;
      if (gui.Button(ref x, ref y, width, height, "FOG", fogEnabled_ ? Skin.ButtonActive : Skin.Button)) {
        fogEnabled_ = !fogEnabled_;
        fog_.meanFreePath.Override(fogEnabled_ ? data_.FogDistance : fogDistanceDefault_);
        fog_.depthExtent.Override(fogEnabled_ ? data_.FogVolume : fogDepthDefault_);
        if (fogEnabled_) {
          fog_.enableVolumetricFog.Override(data_.FogVolume > 2.0f);
        }
        else {
          fog_.enableVolumetricFog.Override(false);
        }
      }

      GUI.enabled = fogEnabled_ && enabled_;
      if (gui.SliderH(ref x, ref y, width, ref data_.FogDistance, 5.0f, fogDistanceDefault_, $"FOG DISTANCE: {data_.FogDistance:F1}")) {
        if (fogOk) {
          fog_.meanFreePath.Override(data_.FogDistance);
        }
      }
      if (gui.SliderH(ref x, ref y, width, ref data_.FogVolume, 1.0f, 100.0f, $"FOG VOLUME: {data_.FogVolume:F1}")) {
        if (fogOk) {
          fog_.depthExtent.Override(data_.FogVolume);
          fog_.enableVolumetricFog.Override(data_.FogVolume > 2.0f);
        }
      }

      GUI.enabled = sunOk && enabled_;
      if (gui.SliderH(ref x, ref y, width, ref data_.SunBrightness, 0.0f, 50.0f, $"SUNLIGHT BRIGHTNESS: {data_.SunBrightness:F1}")) {
        if (sunOk) {
          sunLight_.intensity = data_.SunBrightness;
        }
      }

      GUI.enabled = skyOk && enabled_;
      if (gui.SliderH(ref x, ref y, width, ref data_.SkyExposure, -5.0f, 10.0f, $"SKYBOX EXPOSURE: {data_.SkyExposure:F1}")) {
        if (skyOk) {
          sky_.exposure.Override(data_.SkyExposure);
        }
      }

      GUI.enabled = staticSkyOk && enabled_;
      if (gui.SliderH(ref x, ref y, width, ref data_.AmbientLight, -1.0f, 5.0f, $"AMBIENT LIGHT: {data_.AmbientLight:F1}")) {
        if (staticSkyOk) {
          staticSky_.exposure.Override(data_.AmbientLight);
          typeof(SkySettings).GetField("m_SkySettings", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(staticSkyBeh_, staticSky_);
        }
      }

      GUI.enabled = hdLightOk && enabled_;
      if (gui.SliderH(ref x, ref y, width, ref data_.SunTemp, 1500.0f, 20000.0f, $"COLOR TEMPERATURE: {data_.SunTemp:F1}")) {
        if (hdLightOk) {
          sunLightHd_.EnableColorTemperature(true);
          sunLightHd_.SetColor(Color.white, data_.SunTemp);
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
            data_.FogDistance = fog_.meanFreePath.value;
            data_.FogVolume = fog_.depthExtent.value;
          }
        }

        defaultLoaded_ = false;
        dataLoaded_ = false;
      }

      if (sun_ == null) {
        sun_ = GameObject.Find("sunlight");
        if (sun_ != null) {
          sunLight_ = sun_.GetComponent<Light>();
          sunLightHd_ = sun_.GetComponent<HDAdditionalLightData>();
        }

        defaultLoaded_ = false;
        dataLoaded_ = false;
      }

      if (staticSky_ == null && map_ != null) {
        var components = map_.GetComponents<MonoBehaviour>();
        if (components != null) {
          foreach (var c in components) {
            var type = c.GetType();
            if (type.Name == "StaticLightingSky") {
              staticSkyBeh_ = c;
              staticSky_ = type.GetField("m_SkySettings", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(c) as SkySettings;
              break;
            }
          }
        }

        defaultLoaded_ = false;
        dataLoaded_ = false;
      }

      if (map_ != null) {
        SelectMap(map_.name);
      }
      SaveDefault();
    }

    private void ToggleLights() {
      enabled_ = !enabled_;

      bool fogOk = fog_ != null;
      bool sunOk = sunLight_ != null;
      bool skyOk = sky_ != null;
      bool staticSkyOk = staticSky_ != null;
      bool hdLightOk = sunLightHd_ != null;

      if (enabled_) {
        if (sunOk) {
          sunLight_.intensity = data_.SunBrightness;
        }
        if (skyOk) {
          sky_.exposure.Override(data_.SkyExposure);
        }
        if (staticSkyOk) {
          staticSky_.exposure.Override(data_.AmbientLight);
        }
        if (hdLightOk) {
          sunLightHd_.EnableColorTemperature(true);
          sunLightHd_.SetColor(Color.white, data_.SunTemp);
        }
      }
      else {
        if (fogOk) {
          fog_.meanFreePath.Override(fogDistanceDefault_);
          fog_.depthExtent.Override(fogDepthDefault_);
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
    }

    private void SaveDefault() {
      if (defaultLoaded_) {
        return;
      }

      if (fog_ != null) {
        fogDistanceDefault_ = fog_.meanFreePath.value;
        fogDepthDefault_ = fog_.depthExtent.value;
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

    private void SelectMap(string map) {
      if (dataLoaded_) {
        return;
      }

      int index = allData_.FindIndex(wd => wd.Map == map);
      if (index != -1) {
        data_ = allData_[index];
        Log.Write($"[KN_Lights]: World lights loaded for map '{map}'");
      }
      else {
        allData_.Add(new WorldLightsData(map));
        data_ = allData_.Last();

        data_.FogDistance = fog_ != null ? fog_.meanFreePath.value : 0.0f;
        data_.FogVolume = fog_ != null ? fog_.depthExtent.value : 0.0f;
        data_.SunBrightness = sunLight_ != null ? sunLight_.intensity : 0.0f;
        data_.SkyExposure = sky_ != null ? sky_.exposure.value : 0.0f;
        data_.AmbientLight = staticSky_ != null ? staticSky_.exposure.value : 0.0f;

        Log.Write($"[KN_Lights]: World lights created for map '{map}'");
      }

      enabled_ = false;
      fogEnabled_ = false;

      dataLoaded_ = true;
    }
  }
}