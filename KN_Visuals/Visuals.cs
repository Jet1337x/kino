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

    public Visuals(Core core) : base(core, "VISUALS", 4) { }

    public override void ResetState() {
      pickingFile_ = false;
    }

    public override bool WantsCaptureInput() {
      return true;
    }

    public override void OnGUI(int id, Gui gui, ref float x, ref float y) {
      if (id != Id) {
        return;
      }

      const float width = 90.0f * 3.0f + Gui.OffsetSmall * 4.0f;
      const float height = Gui.Height;

      float xBegin = x;
      x += Gui.OffsetSmall;

      if (gui.Button(ref x, ref y, width, height, "SAVE CURRENT VISUALS", Skin.Button)) {
        SaveCurrentVisuals();
      }

      if (gui.Button(ref x, ref y, width, height, "LOAD VISUALS", Skin.Button)) {
        pickingFile_ = !pickingFile_;
        if (pickingFile_) {
          Core.FilePicker.PickIn(Config.VisualsDir);
        }
      }

      bool enabled = GUI.enabled;
      GUI.enabled = carId_ != -1 && carVisuals_ != null;

      if (gui.Button(ref x, ref y, width, height, $"APPLY FULL VISUALS ON {carName_}", Skin.Button)) {
        selectedCarId_ = carId_;
        ApplyVisuals(selectedCarId_, true);

        RefreshCar();
      }

      if (gui.Button(ref x, ref y, width, height, $"ADD STICKERS ONLY ON {carName_}", Skin.Button)) {
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

      GUI.enabled = selectedCarId_ != -1 && backupVisuals_ != null;

      if (gui.Button(ref x, ref y, width, height, "RESTORE VISUALS", Skin.Button)) {
        if (prefs_ == null || backupVisuals_ == null || selectedCarId_ == -1) {
          return;
        }

        prefs_.carSettings.SetVisualForCar(selectedCarId_, backupVisuals_.Copy());
        RefreshCar();
        ResetVisuals();
      }

      GUI.enabled = enabled;

      y += Gui.OffsetSmall;
      x = xBegin;
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

      int id = Core.PlayerCar.Base.metaInfo.id;
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