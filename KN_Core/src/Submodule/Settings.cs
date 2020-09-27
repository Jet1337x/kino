using UnityEngine;
using Object = UnityEngine.Object;

namespace KN_Core {
  public class Settings : BaseMod {
    private bool rPoints_;
    public bool RPoints {
      get => rPoints_;
      set {
        rPoints_ = value;
        Core.KnConfig.Set("r_points", value);
        GameConsole.Bool["r_points"] = value;
        GameConsole.UpdatePoints();
      }
    }

    private bool hideNames_;
    public bool HideNames {
      get => hideNames_;
      set {
        hideNames_ = value;
        Core.KnConfig.Set("hide_names", value);
      }
    }

    private bool customBackfire_;
    public bool BackFireEnabled {
      get => customBackfire_;
      set {
        customBackfire_ = value;
        Core.KnConfig.Set("custom_backfire", value);
      }
    }

    private bool syncLights_;
    public bool SyncLights {
      get => syncLights_;
      set {
        syncLights_ = value;
        Core.KnConfig.Set("sync_lights", value);
      }
    }

    public bool LogEngines { get; private set; }

    private readonly DisableConsoles disableConsoles_;
    public bool ConsolesDisabled => disableConsoles_.Disabled;

    private bool tachEnabled_;
    public Tachometer Tachometer { get; }

    private readonly Exhaust exhaust_;

    private Canvas rootCanvas_;

    private bool forceWhiteSmoke_;

    public Settings(Core core, int version, int clientVersion) : base(core, "SETTINGS", int.MaxValue - 1, version, clientVersion) {
      exhaust_ = new Exhaust(core);
      Tachometer = new Tachometer(core);
      disableConsoles_ = new DisableConsoles(Core);
    }

    public void Awake() {
      RPoints = Core.KnConfig.Get<bool>("r_points");
      HideNames = Core.KnConfig.Get<bool>("hide_names");
      BackFireEnabled = Core.KnConfig.Get<bool>("custom_backfire");
      tachEnabled_ = Core.KnConfig.Get<bool>("custom_tach");
      syncLights_ = Core.KnConfig.Get<bool>("sync_lights");
      forceWhiteSmoke_ = Core.KnConfig.Get<bool>("force_white_smoke");

      disableConsoles_.OnStart();
      exhaust_.OnStart();
    }

    public override void OnStop() {
      Core.KnConfig.Set("r_points", RPoints);
      Core.KnConfig.Set("hide_names", HideNames);
      Core.KnConfig.Set("custom_backfire", BackFireEnabled);
      Core.KnConfig.Set("custom_tach", tachEnabled_);
      Core.KnConfig.Set("sync_lights", syncLights_);
      Core.KnConfig.Set("force_white_smoke", forceWhiteSmoke_);

      disableConsoles_.OnStop();
      exhaust_.OnStop();
    }

    public override void OnReloadAll() {
      exhaust_.Initialize();
    }

    protected override void OnCarLoaded() {
      exhaust_.Initialize();

      disableConsoles_.OnCarLoaded();

      if (forceWhiteSmoke_) {
        foreach (var car in Core.Cars) {
          car.Base.SetSmokeColor(Color.white);
        }
      }
    }

    public override void Update(int id) {
      if (Core.IsSceneChanged) {
        Tachometer.Enabled = false;
        rootCanvas_ = null;
      }

      disableConsoles_.Update();

      if (rootCanvas_ == null) {
        var cn = Object.FindObjectsOfType<Canvas>();
        foreach (var c in cn) {
          if (c.name == KnConfig.CxUiCanvasName) {
            rootCanvas_ = c;
            Tachometer.Enabled = !c.enabled;
          }
        }
      }

      if (BackFireEnabled) {
        if (Core.IsCarChanged) {
          exhaust_.Initialize();
        }

#if KN_DEV_TOOLS
        exhaust_.Update();
#else
        if (!Core.IsInGarage) {
          exhaust_.Update();
        }
#endif
      }
      if (tachEnabled_) {
        if (rootCanvas_ != null) {
          Tachometer.Enabled = !rootCanvas_.enabled;
        }
      }
      Tachometer.Update();
    }

    public override void OnGUI(int id, Gui gui, ref float x, ref float y) {
      const float width = Gui.Width * 2.0f;
      const float height = Gui.Height;

      float yBegin = y;

      x += Gui.OffsetSmall;

      if (gui.Button(ref x, ref y, width, height, "CUSTOM TACHOMETER", tachEnabled_ ? Skin.ButtonActive : Skin.Button)) {
        tachEnabled_ = !tachEnabled_;
        Core.KnConfig.Set("custom_tach", tachEnabled_);
        if (!tachEnabled_) {
          Tachometer.Enabled = false;
        }
      }

      gui.Line(x, y, width, 1.0f, Skin.SeparatorColor);
      y += Gui.OffsetY;

      if (gui.Button(ref x, ref y, width, height, "HIDE POINTS", RPoints ? Skin.Button : Skin.ButtonActive)) {
        RPoints = !RPoints;
      }

      if (gui.Button(ref x, ref y, width, height, "HIDE NAMES", HideNames ? Skin.ButtonActive : Skin.Button)) {
        HideNames = !HideNames;
      }

      gui.Line(x, y, width, 1.0f, Skin.SeparatorColor);
      y += Gui.OffsetY;

      bool guiEnabled = GUI.enabled;
      GUI.enabled = !Core.IsInGarage;
      if (gui.Button(ref x, ref y, width, height, "CUSTOM BACKFIRE", BackFireEnabled ? Skin.ButtonActive : Skin.Button)) {
        BackFireEnabled = !BackFireEnabled;
        Core.KnConfig.Set("custom_backfire", BackFireEnabled);
        if (!BackFireEnabled) {
          exhaust_.Reset();
        }
        else {
          exhaust_.Initialize();
        }
      }

      GUI.enabled = guiEnabled;

      if (BackFireEnabled) {
        exhaust_.OnGUI(gui, ref x, ref y, width);
      }

      gui.Line(x, y, width, 1.0f, Skin.SeparatorColor);
      y += Gui.OffsetY;

      if (gui.Button(ref x, ref y, width, height, "FORCE WHITE SMOKE TO ALL", forceWhiteSmoke_ ? Skin.ButtonActive : Skin.Button)) {
        forceWhiteSmoke_ = !forceWhiteSmoke_;
      }

      if (gui.Button(ref x, ref y, width, height, "SYNC LIGHTS", SyncLights ? Skin.ButtonActive : Skin.Button)) {
        SyncLights = !SyncLights;
      }

      gui.Line(x, y, width, 1.0f, Skin.SeparatorColor);
      y += Gui.OffsetY;

      GUI.enabled = !Core.IsCheatsEnabled;

      if (gui.Button(ref x, ref y, width, height, "DISABLE CONSOLE COLLISIONS", disableConsoles_.Disabled ? Skin.ButtonActive : Skin.Button)) {
        disableConsoles_.Disabled = !disableConsoles_.Disabled;
      }

      if (gui.Button(ref x, ref y, width, height, "HIDE CONSOLE PLAYERS", disableConsoles_.Hidden ? Skin.ButtonActive : Skin.Button)) {
        disableConsoles_.Hidden = !disableConsoles_.Hidden;
      }

      GUI.enabled = Core.Swaps.Active;
      GuiSwaps(gui, ref x, ref y, width, yBegin);

      GUI.enabled = guiEnabled;
    }

    private void GuiSwaps(Gui gui, ref float x, ref float y, float width, float yBegin) {
      float tempX = x;
      float tempY = y;

      x += width;
      y = yBegin;

      x += Gui.OffsetGuiX;
      gui.Line(x, y, 1.0f, Core.GuiTabsHeight - Gui.OffsetY * 2.0f, Skin.SeparatorColor);
      x += Gui.OffsetGuiX;

      if (gui.Button(ref x, ref y, width, Gui.Height, "LOG ENGINES", LogEngines ? Skin.ButtonActive : Skin.Button)) {
        LogEngines = !LogEngines;
      }

      Core.Swaps.OnGui(gui, ref x, ref y, width);

      x = tempX;
      y = tempY;
    }

    public void ReloadSound() {
      exhaust_.Initialize();
    }
  }
}