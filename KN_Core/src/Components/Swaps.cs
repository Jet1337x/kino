using System.Collections.Generic;
using System.Linq;
using CarX;
using SyncMultiplayer;
using UnityEngine;

namespace KN_Core {
  public class Swaps {
    public bool Active => swapsEnabled_ && dataLoaded_;

    private readonly List<SwapData> allData_;

    private readonly List<EngineData> engines_;
    private readonly List<SwapBalance> balance_;

    private readonly Timer joinTimer_;
    private readonly Timer sendTimer_;

    private readonly Core core_;

    private SwapData currentEngine_;
    private float currentEngineTurboMax_;
    private string defaultSoundId_;
    private float defaultFinalDrive_;
    private float defaultClutch_;
    private readonly CarDesc.Engine defaultEngine_;

    private int activeEngine_;
    private float carListScrollH_;
    private Vector2 carListScroll_;

    private bool swapReload_;

    private readonly bool dataLoaded_;

    private bool swapsEnabled_;
    private bool shouldRequestSwaps_;

    public Swaps(Core core) {
      core_ = core;

      shouldRequestSwaps_ = true;

      engines_ = new List<EngineData>();
      balance_ = new List<SwapBalance>();
      dataLoaded_ = LoadData();
      if (!dataLoaded_) {
        return;
      }

      activeEngine_ = 0;
      defaultSoundId_ = "";
      defaultEngine_ = new CarDesc.Engine();

      currentEngine_ = new SwapData {
        CarId = -1,
        EngineId = 0,
        Turbo = -1.0f,
        FinalDrive = -1.0f
      };
      currentEngineTurboMax_ = 0.0f;

      allData_ = new List<SwapData>();

      sendTimer_ = new Timer(5.0f);
      sendTimer_.Callback += SendSwapData;

      joinTimer_ = new Timer(3.0f, true);
      joinTimer_.Callback += SendSwapData;
    }

    public void OnStart() {
      if (!dataLoaded_) {
        return;
      }

      if (DataSerializer.Deserialize<SwapData>("KN_Swaps", KnConfig.BaseDir + SwapData.ConfigFile, out var data)) {
        Log.Write($"[KN_Swaps]: Swap data loaded {data.Count} items");
        allData_.AddRange(data.ConvertAll(d => (SwapData) d));
      }
    }

    public void OnStop() {
      if (!dataLoaded_) {
        return;
      }

      DataSerializer.Serialize("KN_Swaps", allData_.ToList<ISerializable>(), KnConfig.BaseDir + SwapData.ConfigFile);
    }

    public void OnCarLoaded() {
      if (!dataLoaded_ || !swapsEnabled_) {
        return;
      }

      SetEngine(core_.PlayerCar.Base, currentEngine_.EngineId, true);
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

      if (!dataLoaded_ || !swapsEnabled_) {
        return;
      }

      if (core_.IsCarChanged) {
        defaultSoundId_ = core_.PlayerCar.Base.metaInfo.name;
        defaultFinalDrive_ = core_.PlayerCar.CarX.finaldrive;
        defaultClutch_ = core_.PlayerCar.CarX.clutchMaxTorque;
        var desc = core_.PlayerCar.Base.GetDesc();
        CopyEngine(desc.carXDesc.engine, defaultEngine_);
        FindAndSwap();
      }

      if (core_.IsInGarageChanged || core_.IsSceneChanged) {
        swapReload_ = true;
        joinTimer_.Reset();
      }

      joinTimer_.Update();
      sendTimer_.Update();

      if (swapReload_ && !KnCar.IsNull(core_.PlayerCar)) {
        FindAndSwap();
        swapReload_ = false;
      }
    }

    public void ReloadSound() {
      if (!dataLoaded_ || !swapsEnabled_) {
        return;
      }

      ApplySoundOn(core_.PlayerCar.Base, currentEngine_.EngineId, true);
    }

    private void FindAndSwap() {
      activeEngine_ = 0;
      foreach (var swap in allData_) {
        if (swap.CarId == core_.PlayerCar.Id) {
          if (SetEngine(core_.PlayerCar.Base, swap.EngineId, true)) {
            activeEngine_ = swap.EngineId;
            currentEngine_.Turbo = swap.Turbo;
            currentEngine_.FinalDrive = swap.FinalDrive;
            currentEngine_.CarId = swap.CarId;
            currentEngine_.EngineId = swap.EngineId;
          }
          break;
        }
      }
    }

    public void OnGui(Gui gui, ref float x, ref float y, float width) {
      if (!dataLoaded_ || !swapsEnabled_) {
        return;
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

      if (gui.Button(ref sx, ref sy, w, height, "STOCK", activeEngine_ == 0 ? Skin.ButtonActive : Skin.Button)) {
        if (activeEngine_ != 0) {
          SetStockEngine();
        }
      }

      foreach (var engine in engines_) {
        bool allowed = balance_.Any(b => b.CarId == core_.PlayerCar.Id && b.Rating >= engine.Rating);
        if (!core_.IsDevToolsEnabled && (!engine.Enabled || !allowed)) {
          continue;
        }

        if (gui.Button(ref sx, ref sy, w, height, engine.Name, activeEngine_ == engine.Id ? Skin.ButtonActive : Skin.Button)) {
          if (activeEngine_ != engine.Id) {
            activeEngine_ = engine.Id;
            if (!SetEngine(core_.PlayerCar.Base, activeEngine_)) {
              SetStockEngine();
            }
          }
        }
      }
      carListScrollH_ = gui.EndScrollV(ref x, ref y, sx, sy);

      GUI.enabled = allowSwap && activeEngine_ != 0;
      if (gui.SliderH(ref x, ref y, width, ref currentEngine_.Turbo, 0.0f, currentEngineTurboMax_, $"{Locale.Get("swaps_turbo")}: {currentEngine_.Turbo:F2}")) {
        var desc = core_.PlayerCar.Base.GetDesc();
        desc.carXDesc.engine.turboPressure = currentEngine_.Turbo;
        core_.PlayerCar.Base.SetDesc(desc);

        foreach (var swap in allData_) {
          if (swap.CarId == currentEngine_.CarId && swap.EngineId == currentEngine_.EngineId) {
            swap.Turbo = currentEngine_.Turbo;
            break;
          }
        }
      }
      if (gui.SliderH(ref x, ref y, width, ref currentEngine_.FinalDrive, 2.5f, 5.0f, $"{Locale.Get("swaps_fd")}: {currentEngine_.FinalDrive:F2}")) {
        var desc = core_.PlayerCar.Base.GetDesc();
        desc.carXDesc.gearBox.finalDrive = currentEngine_.FinalDrive;
        core_.PlayerCar.Base.SetDesc(desc);

        foreach (var swap in allData_) {
          if (swap.CarId == currentEngine_.CarId && swap.EngineId == currentEngine_.EngineId) {
            swap.FinalDrive = currentEngine_.FinalDrive;
            break;
          }
        }
      }
      GUI.enabled = enabled;
    }

    private void SetStockEngine() {
      activeEngine_ = 0;
      currentEngine_.FinalDrive = defaultFinalDrive_;
      currentEngine_.Turbo = defaultEngine_.turboPressure;
      SetEngine(core_.PlayerCar.Base, activeEngine_);
    }

    private bool SetEngine(RaceCar car, int engineId, bool silent = false) {
      var data = new SwapData {
        CarId = car.metaInfo.id,
        EngineId = engineId,
        Turbo = -1.0f,
        FinalDrive = -1.0f
      };

      var defaultEngine = GetEngine(engineId);
      if (defaultEngine == null) {
        if (!silent) {
          Log.Write($"[KN_Swaps]: Unable to find engine '{engineId}'");
        }
        return false;
      }

      bool found = false;
      foreach (var swap in allData_) {
        if (swap.CarId == car.metaInfo.id) {
          found = true;

          bool allowed = balance_.Any(b => b.CarId == core_.PlayerCar.Id && b.Rating >= defaultEngine.Rating);
          if (!(core_.IsCheatsEnabled && core_.IsDevToolsEnabled) && !allowed) {
            Log.Write($"[KN_Swaps]: Engine '{engineId}' disabled for car '{swap.CarId}'");
            data.EngineId = 0;
            data.FinalDrive = defaultFinalDrive_;
            currentEngine_.EngineId = 0;
            activeEngine_ = 0;
            return false;
          }

          if (swap.EngineId == engineId) {
            data.Turbo = swap.Turbo;
            data.FinalDrive = swap.FinalDrive;
            if (!silent) {
              Log.Write($"[KN_Swaps]: Found engine '{engineId}' in config, turbo: {swap.Turbo}, finalDrive: {swap.FinalDrive}");
            }
          }
          else {
            swap.EngineId = data.EngineId;
            if (engineId != 0) {
              swap.Turbo = defaultEngine.Engine.turboPressure;
              swap.FinalDrive = car.carX.finaldrive;
              data.Turbo = swap.Turbo;
              data.FinalDrive = swap.FinalDrive;
            }
          }

          break;
        }
      }

      if (engineId == 0) {
        currentEngineTurboMax_ = 0.0f;
        data.FinalDrive = defaultFinalDrive_;
        if (TryApplyEngine(car, data, 0, silent)) {
          SendSwapData();
        }
        return false;
      }

      if (!found) {
        if (!silent) {
          Log.Write($"[KN_Swaps]: Created config for engine '{engineId}'");
        }
        data.Turbo = defaultEngine.Engine.turboPressure;
        data.FinalDrive = car.carX.finaldrive;

        allData_.Add(data);
      }

      currentEngine_ = data;
      currentEngineTurboMax_ = defaultEngine.Engine.turboPressure;

      if (TryApplyEngine(car, data, 0, silent)) {
        SendSwapData();
      }
      return true;
    }

    public void OnUdpData(SmartfoxDataPackage data) {
      if (!dataLoaded_) {
        return;
      }

      int id = data.Data.GetInt("id");
      int engineId = data.Data.GetInt("ei");
      float turbo = data.Data.GetFloat("tb");
      float finalDrive = data.Data.GetFloat("fd");

      if (engineId == 0) {
        return;
      }

      if (core_.Settings.LogEngines) {
        Log.Write($"[KN_Swaps]: Applying engine '{engineId}' on '{id}', turbo: {turbo}, finalDrive: {finalDrive}");
      }

      foreach (var player in NetworkController.InstanceGame.Players) {
        if (player.NetworkID == id) {
          var engine = GetEngine(engineId);
          if (engine == null) {
            return;
          }

          var swapData = new SwapData {
            CarId = player.userCar.metaInfo.id,
            EngineId = engineId,
            Turbo = turbo,
            FinalDrive = finalDrive
          };

          TryApplyEngine(player.userCar, swapData, id, true);
          break;
        }
      }
    }

    private void SendSwapData() {
      if (currentEngine_.CarId == -1 || currentEngine_.EngineId == 0) {
        return;
      }

      int id = NetworkController.InstanceGame?.LocalPlayer?.NetworkID ?? -1;
      if (id == -1) {
        return;
      }

      var nwData = new SmartfoxDataPackage(PacketId.Subroom);
      nwData.Add("1", (byte) 25);
      nwData.Add("type", Udp.TypeSwaps);
      nwData.Add("id", id);
      nwData.Add("ei", currentEngine_.EngineId);
      nwData.Add("tb", currentEngine_.Turbo);
      nwData.Add("fd", currentEngine_.FinalDrive);

      core_.Udp.Send(nwData);
    }

    private bool TryApplyEngine(RaceCar car, SwapData data, int id, bool silent) {
      if (car.metaInfo.id != data.CarId) {
        return false;
      }

      if (data.EngineId == 0) {
        car.carX.finaldrive = defaultFinalDrive_;
        car.carX.clutchMaxTorque = defaultClutch_;

        var d = car.GetDesc();
        CopyEngine(defaultEngine_, d.carXDesc.engine);
        car.SetDesc(d);

        if (!silent) {
          Log.Write($"[KN_Swaps]: Stock engine applied on '{data.CarId}' ({id})");
        }

        ApplySoundOn(car, data.EngineId, silent);
        return true;
      }

      var defaultEngine = GetEngine(data.EngineId);
      if (defaultEngine == null) {
        if (!silent) {
          Log.Write($"[KN_Swaps]: Unable to apply engine '{data.EngineId}' ({id})");
        }
        currentEngine_.EngineId = 0;
        activeEngine_ = 0;
        ApplySoundOn(car, activeEngine_, true);

        return false;
      }

      var engine = new CarDesc.Engine();
      CopyEngine(defaultEngine.Engine, engine);

      engine.turboPressure = data.Turbo;
      car.carX.finaldrive = data.FinalDrive;
      car.carX.clutchMaxTorque = defaultEngine.ClutchTorque;

      if (!Verify(engine, defaultEngine.Engine, defaultEngine.Rating)) {
        if (!silent) {
          Log.Write($"[KN_Swaps]: Engine verification failed '{data.EngineId}', applying default ({id})");
        }
        return false;
      }

      var desc = car.GetDesc();
      CopyEngine(engine, desc.carXDesc.engine);
      car.SetDesc(desc);

      if (!silent) {
        Log.Write($"[KN_Swaps]: Engine '{defaultEngine.Name}' applied on '{car.metaInfo.id}' ({id})");
      }

      ApplySoundOn(car, data.EngineId, silent);

      return true;
    }

    private void ApplySoundOn(RaceCar car, int engineId, bool silent) {
      var engine = GetEngine(engineId);
      if (engine == null) {
        if (!silent) {
          Log.Write($"[KN_Swaps]: Unable to apply sound of engine '{engineId}'");
        }
        return;
      }

      bool found = false;
      for (int i = 0; i < car.transform.childCount; i++) {
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
              raceCar.metaInfo.name = engineId == 0 ? defaultSoundId_ : engine.SoundId;
              KnUtils.SetField(engineSound, "m_raceCar", raceCar);

              onEnable?.Invoke(engineSound, new object[] { });

              raceCar.metaInfo.name = oldName;
              KnUtils.SetField(engineSound, "m_raceCar", raceCar);

              found = true;
            }
          }
          break;
        }
      }

      if (!silent && found) {
        Log.Write($"[KN_Swaps]: Engine sound applied on '{car.metaInfo.id}'");
      }
    }

    private EngineData GetEngine(int id) {
      if (id == 0) {
        return new EngineData(0, 1, true, defaultClutch_, "STOCK", defaultSoundId_, defaultEngine_);
      }

      foreach (var engine in engines_) {
        if (engine.Id == id) {
          return engine;
        }
      }

      Log.Write($"[KN_Swaps]: Unable to find engine '{id}'");
      return null;
    }

    private bool Verify(CarDesc.Engine engine, CarDesc.Engine defaultEngine, int rating) {
      bool allowed = balance_.Any(b => b.CarId == core_.PlayerCar.Id && b.Rating >= rating);
      return engine.turboPressure <= defaultEngine.turboPressure && (allowed || core_.IsCheatsEnabled && core_.IsDevToolsEnabled);
    }

    private void CopyEngine(CarDesc.Engine src, CarDesc.Engine dst) {
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

    private bool LoadData() {
      return LoadEngines() && LoadBalance();
    }

    private bool LoadEngines() {
      var data = WebDataLoader.LoadAsBytes("aHR0cHM6Ly9naXRodWIuY29tL3RyYmZseHIva2lub19kYXRhL3Jhdy9tYXN0ZXIva25fZGF0YTAua25k");
      if (data == null) {
        return false;
      }

      Log.Write("[KN_Swaps]: Engine data loaded from remote");

      if (DataSerializer.Deserialize<EngineData>("KN_Swaps", data, out var engines)) {
        engines_.AddRange(engines.ConvertAll(d => (EngineData) d));
        Log.Write($"[KN_Swaps]: Engine data parsed, count: {engines_.Count}");
      }
      else {
        Log.Write("[KN_Swaps]: Unable to parse engine data");
        return false;
      }

      return true;
    }

    private bool LoadBalance() {
      var data = WebDataLoader.LoadAsBytes("aHR0cHM6Ly9naXRodWIuY29tL3RyYmZseHIva2lub19kYXRhL3Jhdy9tYXN0ZXIva25fZGF0YTEua25k");
      if (data == null) {
        return false;
      }

      Log.Write("[KN_Swaps]: Balance data loaded from remote");

      if (DataSerializer.Deserialize<SwapBalance>("KN_Swaps", data, out var balance)) {
        balance_.AddRange(balance.ConvertAll(d => (SwapBalance) d));
        Log.Write($"[KN_Swaps]: Balance data parsed, count: {balance_.Count}");
      }
      else {
        Log.Write("[KN_Swaps]: Unable to parse balance data");
        return false;
      }

      return true;
    }
  }
}