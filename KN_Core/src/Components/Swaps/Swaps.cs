using System.Collections.Generic;
using System.Linq;
using CarX;
using KN_Loader;
using SyncMultiplayer;
using UnityEngine;

namespace KN_Core {
  public class Swaps {
    public bool Active => swapsEnabled_ && dataLoaded_;
    private readonly List<EngineData> engines_;
    private readonly List<SwapBalance> balance_;

    private float carListScrollH_;
    private Vector2 carListScroll_;

    private bool swapsEnabled_;
    private bool shouldRequestSwaps_;

    private readonly bool dataLoaded_;

    private readonly Core core_;

    public Swaps(Core core) {
      core_ = core;

      shouldRequestSwaps_ = true;

      engines_ = new List<EngineData>();
      balance_ = new List<SwapBalance>();
      dataLoaded_ = SwapsLoader.LoadData(ref engines_, ref balance_);
      if (!dataLoaded_) {
        return;
      }
    }

    public void OnInit() {
      if (!dataLoaded_) {
        return;
      }

      if (DataSerializer.Deserialize<SwapData>("KN_Swaps", KnConfig.BaseDir + SwapData.ConfigFile, out var data)) {
        Log.Write($"[KN_Core::Swaps]: Swap data loaded {data.Count} items");
        // allData_.AddRange(data.ConvertAll(d => (SwapData) d));
      }
    }

    public void OnDeinit() {
      if (!Active) {
        return;
      }

      // DataSerializer.Serialize("KN_Swaps", allData_.ToList<ISerializable>(), KnConfig.BaseDir + SwapData.ConfigFile);
    }

    public void OnCarLoaded() {
      if (!Active) {
        return;
      }
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

      if (!Active) {
        return;
      }

    }

    public void ReloadSound() {
      if (!Active) {
        return;
      }

    }

    public void OnGui(Gui gui, ref float x, ref float y, float width) {
      if (!Active) {
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

      // if (gui.Button(ref sx, ref sy, w, height, "STOCK", activeEngine_ == 0 ? Skin.ButtonActive : Skin.Button)) {
      //   if (activeEngine_ != 0) {
      //     SetStockEngine();
      //   }
      // }
      //
      // foreach (var engine in engines_) {
      //   if (gui.Button(ref sx, ref sy, w, height, engine.Name, activeEngine_ == engine.Id ? Skin.ButtonActive : Skin.Button)) {
      //     if (activeEngine_ != engine.Id) {
      //       activeEngine_ = engine.Id;
      //       if (!SetEngine(core_.PlayerCar.Base, activeEngine_)) {
      //         SetStockEngine();
      //       }
      //     }
      //   }
      // }
      carListScrollH_ = gui.EndScrollV(ref x, ref y, sx, sy);

      // GUI.enabled = allowSwap && activeEngine_ != 0;
      // if (gui.SliderH(ref x, ref y, width, ref currentEngine_.Turbo, 0.0f, currentEngineTurboMax_, $"{Locale.Get("swaps_turbo")}: {currentEngine_.Turbo:F2}")) {
      //   var desc = core_.PlayerCar.Base.GetDesc();
      //   desc.carXDesc.engine.turboPressure = currentEngine_.Turbo;
      //   core_.PlayerCar.Base.SetDesc(desc);
      //
      //   foreach (var swap in allData_) {
      //     if (swap.CarId == currentEngine_.CarId && swap.EngineId == currentEngine_.EngineId) {
      //       swap.Turbo = currentEngine_.Turbo;
      //       break;
      //     }
      //   }
      // }
      // if (gui.SliderH(ref x, ref y, width, ref currentEngine_.FinalDrive, 2.5f, 5.0f, $"{Locale.Get("swaps_fd")}: {currentEngine_.FinalDrive:F2}")) {
      //   var desc = core_.PlayerCar.Base.GetDesc();
      //   desc.carXDesc.gearBox.finalDrive = currentEngine_.FinalDrive;
      //   core_.PlayerCar.Base.SetDesc(desc);
      //
      //   foreach (var swap in allData_) {
      //     if (swap.CarId == currentEngine_.CarId && swap.EngineId == currentEngine_.EngineId) {
      //       swap.FinalDrive = currentEngine_.FinalDrive;
      //       break;
      //     }
      //   }
      // }
      GUI.enabled = enabled;
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
        Log.Write($"[KN_Core::Swaps]: Applying engine '{engineId}' on '{id}', turbo: {turbo}, finalDrive: {finalDrive}");
      }

    }

    private void SendSwapData() {

      // var nwData = new SmartfoxDataPackage(PacketId.Subroom);
      // nwData.Add("1", (byte) 25);
      // nwData.Add("type", Udp.TypeSwaps);
      // nwData.Add("id", id);
      // nwData.Add("ei", currentEngine_.EngineId);
      // nwData.Add("tb", currentEngine_.Turbo);
      // nwData.Add("fd", currentEngine_.FinalDrive);
      //
      // core_.Udp.Send(nwData);
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