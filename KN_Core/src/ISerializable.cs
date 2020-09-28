using System;
using System.Collections.Generic;
using System.IO;

namespace KN_Core {
  public interface ISerializable {
    void Serialize(BinaryWriter writer);
    void Deserialize(BinaryReader reader, int version);
  }

  public static class DataSerializer {
    public static bool Serialize(string module, IList<ISerializable> data, string file) {
      try {
        using (var memoryStream = new MemoryStream()) {
          using (var writer = new BinaryWriter(memoryStream)) {
            writer.Write(KnConfig.Version);
            writer.Write(data.Count);
            foreach (var d in data) {
              d.Serialize(writer);
            }
            using (var fileStream = File.Open(file, FileMode.Create)) {
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

    public static bool Deserialize<T>(string module, Stream stream, out List<ISerializable> data) where T : ISerializable, new() {
      try {
        data = new List<ISerializable>();
        using (var reader = new BinaryReader(stream)) {
          int version = reader.ReadInt32();
          int size = reader.ReadInt32();
          for (int i = 0; i < size; i++) {
            var d = new T();
            d.Deserialize(reader, version);
            data.Add(d);
          }
        }
      }
      catch (Exception e) {
        Log.Write($"[{module}]: Unable to read stream, {e.Message}");
        data = default;
        return false;
      }
      return true;
    }

    public static bool Deserialize<T>(string module, string file, out List<ISerializable> data) where T : ISerializable, new() {
      try {
        data = new List<ISerializable>();
        using (var memoryStream = new MemoryStream(File.ReadAllBytes(file))) {
          return Deserialize<T>(module, memoryStream, out data);
        }
      }
      catch (Exception e) {
        Log.Write($"[{module}]: Unable to read file '{file}', {e.Message}");
        data = default;
        return false;
      }
    }

    public static bool Deserialize<T>(string module, byte[] bytes, out List<ISerializable> data) where T : ISerializable, new() {
      try {
        data = new List<ISerializable>();
        using (var memoryStream = new MemoryStream(bytes)) {
          return Deserialize<T>(module, memoryStream, out data);
        }
      }
      catch (Exception e) {
        Log.Write($"[{module}]: Unable to bytes, {e.Message}");
        data = default;
        return false;
      }
    }
  }
}