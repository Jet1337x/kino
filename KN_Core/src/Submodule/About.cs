using System.Linq;
using KN_Loader;

namespace KN_Core {

  public class About : BaseMod {
    private readonly bool badVersion_;

    public About(Core core, int version, int patch, int clientVersion, bool badVersion) : base(core, "about", int.MaxValue, version, patch, clientVersion) {
      SetIcon(Skin.PlusSkin);
      AddTab("about", OnGui);

      badVersion_ = badVersion;
    }

    private bool OnGui(Gui gui, float x, float y) {
      float width = Core.DummyWidth;

      x -= Gui.Offset;
      y -= Gui.Offset;

      if (badVersion_) {
        GuiBadVersion(gui, ref x, ref y, width, Gui.Height);
      }
      else {
        GuiAbout(gui, ref x, ref y, width, Gui.Height);
      }

      return false;
    }

    private void GuiAbout(Gui gui, ref float x, ref float y, float width, float height) {
      gui.Box1(x, y, width, height, Locale.Get("about0"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.Box1(x, y, width, height, Locale.Get("about1"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.Box1(x, y, width, height, Locale.Get("about2"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.Box1(x, y, width, height, Locale.Get("about3"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.Box1(x, y, width, height, Locale.Get("about4"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.Box1(x, y, width, height, Locale.Get("about5"), Skin.BoxLeftSkin.Normal);
      y += height;

      string supporters = Locale.Supporters.Aggregate("", (current, s) => current + $"{s}, ");
      if (!string.IsNullOrEmpty(supporters)) {
        supporters = supporters.Substring(0, supporters.Length - 2);

        gui.Box1(x, y, width, height, $"{Locale.Get("about6")} {supporters} {Locale.Get("about7")}", Skin.BoxLeftSkin.Normal);
        y += height;
      }

      gui.Box1(x, y, width, height, Locale.Get("about8"), Skin.BoxLeftSkin.Normal);
      y += height;

      foreach (string author in Locale.Authors) {
        gui.Box1(x, y, width, height, $"  - {author}", Skin.BoxLeftSkin.Normal);
        y += height;
      }
    }

    private void GuiBadVersion(Gui gui, ref float x, ref float y, float width, float height) {
      gui.Box1(x, y, width, height, Locale.Get("about0v"), Skin.MainContainerRed);
      y += height;

      gui.Box1(x, y, width, height, Locale.Get("about1v"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.Box1(x, y, width, height, Locale.Get("about2v"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.Box1(x, y, width, height, Locale.Get("about3v"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.Box1(x, y, width, height, $"{Locale.Get("about4v")}", Skin.BoxLeftSkin.Normal);
      y += height;

      gui.Box1(x, y, width, height, $"{Locale.Get("about5v")}: {GameVersion.version}", Skin.BoxLeftSkin.Normal);
      y += height;

      gui.Box1(x, y, width, height, $"{Locale.Get("about6v")}: {ModLoader.ClientVersion}", Skin.BoxLeftSkin.Normal);
      y += height;
    }
  }
}