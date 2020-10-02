using System;
using System.IO;
using SyncMultiplayer;
using KN_Core;
using KN_Loader;
using UnityEngine;
#if TEST_MOVE
using VinylSystem;
#endif
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

    private Camera mainCamera_;

    private bool liveryCamEnabled_;

    private float zoom_;
    private float shiftZ_;
    private float shiftY_;

#if TEST_MOVE
    private Livery livery_;
#endif

    public Visuals(Core core, int version, int patch, int clientVersion) : base(core, "visuals", 3, version, patch, clientVersion) { }

    public override void ResetState() {
      liveryCamEnabled_ = false;
    }

    public override bool LockCameraRotation() {
      return liveryCamEnabled_;
    }

    public override void OnStart() {
      zoom_ = Core.KnConfig.Get<float>("vinylcam_zoom");
      shiftY_ = Core.KnConfig.Get<float>("vinylcam_shift_y");
      shiftZ_ = Core.KnConfig.Get<float>("vinylcam_shift_z");
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
      if (gui.Button(ref x, ref y, width, height, Locale.Get("zoom"), liveryCamEnabled_ ? Skin.ButtonActive : Skin.Button)) {
        liveryCamEnabled_ = !liveryCamEnabled_;
      }

      if (gui.SliderH(ref x, ref y, width, ref zoom_, 0.0f, 20.0f, $"{Locale.Get("zoom")}: {zoom_:F1}")) {
        Core.KnConfig.Set("vinylcam_zoom", zoom_);
      }

      if (gui.SliderH(ref x, ref y, width, ref shiftY_, -5.0f, 5.0f, $"{Locale.Get("shift")} Y: {shiftY_:F1}")) {
        Core.KnConfig.Set("vinylcam_shift_y", shiftY_);
      }

      if (gui.SliderH(ref x, ref y, width, ref shiftZ_, -20.0f, 20.0f, $"{Locale.Get("shift")} Z: {shiftZ_:F1}")) {
        Core.KnConfig.Set("vinylcam_shift_z", shiftZ_);
      }

#if TEST_MOVE
      if (gui.Button(ref x, ref y, width, height, "HOOK", livery_ != null ? Skin.ButtonActive : Skin.Button)) {
        if (livery_ == null) {
          if (!KnCar.IsNull(Core.PlayerCar)) {
            livery_ = Core.PlayerCar.Base.carModel.livery;
          }
        }
        else {
          livery_ = null;
        }
      }

      if (gui.Button(ref x, ref y, width, height, "Move Z+", Skin.Button)) {
        MoveLivery(new Vector3(100.0f, 0.0f, 0.0f));
      }

      if (gui.Button(ref x, ref y, width, height, "Move Z-", Skin.Button)) {
        MoveLivery(new Vector3(-100.0f, 0.0f, 0.0f));
      }

      if (gui.Button(ref x, ref y, width, height, "Move Y+", Skin.Button)) {
        MoveLivery(new Vector3(0.0f, 100.0f, 0.0f));
      }

      if (gui.Button(ref x, ref y, width, height, "Move Y-", Skin.Button)) {
        MoveLivery(new Vector3(0.0f, -100.0f, 0.0f));
      }
#endif
    }

    private void GuiVisuals(Gui gui, ref float x, ref float y, float width, float height) {
      if (gui.Button(ref x, ref y, width, height, Locale.Get("save_current"), Skin.Button)) {
        SaveCurrentVisuals();
      }

      if (gui.Button(ref x, ref y, width, height, Locale.Get("load_design"), Skin.Button)) {
        Core.FilePicker.Toggle(KnConfig.VisualsDir);
      }

      bool enabled = GUI.enabled;
      GUI.enabled = carId_ != -1 && carVisuals_ != null && Core.IsInGarage;

      if (gui.Button(ref x, ref y, width, height, $"{Locale.Get("apply_design")} {carName_}", Skin.Button)) {
        selectedCarId_ = carId_;
        ApplyVisuals(selectedCarId_, true);

        RefreshCar();
      }

      if (gui.Button(ref x, ref y, width, height, $"{Locale.Get("add_livery")} {carName_}", Skin.Button)) {
        selectedCarId_ = carId_;
        ApplyVisuals(selectedCarId_, false);

        RefreshCar();
      }

      GUI.enabled = selectedCarId_ != -1 && backupVisuals_ != null && Core.IsInGarage;

      if (gui.Button(ref x, ref y, width, height, Locale.Get("restore_design"), Skin.Button)) {
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

      if (Core.FilePicker.IsPicking) {
        string file = Core.FilePicker.PickedFile;
        if (file != null) {
          Core.FilePicker.Reset();

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

      int count = prefs_.carSettings.GetVinylsInfoForCar(id).GetPresetsList().Count;
      int pid = id + count * 2 << 0x2;
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
      if (prefs_ == null || KnCar.IsNull(Core.PlayerCar) || Core.PlayerCar.Base.metaInfo == null) {
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
          writer.Write(ModLoader.ModVersion);
          writer.Write(name);
          writer.Write(buffer.Length);
          writer.Write(id);
          writer.Write(buffer);

          string fileName = name + "_" + DateTime.Now.ToString("HH.mm.ss") + ".knvis";
          Log.Write($"[KN_Visuals]: Saving visuals to '{KnConfig.VisualsDir + fileName}'");

          using (var fs = File.Open(KnConfig.VisualsDir + fileName, FileMode.Create)) {
            stream.WriteTo(fs);
          }
        }
      }
    }

    private void LoadVisuals(string file) {
      Log.Write($"[KN_Visuals]: Loading file '{file}'...");

      using (var stream = new MemoryStream(File.ReadAllBytes(file))) {
        using (var reader = new BinaryReader(stream)) {
          reader.ReadInt32(); //unused

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

#if TEST_MOVE
    private void MoveLivery(Vector3 direction) {
      if (livery_ != null) {
        var editor = VinylSystemKernel.instance.editor;
        var cam = Camera.main;

        foreach (var l in livery_.layers) {
          var pos = editor.GetWorldSpacePosition(l);
          var ssPos = cam.WorldToScreenPoint(pos);

          editor.SetScreenSpacePositionToLayer(cam, ssPos + direction * Time.deltaTime, l);
        }
        livery_.Invalidate();
      }
    }
#endif
  }
}