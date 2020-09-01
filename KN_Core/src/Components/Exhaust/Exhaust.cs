using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KN_Core {
  public class Exhaust {
    private const string ExhaustConfigFile = "kn_exhaust.kne";
    private const string ExhaustConfigDefaultFile = "kn_exhaust_default.kne";

    public const float MaxDistance = 70.0f;

    public const float TriggerFade = 3.0f;
    public const float RevTrigger = 500.0f;
    public const float LoadTrigger = 0.55f;
    public const float RpmLowBound = 4000.0f;
    public const float RpmHighBound = 5500.0f;

    private ExhaustData activeExhaust_;
    private readonly List<ExhaustData> exhausts_;
    private readonly List<ExhaustData> exhaustsToRemove_;
    private readonly Core core_;

    private List<ExhaustFifeData> exhaustConfig_;
    public List<ExhaustFifeData> ExhaustConfig => exhaustConfig_;

    private List<ExhaustFifeData> exhaustConfigDefault_;
    public List<ExhaustFifeData> ExhaustConfigDefault => exhaustConfigDefault_;

#if KN_DEV_TOOLS
    private List<ExhaustFifeData> exhaustConfigsDev_;
#endif

    public Exhaust(Core core) {
      core_ = core;
      exhausts_ = new List<ExhaustData>();
      exhaustsToRemove_ = new List<ExhaustData>();
    }

    public void OnStart() {
      var assembly = Assembly.GetExecutingAssembly();
      using (var stream = assembly.GetManifestResourceStream("KN_Core.Resources." + ExhaustConfigDefaultFile)) {
        ExhaustSerializer.Deserialize(stream, out exhaustConfigDefault_);
      }

      if (!ExhaustSerializer.Deserialize(ExhaustConfigFile, out exhaustConfig_)) {
        exhaustConfig_ = new List<ExhaustFifeData>();
      }

#if KN_DEV_TOOLS
      if (!ExhaustSerializer.Deserialize(ExhaustConfigDefaultFile, out exhaustConfigsDev_)) {
        exhaustConfigsDev_ = new List<ExhaustFifeData>();
      }
#endif
    }

    public void OnStop() {
      ExhaustSerializer.Serialize(exhaustConfig_, ExhaustConfigFile);

#if KN_DEV_TOOLS
      ExhaustSerializer.Serialize(exhaustConfigsDev_, ExhaustConfigDefaultFile);
#endif
    }

    public void Reset() {
      foreach (var e in exhausts_) {
        e.ToggleLights(false);
      }
      foreach (var e in exhaustsToRemove_) {
        e.ToggleLights(false);
        exhausts_.Remove(e);
      }
      activeExhaust_ = null;
    }

    public void Update() {
#if !KN_DEV_TOOLS
      if (core_.IsInGarage) {
        return;
      }
#endif

      foreach (var e in exhausts_) {
#if KN_DEV_TOOLS
        if (Input.GetKey(KeyCode.PageUp)) {
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
            Log.Write($"[TF_Core]: Exhaust dev tools | Override config for {e.Car.Id} | Total: {exhaustConfigsDev_.Count}");
            return;
          }
          exhaustConfigsDev_.Add(new ExhaustFifeData(e.Car.Id, e.MaxTime, e.FlamesTrigger, e.Volume));
          Log.Write($"[TF_Core]: Exhaust dev tools | Added config for {e.Car.Id} | Total: {exhaustConfigsDev_.Count}");
        }
#else
        if (TFCar.IsNull(e.Car)) {
          exhaustsToRemove_.Add(e);
          continue;
        }
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

      if (exhaustsToRemove_.Count > 0) {
        foreach (var e in exhaustsToRemove_) {
          Log.Write($"[TF_Core]: Removed exhaust for car '{e.Car.Name}'");
          exhausts_.Remove(e);
        }
        exhaustsToRemove_.Clear();
      }
    }

    public void OnGUI(Gui gui, ref float x, ref float y, float width) {
      bool guiEnabled = GUI.enabled;
      GUI.enabled = activeExhaust_ != null && !core_.IsInGarage;

      float volume = activeExhaust_?.Volume ?? 1.0f;
      if (gui.SliderH(ref x, ref y, width, ref volume, 0.1f, 1.2f, $"VOLUME: {volume:F}")) {
        if (activeExhaust_ != null) {
          activeExhaust_.Volume = volume;
          UpdateConfig(activeExhaust_);
        }
      }

      float maxTime = activeExhaust_?.MaxTime ?? 1.0f;
      if (gui.SliderH(ref x, ref y, width, ref maxTime, 0.1f, 3.0f, $"MAX TIME: {maxTime:F}")) {
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
      foreach (var e in exhaustsToRemove_) {
        e.RemoveLights();
      }

      exhausts_.Clear();
      exhaustsToRemove_.Clear();

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
      Log.Write($"[TF_Core]: Conf size: {exhaustConfig_.Count} / Car data id: {data.Car.Id}");
      int id = exhaustConfig_.FindIndex(ed => ed.CarId == data.Car.Id);
      if (id != -1) {
        Log.Write($"[TF_Core]: Override exhaust for car '{data.Car.Name}'");
        exhaustConfig_[id] = new ExhaustFifeData(data.Car.Id, data.MaxTime, data.FlamesTrigger, data.Volume);
      }
      else {
        exhaustConfig_.Add(new ExhaustFifeData(data.Car.Id, data.MaxTime, data.FlamesTrigger, data.Volume));
      }
    }
  }
}