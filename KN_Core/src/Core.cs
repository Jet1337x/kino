using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using CarX;
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

    private readonly List<BaseMod> mods_;
    private readonly List<string> tabs_;
    private int selectedTab_;
    private int selectedTabPrev_;
    private int selectedModId_;

    private CameraRotation cameraRotation_;
    private readonly Settings settings_;

    public CarXUDP Udp { get; private set; }

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

      Udp = new CarXUDP();
      Udp.ProcessPacket += HandlePacket;

      settings_ = new Settings(this);
      AddMod(settings_);
      AddMod(new About(this));
    }

    public void AddMod(BaseMod mod) {
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

      Timeline.Update();

      foreach (var mod in mods_) {
        mod.Update(selectedModId_);
      }

      if (Input.GetKeyDown(KeyCode.P)) {
        Udp.ReloadClient = true;
      }

      Udp.Update();

      foreach (var player in NetworkController.InstanceGame.Players.Where(player => player.userCar != null)) {
        player.userCar.SetVisibleUIName(!settings_.HideNames);
      }
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
      if (type == 0) {
        ApplySuspension(data);
        return;
      }

      foreach (var mod in mods_) {
        mod.OnUdpData(data);
      }
    }

    private void ApplySuspension(SmartfoxDataPackage data) {
      int id = data.Data.GetInt("id");
      float fl = data.Data.GetFloat("fl");
      float fr = data.Data.GetFloat("fr");
      float rl = data.Data.GetFloat("rl");
      float rr = data.Data.GetFloat("rr");

      foreach (var player in NetworkController.InstanceGame.Players) {
        if (player.NetworkID == id) {
          Adjust(player.userCar.carX, fl, fr, rl, rr);
          break;
        }
      }
    }

    private void Adjust(Car car, float fl, float fr, float rl, float rr) {
      var flW = car.GetWheel(WheelIndex.FrontLeft);
      var frW = car.GetWheel(WheelIndex.FrontRight);
      var rlW = car.GetWheel(WheelIndex.RearLeft);
      var rrW = car.GetWheel(WheelIndex.RearRight);

      flW.maxSpringLen = fl;
      frW.maxSpringLen = fr;
      rlW.maxSpringLen = rl;
      rrW.maxSpringLen = rr;
    }

    public static object Call(object o, string methodName, params object[] args) {
      var mi = o.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
      return mi != null ? mi.Invoke(o, args) : null;
    }
  }
}