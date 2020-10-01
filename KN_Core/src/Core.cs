using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using FMODUnity;
using GameInput;
using SyncMultiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KN_Core {
  [BepInPlugin("trbflxr.kn_0core", "KN_Core", KnConfig.StringVersion)]
  public class Core : BaseUnityPlugin {
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

    public bool ShowUpdateWarn { get; set; }
    public bool DisplayTextAsId { get; set; }

    public Udp Udp { get; }
    public Settings Settings { get; }
    public KnConfig KnConfig { get; }

    public Swaps Swaps { get; }

    private bool shouldRequestTools_;
    private bool scheduleUpdate_;

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

    private readonly bool badVersion_;
    private readonly int latestVersion_;
    private readonly List<string> changelog_;

    private readonly Timer saveTimer_;

    private CameraRotation cameraRotation_;

    private readonly Gui gui_;

    private static Assembly assembly_;

    public Core() {
      CheckUpdaterLocation();

      Changelog.Initialize();
      latestVersion_ = Changelog.GetVersion();
      changelog_ = Changelog.GetChangelog();

      badVersion_ = KnConfig.ClientVersion != GameVersion.version;
      ShowUpdateWarn = latestVersion_ != 0 && KnConfig.Version < latestVersion_;

      if (ShowUpdateWarn) {
        DownloadNewUpdater();
      }

      shouldRequestTools_ = true;

      AccessValidator.Initialize("aHR0cHM6Ly9naXRodWIuY29tL3RyYmZseHIva2lub19kYXRhL3Jhdy9tYXN0ZXIvZGF0YS5rbmQ=");

      //KillFlyMod();

      CoreInstance = this;

      Patcher.Hook();

      assembly_ = Assembly.GetExecutingAssembly();

      KnConfig = new KnConfig();

      Swaps = new Swaps(this);

      gui_ = new Gui();

      mods_ = new List<BaseMod>();
      tabs_ = new List<string>();

      CarPicker = new CarPicker();
      ColorPicker = new ColorPicker();
      FilePicker = new FilePicker();

      AddMod(new About(this, KnConfig.Version, GameVersion.version, badVersion_));

      if (badVersion_) {
        return;
      }

      Settings = new Settings(this, KnConfig.Version, KnConfig.ClientVersion);
      AddMod(Settings);

      Udp = new Udp();
      Udp.ProcessPacket += HandlePacket;

      CarPicker.OnCarLoaded += Swaps.OnCarLoaded;

      saveTimer_ = new Timer(60.0f);
      saveTimer_.Callback += KnConfig.Write;
    }

    public void AddMod(BaseMod mod) {
      if (badVersion_ && Locale.Get(mod.Name) != Locale.Get("about") ||
          mod.Version != KnConfig.Version ||
          mod.ClientVersion != GameVersion.version) {
        string modName = Locale.Get(mod.Name);
        Log.Write($"[KN_Core]: Mod {modName} outdated!");

        if (modName != "CHEATS" && modName != "AIR") {
          scheduleUpdate_ = true;
          Log.Write("[KN_Core]: Scheduling update.");
        }
        return;
      }

      if (mod.Name == "CHEATS") {
        IsCheatsEnabled = true;
      }

      mods_.Add(mod);
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
      tabs_.Remove(Locale.Get(mod.Name));

      mod.OnStop();

      selectedTab_ = 0;
      selectedModId_ = mods_[selectedTab_].Id;

      Log.Write($"[KN_Core]: Mod {Locale.Get(mod.Name)} was removed");
    }

    private void Awake() {
      KnConfig.Read();
      Skin.LoadAll();

      Locale.Initialize(KnConfig.Get<string>("locale"), this);

      if (badVersion_) {
        return;
      }

      Swaps.OnStart();

      Settings.Awake();
    }

    private void OnDestroy() {
      if (badVersion_) {
        return;
      }

      Swaps.OnStop();

      foreach (var mod in mods_) {
        mod.OnStop();
      }
      KnConfig.Write();

      StartUpdater();
    }

    public void ReloadAll() {
      Udp.ReloadClient = true;
      Udp.ReloadSubRoom = true;

      foreach (var mod in mods_) {
        mod.OnReloadAll();
      }
    }

    private void FixedUpdate() {
      if (badVersion_) {
        return;
      }

      foreach (var mod in mods_) {
        mod.FixedUpdate(selectedModId_);
      }
    }

    private void Update() {
      AccessValidator.Update();

      if (shouldRequestTools_) {
        var status = AccessValidator.IsGranted(1, "KN_Devtools");
        if (status != AccessValidator.Status.Loading) {
          shouldRequestTools_ = false;
        }
        if (status == AccessValidator.Status.Granted) {
          IsDevToolsEnabled = true;
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
        cameraRotation_ = FindObjectOfType<CameraRotation>();
      }

      bool captureInput = mods_[selectedTab_].WantsCaptureInput();
      bool lockCameraRot = IsInGarage && mods_[selectedTab_].LockCameraRotation();

      if (IsGuiEnabled && IsInGarage && cameraRotation_ != null && lockCameraRot) {
        cameraRotation_.Stop();
      }

      if (IsGuiEnabled && captureInput) {
        if (InputManager.GetLockedInputObject() != this) {
          InputManager.LockInput(this);
        }
      }
      else {
        if (InputManager.GetLockedInputObject() == this) {
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

    public void OnGUI() {
      if (!badVersion_) {
        Settings.Tachometer.OnGui(mods_[selectedTab_].WantsHideUi());
      }

      if (!IsGuiEnabled) {
        return;
      }

      if (ShowUpdateWarn) {
        GuiUpdateWarn();
      }

      float x = GuiYTop;
      float y = GuiXLeft;

      GuiTabsWidth = Gui.MinTabsWidth;

      bool forceSwitchTab = gui_.Button(ref x, ref y, Gui.Width, Gui.TabButtonHeight, "KINO v" + KnConfig.StringVersion, badVersion_ ? Skin.ButtonDummyRed : Skin.ButtonDummy);
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
      if (changelog_ != null) {
        changelog += $"{Locale.Get("changes")}:\n";
        foreach (string line in changelog_) {
          height += Gui.Height * 0.75f;
          changelog += $"- {line}\n";
        }
      }
      if (gui_.Button(ref x, ref y, width, height, $"{Locale.Get("outdated0")}: {latestVersion_}!\n{changelog}" + Locale.Get("outdated1"),
        Skin.ButtonDummyRed)) {
        Process.Start("https://discord.gg/FkYYAKb");
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

    //load texture from KN_Core.dll
    public static Texture2D LoadCoreTexture(string name) {
      return LoadTexture(assembly_, "KN_Core", name);
    }

    public static Texture2D LoadTexture(Assembly assembly, string ns, string name) {
      var tex = new Texture2D(4, 4);
      using (var stream = assembly.GetManifestResourceStream(ns + ".Resources." + name)) {
        using (var memoryStream = new MemoryStream()) {
          if (stream != null) {
            stream.CopyTo(memoryStream);
            tex.LoadImage(memoryStream.ToArray());
          }
          else {
            tex = Texture2D.grayTexture;
          }
        }
      }
      return tex;
    }

    public static Stream LoadCoreFile(string name) {
      string file = $"KN_Core.Resources.{name}";
      try {
        return assembly_.GetManifestResourceStream(file);
      }
      catch (Exception e) {
        Log.Write($"[KN_Core]: Unable to load embedded file '{file}', {e.Message}");
      }
      return null;
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

    public static Color32 DecodeColor(int color) {
      return new Color32 {
        a = (byte) ((color >> 24) & 0xff),
        r = (byte) ((color >> 16) & 0xff),
        g = (byte) ((color >> 8) & 0xff),
        b = (byte) (color & 0xff)
      };
    }

    public static int EncodeColor(Color32 color) {
      return (color.a & 0xff) << 24 | (color.r & 0xff) << 16 | (color.g & 0xff) << 8 | (color.b & 0xff);
    }

    public static Texture2D CreateTexture(Color color) {
      var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false) {
        wrapMode = TextureWrapMode.Clamp
      };
      texture.SetPixel(0, 0, color);
      texture.Apply();

      return texture;
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

    public void StartUpdater(bool outdated = false) {
      string updater = Paths.PluginPath + Path.DirectorySeparatorChar + "KN_Updater.exe";

      if (scheduleUpdate_) {
        DownloadNewUpdater();
      }

      string version = scheduleUpdate_ || outdated ? "0.0.0" : KnConfig.StringVersion;
      bool devMode = IsDevToolsEnabled && !badVersion_;
      string args = $"{version} {devMode}";

      Log.Write($"[KN_Core]: Starting updater, current version: {version}, dev mode: {devMode}");
      var proc = Process.Start(updater, args);
      if (proc != null) {
        proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        Log.Write("[KN_Core]: Updater started ...");
      }
      else {
        Log.Write("[KN_Core]: Unable to start updater");
      }
    }

    public void DownloadNewUpdater() {
      var bytes = WebDataLoader.DownloadNewUpdater();

      string updater = Paths.PluginPath + Path.DirectorySeparatorChar + "KN_Updater.exe";

      try {
        using (var memory = new MemoryStream(bytes)) {
          using (var fileStream = File.Open(updater, FileMode.Create)) {
            memory.CopyTo(fileStream);
          }
        }
      }
      catch (Exception e) {
        Log.Write($"[KN_Core]: Failed to save updater to disc, {e.Message}");
      }
    }

    private void CheckUpdaterLocation() {
      string updater = Paths.PluginPath + Path.DirectorySeparatorChar + "KN_Updater.exe";

      if (!File.Exists(updater)) {
        Log.Write($"[KN_Core]: Unable to locate updater at '{updater}'");
        throw new Exception($"[KN_Core]: Unable to locate updater at '{updater}'");
      }
    }

    private void KillFlyMod() {
      const string flyModGuid = "fly.mod.goat";

      if (Chainloader.PluginInfos.ContainsKey(flyModGuid)) {
        var flyMod = Chainloader.PluginInfos[flyModGuid];
        if (flyMod != null) {
          flyMod.Instance.enabled = false;
        }
      }
    }
  }
}