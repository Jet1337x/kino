using System.Collections.Generic;
using System.Diagnostics;
using FMODUnity;
using GameInput;
using KN_Loader;
using SyncMultiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace KN_Core {
  public class Core : ICore {
    private const int Version = 126;
    private const int Patch = 2;
    private const int ClientVersion = 272;
    private const string StringVersion = "1.2.6";

    private const float SoundReloadDistance = 70.0f;

    private const float GuiXLeft = 25.0f;
    private const float GuiYTop = 25.0f;

    public static Core CoreInstance { get; private set; }

    public float GuiContentBeginY { get; private set; }
    public float GuiTabsHeight { get; private set; }
    public float GuiTabsWidth { get; private set; }

    public GameObject MainCamera { get; private set; }
    public GameObject ActiveCamera { get; set; }

    public CarPicker CarPicker { get; }
    public KnCar PlayerCar => CarPicker.PlayerCar;
    public IEnumerable<KnCar> Cars => CarPicker.Cars;

    public ColorPicker ColorPicker { get; }
    public FilePicker FilePicker { get; }

    public bool IsInGarage { get; private set; }
    public bool IsInGarageChanged { get; private set; }
    public bool IsSceneChanged { get; private set; }
    public bool IsCarChanged { get; private set; }

    public bool IsGuiEnabled { get; set; }

    public bool IsCheatsEnabled { get; private set; }
    public bool IsDevToolsEnabled { get; private set; }

    public bool DisplayTextAsId { get; set; }

    public Udp Udp { get; }
    public Settings Settings { get; }
    public KnConfig KnConfig { get; }

    public Swaps Swaps { get; }

    private bool shouldRequestTools_;

    private string prevScene_;
    private bool prevInGarage_;
    private int carId_ = -1;

    private readonly List<BaseMod> mods_;
    private readonly List<string> tabs_;
    private int selectedTab_;
    private int selectedTabPrev_;
    private int selectedModId_;

    private bool soundReload_;
    private bool soundReloadNext_;

    private readonly ModLoader loader_;
    private readonly bool badVersion_;
    private readonly bool newPatch_;

    private readonly Timer saveTimer_;

    private CameraRotation cameraRotation_;

    private readonly Gui gui_;

    public Core(ModLoader loader) {
      loader_ = loader;
      badVersion_ = loader_.BadVersion || loader_.LatestVersion != Version || ClientVersion != GameVersion.version;
      newPatch_ = loader_.NewPatch || loader_.LatestPatch != Patch && loader_.LatestVersion == Version;

      Embedded.Initialize();

      shouldRequestTools_ = true;

      AccessValidator.Initialize("aHR0cHM6Ly9naXRodWIuY29tL3RyYmZseHIva2lub19kYXRhL3Jhdy9tYXN0ZXIvZGF0YS5rbmQ=");

      KnConfig = new KnConfig();

      gui_ = new Gui();

      mods_ = new List<BaseMod>();
      tabs_ = new List<string>();

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

      // locale is still loading if about or settings added
      string modName = Locale.Get(mod.Name);
      bool skipMod = false;
      if (mod.Version != loader_.LatestVersion ||
          mod.ClientVersion != GameVersion.version ||
          mod.Patch != loader_.LatestPatch) {
        Log.Write($"[KN_Core]: Mod {modName} outdated!");

        if (modName != "CHEATS" && modName != "AIR" && modName != Locale.Get("extras")) {
          loader_.ForceUpdate = true;
          Log.Write("[KN_Core]: Scheduling update.");
          return;
        }

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

      if (mod.Name == "CHEATS") {
        IsCheatsEnabled = true;
      }

      mods_.Add(mod);
      CarPicker.OnCarLoaded += mod.OnCarLoaded;
      mods_.Sort((m0, m1) => m0.Id.CompareTo(m1.Id));

      UpdateLanguage();

      Log.Write($"[KN_Core]: Mod {Locale.Get(mod.Name)} was added");

      mod.OnStart();

      selectedModId_ = mods_[selectedTab_].Id;
    }

    public void RemoveMod(BaseMod mod) {
      if (mod.Name == "CHEATS") {
        IsCheatsEnabled = false;
      }

      mods_.Remove(mod);
      CarPicker.OnCarLoaded -= mod.OnCarLoaded;
      UpdateLanguage();

      mod.OnStop();

      selectedTab_ = 0;
      selectedModId_ = mods_[selectedTab_].Id;

      Log.Write($"[KN_Core]: Mod {mod.Name} was removed");
    }

    public void OnInit() {
      KnConfig.Read();
      Skin.LoadAll();

      Locale.Initialize(KnConfig.Get<string>("locale"));
      UpdateLanguage();

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

      if (IsInGarage && cameraRotation_ == null) {
        cameraRotation_ = Object.FindObjectOfType<CameraRotation>();
      }

      bool captureInput = mods_[selectedTab_].WantsCaptureInput();
      bool lockCameraRot = IsInGarage && mods_[selectedTab_].LockCameraRotation();

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
        Settings.Tachometer.OnGui(mods_[selectedTab_].WantsHideUi());
      }

      if (!IsGuiEnabled) {
        return;
      }

      if (loader_.ShowUpdateWarn || newPatch_) {
        GuiUpdateWarn();
      }

      float x = GuiYTop;
      float y = GuiXLeft;

      bool forceSwitchTab = gui_.Button(ref x, ref y, Gui.Width, Gui.TabButtonHeight,
        $"KINO v{StringVersion}.{Patch}", badVersion_ ? Skin.ButtonDummyRed : Skin.ButtonDummy);
      y -= Gui.TabButtonHeight + Gui.OffsetY;

      float tempX = x;
      x += Gui.Width + Gui.OffsetGuiX;
      if (gui_.Button(ref x, ref y, Gui.Width, Gui.TabButtonHeight, "DISCORD", badVersion_ ? Skin.ButtonDummyRed : Skin.ButtonDummy)) {
        Process.Start("https://discord.gg/jrMReAB");
      }
      x = tempX;

      selectedTabPrev_ = selectedTab_;
      gui_.Tabs(ref x, ref y, tabs_.ToArray(), ref selectedTab_);

      if (forceSwitchTab) {
        gui_.SelectedTab = tabs_.Count - 1;
        selectedTab_ = tabs_.Count - 1;
        selectedTabPrev_ = 0;
      }

      HandleTabSelection();

      GuiContentBeginY = y;

      mods_[selectedTab_].OnGUI(selectedModId_, gui_, ref x, ref y);

      gui_.EndTabs(ref x, ref y);
      GuiTabsHeight = gui_.TabsMaxHeight;
      GuiTabsWidth = gui_.TabsMaxWidth;

      if (badVersion_) {
        return;
      }

      float tx = GuiXLeft;
      float ty = GuiContentBeginY + GuiTabsHeight - Gui.OffsetY;
      gui_.Button(ref tx, ref ty, GuiTabsWidth, Gui.TabButtonHeight, Locale.Get("input_locked"), Skin.ButtonDummyRed);

      GuiPickers();
    }

    private void GuiPickers() {
      float tx = GuiXLeft + GuiTabsWidth + Gui.OffsetGuiX;
      float ty = GuiContentBeginY - Gui.OffsetY;

      if (CarPicker.IsPicking) {
        CarPicker.OnGui(gui_, ref tx, ref ty);
      }
      if (ColorPicker.IsPicking) {
        ColorPicker.OnGui(gui_, ref tx, ref ty);
      }
      if (FilePicker.IsPicking) {
        FilePicker.OnGui(gui_, ref tx, ref ty);
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
          height += Gui.Height * 0.75f;
          changelog += $"- {line}\n";
        }
      }
      if (gui_.Button(ref x, ref y, width, height, $"{Locale.Get("outdated0")}: {loader_.LatestVersionString}!\n" +
                                                   $"{Locale.Get("outdated1")}\n" +
                                                   $"{changelog}" + Locale.Get("outdated2"), Skin.ButtonDummyRed)) {
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

    private void GuiRenderCheck() {
      if (Controls.KeyDown("gui")) {
        IsGuiEnabled = !IsGuiEnabled;

        if (!badVersion_) {
          CarPicker.Reset();
          ColorPicker.Reset();
          FilePicker.Reset();
          mods_[selectedTabPrev_].ResetPickers();
        }
      }
    }

    private void HandleTabSelection() {
      if (selectedTab_ != selectedTabPrev_) {
        CarPicker.Reset();
        ColorPicker.Reset();
        FilePicker.Reset();
        mods_[selectedTabPrev_].ResetState();
        selectedModId_ = mods_[selectedTab_].Id;
      }
    }

    private void HideNames() {
      if (Controls.KeyDown("player_names")) {
        Settings.HideNames = !Settings.HideNames;
      }

      foreach (var car in CarPicker.Cars) {
        car.Base.SetVisibleUIName(!Settings.HideNames);
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

    public void UpdateLanguage() {
      tabs_.Clear();
      foreach (var m in mods_) {
        tabs_.Add(Locale.Get(m.Name));
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