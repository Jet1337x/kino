using System.Collections.Generic;
using UnityEngine;

namespace KN_Core {
  public class Exhaust {
    public const float MaxDistance = 100.0f;

    public const float TriggerFade = 3.0f;
    public const float RevTrigger = 500.0f;
    public const float LoadTrigger = 0.55f;
    public const float RpmLowBound = 3000.0f;
    public const float RpmHighBound = 5000.0f;

    private float maxTime_;
    public float MaxTime => maxTime_;

    private float flamesTrigger_;
    public float FlamesTrigger => flamesTrigger_;

    private float volume_;
    public float Volume => volume_;

    private readonly List<ExhaustData> exhausts_;
    private readonly List<ExhaustData> exhaustsToRemove_;
    private readonly Core core_;

    public Exhaust(Core core) {
      core_ = core;
      exhausts_ = new List<ExhaustData>();
      exhaustsToRemove_ = new List<ExhaustData>();
      maxTime_ = 1.0f;
      flamesTrigger_ = 0.06f;
      volume_ = 0.23f;
    }

    public void Reset() {
      foreach (var e in exhausts_) {
        e.ToggleLights(false);
      }
      foreach (var e in exhaustsToRemove_) {
        e.ToggleLights(false);
        exhausts_.Remove(e);
      }
    }

    public void Update() {
      if (core_.IsInGarage) {
        return;
      }

      foreach (var e in exhausts_) {
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
      }

      if (exhaustsToRemove_.Count > 0) {
        foreach (var e in exhaustsToRemove_) {
          Log.Write($"[TF_Core]: Removed for car '{e.Car.Name}'");
          exhausts_.Remove(e);
        }
        exhaustsToRemove_.Clear();
      }
    }

    public void OnGUI(Gui gui, ref float x, ref float y, float width) {
      if (gui.SliderH(ref x, ref y, width, ref volume_, 0.1f, 1.2f, $"VOLUME: {Volume:F}")) {
        foreach (var e in exhausts_) {
          e.Event.setVolume(Volume);
        }
      }
      gui.SliderH(ref x, ref y, width, ref maxTime_, 0.1f, 3.0f, $"MAX TIME: {MaxTime:F}");
      gui.SliderH(ref x, ref y, width, ref flamesTrigger_, 0.05f, 0.5f, $"FLAMES TRIGGER: {FlamesTrigger:F}");
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
        Log.Write($"{scripts.Length}");
        foreach (var s in scripts) {
          exhausts_.Add(new ExhaustData(this, s));
        }
      }
    }
  }
}