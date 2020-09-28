using System.IO;
using CarX;

namespace KN_Core {
  public class EngineData : ISerializable {
    public int Id;
    public int Rating;
    public bool Enabled;
    public float ClutchTorque;
    public string Name;
    public string SoundId;
    public readonly CarDesc.Engine Engine;

    public EngineData() {
      Engine = new CarDesc.Engine();
    }

    public EngineData(int id,int rating, bool enabled, float clutch, string name, string soundId, CarDesc.Engine engine) {
      Id = id;
      Rating = rating;
      Enabled = enabled;
      ClutchTorque = clutch;
      Name = name;
      SoundId = soundId;
      Engine = engine;
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

    public void Deserialize(BinaryReader reader, int version) {
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
    }
  }

  public class SwapData : ISerializable {
    public const string ConfigFile = "kn_swapdata.knd";

    public int CarId;
    public int EngineId;
    public float Turbo;
    public float FinalDrive;

    public void Serialize(BinaryWriter writer) {
      writer.Write(CarId);
      writer.Write(EngineId);
      writer.Write(Turbo);
      writer.Write(FinalDrive);
    }

    public void Deserialize(BinaryReader reader, int version) {
      CarId = reader.ReadInt32();
      EngineId = reader.ReadInt32();
      Turbo = reader.ReadSingle();
      FinalDrive = reader.ReadSingle();
    }
  }
}