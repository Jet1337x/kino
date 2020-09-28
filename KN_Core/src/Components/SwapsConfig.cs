using System;
using System.Collections.Generic;
using System.IO;
using CarX;

namespace KN_Core {
  public class EngineData {
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
  }

  public class SwapData {
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

    public void Deserialize(BinaryReader reader) {
      CarId = reader.ReadInt32();
      EngineId = reader.ReadInt32();
      Turbo = reader.ReadSingle();
      FinalDrive = reader.ReadSingle();
    }
  }

  public static class SwapsDataSerializer {
    public static bool Serialize(List<SwapData> data, string file) {
      try {
        using (var memoryStream = new MemoryStream()) {
          using (var writer = new BinaryWriter(memoryStream)) {
            writer.Write(KnConfig.Version);
            writer.Write(data.Count);
            foreach (var d in data) {
              d.Serialize(writer);
            }
            using (var fileStream = File.Open(KnConfig.BaseDir + file, FileMode.Create)) {
              memoryStream.WriteTo(fileStream);
            }
          }
        }
        Log.Write($"[KN_Swaps]: Swap data successfully written to '{file}'");
      }
      catch (Exception e) {
        Log.Write($"[KN_Swaps]: Unable to write file '{file}', {e.Message}");
        return false;
      }
      return true;
    }

    public static bool Deserialize(string file, out List<SwapData> data) {
      try {
        using (var memoryStream = new MemoryStream(File.ReadAllBytes(KnConfig.BaseDir + file))) {
          using (var reader = new BinaryReader(memoryStream)) {
            data = new List<SwapData>();
            reader.ReadInt32(); //unused
            int size = reader.ReadInt32();
            for (int i = 0; i < size; i++) {
              var d = new SwapData();
              d.Deserialize(reader);
              data.Add(d);
            }
          }
        }
      }
      catch (Exception e) {
        Log.Write($"[KN_Air]: Unable to read file '{file}', {e.Message}");
        data = default;
        return false;
      }
      return true;
    }
  }
}