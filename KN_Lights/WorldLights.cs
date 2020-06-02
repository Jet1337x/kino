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

    private GameObject sun_;
    private Light sunLight_;
    private HDAdditionalLightData sunLightHd_;


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
        fogEnabled_ = false;
        fogDistance_ = 0.0f;
      }
    }

    public void OnGUI(Gui gui, ref float x, ref float y) {
      bool guiEnabled = GUI.enabled;

      GUI.enabled = fog_ != null;
      if (gui.Button(ref x, ref y, "FOG", fogEnabled_ ? Skin.ButtonActive : Skin.Button)) {
        fogEnabled_ = !fogEnabled_;
        fog_.meanFreePath.Override(fogEnabled_ ? fogDistance_ : fogDistanceDefault_);
      }

      GUI.enabled = fogEnabled_;
      if (gui.SliderH(ref x, ref y, ref fogDistance_, 5.0f, fogDistanceDefault_, $"FOG DISTANCE: {fogDistance_:F1}")) {
        if (fog_ != null) {
          fog_.meanFreePath.Override(fogDistance_);
        }
      }

      GUI.enabled = guiEnabled;
    }

    private void UpdateMap() {
      if (map_ == null) {
        switch (SceneManager.GetActiveScene().name) {
          case "Bathurst":
            map_ = GameObject.Find("bathurst");
            break;
          case "Ebisu":
            map_ = GameObject.Find("fiorano");
            break;
          case "Fiorano2":
            map_ = GameObject.Find("fiorano");
            break;
          case "Irwindale":
            map_ = GameObject.Find("fiorano");
            break;
          case "Japan":
            map_ = GameObject.Find("japan");
            break;
          case "LosAngeles":
            map_ = GameObject.Find("losAngeles");
            break;
          case "Parking":
            map_ = GameObject.Find("parking");
            break;
          case "Petersburg":
            map_ = GameObject.Find("petersburg");
            break;
          case "RedRing":
            map_ = GameObject.Find("redring");
            break;
          case "RedRing_winter":
            map_ = GameObject.Find("fiorano");
            break;
          case "RedRock":
            map_ = GameObject.Find("fiorano");
            break;
          case "SilverStone":
            map_ = GameObject.Find("silverstone");
            break;
          case "Winterfell":
            map_ = GameObject.Find("winterfell");
            break;
          default:
            map_ = GameObject.Find("fiorano");
            break;
        }

        if (map_ != null) {
          volume_ = map_.GetComponent<Volume>();
          volume_.profile.TryGet(out sky_);
          volume_.profile.TryGet(out fog_);
          if (fog_ != null) {
            fogDistanceDefault_ = fog_.meanFreePath.value;
            fogDistance_ = fogDistanceDefault_;
          }
        }
      }

      if (sun_ == null) {
        sun_ = GameObject.Find("sunlight");
        if (sun_ != null) {
          sunLight_ = sun_.GetComponent<Light>();
          sunLightHd_ = sun_.GetComponent<HDAdditionalLightData>();
        }
      }
    }
  }
}