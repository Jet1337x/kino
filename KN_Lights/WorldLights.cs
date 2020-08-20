using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KN_Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;

namespace KN_Lights {
  public class WorldLights {
    private readonly Core core_;

    private List<GameObject> map_;
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
    private float fogVolumeDefault_;
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
      map_ = new List<GameObject>();
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

      bool resetMap = map_.Any(m => m == null) || TFCar.IsNull(core_.PlayerCar);
      if (resetMap) {
        map_.Clear();
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
        fog_.depthExtent.Override(fogEnabled_ ? data_.FogVolume : fogVolumeDefault_);
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

      GUI.enabled = enabled_;
      if (gui.Button(ref x, ref y, width, height, "RESET ALL", Skin.Button)) {
        data_.FogDistance = fogDistanceDefault_;
        data_.FogVolume = fogVolumeDefault_;
        data_.SunBrightness = sunBrightnessDefault_;
        data_.SkyExposure = skyExposureDefault_;
        data_.AmbientLight = ambientLightDefault_;
        data_.SunTemp = 6300.0f;

        enabled_ = true;
        ToggleLights();
        enabled_ = false;
      }
      GUI.enabled = guiEnabled;
    }

    private void UpdateMap() {
      if (map_.Count == 0) {
        switch (SceneManager.GetActiveScene().name) {
          case "Silverstone":
            FindMap("silverstone");
            break;
          case "Bathurst":
            FindMap("bathurst");
            break;
          case "Airfield":
            FindMap("airfield");
            break;
          case "Fiorano2":
            FindMap("fiorano");
            break;
          case "Parking":
            FindMap("parking");
            break;
          case "Japan":
            FindMap("japan");
            break;
          case "Winterfell":
            FindMap("winterfell");
            break;
          case "LosAngeles":
            FindMap("losAngeles");
            break;
          case "Ebisu":
            FindMap("ebisu");
            break;
          case "Petersburg":
            FindMap("petersburg");
            break;
          case "RedRing":
            FindMap("redring");
            break;
          case "RedRock":
            FindMap("redrock");
            break;
          case "Irwindale":
            FindMap("irwindale");
            break;
          case "RedRing_winter":
            FindMap("winterfell");
            break;
          case "Atron":
            FindMap("atron");
            break;
          default: // Fiorano2
            FindMap("fiorano");
            break;
        }

        if (map_.Count > 0) {
          foreach (var m in map_) {
            volume_ = m.GetComponent<Volume>();
            if (volume_ != null) {
              volume_.profile.TryGet(out sky_);
              volume_.profile.TryGet(out fog_);
              break;
            }
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

      if (staticSky_ == null && map_.Count > 0) {
        foreach (var m in map_) {
          var components = m.GetComponents<MonoBehaviour>();
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
        }

        defaultLoaded_ = false;
        dataLoaded_ = false;
      }

      if (map_.Count > 0 && map_[0] != null && !TFCar.IsNull(core_.PlayerCar)) {
        SelectMap(map_[0].name);
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
          fog_.depthExtent.Override(fogVolumeDefault_);
          fog_.enableVolumetricFog.Override(false);
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
        fogVolumeDefault_ = fog_.depthExtent.value;
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

    private void FindMap(string name) {
      map_.Clear();

      var tempList = Resources.FindObjectsOfTypeAll(typeof(GameObject));
      foreach (var o in tempList) {
        if (o is GameObject go) {
          if (go.name == name) {
            map_.Add(go);
          }
        }
      }
    }
  }
}