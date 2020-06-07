using SyncMultiplayer;
using UnityEngine;

namespace KN_Core.Submodule {
  public class Settings : BaseMod {
    private const float JoinTime = 7.0f;

    private bool rPoints_;
    public bool RPoints {
      get => rPoints_;
      set {
        rPoints_ = value;
        Core.ModConfig.Set("r_points", value);
        GameConsole.Bool["r_points"] = value;
        GameConsole.UpdatePoints();
      }
    }

    private bool hideNames_;
    public bool HideNames {
      get => hideNames_;
      set {
        hideNames_ = value;
        Core.ModConfig.Set("hide_names", value);
      }
    }

    private bool customBackfire_;
    public bool BackFireEnabled {
      get => customBackfire_;
      set {
        customBackfire_ = value;
        Core.ModConfig.Set("custom_backfire", value);
      }
    }

    private int prevCarId_;
    private bool prevScene_;
    private int prevPlayersCount_;

    private readonly Exhaust exhaust_;

    private bool timerStart_;
    private float crutchTimer_;

    public Settings(Core core) : base(core, "SETTINGS", int.MaxValue - 1) {
      exhaust_ = new Exhaust(core);
    }

    public void Awake() {
      RPoints = Core.ModConfig.Get<bool>("r_points");
      HideNames = Core.ModConfig.Get<bool>("hide_names");
      BackFireEnabled = Core.ModConfig.Get<bool>("custom_backfire");
      exhaust_.OnStart();
    }

    public override void OnStop() {
      Core.ModConfig.Set("r_points", RPoints);
      Core.ModConfig.Set("hide_names", HideNames);
      Core.ModConfig.Set("custom_backfire", BackFireEnabled);
      exhaust_.OnStop();
    }

    public override void Update(int id) {
      bool sceneChanged = prevScene_ && !Core.IsInGarage || !prevScene_ && Core.IsInGarage;
      prevScene_ = Core.IsInGarage;

      if (BackFireEnabled) {
        int players = NetworkController.InstanceGame?.Players.Count ?? 0;
        if (prevPlayersCount_ != players || sceneChanged) {
          timerStart_ = true;
        }
        prevPlayersCount_ = players;

        if (timerStart_) {
          crutchTimer_ += Time.deltaTime;
          if (crutchTimer_ > JoinTime) {
            exhaust_.Initialize();
            timerStart_ = false;
            crutchTimer_ = 0.0f;
          }
        }

        if (!TFCar.IsNull(Core.PlayerCar)) {
          int carId = Core.PlayerCar.Id;
          if (prevCarId_ != carId) {
            exhaust_.Initialize();
            prevCarId_ = carId;
          }
        }

        if (Input.GetKeyDown(KeyCode.Delete)) {
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
    }

    public override void OnGUI(int id, Gui gui, ref float x, ref float y) {
      const float width = Gui.Width * 1.4f;
      const float height = Gui.Height;

      x += Gui.OffsetSmall;

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
        Core.ModConfig.Set("custom_backfire", BackFireEnabled);
        if (!BackFireEnabled) {
          exhaust_.Reset();
        }
        else {
          exhaust_.Initialize();
        }
      }

      if (BackFireEnabled) {
        exhaust_.OnGUI(gui, ref x, ref y, width);
      }
      GUI.enabled = guiEnabled;
    }
  }
}