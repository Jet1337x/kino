using KN_Core;

namespace KN_Lights {
  public class WorldLights {
    private Core core_;
    public WorldLights(Core core) {
      core_ = core;
    }

    public void ResetState() { }

    public void ResetPickers() { }

    public void OnStop() { }

    public void OnStart() { }

    public void Update() { }

    public void OnGUI(Gui gui, ref float x, ref float y) {
      if (gui.Button(ref x, ref y, "DUMMY", Skin.Button)) { }
    }
  }
}