using System;
using System.Collections.Generic;
using CarX;
using SyncMultiplayer;
using UnityEngine;

namespace KN_Core {
  public class Swaps {
    public bool Active => validator_.Allowed;

    private readonly List<SwapData> allData_;

    private readonly List<Tuple<int, bool, float, string, string, CarDesc.Engine>> engines_;

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

    private readonly AccessValidator validator_;

    public Swaps(Core core) {
      core_ = core;

      validator_ = new AccessValidator("KN_Air");
      validator_.Initialize("aHR0cHM6Ly9yYXcuZ2l0aHVidXNlcmNvbnRlbnQuY29tL3RyYmZseHIva2lub19kYXRhL21hc3Rlci9kYXRhMi50eHQ=");

      activeEngine_ = 0;
      defaultSoundId_ = "";
      defaultEngine_ = new CarDesc.Engine();

      currentEngine_ = new SwapData {
        carId = -1,
        engineId = 0,
        turbo = -1.0f,
        finalDrive = -1.0f
      };
      currentEngineTurboMax_ = 0.0f;

      allData_ = new List<SwapData>();

      sendTimer_ = new Timer(5.0f);
      sendTimer_.Callback += SendSwapData;

      joinTimer_ = new Timer(3.0f, true);
      joinTimer_.Callback += SendSwapData;

      engines_ = new List<Tuple<int, bool, float, string, string, CarDesc.Engine>> {
        new Tuple<int, bool, float, string, string, CarDesc.Engine>(1, false, 1600.0f, "7.0L V8 (LS7)", "Raven RV8", new CarDesc.Engine {
          inertiaRatio = 1.0f,
          maxTorque = 736.5f,
          revLimiter = 9430.3f,
          turboCharged = true,
          turboPressure = 1.1f,
          brakeTorqueRatio = 0.12f,
          revLimiterStep = 450.0f,
          useTC = false,
          cutRPM = 300.0f,
          idleRPM = 810.7f,
          maxTorqueRPM = 4767.0f
        }),
        new Tuple<int, bool, float, string, string, CarDesc.Engine>(2, true, 1200.0f, "6.2L V8 (LS9)", "Spark ZR", new CarDesc.Engine {
          inertiaRatio = 1.0f,
          maxTorque = 437.5f,
          revLimiter = 9000.0f,
          turboCharged = true,
          turboPressure = 1.4f,
          brakeTorqueRatio = 0.12f,
          revLimiterStep = 400.0f,
          useTC = false,
          cutRPM = 300.0f,
          idleRPM = 800.0f,
          maxTorqueRPM = 5145.0f
        }),
        new Tuple<int, bool, float, string, string, CarDesc.Engine>(3, true, 1300.0f, "5.0L V8 (COYOTE)", "Cobra GT530", new CarDesc.Engine {
          inertiaRatio = 1.0f,
          maxTorque = 736.5f,
          revLimiter = 9030.1f,
          turboCharged = true,
          turboPressure = 0.6f,
          brakeTorqueRatio = 0.12f,
          revLimiterStep = 500.0f,
          useTC = false,
          cutRPM = 300.0f,
          idleRPM = 1126.7f,
          maxTorqueRPM = 5160.75f
        }),
        new Tuple<int, bool, float, string, string, CarDesc.Engine>(4, true, 1200.0f, "3.8L V6 (VR38DETT)", "Atlas GT", new CarDesc.Engine {
          inertiaRatio = 1.0f,
          maxTorque = 542.85f,
          revLimiter = 8580.0f,
          turboCharged = true,
          turboPressure = 1.4f,
          brakeTorqueRatio = 0.12f,
          revLimiterStep = 500.0f,
          useTC = false,
          cutRPM = 300.0f,
          idleRPM = 810.7f,
          maxTorqueRPM = 5055.75f
        }),
        new Tuple<int, bool, float, string, string, CarDesc.Engine>(5, true, 1200.0f, "3.4L I6 (2JZ-GTE)", "Carrot II", new CarDesc.Engine {
          inertiaRatio = 1.0f,
          maxTorque = 307.5f,
          revLimiter = 9500.0f,
          turboCharged = true,
          turboPressure = 3.5f,
          brakeTorqueRatio = 0.12f,
          revLimiterStep = 450.0f,
          useTC = false,
          cutRPM = 300.0f,
          idleRPM = 1000.0f,
          maxTorqueRPM = 5145.75f
        }),
        new Tuple<int, bool, float, string, string, CarDesc.Engine>(6, true, 1100.0f, "3.0L I6 (RB26DET)", "Last Prince", new CarDesc.Engine {
          inertiaRatio = 1.1f,
          maxTorque = 453.8f,
          revLimiter = 9294.4f,
          turboCharged = true,
          turboPressure = 1.5f,
          brakeTorqueRatio = 0.12f,
          revLimiterStep = 500.0f,
          useTC = false,
          cutRPM = 300.0f,
          idleRPM = 1200.0f,
          maxTorqueRPM = 5131.75f
        }),
        new Tuple<int, bool, float, string, string, CarDesc.Engine>(7, true, 850.0f, "2.2L I4 (SR20VET)", "Phoenix NX", new CarDesc.Engine {
          inertiaRatio = 1.0f,
          maxTorque = 441.0f,
          revLimiter = 8812.0f,
          turboCharged = true,
          turboPressure = 1.7f,
          brakeTorqueRatio = 0.12f,
          revLimiterStep = 500.0f,
          useTC = false,
          cutRPM = 300.0f,
          idleRPM = 600.0f,
          maxTorqueRPM = 4047.75f
        }),
        new Tuple<int, bool, float, string, string, CarDesc.Engine>(8, true, 800.0f, "1.3L R2 (13B)", "Falcon FC 90-s", new CarDesc.Engine {
          inertiaRatio = 0.95f,
          maxTorque = 367.5f,
          revLimiter = 10000.0f,
          turboCharged = true,
          turboPressure = 1.5f,
          brakeTorqueRatio = 0.2f,
          revLimiterStep = 350.0f,
          useTC = false,
          cutRPM = 300.0f,
          idleRPM = 1200.0f,
          maxTorqueRPM = 5343.75f
        })
      };
    }

    public void OnStart() {
      if (SwapsDataSerializer.Deserialize(SwapData.ConfigFile, out var data)) {
        Log.Write($"[KN_Swaps]: Swap data loaded {data.Count} items");
        allData_.AddRange(data);
      }
    }

    public void OnStop() {
      SwapsDataSerializer.Serialize(allData_, SwapData.ConfigFile);
    }

    public void OnCarLoaded() {
      if (!validator_.Allowed) {
        return;
      }

      SetEngine(core_.PlayerCar.Base, currentEngine_.engineId, true);
    }

    public void Update() {
      validator_.Update();
      if (!validator_.Allowed) {
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
      if (!validator_.Allowed) {
        return;
      }

      ApplySoundOn(core_.PlayerCar.Base, currentEngine_.engineId, true);
    }

    private void FindAndSwap() {
      activeEngine_ = 0;
      foreach (var swap in allData_) {
        if (swap.carId == core_.PlayerCar.Id) {
          SetEngine(core_.PlayerCar.Base, swap.engineId, true);
          activeEngine_ = swap.engineId;
          currentEngine_.turbo = swap.turbo;
          currentEngine_.finalDrive = swap.finalDrive;
          currentEngine_.carId = swap.carId;
          currentEngine_.engineId = swap.engineId;
          break;
        }
      }
    }

    public void OnGui(Gui gui, ref float x, ref float y, float width) {
      if (!validator_.Allowed) {
        return;
      }

      const float listHeight = 220.0f;
      const float height = Gui.Height;

      bool allowSwap = core_.IsInGarage || core_.IsCheatsEnabled;

      bool enabled = GUI.enabled;
      GUI.enabled = allowSwap;

      gui.BeginScrollV(ref x, ref y, width, listHeight, carListScrollH_, ref carListScroll_, "ENGINES");

      y += Gui.OffsetSmall;

      float sx = x;
      float sy = y;
      const float offset = Gui.ScrollBarWidth;
      bool scrollVisible = carListScrollH_ > listHeight;
      float w = scrollVisible ? width - (offset + Gui.OffsetSmall * 3.0f) : width - Gui.OffsetSmall * 3.0f;

      if (gui.Button(ref sx, ref sy, w, height, "STOCK", activeEngine_ == 0 ? Skin.ButtonActive : Skin.Button)) {
        if (activeEngine_ != 0) {
          activeEngine_ = 0;
          currentEngine_.finalDrive = defaultFinalDrive_;
          currentEngine_.turbo = defaultEngine_.turboPressure;
          SetEngine(core_.PlayerCar.Base, activeEngine_);
        }
      }

      foreach (var engine in engines_) {
        if (!core_.IsCheatsEnabled && !engine.Item2) {
          continue;
        }

        if (gui.Button(ref sx, ref sy, w, height, engine.Item4, activeEngine_ == engine.Item1 ? Skin.ButtonActive : Skin.Button)) {
          if (activeEngine_ != engine.Item1) {
            activeEngine_ = engine.Item1;
            SetEngine(core_.PlayerCar.Base, activeEngine_);
          }
        }
      }
      carListScrollH_ = gui.EndScrollV(ref x, ref y, sx, sy);

      GUI.enabled = allowSwap && activeEngine_ != 0;
      if (gui.SliderH(ref x, ref y, width, ref currentEngine_.turbo, 0.0f, currentEngineTurboMax_, $"TURBO: {currentEngine_.turbo:F2}")) {
        var desc = core_.PlayerCar.Base.GetDesc();
        desc.carXDesc.engine.turboPressure = currentEngine_.turbo;
        core_.PlayerCar.Base.SetDesc(desc);

        foreach (var swap in allData_) {
          if (swap.carId == currentEngine_.carId && swap.engineId == currentEngine_.engineId) {
            swap.turbo = currentEngine_.turbo;
            break;
          }
        }
      }
      if (gui.SliderH(ref x, ref y, width, ref currentEngine_.finalDrive, 2.5f, 5.0f, $"FINAL DRIVE: {currentEngine_.finalDrive:F2}")) {
        var desc = core_.PlayerCar.Base.GetDesc();
        desc.carXDesc.gearBox.finalDrive = currentEngine_.finalDrive;
        core_.PlayerCar.Base.SetDesc(desc);

        foreach (var swap in allData_) {
          if (swap.carId == currentEngine_.carId && swap.engineId == currentEngine_.engineId) {
            swap.finalDrive = currentEngine_.finalDrive;
            break;
          }
        }
      }
      GUI.enabled = enabled;
    }

    private void SetEngine(RaceCar car, int engineId, bool silent = false) {
      var data = new SwapData {
        carId = car.metaInfo.id,
        engineId = engineId,
        turbo = -1.0f,
        finalDrive = -1.0f
      };

      var defaultEngine = GetEngine(engineId);
      if (defaultEngine == null) {
        if (!silent) {
          Log.Write($"[KN_Swaps]: Unable to find engine '{engineId}'");
        }
        return;
      }

      bool found = false;
      foreach (var swap in allData_) {
        if (swap.carId == car.metaInfo.id) {
          found = true;

          if (swap.engineId == engineId) {
            data.turbo = swap.turbo;
            data.finalDrive = swap.finalDrive;
            if (!silent) {
              Log.Write($"[KN_Swaps]: Found engine '{engineId}' in config, turbo: {swap.turbo}, finalDrive: {swap.finalDrive}");
            }
          }
          else {
            swap.engineId = data.engineId;
            if (engineId != 0) {
              swap.turbo = defaultEngine.Item6.turboPressure;
              swap.finalDrive = car.carX.finaldrive;
              data.turbo = swap.turbo;
              data.finalDrive = swap.finalDrive;
            }
          }

          break;
        }
      }

      if (engineId == 0) {
        currentEngineTurboMax_ = 0.0f;
        data.finalDrive = defaultFinalDrive_;
        if (TryApplyEngine(car, data, 0, silent)) {
          SendSwapData();
        }
        return;
      }

      if (!found) {
        if (!silent) {
          Log.Write($"[KN_Swaps]: Created config for engine '{engineId}'");
        }
        data.turbo = defaultEngine.Item6.turboPressure;
        data.finalDrive = car.carX.finaldrive;

        allData_.Add(data);
      }

      currentEngine_ = data;
      currentEngineTurboMax_ = defaultEngine.Item6.turboPressure;

      if (TryApplyEngine(car, data, 0, silent)) {
        SendSwapData();
      }
    }

    public void OnUdpData(SmartfoxDataPackage data) {
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
            carId = player.userCar.metaInfo.id,
            engineId = engineId,
            turbo = turbo,
            finalDrive = finalDrive
          };

          TryApplyEngine(player.userCar, swapData, id, true);
          break;
        }
      }
    }

    private void SendSwapData() {
      if (currentEngine_.carId == -1 || currentEngine_.engineId == 0) {
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
      nwData.Add("ei", currentEngine_.engineId);
      nwData.Add("tb", currentEngine_.turbo);
      nwData.Add("fd", currentEngine_.finalDrive);

      core_.Udp.Send(nwData);
    }

    private bool TryApplyEngine(RaceCar car, SwapData data, int id, bool silent) {
      if (car.metaInfo.id != data.carId) {
        return false;
      }

      if (data.engineId == 0) {
        car.carX.finaldrive = defaultFinalDrive_;
        car.carX.clutchMaxTorque = defaultClutch_;

        var d = car.GetDesc();
        CopyEngine(defaultEngine_, d.carXDesc.engine);
        car.SetDesc(d);

        if (!silent) {
          Log.Write($"[KN_Swaps]: Stock engine applied on '{data.carId}' ({id})");
        }

        ApplySoundOn(car, data.engineId, silent);
        return true;
      }

      var defaultEngine = GetEngine(data.engineId);
      if (defaultEngine == null) {
        if (!silent) {
          Log.Write($"[KN_Swaps]: Unable to apply engine '{data.engineId}' ({id})");
        }
        return false;
      }

      var engine = new CarDesc.Engine();
      CopyEngine(defaultEngine.Item6, engine);

      engine.turboPressure = data.turbo;
      car.carX.finaldrive = data.finalDrive;
      car.carX.clutchMaxTorque = defaultEngine.Item3;

      if (!Verify(engine, defaultEngine.Item6)) {
        if (!silent) {
          Log.Write($"[KN_Swaps]: Engine verification failed '{data.engineId}', applying default ({id})");
        }
        return false;
      }

      var desc = car.GetDesc();
      CopyEngine(engine, desc.carXDesc.engine);
      car.SetDesc(desc);

      if (!silent) {
        Log.Write($"[KN_Swaps]: Engine '{defaultEngine.Item4}' applied on '{car.metaInfo.id}' ({id})");
      }

      ApplySoundOn(car, data.engineId, silent);

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
              raceCar.metaInfo.name = engineId == 0 ? defaultSoundId_ : engine.Item5;
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

    private Tuple<int, bool, float, string, string, CarDesc.Engine> GetEngine(int id) {
      if (id == 0) {
        return new Tuple<int, bool, float, string, string, CarDesc.Engine>(0, true, defaultClutch_, "STOCK", defaultSoundId_, defaultEngine_);
      }

      foreach (var engine in engines_) {
        if (engine.Item1 == id) {
          return engine;
        }
      }

      Log.Write($"[KN_Swaps]: Unable to find engine '{id}'");
      return null;
    }

    private static bool Verify(CarDesc.Engine engine, CarDesc.Engine defaultEngine) {
      return engine.turboPressure <= defaultEngine.turboPressure;
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
  }
}