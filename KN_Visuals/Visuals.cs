using System;
using System.IO;
using SyncMultiplayer;
using KN_Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KN_Visuals {
  public class Visuals : BaseMod {
    private GamePrefs prefs_;
    private UIGarageContext garage_;

    private string carName_ = string.Empty;
    private int carId_ = -1;
    private int selectedCarId_ = -1;
    private CarVisualManager.VisualSettings carVisuals_;
    private CarVisualManager.VisualSettings backupVisuals_;

    private bool pickingFile_;

    private Camera mainCamera_;

    private bool liveryCamEnabled_;

    private float zoom_;
    private float shiftZ_;
    private float shiftY_;

    public Visuals(Core core) : base(core, "VISUALS", 3) { }

    public override void ResetState() {
      pickingFile_ = false;
      liveryCamEnabled_ = false;
    }

    public override bool LockCameraRotation() {
      return liveryCamEnabled_;
    }

    public override void OnStart() {
      zoom_ = Core.ModConfig.Get<float>("vinylcam_zoom");
      shiftY_ = Core.ModConfig.Get<float>("vinylcam_shift_y");
      shiftZ_ = Core.ModConfig.Get<float>("vinylcam_shift_z");
    }

    public override void OnGUI(int id, Gui gui, ref float x, ref float y) {
      if (id != Id) {
        return;
      }

      const float width = 90.0f * 3.0f + Gui.OffsetSmall * 4.0f;
      const float height = Gui.Height;

      x += Gui.OffsetSmall;

      bool guiEnabled = GUI.enabled;
      GUI.enabled = Core.IsInGarage;

      GuiLivery(gui, ref x, ref y, width, height);

      gui.Line(x, y, Core.GuiTabsWidth - Gui.OffsetSmall * 2.0f, 1.0f, Skin.SeparatorColor);
      y += Gui.OffsetY;

      GuiVisuals(gui, ref x, ref y, width, height);

      GUI.enabled = guiEnabled;
    }

    private void GuiLivery(Gui gui, ref float x, ref float y, float width, float height) {
      string text = liveryCamEnabled_ ? "DISABLE" : "ENABLE";
      if (gui.Button(ref x, ref y, width, height, text, liveryCamEnabled_ ? Skin.ButtonActive : Skin.Button)) {
        liveryCamEnabled_ = !liveryCamEnabled_;
      }

      if (gui.SliderH(ref x, ref y, width, ref zoom_, 0.0f, 20.0f, $"ZOOM: {zoom_:F1}")) {
        Core.ModConfig.Set("vinylcam_zoom", zoom_);
      }

      if (gui.SliderH(ref x, ref y, width, ref shiftY_, -5.0f, 5.0f, $"SHIFT Y: {shiftY_:F1}")) {
        Core.ModConfig.Set("vinylcam_shift_y", shiftY_);
      }

      if (gui.SliderH(ref x, ref y, width, ref shiftZ_, -20.0f, 20.0f, $"SHIFT Z: {shiftZ_:F1}")) {
        Core.ModConfig.Set("vinylcam_shift_z", shiftZ_);
      }
    }

    private void GuiVisuals(Gui gui, ref float x, ref float y, float width, float height) {
      if (gui.Button(ref x, ref y, width, height, "SAVE CURRENT DESIGN", Skin.Button)) {
        SaveCurrentVisuals();
      }

      if (gui.Button(ref x, ref y, width, height, "LOAD DESIGN", Skin.Button)) {
        pickingFile_ = !pickingFile_;
        if (pickingFile_) {
          Core.FilePicker.PickIn(Config.VisualsDir);
        }
        else {
          Core.FilePicker.Reset();
        }
      }

      bool enabled = GUI.enabled;
      GUI.enabled = carId_ != -1 && carVisuals_ != null && Core.IsInGarage;

      if (gui.Button(ref x, ref y, width, height, $"APPLY DESIGN TO {carName_}", Skin.Button)) {
        selectedCarId_ = carId_;
        ApplyVisuals(selectedCarId_, true);

        RefreshCar();
      }

      if (gui.Button(ref x, ref y, width, height, $"ADD LIVERY TO {carName_}", Skin.Button)) {
        selectedCarId_ = carId_;
        ApplyVisuals(selectedCarId_, false);

        RefreshCar();
      }

#if false
      // todo: apply visuals on other cars
      if (gui.Button(ref x, ref y, width, height, "APPLY VISUALS ON", Skin.Button)) {
        selectedCarId_ = 78;
        ApplyVisuals(78);
      }
#endif

      GUI.enabled = selectedCarId_ != -1 && backupVisuals_ != null && Core.IsInGarage;

      if (gui.Button(ref x, ref y, width, height, "RESTORE DESIGN", Skin.Button)) {
        if (prefs_ == null || backupVisuals_ == null || selectedCarId_ == -1) {
          return;
        }

        prefs_.carSettings.SetVisualForCar(selectedCarId_, backupVisuals_.Copy());
        RefreshCar();
        ResetVisuals();
      }

      GUI.enabled = enabled;
    }

    public override void Update(int id) {
      if (id != Id) {
        return;
      }

      if (prefs_ == null) {
        prefs_ = Object.FindObjectOfType<GamePrefs>();
      }

      if (garage_ == null) {
        garage_ = Object.FindObjectOfType<UIGarageContext>();
      }

      if (pickingFile_) {
        if (Core.FilePicker.PickedFile != null) {
          string file = Core.FilePicker.PickedFile;
          Core.FilePicker.PickedFile = null;
          Core.FilePicker.IsPicking = false;
          pickingFile_ = false;

          carId_ = -1;
          carVisuals_ = null;
          selectedCarId_ = -1;
          LoadVisuals(file);
        }
      }
    }

    public override void LateUpdate(int id) {
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

    private void ApplyVisuals(int id, bool full) {
      if (prefs_ == null || carVisuals_ == null || id == -1) {
        return;
      }

      if (full) {
        backupVisuals_ = prefs_.carSettings.GetVisualForCar(id).Copy();
      }

      int pid = prefs_.carSettings.GetVinylsInfoForCar(id).GetPresetsList().Count * 2;
      var vp = new CarVilylsInfo.VinylPreset(pid);
      vp.layers.AddRange(carVisuals_.vinylLayers);
      prefs_.carSettings.GetVinylsInfoForCar(id).AddPreset(vp);

      if (full) {
        var vis = carVisuals_.Copy();
        vis.vinylLayers = vp.layers;
        vis.vinylPresetId = pid;
        prefs_.carSettings.SetVisualForCar(id, vis);
      }
    }

    private void SaveCurrentVisuals() {
      if (prefs_ == null ||
          Core.PlayerCar == null ||
          Core.PlayerCar.Base == null ||
          Core.PlayerCar.Base.metaInfo == null) {
        return;
      }

      int id = Core.PlayerCar.Id;
      var visuals = prefs_.carSettings.GetVisualForCar(id).Copy();
      string name = Core.PlayerCar.Base.metaInfo.identifier;

      if (visuals == null) {
        Log.Write($"[KN_Visuals]: Visuals for car '{name}' is null");
        return;
      }

      SaveVisuals(name, id, visuals);
    }

    private void SaveVisuals(string name, int id, CarVisualManager.VisualSettings visuals) {
      var buffer = CustomTypes.SerializeCarVS(visuals.Copy());

      using (var stream = new MemoryStream()) {
        using (var writer = new BinaryWriter(stream)) {
          writer.Write(name);
          writer.Write(buffer.Length);
          writer.Write(id);
          writer.Write(buffer);

          string fileName = name + "_" + DateTime.Now.ToString("HH.mm.ss") + ".knvis";
          Log.Write($"[KN_Visuals]: Saving visuals to '{Config.VisualsDir + fileName}'");

          using (var fs = File.Open(Config.VisualsDir + fileName, FileMode.Create)) {
            stream.WriteTo(fs);
          }
        }
      }
    }

    private void LoadVisuals(string file) {
      using (var stream = new MemoryStream(File.ReadAllBytes(file))) {
        using (var reader = new BinaryReader(stream)) {
          carName_ = reader.ReadString();
          int size = reader.ReadInt32();
          carId_ = reader.ReadInt32();
          var buffer = reader.ReadBytes(size);
          carVisuals_ = CustomTypes.DeserializeCarVS(buffer);
        }
      }
    }

    private void RefreshCar() {
      if (garage_ == null || selectedCarId_ == -1) {
        return;
      }

      const int id0 = 3;
      const int id1 = 46;
      int id = selectedCarId_ == id0 ? id1 : id0;

      garage_.car3dView.SelectCarById(id);
      garage_.car3dView.SelectCarById(selectedCarId_);
    }

    private void ResetVisuals() {
      selectedCarId_ = -1;
      backupVisuals_ = null;

      carId_ = -1;
      carVisuals_ = null;
      carName_ = string.Empty;
    }
  }
}