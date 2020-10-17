using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KN_Loader;
using UnityEngine;

namespace KN_Core {
  public class Exhaust {
    private const string ExhaustConfigFile = "kn_exhaust.kne";
    private const string ExhaustConfigDefaultFile = "kn_exhaust_default.kne";

    private const float MaxDistance = 50.0f;

    public const float TriggerFade = 3.0f;
    public const float RevTrigger = 500.0f;
    public const float LoadTrigger = 0.55f;
    public const float RpmLowBound = 4000.0f;
    public const float RpmHighBound = 5500.0f;

    private ExhaustData activeExhaust_;
    private readonly List<ExhaustData> exhausts_;
    private readonly Core core_;

    public List<ExhaustFifeData> ExhaustConfig { get; }
    public List<ExhaustFifeData> ExhaustConfigDefault { get; }

#if KN_DEV_TOOLS
    private readonly List<ExhaustFifeData> exhaustConfigsDev_;
#endif

    public Exhaust(Core core) {
      core_ = core;
      exhausts_ = new List<ExhaustData>();
      ExhaustConfig = new List<ExhaustFifeData>();
      ExhaustConfigDefault = new List<ExhaustFifeData>();
#if KN_DEV_TOOLS
      exhaustConfigsDev_ = new List<ExhaustFifeData>();
#endif
    }

    public void OnStart() {
      var assembly = Assembly.GetExecutingAssembly();
      using (var stream = assembly.GetManifestResourceStream("KN_Core.Resources." + ExhaustConfigDefaultFile)) {
        if (DataSerializer.Deserialize<ExhaustFifeData>("KN_Exhaust", stream, out var defaultData)) {
          ExhaustConfigDefault.AddRange(defaultData.ConvertAll(d => (ExhaustFifeData) d));
        }
      }

      if (DataSerializer.Deserialize<ExhaustFifeData>("KN_Exhaust", KnConfig.BaseDir + ExhaustConfigFile, out var data)) {
        ExhaustConfig.AddRange(data.ConvertAll(d => (ExhaustFifeData) d));
      }

#if KN_DEV_TOOLS
      if (DataSerializer.Deserialize<ExhaustFifeData>("KN_Exhaust", KnConfig.BaseDir + ExhaustConfigDefaultFile, out var devData)) {
        exhaustConfigsDev_.AddRange(devData.ConvertAll(d => (ExhaustFifeData) d));
      }
#endif
    }

    public void OnStop() {
      DataSerializer.Serialize("KN_Exhaust", ExhaustConfig.ToList<ISerializable>(), KnConfig.BaseDir + ExhaustConfigFile, Core.Version);

#if KN_DEV_TOOLS
      DataSerializer.Serialize("KN_Exhaust", exhaustConfigsDev_.ToList<ISerializable>(), KnConfig.BaseDir + "dev/" + ExhaustConfigDefaultFile, Core.Version);
#endif
    }

    public void Reset() {
      foreach (var e in exhausts_) {
        e.ToggleLights(false);
      }
      activeExhaust_ = null;
    }

    public void Update() {
      exhausts_.RemoveAll(e => {
        if (KnCar.IsNull(e.Car)) {
          e.RemoveLights();
          Log.Write($"[KN_Core::Exhaust]: Removed exhaust for car '{e.Car.Name}'");
          return true;
        }
        return false;
      });

#if !KN_DEV_TOOLS
      if (core_.IsInGarage) {
        return;
      }
#endif

      foreach (var e in exhausts_) {
#if KN_DEV_TOOLS
        if (Input.GetKey(KeyCode.Delete)) {
          if (!e.Enabled) {
            e.Initialize();
          }
          e.Enabled = true;
          e.Update();
        }
        else {
          e.Enabled = false;
          e.ToggleLights(false);
        }

        if (Input.GetKeyDown(KeyCode.Insert)) {
          int id = exhaustConfigsDev_.FindIndex(ed => ed.CarId == e.Car.Id);
          if (id != -1) {
            exhaustConfigsDev_[id] = new ExhaustFifeData(e.Car.Id, e.MaxTime, e.FlamesTrigger, e.Volume);
            Log.Write($"[KN_Core::Exhaust]: Exhaust dev tools | Override config for {e.Car.Id} | Total: {exhaustConfigsDev_.Count}");
            return;
          }
          exhaustConfigsDev_.Add(new ExhaustFifeData(e.Car.Id, e.MaxTime, e.FlamesTrigger, e.Volume));
          Log.Write($"[KN_Core::Exhaust]: Exhaust dev tools | Added config for {e.Car.Id} | Total: {exhaustConfigsDev_.Count}");
        }
#else
        if (Vector3.Distance(core_.ActiveCamera.transform.position, e.Car.Transform.position) > MaxDistance) {
          e.Enabled = false;
          e.ToggleLights(false);
        }
        else {
          if (!e.Enabled) {
            e.Enabled = true;
            e.Initialize();
          }
          e.Update();
        }
#endif
      }
    }

    public void OnGui(Gui gui, ref float x, ref float y, float width) {
      bool guiEnabled = GUI.enabled;
      GUI.enabled = activeExhaust_ != null && !core_.IsInGarage;

      float volume = activeExhaust_?.Volume ?? 1.0f;
      if (gui.SliderH(ref x, ref y, width, ref volume, 0.1f, 1.2f, $"{Locale.Get("exh_volume")}: {volume:F}")) {
        if (activeExhaust_ != null) {
          activeExhaust_.Volume = volume;
          UpdateConfig(activeExhaust_);
        }
      }

      float maxTime = activeExhaust_?.MaxTime ?? 1.0f;
      if (gui.SliderH(ref x, ref y, width, ref maxTime, 0.1f, 3.0f, $"{Locale.Get("exh_max_time")}: {maxTime:F}")) {
        if (activeExhaust_ != null) {
          activeExhaust_.MaxTime = maxTime;
          UpdateConfig(activeExhaust_);
        }
      }

#if KN_DEV_TOOLS
      float flamesTrigger = activeExhaust_?.FlamesTrigger ?? 0.06f;
      if (gui.SliderH(ref x, ref y, width, ref flamesTrigger, 0.05f, 0.5f, $"FLAMES TRIGGER: {flamesTrigger:F}")) {
        if (activeExhaust_ != null) {
          activeExhaust_.FlamesTrigger = flamesTrigger;
          UpdateConfig(activeExhaust_);
        }
      }
#endif
      GUI.enabled = guiEnabled;
    }

    public void Initialize() {
      foreach (var e in exhausts_) {
        e.RemoveLights();
      }
      exhausts_.Clear();

      var scripts = Object.FindObjectsOfType<CarPopExhaust>();
      if (scripts != null && scripts.Length > 0) {
        foreach (var s in scripts) {
          exhausts_.Add(new ExhaustData(this, s));
        }
        foreach (var e in exhausts_.Where(e => e.Car == core_.PlayerCar)) {
          activeExhaust_ = e;
          break;
        }
      }
      else {
        activeExhaust_ = null;
      }
    }

    private void UpdateConfig(ExhaustData data) {
      Log.Write($"[KN_Core::Exhaust]: Conf size: {ExhaustConfig.Count} / Car data id: {data.Car.Id}");
      int id = ExhaustConfig.FindIndex(ed => ed.CarId == data.Car.Id);
      if (id != -1) {
        Log.Write($"[KN_Core::Exhaust]: Override exhaust for car '{data.Car.Name}'");
        ExhaustConfig[id] = new ExhaustFifeData(data.Car.Id, data.MaxTime, data.FlamesTrigger, data.Volume);
      }
      else {
        ExhaustConfig.Add(new ExhaustFifeData(data.Car.Id, data.MaxTime, data.FlamesTrigger, data.Volume));
      }
    }
  }
}