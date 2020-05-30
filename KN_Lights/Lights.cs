using KN_Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace KN_Lights {
  public class Lights : BaseMod {

    private GameObject headLight0_;
    // private GameObject headLight1_;
    public Renderer renderer_;

    public Lights(Core core) : base(core, "LIGHTS", 5) {
      var front = new GameObject("KN_LightsFront");
      renderer_ = front.GetComponent<Renderer>();
    }

    public override bool WantsCaptureInput() {
      return true;
    }

    public override void OnGUI(int id, Gui gui, ref float x, ref float y) {
      if (id != Id) {
        return;
      }

      if (gui.Button(ref x, ref y, "SPAWN LIGHTS", Skin.Button)) {
        headLight0_ = new GameObject("KN_HeadLight0");

        var light = headLight0_.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = Color.white;
        light.range = 150f;
        light.intensity = 3000f;
        light.spotAngle = 120f;
        light.innerSpotAngle = 50f;

        light.shadows = LightShadows.Hard;
        // light.lightmapBakeType = LightmapBakeType.Realtime;
        light.shadowResolution = LightShadowResolution.VeryHigh;
        light.cullingMask = -1;
        light.shadowNearPlane = 0.2f;
        light.shadowStrength = 100.0f;

        headLight0_.transform.parent = Core.PlayerCar.Transform;
        headLight0_.transform.rotation = Core.PlayerCar.Transform.rotation;
        headLight0_.transform.localPosition += new Vector3(-0.5f, 0.0f, 0.1f);

        // headLight0_.transform.rotation = Core.PlayerCar.Transform.rotation;
        // headLight0_.transform.position = Core.PlayerCar.Transform.position + new Vector3(0.0f,0.0f,1.5f);

      }
    }

    public override void Update(int id) {
      if (id != Id) {
        return;
      }

    }

    public override void LateUpdate(int id) {
      if (id != Id) {
        return;
      }

      QualitySettings.shadows = ShadowQuality.All;
      QualitySettings.shadowDistance = 150.0f;
      QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
      // Core.PlayerCar.Base.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.On;
      // Core.PlayerCar.Base.GetComponent<MeshRenderer>().receiveShadows = true;
      // Core.PlayerCar.Base.SetShadowVisibility(false);

      // var go = Object.FindObjectsOfType<MeshRenderer>();
      // foreach (var g in go) {
      // }
    }
  }
}