using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ArrayExtension;
using CarModelSystem;
using FMOD.Studio;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace KN_Core {
  public class ExhaustFifeData : ISerializable {
    public int CarId;
    public float MaxTime;
    public float FlamesTrigger;
    public float Volume;

    public ExhaustFifeData() { }

    public ExhaustFifeData(int carId, float time, float trigger, float volume) {
      CarId = carId;
      MaxTime = time;
      FlamesTrigger = trigger;
      Volume = volume;
    }

    public void Serialize(BinaryWriter writer) {
      writer.Write(CarId);
      writer.Write(MaxTime);
      writer.Write(FlamesTrigger);
      writer.Write(Volume);
    }

    public bool Deserialize(BinaryReader reader, int version) {
      CarId = reader.ReadInt32();
      MaxTime = reader.ReadSingle();
      FlamesTrigger = reader.ReadSingle();
      Volume = reader.ReadSingle();

      return true;
    }
  }

  public class ExhaustData {
    private const float IntensityLow = 10.0f;
    private const float IntensityHigh = 30.0f;

    public bool Enabled { get; set; }

    public KnCar Car { get; }

    public List<ParticleSystem> Particles { get; }
    public List<GameObject> LightObjects { get; }
    public List<Light> Lights { get; }
    public FMODBaseScript.FMODEventNode Sound { get; private set; }
    public EventInstance Event { get; private set; }

    public float MaxTime { get; set; }

    public float FlamesTrigger { get; set; }

    public float Volume { get; set; }

    private bool active_;
    private bool timeout_;

    private float time_;
    private float time1_;

    private float prevRevs_;
    private bool engineLoad_;
    private bool firstPop_;

    public ExhaustData(Exhaust exhaust, CarPopExhaust script) {
      Particles = new List<ParticleSystem>();
      LightObjects = new List<GameObject>();
      Lights = new List<Light>();

      if (typeof(CarPopExhaust).GetField("m_car", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(script) is RaceCar car) {
        Car = new KnCar(car);

        int id = exhaust.ExhaustConfig.FindIndex(ed => ed.CarId == Car.Id);
        if (id != -1) {
          var conf = exhaust.ExhaustConfig[id];
          MaxTime = conf.MaxTime;
          FlamesTrigger = conf.FlamesTrigger;
          Volume = conf.Volume;
        }
        else {
          id = exhaust.ExhaustConfigDefault.FindIndex(ed => ed.CarId == Car.Id);
          if (id != -1) {
            var conf = exhaust.ExhaustConfigDefault[id];
            MaxTime = conf.MaxTime;
            FlamesTrigger = conf.FlamesTrigger;
            Volume = conf.Volume;
          }
          else {
            MaxTime = 1.0f;
            FlamesTrigger = 0.06f;
            Volume = 0.23f;
          }
        }

        var points = Car.Base.GetComponentsInChildren<CarComponentRoot>(true).Filter(e => e.typeID == (TypeID) "flame").Convert(e => e.transform);
        foreach (var p in points) {
          Particles.Add(InstantiateParticleSystem(CarResourceProvider.instance.exhaustFlame, p));
          AddLight(p);
        }
      }
      Reload(script);
    }

    public void ToggleLights(bool enabled) {
      foreach (var lo in LightObjects) {
        lo.SetActive(enabled);
      }
    }

    public void Initialize() {
      ToggleLights(false);
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

    private void Reload(CarPopExhaust script) {
      var popSound = typeof(CarPopExhaust).GetField("m_popSound", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(script) as FMODPopExhaust;
      Sound = typeof(FMODPopExhaust).GetField("m_popInstance", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(popSound) as FMODBaseScript.FMODEventNode;
      if (Sound != null) {
        var obj = typeof(FMODBaseScript.FMODEventNode).GetField("m_event", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(Sound);
        if (obj != null) {
          Event = (EventInstance) obj;
        }
      }
    }

    public void Update() {
      if (!Enabled) {
        ToggleLights(false);
        return;
      }


      float t = (Car.CarX.engineRevLimiter - Exhaust.RpmLowBound) / (Exhaust.RpmHighBound - Exhaust.RpmLowBound);
      float norm = MaxTime * t;

      float rpm = Car.CarX.rpm;
      float load = Car.CarX.load;
      engineLoad_ = load >= Exhaust.LoadTrigger;

      if (engineLoad_) {
        prevRevs_ = Car.CarX.rpm;
        active_ = false;
        timeout_ = false;
        firstPop_ = true;
        Event.setVolume(1.0f);
      }
      else {
        if (rpm <= prevRevs_ - Exhaust.RevTrigger && !timeout_) {
          if (rpm > Exhaust.RpmHighBound) {
            active_ = true;
          }
          else {
            active_ = rpm > Exhaust.RpmLowBound;
          }
        }
      }

      ToggleLights(active_);
      if (active_ && !timeout_) {
        time_ += Time.deltaTime * Exhaust.TriggerFade;
        if (time_ > norm) {
          time_ = 0.0f;
          time1_ = FlamesTrigger;
          active_ = false;
          timeout_ = true;
          firstPop_ = true;
          ToggleLights(false);
        }
        else {
          time1_ += Time.deltaTime;
          if (time1_ >= FlamesTrigger) {
            time1_ = 0.0f;
            if (firstPop_) {
              Event.setVolume(1.0f);
              firstPop_ = false;
            }
            else {
              Event.setVolume(Volume);
            }
            ToggleLights(true);
            foreach (var l in Lights) {
              l.intensity = Random.Range(IntensityLow, IntensityHigh);
            }
            PlayOnce();
          }
        }
      }
      else {
        time_ = 0.0f;
        time1_ = FlamesTrigger;
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

    public void RemoveLights() {
      foreach (var lo in LightObjects) {
        Object.Destroy(lo);
      }
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

      obj.transform.parent = parent;
      obj.transform.position = parent.position;
      obj.transform.rotation = parent.rotation;

      obj.SetActive(false);

      LightObjects.Add(obj);
      Lights.Add(light);
    }

    private static ParticleSystem InstantiateParticleSystem(ParticleSystem prefab, Transform parent) {
      if (prefab == null) {
        return null;
      }
      var particleSystem = Object.Instantiate(prefab);
      parent.AddChildResetPRS(particleSystem);
      return particleSystem;
    }
  }
}