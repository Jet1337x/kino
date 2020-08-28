using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using FMODUnity;
using GameInput;
using KN_Core.Submodule;
using SyncMultiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using Settings = KN_Core.Submodule.Settings;

namespace KN_Core {
  [BepInPlugin("trbflxr.kn_0core", "KN_Core", "0.1.1")]
  public class Core : BaseUnityPlugin {
    public static Core CoreInstance { get; private set; }

    public const float GuiXLeft = 25.0f;
    public const float GuiYTop = 25.0f;

    public Config ModConfig { get; }

    public bool DrawTimeline { get; set; }
    public Timeline Timeline { get; }

    public float GuiContentBeginY { get; private set; }
    public float GuiTabsHeight { get; private set; }
    public float GuiTabsWidth { get; private set; }

    private bool hideCxUi_;
    public bool HideCxUi {
      get => hideCxUi_;
      set {
        hideCxUi_ = value;
        ModConfig.Set("hide_cx_ui", hideCxUi_);
      }
    }

    public GameObject MainCamera { get; private set; }
    public GameObject ActiveCamera { get; set; }

    public TFCar PlayerCar { get; private set; }

    private bool isInGaragePrev_;
    public bool IsInGarage { get; private set; }

    private readonly Gui gui_;
    public bool IsGuiEnabled { get; set; }

    public bool IsCheatsEnabled { get; private set; }

    private readonly List<BaseMod> mods_;
    private readonly List<string> tabs_;
    private int selectedTab_;
    private int selectedTabPrev_;
    private int selectedModId_;

    private CameraRotation cameraRotation_;
    private readonly Settings settings_;

    private readonly List<LoadingCar> loadingCars_;

    public delegate void CarLoadCallback();
    public event CarLoadCallback OnCarLoaded;

    public Udp Udp { get; private set; }

    private static Assembly assembly_;

    public Core() {
      CoreInstance = this;

      Patcher.Hook();

      assembly_ = Assembly.GetExecutingAssembly();

      ModConfig = new Config();

      gui_ = new Gui();

      Timeline = new Timeline(this);

      mods_ = new List<BaseMod>();
      tabs_ = new List<string>();

      settings_ = new Settings(this);
      AddMod(settings_);
      AddMod(new About(this));

      Udp = new Udp(settings_);
      Udp.ProcessPacket += HandlePacket;

      loadingCars_ = new List<LoadingCar>(16);
    }

    public void AddMod(BaseMod mod) {
      if (mod.Name == "CHEATS") {
        IsCheatsEnabled = true;
      }

      mods_.Add(mod);
      mods_.Sort((m0, m1) => m0.Id.CompareTo(m1.Id));

      tabs_.Clear();
      foreach (var m in mods_) {
        tabs_.Add(m.Name);
      }

      Log.Write($"[KN_Core]: Mod {mod.Name} was added");

      mod.OnStart();

      selectedModId_ = mods_[selectedTab_].Id;
    }

    private void Awake() {
      ModConfig.Read();
      Skin.LoadAll();

      hideCxUi_ = ModConfig.Get<bool>("hide_cx_ui");

      settings_.Awake();
    }

    private void OnDestroy() {
      ModConfig.Set("hide_cx_ui", hideCxUi_);
      foreach (var mod in mods_) {
        mod.OnStop();
      }
      ModConfig.Write();
    }

    public void FixedUpdate() {
      foreach (var mod in mods_) {
        mod.FixedUpdate(selectedModId_);
      }
    }

    private void Update() {
      if (TFCar.IsNull(PlayerCar)) {
        FindPlayerCar();
      }

      if (MainCamera == null) {
        ActiveCamera = null;
        SetMainCamera(true);
      }
      if (ActiveCamera == null && MainCamera != null) {
        ActiveCamera = MainCamera.gameObject;
      }

      isInGaragePrev_ = IsInGarage;
      IsInGarage = SceneManager.GetActiveScene().name == "SelectCar";

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

      GuiRenderCheck();

      if (Controls.KeyDown("reload_all")) {
        Udp.ReloadClient = true;
        Udp.ReloadSubRoom = true;

        foreach (var mod in mods_) {
          mod.OnReloadAll();
        }
      }

      Timeline.Update();

      loadingCars_.RemoveAll(car => car.Player == null || car.Player.userCar == null);

      var nwPlayers = NetworkController.InstanceGame?.Players;
      if (nwPlayers != null) {
        if (loadingCars_.Count != nwPlayers.Count) {
          foreach (var player in nwPlayers) {
            if (loadingCars_.All(c => c.Player != player)) {
              loadingCars_.Add(new LoadingCar {Player = player});
              Log.Write($"[KN_Core]: Added car to load: {player.NetworkID}");
            }
          }
        }

        foreach (var car in loadingCars_) {
          if (car.Player.IsCarLoading()) {
            car.Loading = true;
          }
          if (!car.Player.IsCarLoading() && car.Loading) {
            car.Loaded = true;
            car.Loading = false;
            Log.Write($"[KN_Core]: Car loaded: {car.Player.NetworkID}");
            OnCarLoaded?.Invoke();
          }
        }

        foreach (var player in nwPlayers.Where(player => player.userCar != null)) {
          player.userCar.SetVisibleUIName(!settings_.HideNames);
        }
      }

      foreach (var mod in mods_) {
        mod.Update(selectedModId_);
      }

      Udp.Update();
    }

    public void LateUpdate() {
      foreach (var mod in mods_) {
        mod.LateUpdate(selectedModId_);
      }

      HideStuff();
    }

    public void OnGUI() {
      settings_.GuiTachometer(mods_[selectedTab_].WantsHideUi());

      if (!IsGuiEnabled) {
        return;
      }

      float x = GuiYTop;
      float y = GuiXLeft;

      bool forceSwitchTab = gui_.Button(ref x, ref y, Gui.Width, Gui.TabButtonHeight, "KINO", Skin.ButtonDummy);

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

      float tx = GuiXLeft + GuiTabsWidth + Gui.OffsetGuiX;
      float ty = GuiContentBeginY - Gui.OffsetY;

      mods_[selectedTab_].GuiPickers(selectedModId_, gui_, ref tx, ref ty);

      if (DrawTimeline) {
        Timeline.OnGUI(gui_);
      }
    }

    private void GuiRenderCheck() {
      if (Controls.KeyDown("gui")) {
        IsGuiEnabled = !IsGuiEnabled;

        mods_[selectedTabPrev_].ResetPickers();
      }
    }

    private void HandleTabSelection() {
      if (selectedTab_ != selectedTabPrev_) {
        mods_[selectedTabPrev_].ResetState();
        selectedModId_ = mods_[selectedTab_].Id;
      }
    }

    private void HideStuff() {
      if (Controls.KeyDown("player_names")) {
        settings_.HideNames = !settings_.HideNames;
      }
    }

    public void ToggleCxUi(bool active) {
      KeepAliveManager.SetUIVisible(active);
      if (IsInGarage) {
        InputManager.instance.SetCursorVisibility(true);
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

    public bool SetMainCamera(bool camEnabled) {
      MainCamera = GameObject.FindGameObjectWithTag(KN_Core.Config.CxMainCameraTag);
      if (MainCamera != null) {
        MainCamera.GetComponent<Camera>().enabled = camEnabled;
        MainCamera.GetComponent<StudioListener>().enabled = camEnabled;
        return true;
      }
      return false;
    }

    private void FindPlayerCar() {
      PlayerCar = null;
      var cars = FindObjectsOfType<RaceCar>();
      if (cars != null && cars.Length > 0) {
        foreach (var c in cars) {
          if (!c.isNetworkCar) {
            PlayerCar = new TFCar(c);
            return;
          }
        }
      }
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
      if (!settings_.ReceiveUdp) {
        return;
      }

      int type = data.Data.GetInt("type");
      if (type == Udp.TypeSuspension) {
        Suspension.Apply(data);
        return;
      }

      foreach (var mod in mods_) {
        mod.OnUdpData(data);
      }
    }
  }
}