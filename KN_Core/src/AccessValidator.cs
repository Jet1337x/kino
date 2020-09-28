using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Bootstrap;
using GameOverlay;

namespace KN_Core {
  public class AccessValidator {
    public bool Allowed { get; private set; }
    public bool Initialized { get; set; }

    private IEnumerable<string> data_;

    private readonly string module_;
    private readonly BaseMod mod_;
    private readonly string modId_;

    private readonly bool separateModule_;

    public AccessValidator(string module, BaseMod mod, string id) {
      module_ = module;
      mod_ = mod;
      modId_ = id;
      separateModule_ = true;
    }

    public AccessValidator(string module) {
      module_ = module;
      separateModule_ = false;
    }

    public void Initialize(string remote) {
      Allowed = false;
      Initialized = false;
      data_ = WebDataLoader.LoadAsList(remote, module_);

      if (data_ == null) {
        Initialized = true;
        Disable();
      }
    }

    public void Update() {
      if (data_ == null || Initialized) {
        return;
      }

      if (!(Overlay.instance is SteamOverlay overlay)) {
        return;
      }
      Initialized = true;

      ulong sid = overlay.steamID.m_SteamID;

      foreach (string line in data_) {
        string bytes = Encoding.UTF8.GetString(Convert.FromBase64String(line));
        ulong id = Convert.ToUInt64(bytes);
        if (id == sid) {
          Allowed = true;
          return;
        }
      }

      Log.Write($"[KN_AccessValidator]: You not allowed to use '{module_}'!");
      Allowed = false;

      if (Initialized && !Allowed) {
        Disable();
      }
    }

    private void Disable() {
      if (!separateModule_) {
        return;
      }

      if (Chainloader.PluginInfos.ContainsKey(modId_)) {
        var air = Chainloader.PluginInfos[modId_];
        if (air != null) {
          air.Instance.enabled = false;
          Core.CoreInstance.RemoveMod(mod_);
        }
      }
    }
  }
}