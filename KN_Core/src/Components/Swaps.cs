using System;
using System.Collections.Generic;
using CarX;
using SyncMultiplayer;

namespace KN_Core {
  public class Swaps {
    private readonly List<SwapData> allData_;

    private readonly List<Tuple<int, string, string, CarDesc.Engine>> engines_;

    private readonly Timer joinTimer_;

    private readonly Core core_;

    private SwapData currentEngine_;
    private string defaultSoundId_;
    private readonly CarDesc.Engine defaultEngine_;

    public Swaps(Core core) {
      core_ = core;

      defaultSoundId_ = "";
      defaultEngine_ = new CarDesc.Engine();

      currentEngine_ = new SwapData {
        carId = -1,
        turbo = -1.0f,
        engineId = 0
      };

      allData_ = new List<SwapData>();

      joinTimer_ = new Timer(5.0f, true);
      joinTimer_.Callback += SendSwapData;

      engines_ = new List<Tuple<int, string, string, CarDesc.Engine>> {
        new Tuple<int, string, string, CarDesc.Engine>(1, "6.0L V8 (L98)", "Raven RV8", new CarDesc.Engine {
          inertiaRatio = 1.0f,
          maxTorque = 758.5f,
          revLimiter = 8036.1f,
          turboCharged = true,
          turboPressure = 0.4f,
          brakeTorqueRatio = 0.12f,
          revLimiterStep = 500.0f,
          useTC = false,
          cutRPM = 300.0f,
          idleRPM = 810.7f,
          maxTorqueRPM = 5160.75f
        }),
        new Tuple<int, string, string, CarDesc.Engine>(2, "6.2L V8 (LS9)", "Spark ZR", new CarDesc.Engine {
          inertiaRatio = 1.0f,
          maxTorque = 437.5f,
          revLimiter = 8800.0f,
          turboCharged = true,
          turboPressure = 1.4f,
          brakeTorqueRatio = 0.12f,
          revLimiterStep = 400.0f,
          useTC = false,
          cutRPM = 300.0f,
          idleRPM = 800.0f,
          maxTorqueRPM = 5145.0f
        }),
        new Tuple<int, string, string, CarDesc.Engine>(3, "3.8L V6 (VR38DETT)", "Atlas GT", new CarDesc.Engine {
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
        new Tuple<int, string, string, CarDesc.Engine>(4, "3.4L I6 (2JZ-GTE)", "Carrot II", new CarDesc.Engine {
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
        new Tuple<int, string, string, CarDesc.Engine>(5, "5.0L V8 (Coyote)", "Cobra GT530", new CarDesc.Engine {
          inertiaRatio = 1.0f,
          maxTorque = 736.5f,
          revLimiter = 9430.3f,
          turboCharged = true,
          turboPressure = 1.1f,
          brakeTorqueRatio = 0.12f,
          revLimiterStep = 450.0f,
          useTC = false,
          cutRPM = 300.0f,
          idleRPM = 1126.7f,
          maxTorqueRPM = 4767.0f
        }),
        new Tuple<int, string, string, CarDesc.Engine>(6, "3.0L I6 (RB30DET)", "Last Prince", new CarDesc.Engine {
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

    public void Update() {
      if (core_.IsCarChanged) {
        defaultSoundId_ = core_.PlayerCar.Base.metaInfo.name;
        var desc = core_.PlayerCar.Base.GetDesc();
        CopyEngine(desc.carXDesc.engine, defaultEngine_);
      }

      if (core_.IsInGarageChanged) {
        joinTimer_.Reset();
      }

      joinTimer_.Update();
    }

    public void SetEngine(RaceCar car, int engineId) {
      var data = new SwapData {
        carId = car.metaInfo.id,
        turbo = -1.0f,
        engineId = engineId
      };

      if (engineId == 0) {
        if (TryApplyEngine(car, data)) {
          SendSwapData();
        }
        return;
      }

      bool found = false;
      foreach (var swap in allData_) {
        if (swap.carId == car.metaInfo.id && swap.engineId == engineId) {
          found = true;
          data.turbo = swap.turbo;
          Log.Write($"[KN_Swaps]: Found engine '{engineId}' in config, turbo: {swap.turbo}");
          break;
        }
      }

      if (!found) {
        var defaultEngine = GetEngine(engineId);
        if (defaultEngine == null) {
          Log.Write($"[KN_Swaps]: Unable to find engine '{engineId}'");
          return;
        }
        Log.Write($"[KN_Swaps]: Created config for engine '{engineId}'");
        data.turbo = defaultEngine.Item4.turboPressure;

        allData_.Add(data);
      }

      currentEngine_ = data;

      if (TryApplyEngine(car, data)) {
        SendSwapData();
      }
    }

    public void OnUdpData(SmartfoxDataPackage data) {
      int id = data.Data.GetInt("id");
      int engineId = data.Data.GetInt("engineId");
      float turbo = data.Data.GetFloat("turbo");

      foreach (var player in NetworkController.InstanceGame.Players) {
        if (player.NetworkID == id) {
          var engine = GetEngine(engineId);
          if (engine == null) {
            Log.Write($"[KN_Swaps]: Unable to find engine '{engineId}' for '{id}'. Check for mod updates!");
            return;
          }

          var swapData = new SwapData {
            carId = player.userCar.metaInfo.id,
            turbo = turbo,
            engineId = engineId
          };

          TryApplyEngine(player.userCar, swapData);
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
      nwData.Add("engineId", currentEngine_.engineId);
      nwData.Add("turbo", currentEngine_.turbo);

      core_.Udp.Send(nwData);
    }

    private bool TryApplyEngine(RaceCar car, SwapData data) {
      if (car.metaInfo.id != data.carId) {
        return false;
      }

      if (data.engineId == 0) {
        var d = car.GetDesc();
        CopyEngine(defaultEngine_, d.carXDesc.engine);
        car.SetDesc(d);

        Log.Write($"[KN_Swaps]: Stock engine applied on '{data.carId}'");

        ApplySoundOn(car, data.engineId);
        return true;
      }

      var defaultEngine = GetEngine(data.engineId);
      if (defaultEngine == null) {
        Log.Write($"[KN_Swaps]: Unable to apply engine '{data.engineId}'");
        return false;
      }

      var engine = new CarDesc.Engine();
      CopyEngine(defaultEngine.Item4, engine);

      engine.turboPressure = data.turbo;

      if (!Verify(engine, defaultEngine.Item4)) {
        Log.Write($"[KN_Swaps]: Engine verification failed '{data.engineId}', applying default");
        return false;
      }

      var desc = car.GetDesc();
      CopyEngine(engine, desc.carXDesc.engine);
      car.SetDesc(desc);

      Log.Write($"[KN_Swaps]: Engine '{defaultEngine.Item2}' applied on '{car.metaInfo.id}'");

      ApplySoundOn(car, data.engineId);

      return true;
    }

    private void ApplySoundOn(RaceCar car, int engineId) {
      var engine = GetEngine(engineId);
      if (engine == null) {
        Log.Write($"[KN_Swaps]: Unable to apply sound of engine '{engineId}'");
        return;
      }

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
              raceCar.metaInfo.name = engineId == 0 ? defaultSoundId_ : engine.Item3;
              KnUtils.SetField(engineSound, "m_raceCar", raceCar);

              onEnable?.Invoke(engineSound, new object[] { });

              raceCar.metaInfo.name = oldName;
              KnUtils.SetField(engineSound, "m_raceCar", raceCar);
            }
          }
        }
      }

      Log.Write($"[KN_Swaps]: Engine sound applied on '{car.metaInfo.id}'");
    }

    private Tuple<int, string, string, CarDesc.Engine> GetEngine(int id) {
      if (id == 0) {
        return new Tuple<int, string, string, CarDesc.Engine>(0, "STOCK", defaultSoundId_, defaultEngine_);
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