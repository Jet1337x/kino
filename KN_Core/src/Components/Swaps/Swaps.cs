using System;
using System.Collections.Generic;
using System.Linq;
using CarX;
using KN_Loader;
using SyncMultiplayer;
using UnityEngine;

namespace KN_Core {
  public class Swaps {
    private const float SoundReloadDistance = 90.0f;

    public bool Active => swapsEnabled_ && dataLoaded_;
    private readonly List<EngineData> engines_;
    private readonly List<EngineBalance> balance_;

    private readonly List<SwapData> allData_;

    private bool swapReload_;
    private SwapData currentSwap_;
    private string currentSound_;
    private float currentEngineTurboMax_;

    private string defaultSoundId_;
    private float defaultFinalDrive_;
    private float defaultClutch_;
    private readonly CarDesc.Engine defaultEngine_;

    private readonly List<NetworkSwap> networkSwaps_;

    private float carListScrollH_;
    private Vector2 carListScroll_;

    private bool swapsEnabled_;
    private bool shouldRequestSwaps_;
    private bool reloadSwap_;

    private readonly bool dataLoaded_;

    private readonly Core core_;

    public Swaps(Core core) {
      core_ = core;

      shouldRequestSwaps_ = true;

      engines_ = new List<EngineData>();
      balance_ = new List<EngineBalance>();
      dataLoaded_ = SwapsLoader.LoadData(ref engines_, ref balance_);
      if (!dataLoaded_) {
        return;
      }

      Log.Write($"[KN_Core::Swaps]: Swaps data successfully loaded from remote, engines: {engines_.Count}, balance: {balance_.Count}");

      allData_ = new List<SwapData>();
      defaultEngine_ = new CarDesc.Engine();

      networkSwaps_ = new List<NetworkSwap>(16);
    }

    public void OnInit() {
      if (!dataLoaded_) {
        return;
      }

      if (DataSerializer.Deserialize<SwapData>("KN_Swaps", KnConfig.BaseDir + SwapData.ConfigFile, out var data)) {
        Log.Write($"[KN_Core::Swaps]: User swap data loaded, items: {data.Count}");
        allData_.AddRange(data.ConvertAll(d => (SwapData) d));
      }
    }

    public void OnDeinit() {
      if (!Active) {
        return;
      }

      DataSerializer.Serialize("KN_Swaps", allData_.ToList<ISerializable>(), KnConfig.BaseDir + SwapData.ConfigFile, Core.Version);
    }

    public void OnCarLoaded(KnCar car) {
      if (!Active) {
        return;
      }

      var nwSwap = new NetworkSwap(car.Base);
      networkSwaps_.Add(nwSwap);

      if (core_.CarPicker.Cars.Count <= 1) {
        FindEngineAndSwap();
      }
      SendSwapData();
    }

    public void Update() {
      if (shouldRequestSwaps_) {
        var status = AccessValidator.IsGranted(4, "KN_Swaps");
        if (status != AccessValidator.Status.Loading) {
          shouldRequestSwaps_ = false;
        }
        if (status == AccessValidator.Status.Granted) {
          swapsEnabled_ = true;
        }
      }

      if (core_.IsCarChanged && !KnCar.IsNull(core_.PlayerCar)) {
        reloadSwap_ = true;
      }

      if (!Active) {
        return;
      }

      networkSwaps_.RemoveAll(s => s == null || s.Car == null);

      if (reloadSwap_ && !KnCar.IsNull(core_.PlayerCar)) {
        reloadSwap_ = false;
        defaultSoundId_ = core_.PlayerCar.Base.metaInfo.name;
        defaultFinalDrive_ = core_.PlayerCar.CarX.finaldrive;
        defaultClutch_ = core_.PlayerCar.CarX.clutchMaxTorque;

        var desc = core_.PlayerCar.Base.GetDesc();
        CopyEngine(desc.carXDesc.engine, defaultEngine_);
        Log.Write("[KN_Core::Swaps]: Car changed storing default engine");

        FindEngineAndSwap();
      }

      if (core_.IsInGarageChanged || core_.IsSceneChanged || core_.IsInLobbyChanged) {
        swapReload_ = true;
      }

      foreach (var swap in networkSwaps_) {
        if (swap.ShouldSent && swap.Car != null) {
          swap.SentTimer += Time.deltaTime;
          if (swap.SentTimer >= NetworkSwap.JoinDelay) {
            swap.SentTimer = 0.0f;
            swap.ShouldSent = false;
            SendSwapData();
          }
        }

        if (swap.ReloadNext) {
          swap.ReloadNext = false;
          SetEngine(swap.Car, swap.Swap, swap.EngineData, swap.NwId, false, core_.Settings.LogEngines);
        }

        if (swap.Car != null) {
          float distance = Vector3.Distance(core_.MainCamera.transform.position, swap.Car.transform.position);
          if (distance > SoundReloadDistance) {
            swap.Reload = true;
          }
          else if (swap.Reload || swap.ReloadNext) {
            swap.Reload = false;
            swap.ReloadNext = true;
          }
        }
      }

      if (swapReload_ && !KnCar.IsNull(core_.PlayerCar)) {
        FindEngineAndSwap();
        swapReload_ = false;
      }
    }

    public void ReloadSound() {
      if (!Active) {
        return;
      }

      var currentEngine = currentSwap_?.GetCurrentEngine();
      if (currentEngine != null && currentEngine.EngineId != 0 && !KnCar.IsNull(core_.PlayerCar)) {
        SetSoundSound(core_.PlayerCar.Base, currentSound_, true);
      }
    }

    public void OnGui(Gui gui, ref float x, ref float y, float width) {
      if (!Active || KnCar.IsNull(core_.PlayerCar)) {
        return;
      }

      if (gui.TextButton(ref x, ref y, width, Gui.Height, Locale.Get("log_engines"), core_.Settings.LogEngines ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
        core_.Settings.LogEngines = !core_.Settings.LogEngines;
      }

      if (gui.TextButton(ref x, ref y, width, Gui.Height, Locale.Get("reload"), Skin.ButtonSkin.Normal)) {
        FindEngineAndSwap();
      }

      const float listHeight = 220.0f;
      const float height = Gui.Height;

      bool allowSwap = core_.IsInGarage || core_.IsCheatsEnabled && core_.IsDevToolsEnabled;

      bool enabled = GUI.enabled;
      GUI.enabled = allowSwap;

      gui.BeginScrollV(ref x, ref y, width, listHeight, carListScrollH_, ref carListScroll_, Locale.Get("swaps_engines"));

      y += Gui.OffsetSmall;

      float sx = x;
      float sy = y;
      const float offset = Gui.ScrollBarWidth;
      bool scrollVisible = carListScrollH_ > listHeight;
      float w = scrollVisible ? width - (offset + Gui.OffsetSmall * 3.0f) : width - Gui.OffsetSmall * 3.0f;

      var currentEngine = currentSwap_?.GetCurrentEngine();
      int engineId = currentEngine?.EngineId ?? 0;
      if (gui.TextButton(ref sx, ref sy, w, height, "STOCK", engineId == 0 ? Skin.ListButtonSkin.Active : Skin.ListButtonSkin.Normal)) {
        if (engineId != 0) {
          SwapEngineTo(null);
        }
      }

      bool carOk = !KnCar.IsNull(core_.PlayerCar);
      foreach (var engine in engines_) {
        bool allowed = carOk && balance_.Any(balance => balance.CarId == core_.PlayerCar.Id && balance.Rating >= engine.Rating);
        if (!core_.IsDevToolsEnabled && (!engine.Enabled || !allowed)) {
          continue;
        }

        if (gui.TextButton(ref sx, ref sy, w, height, engine.Name, engineId == engine.Id ? Skin.ListButtonSkin.Active : Skin.ListButtonSkin.Normal)) {
          if (engineId != engine.Id) {
            SwapEngineTo(engine);
          }
        }
      }
      carListScrollH_ = gui.EndScrollV(ref x, ref y, sy);

      var ce = currentSwap_?.GetCurrentEngine();
      GUI.enabled = allowSwap && engineId != 0;

      float turbo = ce?.Turbo ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref turbo, 0.0f, currentEngineTurboMax_, $"{Locale.Get("swaps_turbo")}: {turbo:F2}")) {
        var desc = core_.PlayerCar.Base.GetDesc();
        desc.carXDesc.engine.turboPressure = turbo;
        core_.PlayerCar.Base.SetDesc(desc);

        if (ce != null) {
          ce.Turbo = turbo;
        }
      }

      GUI.enabled = engineId != 0;
      float finalDrive = ce?.FinalDrive ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref finalDrive, 2.5f, 5.0f, $"{Locale.Get("swaps_fd")}: {finalDrive:F2}")) {
        var desc = core_.PlayerCar.Base.GetDesc();
        desc.carXDesc.gearBox.finalDrive = finalDrive;
        core_.PlayerCar.Base.SetDesc(desc);

        if (ce != null) {
          ce.FinalDrive = finalDrive;
        }
      }
      GUI.enabled = enabled;
    }

    public void OnUdpData(SmartfoxDataPackage data) {
      if (!dataLoaded_) {
        return;
      }

      try {
        int id = data.Data.GetInt("id");
        int engineId = data.Data.GetInt("ei");
        float turbo = data.Data.GetFloat("tb");
        float finalDrive = data.Data.GetFloat("fd");

        foreach (var player in NetworkController.InstanceGame.Players) {
          if (player.NetworkID == id) {
            var engine = GetEngine(engineId);

            var swap = new SwapData.Engine {
              EngineId = engineId,
              FinalDrive = finalDrive,
              Turbo = turbo
            };

            var nwSwap = networkSwaps_.Find(s => s.NwId == id);
            nwSwap?.SetData(swap, engine);

            SetEngine(player.userCar, swap, engine, id, false, core_.Settings.LogEngines);
            break;
          }
        }
      }
      catch (Exception e) {
        Log.Write($"[KN_Core::Swaps]: An error occured while receiving udp data, {e.Message}");
      }
    }

    private void SendSwapData() {
      int id = NetworkController.InstanceGame?.LocalPlayer?.NetworkID ?? -1;
      if (id == -1) {
        return;
      }

      var engine = currentSwap_?.GetCurrentEngine();
      int engineId = 0;
      float turbo = 0.0f;
      float finalDrive = 0.0f;
      if (engine != null) {
        engineId = engine.EngineId;
        turbo = engine.Turbo;
        finalDrive = engine.FinalDrive;
      }

      var nwData = new SmartfoxDataPackage(PacketId.Subroom);
      nwData.Add("1", (byte) 25);
      nwData.Add("type", Udp.TypeSwaps);
      nwData.Add("id", id);
      nwData.Add("ei", engineId);
      nwData.Add("tb", turbo);
      nwData.Add("fd", finalDrive);

      try {
        core_.Udp.Send(nwData);
      }
      catch (Exception e) {
        Log.Write($"[KN_Core::Swaps]: An error occured while sending udp data, {e.Message}");
      }
    }

    private void FindEngineAndSwap() {
      bool found = false;
      foreach (var swap in allData_) {
        if (swap.CarId == core_.PlayerCar.Id) {
          Log.Write($"[KN_Core::Swaps]: Found SwapData for car '{swap.CarId}'");

          var toSwap = swap.GetCurrentEngine();
          currentSwap_ = swap;
          if (toSwap != null) {
            var engine = GetEngine(toSwap.EngineId);
            if (!SetEngine(core_.PlayerCar.Base, toSwap, engine, -1, true, true)) {
              swap.RemoveEngine(toSwap);
            }
          }
          found = true;
          break;
        }
      }

      if (!found) {
        var swap = new SwapData(core_.PlayerCar.Id);
        allData_.Add(swap);
        currentSwap_ = swap;
        Log.Write($"[KN_Core::Swaps]: Created new SwapData for car '{swap.CarId}', swaps: {allData_.Count}");
      }
    }

    private bool SetEngine(RaceCar car, SwapData.Engine swap, EngineData engine, int nwId, bool self, bool log) {
      if (engine == null) {
        SetStockEngine(car, nwId, log);
        return false;
      }

      if (self && !Verify(car, swap, engine)) {
        SetStockEngine(car, nwId, log);
        if (log) {
          Log.Write($"[KN_Core::Swaps]: Engine verification failed '{engine.Id}', applying default ({nwId})");
        }
        return false;
      }

      var newEngine = new CarDesc.Engine();
      CopyEngine(engine.Engine, newEngine);

      newEngine.turboPressure = swap.Turbo;
      car.carX.finaldrive = swap.FinalDrive;
      car.carX.clutchMaxTorque = engine.ClutchTorque;

      var desc = car.GetDesc();
      CopyEngine(newEngine, desc.carXDesc.engine);
      car.SetDesc(desc);

      SetSoundSound(car, engine.SoundId, log);

      if (self) {
        currentSound_ = engine.SoundId;
        currentEngineTurboMax_ = engine.Engine.turboPressure;
        currentSwap_.SetCurrentEngine(engine.Id);
        SendSwapData();
      }

      if (log) {
        Log.Write($"[KN_Core::Swaps]: Engine '{engine.Name}' ({engine.Id}) was set to '{car.metaInfo.id}' ({nwId}), turbo: {swap.Turbo}, finalDrive: {swap.FinalDrive}");
      }

      return true;
    }

    private void SetSoundSound(RaceCar car, string soundId, bool log) {
      for (int i = 0; i < car.transform.childCount; ++i) {
        var child = car.transform.GetChild(i);
        if (child.name == "Engine") {
          var engineSound = child.GetComponent<FMODCarEngine>();
          if (engineSound != null) {
            var onEnable = KnUtils.GetMethod(engineSound, "OnEnableHandler");
            var onDisable = KnUtils.GetMethod(engineSound, "OnDisableHandler");

            var raceCar = KnUtils.GetField(engineSound, "m_raceCar") as RaceCar;
            if (raceCar != null) {
              onDisable?.Invoke(engineSound, new object[] { });

              string oldName = raceCar.metaInfo.name;
              raceCar.metaInfo.name = soundId;
              KnUtils.SetField(engineSound, "m_raceCar", raceCar);

              onEnable?.Invoke(engineSound, new object[] { });

              raceCar.metaInfo.name = oldName;
              KnUtils.SetField(engineSound, "m_raceCar", raceCar);

              if (log) {
                Log.Write($"[KN_Core::Swaps]: Engine sound is set to '{car.metaInfo.id}'");
              }
            }
          }
          break;
        }
      }
    }

    private bool Verify(RaceCar car, SwapData.Engine swap, EngineData engine) {
      bool allowed = balance_.Any(b => b.CarId == car.metaInfo.id && b.Rating >= engine.Rating);
      return swap.Turbo <= engine.Engine.turboPressure && allowed || core_.IsCheatsEnabled && core_.IsDevToolsEnabled;
    }

    private void SwapEngineTo(EngineData engine) {
      if (engine == null) {
        SetStockEngine(core_.PlayerCar.Base, -1, true);
        currentSwap_?.SetCurrentEngine(-1);
        SendSwapData();
        return;
      }

      if (currentSwap_ != null) {
        var swap = currentSwap_.Get(engine.Id) ?? AddNewEngineToCurrent(engine);
        if (engine.Id > 0 && !SetEngine(core_.PlayerCar.Base, swap, engine, -1, true, true)) {
          currentSwap_.RemoveEngine(swap);
        }
      }
    }

    private SwapData.Engine AddNewEngineToCurrent(EngineData engine) {
      var newEngine = new SwapData.Engine {
        EngineId = engine.Id,
        Turbo = engine.Engine.turboPressure,
        FinalDrive = core_.PlayerCar.CarX.finaldrive
      };
      currentSwap_.AddEngine(newEngine);

      return newEngine;
    }

    private void SetStockEngine(RaceCar car, int nwId, bool log) {
      var desc = car.GetDesc();

      if (nwId != -1) {
        var nwSwap = networkSwaps_.Find(s => s != null && s.NwId == nwId);
        if (nwSwap.Car == null) {
          return;
        }

        CopyEngine(nwSwap.Engine, desc.carXDesc.engine);
        car.SetDesc(desc);

        car.carX.clutchMaxTorque = nwSwap.Clutch;
        car.carX.finaldrive = nwSwap.FinalDrive;

        if (log) {
          Log.Write($"[KN_Core::Swaps]: Stock engine was set to network car '{car.metaInfo.id}' ({nwId})");
        }

        SetSoundSound(car, nwSwap.SoundId, log);
        return;
      }

      CopyEngine(defaultEngine_, desc.carXDesc.engine);
      car.SetDesc(desc);

      car.carX.clutchMaxTorque = defaultClutch_;
      car.carX.finaldrive = defaultFinalDrive_;

      if (log) {
        Log.Write($"[KN_Core::Swaps]: Stock engine was set to own car '{car.metaInfo.id}' ({nwId})");
      }

      SetSoundSound(car, defaultSoundId_, log);
      currentSound_ = defaultSoundId_;
      currentEngineTurboMax_ = 0.0f;
    }

    private EngineData GetEngine(int id) {
      if (id == 0) {
        return null;
      }

      foreach (var engine in engines_) {
        if (engine.Id == id) {
          return engine;
        }
      }

      Log.Write($"[KN_Core::Swaps]: Unable to find engine '{id}'");
      return null;
    }

    public static void CopyEngine(CarDesc.Engine src, CarDesc.Engine dst) {
      dst.inertiaRatio = src.inertiaRatio;
      dst.maxTorque = src.maxTorque;
      dst.revLimiter = src.revLimiter;
      dst.turboCharged = src.turboCharged;
      dst.turboPressure = src.turboPressure;
      dst.brakeTorqueRatio = src.brakeTorqueRatio;
      dst.revLimiterStep = src.revLimiterStep;
      dst.useTC = src.useTC;
      dst.cutRPM = src.cutRPM;
      dst.idleRPM = src.idleRPM;
      dst.maxTorqueRPM = src.maxTorqueRPM;
    }
  }
}