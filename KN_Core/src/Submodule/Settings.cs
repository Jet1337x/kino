using System.Collections.Generic;
using GameOverlay;
using SyncMultiplayer;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KN_Core.Submodule {
  public class Settings : BaseMod {
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

    private bool receiveUdp_;
    public bool ReceiveUdp {
      get => receiveUdp_;
      set {
        receiveUdp_ = value;
        Core.ModConfig.Set("udp_receive", value);
      }
    }

    private bool syncLights_;
    public bool SyncLights {
      get => syncLights_;
      set {
        syncLights_ = value;
        Core.ModConfig.Set("sync_lights", value);
      }
    }

    private int prevCarId_;
    private bool prevScene_;

    private readonly Exhaust exhaust_;

    private const float OffsetTx = 50.0f;
    private const float OffsetTy = 45.0f;
    private bool tachometerEnabled_;
    private bool tachometerEnabledSettings_;
    private readonly Tachometer tachometer_;

    private Canvas rootCanvas_;

    private bool consolesDisabled_;
    private bool consolesHidden_;
    private readonly CarPicker carPicker_;
    private readonly List<TFCar> disabledCars_;

    private readonly Timer updateCarsTimer_;

    private bool forceWhiteSmoke_;

    private NetGameCollisionManager collisionManager_;

    public Settings(Core core) : base(core, "SETTINGS", int.MaxValue - 1) {
      exhaust_ = new Exhaust(core);
      tachometer_ = new Tachometer(core);

      carPicker_ = new CarPicker(core, true);
      disabledCars_ = new List<TFCar>(16);

      updateCarsTimer_ = new Timer(10.0f);
      updateCarsTimer_.Callback += UpdateDisabledPlayers;
    }

    public void Awake() {
      RPoints = Core.ModConfig.Get<bool>("r_points");
      HideNames = Core.ModConfig.Get<bool>("hide_names");
      BackFireEnabled = Core.ModConfig.Get<bool>("custom_backfire");
      tachometerEnabledSettings_ = Core.ModConfig.Get<bool>("custom_tach");
      receiveUdp_ = Core.ModConfig.Get<bool>("receive_udp");
      syncLights_ = Core.ModConfig.Get<bool>("sync_lights");
      forceWhiteSmoke_ = Core.ModConfig.Get<bool>("force_white_smoke");

      if (!Core.IsCheatsEnabled) {
        consolesDisabled_ = Core.ModConfig.Get<bool>("trash_autodisable");
        consolesHidden_ = Core.ModConfig.Get<bool>("trash_autohide");
      }

      exhaust_.OnStart();
    }

    public override void OnStop() {
      Core.ModConfig.Set("r_points", RPoints);
      Core.ModConfig.Set("hide_names", HideNames);
      Core.ModConfig.Set("custom_backfire", BackFireEnabled);
      Core.ModConfig.Set("custom_tach", tachometerEnabledSettings_);
      Core.ModConfig.Set("receive_udp", receiveUdp_);
      Core.ModConfig.Set("sync_lights", syncLights_);
      Core.ModConfig.Set("force_white_smoke", forceWhiteSmoke_);

      if (!Core.IsCheatsEnabled) {
        Core.ModConfig.Set("trash_autodisable", consolesDisabled_);
        Core.ModConfig.Set("trash_autohide", consolesHidden_);
      }

      exhaust_.OnStop();
    }

    public override void OnReloadAll() {
      exhaust_.Initialize();
    }

    protected override void OnCarLoaded() {
      exhaust_.Initialize();

      UpdateDisabledPlayers();

      if (forceWhiteSmoke_) {
        carPicker_.IsPicking = true;
        carPicker_.IsPicking = false;
        foreach (var car in carPicker_.Cars) {
          car.Base.SetSmokeColor(Color.white);
        }
      }
    }

    public override void Update(int id) {
      bool sceneChanged = prevScene_ && !Core.IsInGarage || !prevScene_ && Core.IsInGarage;
      prevScene_ = Core.IsInGarage;

      if (sceneChanged) {
        tachometerEnabled_ = false;
        rootCanvas_ = null;
      }

      if (collisionManager_ == null || sceneChanged) {
        collisionManager_ = NetworkController.InstanceGame.systems.Get<NetGameCollisionManager>();
      }

      if (rootCanvas_ == null) {
        var cn = Object.FindObjectsOfType<Canvas>();
        foreach (var c in cn) {
          if (c.name == Config.CxUiCanvasName) {
            rootCanvas_ = c;
            tachometerEnabled_ = !c.enabled;
          }
        }
      }

      if (BackFireEnabled) {
        if (!TFCar.IsNull(Core.PlayerCar)) {
          int carId = Core.PlayerCar.Id;
          if (prevCarId_ != carId) {
            exhaust_.Initialize();
            prevCarId_ = carId;
          }
        }

#if KN_DEV_TOOLS
        exhaust_.Update();
#else
        if (!Core.IsInGarage) {
          exhaust_.Update();
        }
#endif
      }
      if (tachometerEnabledSettings_) {
        if (rootCanvas_ != null) {
          tachometerEnabled_ = !rootCanvas_.enabled;
        }
      }
      tachometer_.Update();

      if (consolesHidden_ || consolesDisabled_) {
        updateCarsTimer_.Update();
      }

      if (consolesHidden_) {
        foreach (var car in disabledCars_) {
          if (!TFCar.IsNull(car)) {
            var pos = car.CxTransform.position;
            pos.y += 1000.0f;
            car.CxTransform.position = pos;
          }
        }
      }
    }

    private void UpdateDisabledPlayers() {
      if (!Core.IsCheatsEnabled) {
        disabledCars_.RemoveAll(TFCar.IsNull);

        if (consolesDisabled_) {
          carPicker_.IsPicking = true;
          carPicker_.IsPicking = false;
          foreach (var car in carPicker_.Cars) {
            if (car != Core.PlayerCar && car.Base.networkPlayer != null && car.Base.networkPlayer.PlayerId.platform != UserPlatform.Id.Steam) {
              if (!disabledCars_.Contains(car)) {
                disabledCars_.Add(car);
                collisionManager_?.MovePlayerToColliderGroup("none", car.Base.networkPlayer);
                Core.Udp.SendChangeRoomId(car.Base.networkPlayer, false);
              }
            }
          }
        }
        else {
          foreach (var car in disabledCars_) {
            collisionManager_?.MovePlayerToColliderGroup("", car.Base.networkPlayer);
            Core.Udp.SendChangeRoomId(car.Base.networkPlayer, true);
          }
          disabledCars_.Clear();
        }
      }
    }

    public override void OnGUI(int id, Gui gui, ref float x, ref float y) {
      const float width = Gui.Width * 2.0f;
      const float height = Gui.Height;

      x += Gui.OffsetSmall;

      if (gui.Button(ref x, ref y, width, height, "CUSTOM TACHOMETER", tachometerEnabledSettings_ ? Skin.ButtonActive : Skin.Button)) {
        tachometerEnabledSettings_ = !tachometerEnabledSettings_;
        Core.ModConfig.Set("custom_tach", tachometerEnabledSettings_);
        if (!tachometerEnabledSettings_) {
          tachometerEnabled_ = false;
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
        Core.ModConfig.Set("custom_backfire", BackFireEnabled);
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

      if (gui.Button(ref x, ref y, width, height, "DISABLE CONSOLE COLLISIONS", consolesDisabled_ ? Skin.ButtonActive : Skin.Button)) {
        consolesDisabled_ = !consolesDisabled_;
        UpdateDisabledPlayers();
      }

      if (gui.Button(ref x, ref y, width, height, "HIDE CONSOLE PLAYERS", consolesHidden_ ? Skin.ButtonActive : Skin.Button)) {
        consolesHidden_ = !consolesHidden_;
      }

      GUI.enabled = guiEnabled;
    }

    public void GuiTachometer(bool hideUi) {
      if (!tachometerEnabled_ || hideUi) {
        return;
      }

      tachometer_.OnGUI(Screen.width - (OffsetTx + 290.0f), Screen.height - (OffsetTy + 45.0f));
    }
  }
}