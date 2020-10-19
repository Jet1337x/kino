using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FMODUnity;
using GameInput;
using KN_Loader;
using SyncMultiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace KN_Core {
  public class Core : ICore {
    public const int Version = 200;
    public const int Patch = 1;
    public const int ClientVersion = 273;
    public const string StringVersion = "2.0.0";

    public const float GuiStartX = 25.0f;
    public const float GuiStartY = 25.0f;

    private const float SoundReloadDistance = 70.0f;

    public static Core CoreInstance { get; private set; }

    public float GuiContentBeginY { get; private set; }

    public GameObject MainCamera { get; private set; }
    public GameObject ActiveCamera { get; set; }

    public CarPicker CarPicker { get; }
    public KnCar PlayerCar => CarPicker.PlayerCar;
    public IEnumerable<KnCar> Cars => CarPicker.Cars;

    public ColorPicker ColorPicker { get; }
    public FilePicker FilePicker { get; }

    public bool IsInGarage { get; private set; }
    public bool IsInLobby { get; private set; }
    public bool IsCarFlipped { get; private set; }
    public bool IsInGarageChanged { get; private set; }
    public bool IsInLobbyChanged { get; private set; }
    public bool IsSceneChanged { get; private set; }
    public bool IsCarChanged { get; private set; }
    public bool IsCarFlippedChanged { get; private set; }

    public bool IsGuiEnabled { get; set; }

    public bool IsCheatsEnabled { get; private set; }
    public bool IsExtrasEnabled { get; private set; }
    public bool IsDevToolsEnabled { get; private set; }

    public bool DisplayTextAsId { get; set; }

    public Udp Udp { get; }
    public Settings Settings { get; }
    public KnConfig KnConfig { get; }

    public Swaps Swaps { get; }

    private readonly List<string> skipPatch_;

    private bool shouldRequestTools_;

    private string prevScene_;
    private bool prevInGarage_;
    private bool prevInLobby_;
    private bool prevFlipped_;
    private int carId_ = -1;

    private readonly List<BaseMod> mods_;
    private int selectedMod_;
    private int prevSelectedMod_;
    private int selectedModId_;

    private bool soundReload_;
    private bool soundReloadNext_;

    private readonly ModLoader loader_;
    private readonly bool badVersion_;
    private readonly bool newPatch_;

    private readonly Timer saveTimer_;

    private CameraRotation cameraRotation_;

    private readonly Gui gui_;
    private int hoveredMod_;
    private float hoveredModY_;

    public Core(ModLoader loader) {
      loader_ = loader;

#if !KN_DEV_TOOLS
      badVersion_ = loader_.BadVersion || loader_.LatestVersion != Version || ClientVersion != GameVersion.version;
      newPatch_ = loader_.NewPatch || loader_.LatestPatch != Patch && loader_.LatestVersion == Version;
#endif

      Embedded.Initialize();

      Skin.LoadAll();

      shouldRequestTools_ = true;

      AccessValidator.Initialize("aHR0cHM6Ly9naXRodWIuY29tL3RyYmZseHIva2lub19kYXRhL3Jhdy9tYXN0ZXIvZGF0YS5rbmQ=");

      skipPatch_ = new List<string>();

      KnConfig = new KnConfig();

      gui_ = new Gui();

      mods_ = new List<BaseMod>();

      CarPicker = new CarPicker();
      ColorPicker = new ColorPicker();
      FilePicker = new FilePicker();

      AddMod(new About(this, loader_.LatestVersion, loader_.LatestPatch, GameVersion.version, badVersion_));

      if (badVersion_) {
        return;
      }

      Settings = new Settings(this, Version, Patch, ClientVersion);
      AddMod(Settings);

      Swaps = new Swaps(this);
      CarPicker.OnCarLoaded += Swaps.OnCarLoaded;

      Udp = new Udp();
      Udp.ProcessPacket += HandlePacket;

      saveTimer_ = new Timer(60.0f);
      saveTimer_.Callback += KnConfig.Write;

      CoreInstance = this;
    }

    public void AddMod(BaseMod mod) {
      if (mod == null) {
        return;
      }

#if !KN_DEV_TOOLS
      // locale is still loading if about or settings added
      string modName = Locale.Get(mod.Name);
      bool skipMod = false;
      if (mod.Version != loader_.LatestVersion ||
          mod.ClientVersion != GameVersion.version ||
          mod.Patch != loader_.LatestPatch) {
        Log.Write($"[KN_Core]: Mod {modName} outdated!");

        bool skipPatch = skipPatch_.Any(m => m == modName);
        if (!skipPatch) {
          loader_.ForceUpdate = true;
          Log.Write("[KN_Core]: Scheduling update.");
          return;
        }
        Log.Write($"[KN_Core]: Skipping patch check for '{modName}'");

        if (mod.Version == loader_.LatestVersion && mod.ClientVersion == GameVersion.version) {
          skipMod = true;
        }
      }
      else {
        skipMod = true;
      }

      if (!skipMod) {
        return;
      }
#endif

      switch (mod.Name) {
        case "CHEATS": {
          IsCheatsEnabled = true;
          break;
        }
        case "extras": {
          IsExtrasEnabled = true;
          break;
        }
      }

      mods_.Add(mod);
      CarPicker.OnCarLoaded += mod.OnCarLoaded;
      mods_.Sort((m0, m1) => m0.Id.CompareTo(m1.Id));

      gui_.UpdateMinModHeight(mods_.Count);

      Log.Write($"[KN_Core]: Mod {Locale.Get(mod.Name)} was added");

      mod.OnStart();

      selectedModId_ = mods_[selectedMod_].Id;
    }

    public void RemoveMod(BaseMod mod) {
      if (mod.Name == "CHEATS") {
        IsCheatsEnabled = false;
      }

      mod.OnStop();
      CarPicker.OnCarLoaded -= mod.OnCarLoaded;

      mods_.Remove(mod);

      selectedMod_ = 0;
      selectedModId_ = mods_[selectedMod_].Id;

      Log.Write($"[KN_Core]: Mod {mod.Name} was removed");
    }

    public void SkipPatch(string modName) {
      if (skipPatch_.All(m => m != modName)) {
        skipPatch_.Add(modName);
      }
    }

    public void OnInit() {
      KnConfig.Read();

      Locale.Initialize(KnConfig.Get<string>("locale"));

      loader_.SaveUpdateLog = KnConfig.Get<bool>("save_updater_log");

      if (badVersion_) {
        return;
      }

      Swaps.OnInit();

      Settings.OnInit();
    }

    public void OnDeinit() {
      if (badVersion_) {
        return;
      }

      Swaps.OnDeinit();

      foreach (var mod in mods_) {
        mod.OnStop();
      }
      KnConfig.Write();
    }

    public void FixedUpdate() {
      if (badVersion_) {
        return;
      }

      foreach (var mod in mods_) {
        mod.FixedUpdate(selectedModId_);
      }
    }

    public void Update() {
      AccessValidator.Update();

      if (shouldRequestTools_) {
        var status = AccessValidator.IsGranted(1, "KN_Devtools");
        if (status != AccessValidator.Status.Loading) {
          shouldRequestTools_ = false;
        }
        if (status == AccessValidator.Status.Granted) {
          // to write update log
          IsDevToolsEnabled = true;
          loader_.DevMode = true;
        }
      }

      GuiRenderCheck();

      bool captureInput = mods_[selectedMod_].WantsCaptureInput();
      bool lockCameraRot = IsInGarage && mods_[selectedMod_].LockCameraRotation();

      if (IsGuiEnabled && IsInGarage && cameraRotation_ != null && lockCameraRot) {
        cameraRotation_.Stop();
      }

      if (IsGuiEnabled && captureInput) {
        if (InputManager.GetLockedInputObject() != loader_) {
          InputManager.LockInput(loader_);
        }
      }
      else {
        if (InputManager.GetLockedInputObject() == loader_) {
          InputManager.LockInput(null);
        }
      }

      if (badVersion_) {
        return;
      }

      CarPicker.Update();

      UpdateCamera();

      string scene = SceneManager.GetActiveScene().name;
      IsInGarage = scene == "SelectCar";
      IsSceneChanged = scene != prevScene_;
      IsInGarageChanged = prevInGarage_ && !IsInGarage || !prevInGarage_ && IsInGarage;
      prevInGarage_ = IsInGarage;
      prevScene_ = scene;

      if (PlayerCar != null) {
        IsCarChanged = PlayerCar.Id != carId_;
        carId_ = PlayerCar.Id;
      }

      IsInLobby = NetworkController.IsActive;
      IsInLobbyChanged = prevInLobby_ && !IsInLobby || !prevInLobby_ && IsInLobby;
      prevInLobby_ = IsInLobby;

      IsCarFlipped = PlayerCar?.Base.flipped ?? false;
      IsCarFlippedChanged = prevFlipped_ && !IsCarFlipped || !prevFlipped_ && IsCarFlipped;
      prevFlipped_ = IsCarFlipped;

      if (IsInGarage && cameraRotation_ == null) {
        cameraRotation_ = Object.FindObjectOfType<CameraRotation>();
      }

      foreach (var mod in mods_) {
        mod.Update(selectedModId_);
      }

      if (soundReloadNext_) {
        soundReloadNext_ = false;
        Swaps.ReloadSound();
        Settings.ReloadSound();
      }

      if (!KnCar.IsNull(PlayerCar)) {
        float distance = Vector3.Distance(MainCamera.transform.position, PlayerCar.Transform.position);
        if (distance > SoundReloadDistance) {
          soundReload_ = true;
        }
        else if (soundReload_ || soundReloadNext_) {
          soundReload_ = false;
          soundReloadNext_ = true;
        }
      }

      if (IsCarFlippedChanged) {
        soundReloadNext_ = true;
      }

      Swaps.Update();

      Udp.Update();

      saveTimer_.Update();
    }

    public void LateUpdate() {
      if (badVersion_) {
        return;
      }

      foreach (var mod in mods_) {
        mod.LateUpdate(selectedModId_);
      }

      HideNames();
    }

    public void OnGui() {
      if (!badVersion_) {
        Settings.Tachometer.OnGui(mods_[selectedMod_].WantsHideUi());
      }

      if (!IsGuiEnabled) {
        return;
      }

      gui_.PreRender();

      if (badVersion_ || newPatch_ || loader_.ShowUpdateWarn || loader_.NewPatch) {
        GuiUpdateWarn();
      }

      float x = GuiStartX;
      float y = GuiStartY;

      HandleModSelection();

      gui_.Begin(x, y);

      GuiVersion(ref x, ref y);
      GuiContentBeginY = y;

      prevSelectedMod_ = selectedMod_;
      GuiModPanel(ref x, ref y);

      GuiModContent(ref x);

      gui_.End();

      GuiInputLocked();

      if (badVersion_) {
        return;
      }

      GuiPickers();

      GuiTooltips();
    }

    private void GuiVersion(ref float x, ref float y) {
      string versionText = $"Kino\nv{StringVersion}.{Patch}";
      gui_.Box(x, y, Gui.ModIconSize, Gui.ModTabHeight, versionText, Skin.ModPanelSkin.Normal);
      y += Gui.ModTabHeight;
    }

    private void GuiModPanel(ref float x, ref float y) {
      float ty = y;

      gui_.Box(x, y, Gui.ModIconSize, gui_.MaxContentHeight, Skin.ModPanelSkin.Normal);

      // tooltip stuff
      hoveredMod_ = -1;
      var mousePos = Input.mousePosition;
      mousePos.y = Screen.height - mousePos.y;

      for (int i = 0; i < mods_.Count; ++i) {
        // mod icon background
        if (gui_.ImageButton(ref x, ref y, selectedMod_ == i ? Skin.ModPanelBackSkin.Active : Skin.ModPanelBackSkin.Normal)) {
          gui_.ResetSize();

          prevSelectedMod_ = selectedMod_;
          selectedMod_ = i;
          selectedModId_ = mods_[i].Id;
          return;
        }

        // tooltip render check
        if (mousePos.x >= x && mousePos.x <= x + Gui.ModIconSize && mousePos.y >= y && mousePos.y <= y + Gui.ModIconSize) {
          hoveredMod_ = i;
          hoveredModY_ = y + Gui.ModIconSize / 2.0f;
        }

        // actual mod icon
        var icon = mods_[i].Icon ?? Skin.PuzzleSkin;
        gui_.ImageButton(ref x, ref y, selectedMod_ == i ? icon.Active : icon.Normal);

        y += Gui.ModIconSize;
      }

      // bottom-most discord button
      y = gui_.MaxContentHeight - Gui.ModIconSize + ty;
      if (gui_.ImageButton(ref x, ref y, Skin.ModPanelBackSkin.Normal)) {
        Process.Start("https://discord.gg/FkYYAKb");
      }
      gui_.ImageButton(ref x, ref y, Skin.DiscordSkin.Normal);

      // discord tooltip render check
      if (hoveredMod_ == -1 && mousePos.x >= x && mousePos.x <= x + Gui.ModIconSize && mousePos.y >= y && mousePos.y <= y + Gui.ModIconSize) {
        hoveredMod_ = int.MaxValue;
        hoveredModY_ = y + Gui.ModIconSize / 2.0f;
      }
    }

    private void GuiModContent(ref float x) {
      x += Gui.ModIconSize;
      float y = GuiStartY;

      gui_.Box(x, y + Gui.ModTabHeight, gui_.MaxContentWidth, gui_.MaxContentHeight, Skin.BackgroundSkin.Normal);

      mods_[selectedMod_].OnGui(gui_, ref x, ref y);
    }

    private void GuiInputLocked() {
      float x = GuiStartX;
      float y = GuiContentBeginY + gui_.MaxContentHeight;
      gui_.TextButton(ref x, ref y, gui_.MaxContentWidth + Gui.ModIconSize, Gui.Height, Locale.Get("input_locked"), Skin.WarningSkin.Normal);
    }

    private void GuiPickers() {
      float x = GuiStartX + gui_.MaxContentWidth + Gui.ModIconSize + Gui.Offset;
      float y = GuiStartY;

      if (CarPicker.IsPicking) {
        CarPicker.OnGui(gui_, ref x, ref y);
        x += Gui.Offset;
      }
      if (ColorPicker.IsPicking) {
        ColorPicker.OnGui(gui_, ref x, ref y);
        x += Gui.Offset;
      }
      if (FilePicker.IsPicking) {
        FilePicker.OnGui(gui_, ref x, ref y);
      }
    }

    private void GuiTooltips() {
      const float tooltipX = GuiStartX + Gui.ModIconSize + Gui.OffsetSmall;
      const float tooltipWidth = 150.0f;
      const float tooltipHeight = 30.0f;

      if (hoveredMod_ != -1) {
        string text = hoveredMod_ == int.MaxValue ? " DISCORD" : $" {Locale.Get(mods_[hoveredMod_].Name)}";
        gui_.Box(tooltipX, hoveredModY_ - tooltipHeight / 2.0f, tooltipWidth, tooltipHeight, text, Skin.TooltipSkin.Normal);
      }
    }

    private void GuiUpdateWarn() {
      const float width = Gui.Width * 3.0f;
      float height = Gui.Height * 3.0f;

      float x = Screen.width / 2.0f - width / 2.0f;
      float y = Screen.height / 2.0f - height / 2.0f;

      string changelog = "";
      if (loader_.Changelog != null) {
        changelog += $"{Locale.Get("changes")}:\n";
        foreach (string line in loader_.Changelog) {
          height += Gui.Height * 0.9f;
          changelog += $"- {line}\n";
        }
      }
      if (gui_.TextButton(ref x, ref y, width, height, $"{Locale.Get("outdated0")}: {loader_.LatestVersionString}!\n" +
                                                       $"{Locale.Get("outdated1")}\n" +
                                                       $"{changelog}" + Locale.Get("outdated2"), Skin.WarningSkin.Normal)) {
        Process.Start("https://discord.gg/FkYYAKb");
      }
    }

    public void ReloadAll() {
      Udp.ReloadClient = true;
      Udp.ReloadSubRoom = true;

      foreach (var mod in mods_) {
        mod.OnReloadAll();
      }
    }

    public void ResetPickers() {
      CarPicker.Reset();
      ColorPicker.Reset();
      FilePicker.Reset();
    }

    private void GuiRenderCheck() {
      if (Controls.KeyDown("gui")) {
        IsGuiEnabled = !IsGuiEnabled;

        if (!badVersion_) {
          mods_[selectedMod_].OnGuiToggle();
          ResetPickers();
        }
      }

#if KN_DEV_TOOLS
      if (Input.GetKeyDown(KeyCode.H)) {
        gui_.RenderWhiteBg = !gui_.RenderWhiteBg;
      }
#endif
    }

    private void HandleModSelection() {
      if (selectedMod_ != prevSelectedMod_) {
        gui_.ResetSize();

        ResetPickers();
        mods_[prevSelectedMod_].ResetState();
        selectedModId_ = mods_[selectedMod_].Id;
      }
    }

    private void HideNames() {
      if (Controls.KeyDown("player_names")) {
        Settings.HideNames = !Settings.HideNames;
      }

      foreach (var car in CarPicker.Cars) {
        if (!KnCar.IsNull(car)) {
          car.Base.SetVisibleUIName(!Settings.HideNames);
        }
      }
    }

    private void UpdateCamera() {
      if (MainCamera == null) {
        ActiveCamera = null;
        SetMainCamera(true);
      }
      if (ActiveCamera == null && MainCamera != null) {
        ActiveCamera = MainCamera.gameObject;
      }
    }

    private bool SetMainCamera(bool camEnabled) {
      MainCamera = GameObject.FindGameObjectWithTag(KnConfig.CxMainCameraTag);
      if (MainCamera != null) {
        MainCamera.GetComponent<Camera>().enabled = camEnabled;
        MainCamera.GetComponent<StudioListener>().enabled = camEnabled;
        return true;
      }
      return false;
    }

    private void HandlePacket(SmartfoxDataPackage data) {
      int type = data.Data.GetInt("type");
      switch (type) {
        case Udp.TypeSuspension: {
          Suspension.Apply(data);
          return;
        }
        case Udp.TypeSwaps: {
          Swaps.OnUdpData(data);
          return;
        }
      }

      foreach (var mod in mods_) {
        mod.OnUdpData(data);
      }
    }
  }
}