using KN_Core;
using UnityEngine;
using UnityEngine.Rendering;
using Flare = UnityEngine.Flare;

namespace KN_Lights {
  public class Lights : BaseMod {

    private GameObject headLightLeft_;
    private GameObject headLightRight_;
    private Renderer renderer_;

    private float offsetX_ = 0.6f;
    private float offsetY_ = 0.6f;
    private float offsetZ_ = 1.9f;

    private float offsetHeading_;

    private float brightness_ = 3000.0f;
    private float range_ = 150.0f;
    private float spotAngle_ = 120.0f;
    private float innerAngle_ = 50.0f;

    public Lights(Core core) : base(core, "LIGHTS", 6) {
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

      const float width = Gui.Width * 2.0f;
      const float height = Gui.Height;

      if (gui.Button(ref x, ref y, width, height, "SPAWN LIGHTS", Skin.Button)) {
        headLightLeft_ = MakeLight(Core.PlayerCar, new Vector3(offsetX_, offsetY_, offsetZ_), offsetHeading_, "TN_HeadLightLeft");
        headLightRight_ = MakeLight(Core.PlayerCar, new Vector3(-offsetX_, offsetY_, offsetZ_), offsetHeading_, "TN_HeadLightRight");
      }

      if (gui.SliderH(ref x, ref y, width, ref offsetX_, -3.0f, 3.0f, $"X: {offsetX_:F1}")) {
        var posLeft = headLightLeft_.transform.localPosition;
        var posRight = headLightRight_.transform.localPosition;

        posLeft.x = offsetX_;
        posRight.x = -offsetX_;

        headLightLeft_.transform.localPosition = posLeft;
        headLightRight_.transform.localPosition = posRight;
      }

      if (gui.SliderH(ref x, ref y, width, ref offsetY_, -3.0f, 3.0f, $"Y: {offsetY_:F1}")) {
        var posLeft = headLightLeft_.transform.localPosition;
        var posRight = headLightRight_.transform.localPosition;

        posLeft.y = offsetY_;
        posRight.y = offsetY_;

        headLightLeft_.transform.localPosition = posLeft;
        headLightRight_.transform.localPosition = posRight;
      }

      if (gui.SliderH(ref x, ref y, width, ref offsetZ_, -3.0f, 3.0f, $"Z: {offsetZ_:F1}")) {
        var posLeft = headLightLeft_.transform.localPosition;
        var posRight = headLightRight_.transform.localPosition;

        posLeft.z = offsetZ_;
        posRight.z = offsetZ_;

        headLightLeft_.transform.localPosition = posLeft;
        headLightRight_.transform.localPosition = posRight;
      }

      if (gui.SliderH(ref x, ref y, width, ref offsetHeading_, -20.0f, 20.0f, $"HEADING: {offsetHeading_:F1}")) {
        var rotation = Core.PlayerCar.Transform.rotation;
        headLightLeft_.transform.rotation = rotation * Quaternion.AngleAxis(offsetHeading_, Vector3.right);
        headLightRight_.transform.rotation = rotation * Quaternion.AngleAxis(offsetHeading_, Vector3.right);
      }

      if (gui.SliderH(ref x, ref y, width, ref brightness_, 1000.0f, 20000.0f, $"BRIGHTNESS: {brightness_:F1}")) {
        var lightLeft = headLightLeft_.GetComponent<Light>();
        var lightRight = headLightRight_.GetComponent<Light>();

        lightLeft.intensity = brightness_;
        lightRight.intensity = brightness_;
      }

      if (gui.SliderH(ref x, ref y, width, ref range_, 100.0f, 1000.0f, $"RANGE: {range_:F1}")) {
        var lightLeft = headLightLeft_.GetComponent<Light>();
        var lightRight = headLightRight_.GetComponent<Light>();

        lightLeft.range = range_;
        lightRight.range = range_;
      }

      if (gui.SliderH(ref x, ref y, width, ref spotAngle_, 50.0f, 180.0f, $"SPOT: {spotAngle_:F1}")) {
        var lightLeft = headLightLeft_.GetComponent<Light>();
        var lightRight = headLightRight_.GetComponent<Light>();

        lightLeft.spotAngle = spotAngle_;
        lightRight.spotAngle = spotAngle_;
      }

      if (gui.SliderH(ref x, ref y, width, ref innerAngle_, 10.0f, 180.0f, $"INNER: {innerAngle_:F1}")) {
        var lightLeft = headLightLeft_.GetComponent<Light>();
        var lightRight = headLightRight_.GetComponent<Light>();

        lightLeft.innerSpotAngle = innerAngle_;
        lightRight.innerSpotAngle = innerAngle_;
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
    }

    private GameObject MakeLight(TFCar car, Vector3 offset, float headingOffset, string id) {
      var go = new GameObject(id);

      var light = go.AddComponent<Light>();
      light.type = LightType.Spot;
      light.color = Color.white;
      light.range = range_;
      light.intensity = brightness_;
      light.spotAngle = spotAngle_;
      light.innerSpotAngle = innerAngle_;
      light.cookie = Skin.SpotMask;

      go.transform.parent = car.Transform;
      go.transform.position = car.Transform.position;
      go.transform.rotation = car.Transform.rotation * Quaternion.AngleAxis(headingOffset, Vector3.right);
      go.transform.localPosition += offset;

      return go;
    }
  }
}