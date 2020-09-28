using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Bootstrap;
using GameOverlay;

namespace KN_Core {

  public static class AccessValidator {
    public enum Status {
      Denied,
      Granted,
      Loading
    }

    private struct AccessData {
      public ulong Sid;
      public int Rights;
    }

    public static bool Initialized { get; private set; }

    private static ulong sid_;

    private static bool disabled_;
    private static IEnumerable<AccessData> data_;

    public static void Initialize(string remote) {
      data_ = LoadFromRemote(remote);

      if (data_ == null) {
        disabled_ = true;
        Initialized = true;
      }
    }

    public static Status IsGranted(int rights, string module, BaseMod mod = null, string modId = "") {
      if (disabled_ || data_ == null) {
        return Status.Denied;
      }

      if (!Initialized) {
        return Status.Loading;
      }

      foreach (var d in data_) {
        if (d.Sid == sid_) {
          bool granted = (rights & d.Rights) != 0;
          Log.Write(granted ? $"[KN_AccessValidator]: You allowed to use '{module}'!" : $"[KN_AccessValidator]: You not allowed to use '{module}'!");
          if (granted) {
            return Status.Granted;
          }
        }
      }

      if (mod != null) {
        Disable(mod, modId);
      }

      return Status.Denied;
    }

    public static void Update() {
      if (!Initialized && Overlay.instance is SteamOverlay overlay) {
        sid_ = overlay.steamID.m_SteamID;
        Initialized = true;
      }
    }

    private static void Disable(BaseMod mod, string id) {
      if (Chainloader.PluginInfos.ContainsKey(id)) {
        var air = Chainloader.PluginInfos[id];
        if (air != null) {
          air.Instance.enabled = false;
          Core.CoreInstance.RemoveMod(mod);
        }
      }
    }

    private static IEnumerable<AccessData> LoadFromRemote(string remote) {
      var data = new List<AccessData>();
      try {
        var bytes = WebDataLoader.LoadAsBytes(remote);
        using (var stream = new MemoryStream(bytes)) {
          using (var reader = new BinaryReader(stream)) {
            int size = reader.ReadInt32();
            for (int i = 0; i < size; i++) {
              ulong sid = reader.ReadUInt64();
              int rights = reader.ReadInt32();

              data.Add(new AccessData {
                Sid = sid,
                Rights = rights
              });
            }
          }
        }
        Console.WriteLine($"[KN_AccessValidator]: Rights data successfully loaded, size: {data.Count}");
      }
      catch (Exception e) {
        Console.WriteLine($"[KN_AccessValidator]: Unable to load rights data from remote, {e.Message}");
        return null;
      }
      return data;
    }
  }
}