using KN_Core;

namespace KN_Maps {
  public class Maps : BaseMod {

    private readonly SafeFlyMod fly_;
    private readonly MapList mapList_;

    public Maps(Core core, int version, int patch, int clientVersion) : base(core, "maps", 4, version, patch, clientVersion) {
      fly_ = new SafeFlyMod(core);

      mapList_ = new MapList();
      mapList_.OnMapSelected += LoadMap;
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
      const float width = Gui.Width * 1.5f;

      float yBegin = y;

      x += Gui.OffsetSmall;

      fly_.OnGui(gui, ref x, ref y, width);

      x += width;
      y = yBegin;

      x += Gui.OffsetGuiX;
      gui.Line(x, y, 1.0f, Core.GuiTabsHeight - Gui.OffsetY * 2.0f, Skin.SeparatorColor);
      x += Gui.OffsetGuiX;

      mapList_.OnGui(gui, ref x, ref y);
    }

    public override void Update(int id) {
      fly_.Update();

      if (id != Id && Core.IsGuiEnabled) {
        return;
      }
      mapList_.Update();
    }

    private void LoadMap(string map, string name) {
      Log.Write($"[KN_Maps]: Loading map '{name}'");
    }
  }
}