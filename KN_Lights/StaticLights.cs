using KN_Core;

namespace KN_Lights {
  public class StaticLights {
    private readonly Core core_;

    public StaticLights(Core core) {
      core_ = core;
    }

    public void OnStart() { }

    public void OnStop() { }

    public void Update() { }

    public void OnGUI(Gui gui, ref float x, ref float y) {
      const float width = Gui.Width * 2.0f;
      const float height = Gui.Height;

      if (gui.Button(ref x, ref y, width, height, "DUMMY", Skin.Button)) { }
    }
  }
}