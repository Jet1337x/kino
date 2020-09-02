using SyncMultiplayer;

namespace KN_Core {
  public abstract class BaseMod {
    public Core Core { get; }
    public string Name { get; }
    public int Id { get; }

    public int Version { get; }
    public int ClientVersion { get; }

    public BaseMod(Core core, string name, int id, int version, int clientVersion) {
      Core = core;
      Name = name;
      Id = id;
      Version = version;
      ClientVersion = clientVersion;

      Core.CarPicker.OnCarLoaded += OnCarLoaded;
    }

    public virtual void OnStart() { }
    public virtual void OnStop() { }

    public virtual void OnGUI(int id, Gui gui, ref float x, ref float y) { }

    public virtual void Update(int id) { }
    public virtual void LateUpdate(int id) { }
    public virtual void FixedUpdate(int id) { }

    public virtual void ResetState() { }
    public virtual void ResetPickers() { }

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

    protected virtual void OnCarLoaded() { }
  }
}