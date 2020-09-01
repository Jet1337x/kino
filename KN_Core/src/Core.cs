using System.Collections.Generic;
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
    public const float GuiXLeft = 25.0f;
    public const float GuiYTop = 25.0f;

    public static Core CoreInstance { get; private set; }

    public KnConfig KnConfig { get; }

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
        KnConfig.Set("hide_cx_ui", hideCxUi_);
      }
    }

    public GameObject MainCamera { get; private set; }
    public GameObject ActiveCamera { get; set; }

    public CarPicker CarPicker { get; }
    public TFCar PlayerCar => CarPicker.PlayerCar;
    public List<TFCar> Cars => CarPicker.Cars;

    public ColorPicker ColorPicker { get; }

    public Settings Settings { get; }

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

    public Udp Udp { get; }

    private static Assembly assembly_;

    public Core() {
      KillFlyMod();

      CoreInstance = this;

      Patcher.Hook();

      assembly_ = Assembly.GetExecutingAssembly();

      KnConfig = new KnConfig();

      gui_ = new Gui();

      Timeline = new Timeline(this);

      CarPicker = new CarPicker();
      ColorPicker = new ColorPicker();

      mods_ = new List<BaseMod>();
      tabs_ = new List<string>();

      Settings = new Settings(this, KnConfig.Version);
      AddMod(Settings);
      AddMod(new About(this, KnConfig.Version));

      Udp = new Udp(Settings);
      Udp.ProcessPacket += HandlePacket;
    }

    public void AddMod(BaseMod mod) {
      if (mod.Version != KnConfig.Version) {
        return;
      }

      if (mod.Name == "CINEMATIC") {
        Log.Write($"[KN_Core]: Cinematic module is currently disabled.");
        return;
      }

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
      KnConfig.Read();
      Skin.LoadAll();

      hideCxUi_ = KnConfig.Get<bool>("hide_cx_ui");

      Settings.Awake();
    }

    private void OnDestroy() {
      KnConfig.Set("hide_cx_ui", hideCxUi_);
      foreach (var mod in mods_) {
        mod.OnStop();
      }
      KnConfig.Write();
    }

    public void FixedUpdate() {
      foreach (var mod in mods_) {
        mod.FixedUpdate(selectedModId_);
      }
    }

    private void Update() {
      CarPicker.Update();

      if (MainCamera == null) {
        ActiveCamera = null;
        SetMainCamera(true);
      }
      if (ActiveCamera == null && MainCamera != null) {
        ActiveCamera = MainCamera.gameObject;
      }

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

#if false
      if (Input.GetKeyDown(KeyCode.Delete)) {
        Udp.ReloadClient = true;
        Udp.ReloadSubRoom = true;

        foreach (var mod in mods_) {
          mod.OnReloadAll();
        }
      }
#endif

      Timeline.Update();

      foreach (var mod in mods_) {
        mod.Update(selectedModId_);
      }

      Udp.Update();
    }

    public void LateUpdate() {
      foreach (var mod in mods_) {
        mod.LateUpdate(selectedModId_);
      }

      HideNames();
    }

    public void OnGUI() {
      Settings.GuiTachometer(mods_[selectedTab_].WantsHideUi());

      if (!IsGuiEnabled) {
        return;
      }

      float x = GuiYTop;
      float y = GuiXLeft;

      bool forceSwitchTab = gui_.Button(ref x, ref y, Gui.Width, Gui.TabButtonHeight, "KINO v" + KnConfig.StringVersion, Skin.ButtonDummy);

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

      if (ColorPicker.IsPicking) {
        if (CarPicker.IsPicking) {
          tx += Gui.OffsetGuiX;
        }
        ColorPicker.OnGui(gui_, ref tx, ref ty);
      }
      CarPicker.OnGUI(gui_, ref tx, ref ty);

      mods_[selectedTab_].GuiPickers(selectedModId_, gui_, ref tx, ref ty);

      if (DrawTimeline) {
        Timeline.OnGUI(gui_);
      }
    }

    private void GuiRenderCheck() {
      if (Controls.KeyDown("gui")) {
        IsGuiEnabled = !IsGuiEnabled;

        CarPicker.Reset();
        ColorPicker.Reset();
        mods_[selectedTabPrev_].ResetPickers();
      }
    }

    private void HandleTabSelection() {
      if (selectedTab_ != selectedTabPrev_) {
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
      if (!Settings.ReceiveUdp) {
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