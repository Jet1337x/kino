using System;
using System.Collections.Generic;
using System.IO;

namespace KN_Core {
  public interface ISerializable {
    void Serialize(BinaryWriter writer);
    void Deserialize(BinaryReader reader);
  }

  public static class DataSerializer {
    public static bool Serialize(string module, List<ISerializable> data, string file) {
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
        Log.Write($"[{module}]: Data successfully written to '{file}'");
      }
      catch (Exception e) {
        Log.Write($"[{module}]: Unable to write file '{file}', {e.Message}");
        return false;
      }
      return true;
    }

    public static bool Deserialize<T>(string module, string file, out List<ISerializable> data) where T : ISerializable, new() {
      try {
        using (var memoryStream = new MemoryStream(File.ReadAllBytes(KnConfig.BaseDir + file))) {
          using (var reader = new BinaryReader(memoryStream)) {
            data = new List<ISerializable>();
            reader.ReadInt32(); //unused
            int size = reader.ReadInt32();
            for (int i = 0; i < size; i++) {
              var d = new T();
              d.Deserialize(reader);
              data.Add(d);
            }
          }
        }
      }
      catch (Exception e) {
        Log.Write($"[{module}]: Unable to read file '{file}', {e.Message}");
        data = default;
        return false;
      }
      return true;
    }
  }
}