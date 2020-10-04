using System.Collections.Generic;
using System.IO;
using CarX;
using KN_Loader;

namespace KN_Core {
  public class EngineData : ISerializable {
    public int Id { get; private set; }
    public int Rating { get; private set; }
    public bool Enabled { get; private set; }
    public float ClutchTorque { get; private set; }
    public string Name { get; private set; }
    public string SoundId { get; private set; }
    public CarDesc.Engine Engine { get; }

    public EngineData() {
      Engine = new CarDesc.Engine();
    }

    public void Serialize(BinaryWriter writer) {
      writer.Write(Id);
      writer.Write(Rating);
      writer.Write(Enabled);
      writer.Write(ClutchTorque);
      writer.Write(Name);
      writer.Write(SoundId);
      writer.Write(Engine.inertiaRatio);
      writer.Write(Engine.maxTorque);
      writer.Write(Engine.revLimiter);
      writer.Write(Engine.turboCharged);
      writer.Write(Engine.turboPressure);
      writer.Write(Engine.brakeTorqueRatio);
      writer.Write(Engine.revLimiterStep);
      writer.Write(Engine.useTC);
      writer.Write(Engine.cutRPM);
      writer.Write(Engine.idleRPM);
      writer.Write(Engine.maxTorqueRPM);
    }

    public bool Deserialize(BinaryReader reader, int version) {
      Id = reader.ReadInt32();
      Rating = reader.ReadInt32();
      Enabled = reader.ReadBoolean();
      ClutchTorque = reader.ReadSingle();
      Name = reader.ReadString();
      SoundId = reader.ReadString();
      Engine.inertiaRatio = reader.ReadSingle();
      Engine.maxTorque = reader.ReadSingle();
      Engine.revLimiter = reader.ReadSingle();
      Engine.turboCharged = reader.ReadBoolean();
      Engine.turboPressure = reader.ReadSingle();
      Engine.brakeTorqueRatio = reader.ReadSingle();
      Engine.revLimiterStep = reader.ReadSingle();
      Engine.useTC = reader.ReadBoolean();
      Engine.cutRPM = reader.ReadSingle();
      Engine.idleRPM = reader.ReadSingle();
      Engine.maxTorqueRPM = reader.ReadSingle();

      return true;
    }
  }

  public class EngineBalance : ISerializable {
    public int CarId { get; private set; }
    public int Rating { get; private set; }

    public void Serialize(BinaryWriter writer) {
      writer.Write(CarId);
      writer.Write(Rating);
    }

    public bool Deserialize(BinaryReader reader, int version) {
      CarId = reader.ReadInt32();
      Rating = reader.ReadInt32();

      return true;
    }
  }

  public class SwapData : ISerializable {
    public const string ConfigFile = "kn_swapdata.knd";
    public const int MinVersion = 127;

    public class Engine {
      public int EngineId;
      public float Turbo;
      public float FinalDrive;
    }

    public int CarId { get; private set; }
    public int CurrentEngine { get; private set; }
    public List<Engine> Engines { get; }

    public SwapData() {
      Engines = new List<Engine>();
      CurrentEngine = -1;
    }

    public SwapData(int carId) {
      CarId = carId;
      CurrentEngine = -1;
      Engines = new List<Engine>();
    }

    public void AddEngine(Engine engine) {
      if (engine == null) {
        return;
      }

      for (int i = 0; i < Engines.Count; ++i) {
        if (Engines[i].EngineId == engine.EngineId) {
          CurrentEngine = i;
          Engines[i].Turbo = engine.Turbo;
          Engines[i].FinalDrive = engine.FinalDrive;
          return;
        }
      }

      Engines.Add(engine);
      CurrentEngine = Engines.IndexOf(engine);
      Log.Write($"[KN_Core::SwapsConfig]: Added new engine '{engine.EngineId}', size: {Engines.Count}");
    }

    public void RemoveEngine(Engine engine) {
      if (engine == null) {
        return;
      }

      Engines.Remove(engine);
      CurrentEngine = -1;
      Log.Write($"[KN_Core::SwapsConfig]: Engine '{engine.EngineId}' was removed, size: {Engines.Count}");
    }

    public Engine GetCurrentEngine() {
      if (Engines == null || CurrentEngine < 0 || CurrentEngine > Engines.Count) {
        return null;
      }
      return Engines[CurrentEngine];
    }

    public bool SetCurrentEngine(int engineId) {
      if (engineId <= 0) {
        CurrentEngine = -1;
        return false;
      }

      for (int i = 0; i < Engines.Count; ++i) {
        if (Engines[i].EngineId == engineId) {
          CurrentEngine = i;
          return true;
        }
      }
      return false;
    }

    public void Serialize(BinaryWriter writer) {
      writer.Write(CarId);
      writer.Write(CurrentEngine);
      writer.Write(Engines.Count);
      foreach (var engine in Engines) {
        writer.Write(engine.EngineId);
        writer.Write(engine.Turbo);
        writer.Write(engine.FinalDrive);
      }
    }

    public bool Deserialize(BinaryReader reader, int version) {
      if (version < MinVersion) {
        return false;
      }

      CarId = reader.ReadInt32();
      CurrentEngine = reader.ReadInt32();
      int size = reader.ReadInt32();
      for (int i = 0; i < size; ++i) {
        Engines.Add(new Engine {
          EngineId = reader.ReadInt32(),
          Turbo = reader.ReadSingle(),
          FinalDrive = reader.ReadSingle()
        });
      }
      return true;
    }
  }

  public class NetworkSwap {
    public int NwId { get; }
    public RaceCar Car { get; }
    public float Clutch { get; }
    public float FinalDrive { get; }
    public string SoundId { get; }
    public CarDesc.Engine Engine { get; }

    public SwapData.Engine Swap;
    public EngineData EngineData;

    public bool Reload;
    public bool ReloadNext;

    public NetworkSwap(RaceCar car) {
      NwId = car.networkPlayer.NetworkID;
      Car = car;

      var desc = Car.GetDesc();
      Engine = new CarDesc.Engine();
      Swaps.CopyEngine(desc.carXDesc.engine, Engine);

      Clutch = car.carX.clutchMaxTorque;
      FinalDrive = car.carX.finaldrive;
      SoundId = car.metaInfo.name;
    }

    public void SetData(SwapData.Engine swap, EngineData engineData) {
      Swap = swap;
      EngineData = engineData;
    }
  }
}