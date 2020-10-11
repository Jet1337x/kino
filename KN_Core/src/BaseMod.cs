using System;
using System.Collections.Generic;
using KN_Loader;
using SyncMultiplayer;

namespace KN_Core {
  public class ModTab {
    public string Name { get; }
    public Func<Gui, float, float, bool> OnGui { get; }

    public ModTab(string name, Func<Gui, float, float, bool> onGui) {
      Name = name;
      OnGui = onGui;
    }
  }

  public abstract class BaseMod {
    public Core Core { get; }
    public string Name { get; }
    public int Id { get; }

    public int Version { get; }
    public int Patch { get; }
    public int ClientVersion { get; }

    public KnSkin Icon { get; private set; }

    public int SelectedTab { get; protected set; }
    public int PrevSelectedTab { get; protected set; }

    private readonly List<ModTab> tabs_;

    public BaseMod(Core core, string name, int id, int version, int patch, int clientVersion) {
      Core = core;
      Name = name;
      Id = id;
      Version = version;
      Patch = patch;
      ClientVersion = clientVersion;

      tabs_ = new List<ModTab>(1);
      SelectedTab = 0;
      PrevSelectedTab = 0;
    }

    public virtual void OnStart() { }
    public virtual void OnStop() { }

    public void OnGui(Gui gui, ref float x, ref float y) {
      if (tabs_.Count <= 0) {
        Log.Write($"[KN_Core::BaseMod]: Unable to draw mod '{Name}' gui. Tabs are empty.");
        return;
      }

      // tabs bar
      float tx = x;
      float tabWidth = gui.MaxContentWidth / tabs_.Count;
      for (int i = 0; i < tabs_.Count; ++i) {
        if (gui.TabButton(ref x, ref y, tabWidth, Gui.ModTabHeight, Locale.Get(tabs_[i].Name), i == SelectedTab ? Skin.ModTabSkin.Active : Skin.ModTabSkin.Normal)) {
          Core.ResetPickers();
          gui.ResetSize();
          ResetTab();

          PrevSelectedTab = SelectedTab;
          SelectedTab = i;
          break;
        }
      }
      x = tx + Gui.Offset;
      y += Gui.ModTabHeight + Gui.Offset;

      // tab gui
      tabs_[SelectedTab].OnGui(gui, x, y);

      x += Gui.Offset;
    }

    public virtual void Update(int id) { }
    public virtual void LateUpdate(int id) { }
    public virtual void FixedUpdate(int id) { }

    public virtual void ResetState() { }
    public virtual void ResetTab() { }

    public virtual bool WantsCaptureInput() {
      return true;
    }

    public virtual bool LockCameraRotation() {
      return false;
    }

    public virtual bool WantsHideUi() {
      return false;
    }

    public virtual void OnReloadAll() { }

    public virtual void OnUdpData(SmartfoxDataPackage data) { }

    public virtual void OnCarLoaded(KnCar car) { }

    protected void SetIcon(KnSkin icon) {
      Icon = icon;
    }

    protected void AddTab(string name, Func<Gui, float, float, bool> onGui) {
      tabs_.Add(new ModTab(name, onGui));
    }
  }
}