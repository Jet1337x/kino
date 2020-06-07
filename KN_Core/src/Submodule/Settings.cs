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
    }

    public override void OnStop() {
      Core.ModConfig.Set("r_points", RPoints);
      Core.ModConfig.Set("hide_names", HideNames);
      Core.ModConfig.Set("custom_backfire", BackFireEnabled);
    }

    public override void Update(int id) {
      bool sceneChanged = prevScene_ && !Core.IsInGarage || !prevScene_ && Core.IsInGarage;
      prevScene_ = Core.IsInGarage;

      Log.Write($"{prevScene_} / {NetworkController.InstanceGame.Players.Count}");

      if (BackFireEnabled) {
        int players = NetworkController.InstanceGame.Players.Count;
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

        if (!Core.IsInGarage) {
          exhaust_.Update();
        }

        if (Input.GetKeyDown(KeyCode.Delete)) {
          exhaust_.Initialize();
        }
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

      if (gui.Button(ref x, ref y, width, height, "CUSTOM BACKFIRE", BackFireEnabled ? Skin.ButtonActive : Skin.Button)) {
        BackFireEnabled = !BackFireEnabled;
        Core.ModConfig.Set("custom_backfire", BackFireEnabled);
        if (!BackFireEnabled) {
          exhaust_.Initialize();
        }
        else {
          exhaust_.Reset();
        }
      }

      if (BackFireEnabled) {
        exhaust_.OnGUI(gui, ref x, ref y, width);
      }
    }
  }
}