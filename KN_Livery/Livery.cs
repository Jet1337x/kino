using KN_Core;
using UnityEngine;

namespace KN_Livery {
  public class Livery : BaseMod {
    private Camera mainCamera_;

    private bool liveryCamEnabled_;

    private float zoom_;
    private float shiftZ_;
    private float shiftY_;

    public Livery(Core core) : base(core, "LIVERY", 3) { }

    public override void OnStart() {
      zoom_ = Core.ModConfig.Get<float>("vinylcam_zoom");
      shiftY_ = Core.ModConfig.Get<float>("vinylcam_shift_y");
      shiftZ_ = Core.ModConfig.Get<float>("vinylcam_shift_z");
    }

    public override void ResetState() {
      liveryCamEnabled_ = false;
    }

    public override bool LockCameraRotation() {
      return liveryCamEnabled_;
    }

    public override bool WantsCaptureInput() {
      return liveryCamEnabled_;
    }

    public override void OnGUI(int id, Gui gui, ref float x, ref float y) {
      if (id != Id) {
        return;
      }

      const float width = 90.0f * 3.0f + Gui.OffsetSmall * 2.0f;
      const float height = Gui.Height;
      const float boxWidth = width + Gui.OffsetSmall * 2.0f;

      x += Gui.OffsetSmall;

      string text = liveryCamEnabled_ ? "DISABLE" : "ENABLE";
      if (gui.Button(ref x, ref y, boxWidth, height, text, liveryCamEnabled_ ? Skin.ButtonActive : Skin.Button)) {
        liveryCamEnabled_ = !liveryCamEnabled_;
        if (!Core.IsInGarage) {
          liveryCamEnabled_ = false;
        }
      }

      if (gui.SliderH(ref x, ref y, boxWidth, ref zoom_, 0.0f, 20.0f, $"ZOOM: {zoom_:F1}")) {
        Core.ModConfig.Set("vinylcam_zoom", zoom_);
      }

      if (gui.SliderH(ref x, ref y, boxWidth, ref shiftY_, -5.0f, 5.0f, $"SHIFT Y: {shiftY_:F1}")) {
        Core.ModConfig.Set("vinylcam_shift_y", shiftY_);
      }

      if (gui.SliderH(ref x, ref y, boxWidth, ref shiftZ_, -20.0f, 20.0f, $"SHIFT Z: {shiftZ_:F1}")) {
        Core.ModConfig.Set("vinylcam_shift_z", shiftZ_);
      }

      x -= Gui.OffsetSmall;
    }

    public override void LateUpdate(int id) {
      if (id != Id) {
        return;
      }

      if (mainCamera_ == null) {
        if (liveryCamEnabled_) {
          liveryCamEnabled_ = false;
        }
        mainCamera_ = Camera.main;
      }

      if (!liveryCamEnabled_ || mainCamera_ == null) {
        return;
      }

      var transform = mainCamera_.transform;
      var position = transform.position;

      //garage camera offset crutch
      position += transform.forward * zoom_ * Time.deltaTime;
      position.y += shiftY_ * Time.deltaTime;
      position.z += shiftZ_ * Time.deltaTime;

      transform.position = position;
    }
  }
}