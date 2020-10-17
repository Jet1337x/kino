using KN_Core;
using KN_Loader;

namespace KN_Maps {
  public class Maps : BaseMod {

    private readonly SafeFlyMod fly_;
    private readonly MapList mapList_;

    public Maps(Core core, int version, int patch, int clientVersion) : base(core, "maps", 4, version, patch, clientVersion) {
      SetIcon(Skin.MapsSkin);
      AddTab("maps", OnGui);

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

    public override void OnCarLoaded(KnCar car) {
      fly_.OnCarLoaded();
    }

    public override void ResetState() { }

    private bool OnGui(Gui gui, float x, float y) {
      const float width = Gui.Width * 1.5f;

      float yBegin = y;

      x += Gui.OffsetSmall;

      fly_.OnGui(gui, ref x, ref y, width);

      x += width;
      y = yBegin;

      x += Gui.Offset;
      gui.Line(x, y, 1.0f, gui.MaxContentHeight - Gui.Offset * 2.0f, Skin.SeparatorColor);
      x += Gui.Offset;

      mapList_.OnGui(gui, ref x, ref y);

      return false;
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