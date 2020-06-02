using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using GameInput;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KN_Core {
  [BepInPlugin("trbflxr.kn_0core", "KN_Core", "0.1.1")]
  public class Core : BaseUnityPlugin {
    public static Core CoreInstance { get; private set; }

    public Config ModConfig { get; }

    public bool DrawTimeline { get; set; }
    public Timeline Timeline { get; }
    public Replay Replay { get; }

    public const float GuiXLeft = 25.0f;
    public const float GuiYTop = 25.0f;

    public float GuiContentBeginY { get; private set; }
    public float GuiTabsHeight { get; private set; }
    public float GuiTabsWidth { get; private set; }

    private float carsListHeight_;
    private bool showCars_;
    public bool ShowCars {
      get => showCars_;
      set {
        showCars_ = value;
        if (showCars_) {
          UpdateCars();
        }
      }
    }

    private bool hideCxUi_;
    public bool HideCxUi {
      get => hideCxUi_;
      set {
        hideCxUi_ = value;
        ModConfig.Set("hide_cx_ui", hideCxUi_);
      }
    }

    public TFCar PickedCar { get; set; }
    public TFCar PlayerCar { get; private set; }

    public List<TFCar> Cars { get; }
    public List<TFCar> Ghosts { get; }

    public bool IsInGarage { get; private set; }

    private bool showNames_ = true;
    private bool showNamesToggle_ = true;

    private readonly Gui gui_;
    private bool isGuiEnabled_;

    private readonly Dictionary<string, BaseMod> mods_;
    private readonly List<string> tabs_;
    private int selectedTab_;
    private int selectedTabPrev_;
    private int selectedModId_;

    private bool isInGaragePrev_;

    private CameraRotation cameraRotation_;

    private static Assembly assembly_;

    public Core() {
      CoreInstance = this;

      Patcher.Hook();

      assembly_ = Assembly.GetExecutingAssembly();

      ModConfig = new Config();

      gui_ = new Gui();

      Cars = new List<TFCar>(16);
      Ghosts = new List<TFCar>(16);

      Timeline = new Timeline(this);
      Replay = new Replay(this);

      mods_ = new Dictionary<string, BaseMod>();
      tabs_ = new List<string>();

      AddMod(new About(this));
    }

    public void AddMod(BaseMod mod) {
      bool aboutFound = false;
      var about = new KeyValuePair<string, BaseMod>();
      if (mods_.Count > 0) {
        about = mods_.Last();
        if (about.Key == "ABOUT") {
          mods_.Remove("ABOUT");
          tabs_.RemoveAt(tabs_.Count - 1);
          aboutFound = true;
        }
      }

      mods_.Add(mod.Name, mod);
      tabs_.Add(mod.Name);

      if (aboutFound) {
        mods_.Add(about.Key, about.Value);
        tabs_.Add(about.Key);
      }

      Log.Write($"[KN_Core]: Mod {mod.Name} was added");

      mod.OnStart();

      selectedModId_ = mods_[tabs_[selectedTab_]].Id;
    }

    private void Awake() {
      ModConfig.Read();

      GameConsole.Bool["r_points"] = ModConfig.Get<bool>("r_points");
      GameConsole.UpdatePoints();

      hideCxUi_ = ModConfig.Get<bool>("hide_cx_ui");

      Skin.LoadAll();
    }

    private void OnDestroy() {
      ModConfig.Write();

      ModConfig.Set("r_points", GameConsole.Bool["r_points"]);
      ModConfig.Set("hide_cx_ui", hideCxUi_);

      foreach (var mod in mods_) {
        mod.Value.OnStop();
      }
    }

    public void FixedUpdate() {
      foreach (var mod in mods_) {
        mod.Value.FixedUpdate(selectedModId_);
      }
    }

    private void Update() {
      isInGaragePrev_ = IsInGarage;
      IsInGarage = SceneManager.GetActiveScene().name == "SelectCar";

      if (IsInGarage != isInGaragePrev_) {
        Replay.StopRecord();
      }

      if (IsInGarage && cameraRotation_ == null) {
        cameraRotation_ = FindObjectOfType<CameraRotation>();
      }

      bool captureInput = mods_[tabs_[selectedTab_]].WantsCaptureInput();
      bool lockCameraRot = IsInGarage && mods_[tabs_[selectedTab_]].LockCameraRotation();

      if (isGuiEnabled_ && IsInGarage && cameraRotation_ != null && lockCameraRot) {
        cameraRotation_.Stop();
      }

      if (isGuiEnabled_ && captureInput) {
        if (InputManager.GetLockedInputObject() != this) {
          InputManager.LockInput(this);
        }
      }
      else {
        if (InputManager.GetLockedInputObject() == this) {
          InputManager.LockInput(null);
        }
      }

      if (PlayerCar == null || PlayerCar.Base == null) {
        UpdateCars();
      }

      GuiRenderCheck();

      Timeline.Update();

      foreach (var mod in mods_) {
        mod.Value.Update(selectedModId_);
      }
    }

    public void LateUpdate() {
      foreach (var mod in mods_) {
        mod.Value.LateUpdate(selectedModId_);
      }

      HideStuff();
    }

    public void OnGUI() {
      if (!isGuiEnabled_) {
        ShowCars = false;
        PickedCar = null;
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

      mods_[tabs_[selectedTab_]].OnGUI(selectedModId_, gui_, ref x, ref y);

      gui_.EndTabs(ref x, ref y);
      GuiTabsHeight = gui_.TabsMaxHeight;
      GuiTabsWidth = gui_.TabsMaxWidth;

      float tx = GuiXLeft + GuiTabsWidth + Gui.OffsetGuiX;
      float ty = GuiContentBeginY - Gui.OffsetY;
      if (ShowCars) {
        CarsGui(ref tx, ref ty);
      }

      mods_[tabs_[selectedTab_]].GuiPickers(selectedModId_, gui_, ref tx, ref ty);

      if (DrawTimeline) {
        Timeline.OnGUI(gui_);
      }
    }

    private void CarsGui(ref float x, ref float y) {
      float yBegin = y;

      gui_.Box(x, y, Gui.Width + Gui.OffsetGuiX * 2.0f, Gui.Height, "CARS", Skin.MainContainerDark);
      y += Gui.Height;

      gui_.Box(x, y, Gui.Width + Gui.OffsetGuiX * 2.0f, carsListHeight_, Skin.MainContainer);
      y += Gui.OffsetY;
      x += Gui.OffsetGuiX;

      if (gui_.Button(ref x, ref y, "PLAYER CAR", Skin.Button)) {
        PickedCar = PlayerCar;
        ShowCars = false;
      }

      if (Cars.Count > 0) {
        gui_.Line(x, y, Gui.Width, 1.0f, Skin.SeparatorColor);
        y += Gui.OffsetY;

        foreach (var c in Cars) {
          if (gui_.Button(ref x, ref y, c.Name, Skin.Button)) {
            PickedCar = c;
            ShowCars = false;
          }
        }
      }

      if (Ghosts.Count > 0) {
        gui_.Line(x, y, Gui.Width, 1.0f, Skin.SeparatorColor);
        y += Gui.OffsetY;

        gui_.Box(x, y, Gui.Width, Gui.Height, "GHOSTS", Skin.MainContainerDark);
        y += Gui.Height + Gui.OffsetY;

        foreach (var c in Ghosts) {
          if (gui_.Button(ref x, ref y, c.Name, Skin.Button)) {
            PickedCar = c;
            ShowCars = false;
          }
        }
      }

      carsListHeight_ = y - yBegin - Gui.Height;
      x += Gui.Width + Gui.OffsetGuiX;
      y = yBegin;
    }

    private void GuiRenderCheck() {
      if (Controls.KeyDown("gui")) {
        isGuiEnabled_ = !isGuiEnabled_;

        Replay.ResetState();
        mods_[tabs_[selectedTabPrev_]].ResetPickers();
      }
    }

    private void HandleTabSelection() {
      if (selectedTab_ != selectedTabPrev_) {
        PickedCar = null;
        ShowCars = false;
        Replay.ResetState();
        mods_[tabs_[selectedTabPrev_]].ResetState();
        selectedModId_ = mods_[tabs_[selectedTab_]].Id;
      }
    }

    private void HideStuff() {
      if (Controls.KeyDown("player_names")) {
        showNames_ = !showNames_;
      }

      //todo: optimize
      if (!showNames_) {
        var allCars = FindObjectsOfType<RaceCar>();
        foreach (var c in allCars) {
          if (c.isNetworkCar) {
            GUICommonNickNames.SetVisibleNick(c, false);
          }
        }

        showNamesToggle_ = true;
      }
      else if (showNamesToggle_) {
        var allCars = FindObjectsOfType<RaceCar>();
        foreach (var c in allCars) {
          if (c.isNetworkCar) {
            GUICommonNickNames.SetVisibleNick(c, true);
          }
        }
        showNamesToggle_ = false;
      }
    }

    public void UpdateCars() {
      PlayerCar = null;
      PickedCar = null;

      Cars.Clear();
      Ghosts.Clear();

      var cars = FindObjectsOfType<RaceCar>();
      if (cars != null && cars.Length > 0) {
        foreach (var c in cars) {
          if (!c.isNetworkCar) {
            PlayerCar = new TFCar(c);
          }
          else {
            Cars.Add(new TFCar(c));
          }
        }
      }

      var ghosts = Replay.Player.players;
      if (ghosts != null && ghosts.Count > 0) {
        for (int i = 0; i < ghosts.Count; i++) {
          string ghostName = $"Ghost_{i}";
          Ghosts.Add(new TFCar(ghostName, ghosts[i].ghostCar));
        }
      }
    }

    public void ToggleCxUi(bool active) {
      foreach (var canvas in FindObjectsOfType<Canvas>()) {
        if (canvas.name == KN_Core.Config.CxUiCanvasName) {
          canvas.enabled = active;
        }
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

    public static object Call(object o, string methodName, params object[] args) {
      var mi = o.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
      return mi != null ? mi.Invoke(o, args) : null;
    }
  }
}