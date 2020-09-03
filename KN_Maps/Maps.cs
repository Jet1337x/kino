using KN_Core;

namespace KN_Maps {
  public class Maps : BaseMod {

    private readonly SafeFlyMod fly_;

    public Maps(Core core, int version, int clientVersion) : base(core, "MAPS", 4, version, clientVersion) {
      fly_ = new SafeFlyMod(core);
    }

    public override void OnStart() {
      fly_.OnStart();
    }

    public override void OnStop() {
      fly_.OnStop();
    }

    protected override void OnCarLoaded() {
      fly_.OnCarLoaded();
    }

    public override void ResetState() { }

    public override void ResetPickers() { }

    public override void OnGUI(int id, Gui gui, ref float x, ref float y) {
      const float width = Gui.Width * 2.0f;

      fly_.OnGui(gui, ref x, ref y, width);
    }

    public override void Update(int id) {
      fly_.Update();
    }
  }
}