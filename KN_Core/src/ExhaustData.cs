using System.Collections.Generic;
using System.Reflection;
using FMOD.Studio;
using UnityEngine;

namespace KN_Core {
  internal class ExhaustData {
    private const float IntensityLow = 10.0f;
    private const float IntensityHigh = 30.0f;

    public bool Enabled { get; set; }

    public TFCar Car { get; }

    public List<ParticleSystem> Particles { get; }
    public List<GameObject> LightObjects { get; }
    public List<Light> Lights { get; }
    public FMODBaseScript.FMODEventNode Sound { get; private set; }
    public EventInstance Event { get; private set; }

    private bool active_;
    private bool timeout_;

    private float time_;
    private float time1_;

    private float prevRevs_;
    private bool engineLoad_;
    private bool firstPop_;

    private readonly Exhaust exhaust_;

    public ExhaustData(Exhaust exhaust, CarPopExhaust script) {
      exhaust_ = exhaust;
      Particles = new List<ParticleSystem>();
      LightObjects = new List<GameObject>();
      Lights = new List<Light>();

      if (typeof(CarPopExhaust).GetField("m_car", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(script) is RaceCar car) {
        //todo: ghost check
        Car = new TFCar(car);
      }

      Reload(script);
    }

    public void Initialize() {
      var scripts = Object.FindObjectsOfType<CarPopExhaust>();
      if (scripts != null && scripts.Length > 0) {
        foreach (var s in scripts) {
          if (typeof(CarPopExhaust).GetField("m_car", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(s) is RaceCar car) {
            if (car != null && car.transform.position == Car.Transform.position && car == Car.Base) {
              Reload(s);
              break;
            }
          }
        }
      }
    }

    public void DisposeLights() {
      foreach (var lo in LightObjects) {
        Object.Destroy(lo);
      }
    }

    private void Reload(CarPopExhaust script) {
      DisposeLights();

      Particles.Clear();
      LightObjects.Clear();
      Lights.Clear();

      if (!(typeof(CarPopExhaust).GetField("m_exhaustFlameInstances", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(script) is List<ParticleSystem> particles)) {
        Log.Write($"[TF_Core]: Bad script on {Car.Id} / {Car.Name}");
      }
      else {
        Log.Write($"[TF_Core]: Particles count {particles.Count} on {Car.Name}");
        foreach (var p in particles) {
          Particles.Add(p);
          AddLight(p.transform);
        }
      }

      var popSound = typeof(CarPopExhaust).GetField("m_popSound", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(script) as FMODPopExhaust;
      Sound = typeof(FMODPopExhaust).GetField("m_popInstance", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(popSound) as FMODBaseScript.FMODEventNode;
      if (Sound != null) {
        var obj = typeof(FMODBaseScript.FMODEventNode).GetField("m_event", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(Sound);
        if (obj != null) {
          Event = (EventInstance) obj;
        }
      }

      Log.Write($"[TF_Core]: Constructed exhaust for car {Car.Id} / {Car.Name} / Particles: {Particles.Count} / Lights: {LightObjects.Count}");
    }

    public void Update() {
      if (!Enabled) {
        return;
      }

      float rpm = Car.CarX.rpm;
      float load = Car.CarX.load;
      engineLoad_ = load >= Exhaust.LoadTrigger;

      float t = (Car.CarX.engineRevLimiter - Exhaust.RpmLowBound) / (Exhaust.RpmHighBound - Exhaust.RpmLowBound);
      float norm = exhaust_.MaxTime * t;

      if (engineLoad_) {
        prevRevs_ = Car.CarX.rpm;
        active_ = false;
        timeout_ = false;
        firstPop_ = true;
        foreach (var lo in LightObjects) {
          lo.SetActive(false);
          Event.setVolume(1.0f);
        }
      }
      else {
        if (rpm <= prevRevs_ - Exhaust.RevTrigger && !timeout_) {
          if (rpm > Exhaust.RpmHighBound) {
            active_ = true;
          }
          else {
            active_ = rpm > Exhaust.RpmLowBound;
            if (!active_) {
              foreach (var lo in LightObjects) {
                lo.SetActive(false);
              }
            }
          }
        }
      }

      if (active_ && !timeout_) {
        time_ += Time.deltaTime * Exhaust.TriggerFade;
        if (time_ > norm) {
          time_ = 0.0f;
          time1_ = exhaust_.FlamesTrigger;
          active_ = false;
          timeout_ = true;
          firstPop_ = true;
          foreach (var lo in LightObjects) {
            lo.SetActive(false);
          }
        }
        else {
          time1_ += Time.deltaTime;
          if (time1_ >= exhaust_.FlamesTrigger) {
            time1_ = 0.0f;
            if (firstPop_) {
              Event.setVolume(1.0f);
              firstPop_ = false;
            }
            else {
              Event.setVolume(exhaust_.Volume);
            }
            foreach (var lo in LightObjects) {
              lo.SetActive(true);
            }
            foreach (var l in Lights) {
              l.intensity = Random.Range(IntensityLow, IntensityHigh);
            }
            PlayOnce();
          }
        }
      }
      else {
        time_ = 0.0f;
        time1_ = exhaust_.FlamesTrigger;
      }
    }

    private void PlayOnce() {
      //just in case
      if (!Enabled) {
        return;
      }

      foreach (var p in Particles) {
        p.Stop(true);
        p.Play(true);
      }
      Sound.Stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
      Sound.Start();
    }

    private void AddLight(Transform parent) {
      var obj = new GameObject();
      obj.AddComponent<Light>();
      var light = obj.GetComponent<Light>();
      light.color = new Color32(0xFE, 0x9B, 0x23, 0xff);
      light.type = LightType.Spot;
      light.intensity = IntensityHigh - IntensityLow;
      light.spotAngle = 180.0f;
      light.innerSpotAngle = 10.0f;
      light.range = 2.0f;
      obj.SetActive(false);

      obj.transform.parent = parent;
      obj.transform.position = parent.position;
      obj.transform.rotation = parent.rotation;

      LightObjects.Add(obj);
      Lights.Add(light);
    }
  }
}