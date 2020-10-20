using System;
using System.IO;
using CarModelSystem;
using CarX;
using KN_Core;
using KN_Loader;
using SyncMultiplayer;
using UnityEngine;

namespace KN_Lights {
  public class CarLights : ISerializable {
    private const int MinVersion = 200;

    public const float BrakePower = 2.5f;

    public LightsSet HeadLights { get; private set; }
    public LightsSet TailLights { get; private set; }
    public DashLight DashLight { get; private set; }

    private bool debug_;
    public bool Debug {
      get => debug_;
      set {
        debug_ = value;
        HeadLights.Debug = debug_;
        TailLights.Debug = debug_;
        DashLight.Debug = debug_;
      }
    }

#if KN_DEV_TOOLS
    public bool ForceIl { get; set; }
#endif

    public KnCar Car { get; private set; }
    public ulong Sid { get; private set; }
    public string Name { get; private set; }

    public int CarId { get; private set; }
    public bool IsNetworkCar { get; private set; }

    private Quality quality_;
    private bool own_;

    public bool IsNwCar;

    private CARXCar cxCar_;

    private bool discardedMain_;
    private bool discardedExtras_;

    public CarLights() {
      IsNwCar = false;
      Sid = 0;
      Name = string.Empty;

      HeadLights = new LightsSet(Quality.Medium, own_, Color.white, LightsSet.DefaultIllumination, LightsSet.DefaultRange,
        0.0f, 2300.0f, 150.0f, new Vector3(0.6f, 0.6f, 1.9f), true, true, false);
      TailLights = new LightsSet(Quality.Medium, own_, Color.red, LightsSet.DefaultIllumination, LightsSet.DefaultRange,
        0.0f, 30.0f, 170.0f, new Vector3(0.6f, 0.6f, -1.6f), true, true, true);
      DashLight = new DashLight(Color.white, DashLight.DefaultBrightness, DashLight.DefaultRange, new Vector3(0.0f, 0.6f, 1.0f), true);
    }

    private CarLights(Quality quality, int carId, bool nwCar, string name) {
      quality_ = quality;
      IsNwCar = false;
      Name = name;
      CarId = carId;
      IsNetworkCar = nwCar;
    }

    public void Dispose() {
      HeadLights.Dispose();
      TailLights.Dispose();
      DashLight.Dispose();

      if (!KnCar.IsNull(Car)) {
        if (Singletone<Simulator>.instance) {
          Singletone<Simulator>.instance.OnUpdateWheelsEvent -= CarUpdate;
        }
      }
    }

    public CarLights Copy() {
      var lights = new CarLights(quality_, CarId, IsNetworkCar, Name) {
        HeadLights = HeadLights.Copy(),
        TailLights = TailLights.Copy(),
        DashLight = DashLight.Copy(),
      };

      return lights;
    }

    public void Attach(Quality quality, KnCar car, bool own) {
      own_ = own;
      discardedMain_ = false;
      discardedExtras_ = false;
      quality_ = quality;
      Car = car;
      Sid = Car.Base.networkPlayer?.PlayerId.uid ?? 0;
      Name = car.Name;
      CarId = car.Id;
      IsNetworkCar = car.IsNetworkCar;

      HeadLights.Attach(quality_, own_, car, false, Color.white);
      TailLights.Attach(quality_, own_, car, true, Color.red);
      DashLight.Attach(car);

      cxCar_ = Car.Base.GetComponent<CARXCar>();
      if (Singletone<Simulator>.instance) {
        Singletone<Simulator>.instance.OnUpdateWheelsEvent += CarUpdate;
      }
    }

    public void Discard(bool main, bool extras) {
      if (discardedMain_ != main) {
        discardedMain_ = main;
        HeadLights.Enabled = !discardedMain_;
        HeadLights.Illumination = !extras;
        TailLights.Enabled = !discardedMain_;
        TailLights.Illumination = !extras;
      }
      if (discardedExtras_ != extras) {
        discardedExtras_ = extras;
        HeadLights.Illumination = !discardedExtras_;
        TailLights.Illumination = !discardedExtras_;
        DashLight.Enabled = !discardedExtras_;
      }
    }

    public void LateUpdate() {
      if (!KnCar.IsNull(Car) && cxCar_ != null) {
        if (!discardedMain_) {
          float power = TailLights.Brightness;
          if (cxCar_.brake > 0.2f) {
            power = TailLights.Brightness * BrakePower;
          }
          TailLights.SetIntensity(power);
        }
        if (!discardedExtras_) {
          float power = TailLights.IlIntensity;
          if (cxCar_.brake > 0.2f) {
            power = TailLights.IlIntensity * BrakePower;
          }
          TailLights.SetIlluminationIntensity(power);

#if KN_DEV_TOOLS
          if (ForceIl) {
            TailLights.SetIlluminationIntensity(power * BrakePower);
          }
#endif
        }
      }
    }

    public void ApplyQuality(Quality quality) {
      quality_ = quality;
      HeadLights.ApplyQuality(quality);
      TailLights.ApplyQuality(quality);
    }

    private void CarUpdate() {
      if (!KnCar.IsNull(Car)) {
        bool enabled = !discardedMain_ && (TailLights.EnabledLeft || TailLights.EnabledRight);
        Car.Base.carModel.SetLightsState(enabled, CarLightGroup.Brake);
      }
    }

    public void Send(int id, Udp udp) {
      var data = new SmartfoxDataPackage(PacketId.Subroom);
      data.Add("1", (byte) 25);
      data.Add("type", Udp.TypeLights);
      data.Add("id", id);

      data.Add("c", KnUtils.EncodeColor(HeadLights.Color));
      data.Add("p", HeadLights.Pitch);
      data.Add("hlb", HeadLights.Brightness);
      data.Add("hla", HeadLights.Angle);
      data.Add("pt", TailLights.Pitch);
      data.Add("tlb", TailLights.Brightness);
      data.Add("tla", TailLights.Angle);

      data.Add("hle", HeadLights.EnabledLeft);
      data.Add("lre", HeadLights.EnabledRight);
      data.Add("hx", HeadLights.Offset.x);
      data.Add("hy", HeadLights.Offset.y);
      data.Add("hz", HeadLights.Offset.z);

      data.Add("tle", TailLights.EnabledLeft);
      data.Add("tre", TailLights.EnabledRight);
      data.Add("tx", TailLights.Offset.x);
      data.Add("ty", TailLights.Offset.y);
      data.Add("tz", TailLights.Offset.z);

      data.Add("de", DashLight.Enabled);
      data.Add("dc", KnUtils.EncodeColor(DashLight.Color));

      udp.Send(data);
    }

    public void ModifyFrom(SmartfoxDataPackage data, int id) {
      IsNwCar = true;

      try {
        var color = KnUtils.DecodeColor(data.Data.GetInt("c"));
        float hlPitch = data.Data.GetFloat("p");
        float hlBrightness = data.Data.GetFloat("hlb");
        float hlAngle = data.Data.GetFloat("hla");
        float tlPitch = data.Data.GetFloat("pt");
        float tlBrightness = data.Data.GetFloat("tlb");
        float tlAngle = data.Data.GetFloat("tla");

        bool hlEnabledL = data.Data.GetBool("hle");
        bool hlEnabledR = data.Data.GetBool("lre");
        float x = data.Data.GetFloat("hx");
        float y = data.Data.GetFloat("hy");
        float z = data.Data.GetFloat("hz");
        var hlOffset = new Vector3(x, y, z);

        bool tlEnabledL = data.Data.GetBool("tle");
        bool tlEnabledR = data.Data.GetBool("tre");
        x = data.Data.GetFloat("tx");
        y = data.Data.GetFloat("ty");
        z = data.Data.GetFloat("tz");
        var tlOffset = new Vector3(x, y, z);

        bool dashEnabled = data.Data.GetBool("de");
        var dashColor = KnUtils.DecodeColor(data.Data.GetInt("dc"));

        HeadLights.Color = color;
        HeadLights.Pitch = hlPitch;
        HeadLights.Brightness = hlBrightness;
        HeadLights.Angle = hlAngle;
        HeadLights.Offset = hlOffset;
        HeadLights.EnabledLeft = hlEnabledL;
        HeadLights.EnabledRight = hlEnabledR;

        TailLights.Pitch = tlPitch;
        TailLights.Brightness = tlBrightness;
        TailLights.Angle = tlAngle;
        TailLights.Offset = tlOffset;
        TailLights.EnabledLeft = tlEnabledL;
        TailLights.EnabledRight = tlEnabledR;

        DashLight.Enabled = dashEnabled;
        DashLight.Color = dashColor;
      }
      catch (Exception e) {
        Log.Write($"[KN_Lights::Car]: Failed to modify car lights for {id}, {e.Message}");
      }
    }

    public void Serialize(BinaryWriter writer) {
      writer.Write(CarId);
      writer.Write(IsNetworkCar);
      writer.Write(Sid);

      HeadLights.Serialize(writer);
      TailLights.Serialize(writer);
      DashLight.Serialize(writer);
    }

    public bool Deserialize(BinaryReader reader, int version) {
      if (version < MinVersion) {
        return false;
      }

      CarId = reader.ReadInt32();
      IsNetworkCar = reader.ReadBoolean();
      Sid = reader.ReadUInt64();

      HeadLights = new LightsSet(reader, false);
      TailLights = new LightsSet(reader, true);
      DashLight = new DashLight(reader);

      return true;
    }
  }
}