using System;
using System.IO;
using CarX;

namespace KN_Core {
  public class EngineData : ISerializable {
    public readonly int Id;
    public readonly bool Enabled;
    public readonly float ClutchTorque;
    public readonly string Name;
    public readonly string SoundId;
    public readonly CarDesc.Engine Engine;

    public EngineData(int id, bool enabled, float clutch, string name, string soundId, CarDesc.Engine engine) {
      Id = id;
      Enabled = enabled;
      ClutchTorque = clutch;
      Name = name;
      SoundId = soundId;
      Engine = engine;
    }

    public void Serialize(BinaryWriter writer) {
      throw new NotImplementedException();
    }

    public void Deserialize(BinaryReader reader, int version) {
      throw new NotImplementedException();
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