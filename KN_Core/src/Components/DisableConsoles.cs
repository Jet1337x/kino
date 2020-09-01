using System.Collections.Generic;
using GameOverlay;
using SyncMultiplayer;

namespace KN_Core {
  public class DisableConsoles {
    private bool disabled_;
    public bool Disabled {
      get => disabled_;
      set {
        disabled_ = value;
        OnCarLoaded();
      }
    }
    public bool Hidden { get; set; }

    private readonly Timer updateCarsTimer_;
    private readonly List<KnCar> disabledCars_;

    private NetGameCollisionManager collisionManager_;

    private readonly Core core_;

    public DisableConsoles(Core core) {
      core_ = core;

      disabledCars_ = new List<KnCar>(16);

      updateCarsTimer_ = new Timer(10.0f);
      updateCarsTimer_.Callback += OnCarLoaded;
    }

    public void OnStart() {
      if (!core_.IsCheatsEnabled) {
        Disabled = core_.KnConfig.Get<bool>("trash_autodisable");
        Hidden = core_.KnConfig.Get<bool>("trash_autohide");
      }
    }

    public void OnStop() {
      if (!core_.IsCheatsEnabled) {
        core_.KnConfig.Set("trash_autodisable", Disabled);
        core_.KnConfig.Set("trash_autohide", Hidden);
      }
    }

    public void Update() {
      if (core_.IsCheatsEnabled) {
        return;
      }

      if (Hidden || Disabled) {
        updateCarsTimer_.Update();
      }

      if (collisionManager_ == null || core_.IsSceneChanged) {
        collisionManager_ = NetworkController.InstanceGame.systems.Get<NetGameCollisionManager>();
      }

      if (Hidden) {
        foreach (var car in disabledCars_) {
          if (!KnCar.IsNull(car)) {
            var pos = car.CxTransform.position;
            pos.y += 1000.0f;
            car.CxTransform.position = pos;
          }
        }
      }
    }

    public void OnCarLoaded() {
      if (!core_.IsCheatsEnabled) {
        disabledCars_.RemoveAll(KnCar.IsNull);

        if (Disabled) {
          foreach (var car in core_.Cars) {
            if (car != core_.PlayerCar && car.Base.networkPlayer != null && car.Base.networkPlayer.PlayerId.platform != UserPlatform.Id.Steam) {
              if (!disabledCars_.Contains(car)) {
                disabledCars_.Add(car);
                collisionManager_?.MovePlayerToColliderGroup("none", car.Base.networkPlayer);
                core_.Udp.SendChangeRoomId(car.Base.networkPlayer, false);
              }
            }
          }
        }
        else {
          foreach (var car in disabledCars_) {
            collisionManager_?.MovePlayerToColliderGroup("", car.Base.networkPlayer);
            core_.Udp.SendChangeRoomId(car.Base.networkPlayer, true);
          }
          disabledCars_.Clear();
        }
      }
    }
  }
}