using System;
using System.Collections.Generic;
using System.IO;

namespace KN_Core {
  public class SwapData {
    public const string ConfigFile = "kn_swapdata.knd";

    public int carId;
    public int engineId;
    public float turbo;

    public void Serialize(BinaryWriter writer) {
      writer.Write(carId);
      writer.Write(engineId);
      writer.Write(turbo);
    }

    public void Deserialize(BinaryReader reader) {
      carId = reader.ReadInt32();
      engineId = reader.ReadInt32();
      turbo = reader.ReadSingle();
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