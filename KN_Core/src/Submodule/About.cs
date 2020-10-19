using System.Linq;
using KN_Loader;

namespace KN_Core {

  public class About : BaseMod {
    private readonly bool badVersion_;

    public About(Core core, int version, int patch, int clientVersion, bool badVersion) : base(core, "about", int.MaxValue, version, patch, clientVersion) {
      SetIcon(Skin.AboutSkin);
      AddTab("about", OnGui);

      badVersion_ = badVersion;
    }

    private bool OnGui(Gui gui, float x, float y) {
      float width = gui.MaxContentWidth;

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
      gui.BoxAutoWidth(x, y, width, height, Locale.Get("about0"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.BoxAutoWidth(x, y, width, height, Locale.Get("about1"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.BoxAutoWidth(x, y, width, height, Locale.Get("about2"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.BoxAutoWidth(x, y, width, height, Locale.Get("about3"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.BoxAutoWidth(x, y, width, height, Locale.Get("about4"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.BoxAutoWidth(x, y, width, height, Locale.Get("about5"), Skin.BoxLeftSkin.Normal);
      y += height;

      if (Locale.Supporters.Count > 0) {
        gui.BoxAutoWidth(x, y, width, height, Locale.Get("about6"), Skin.BoxLeftSkin.Normal);
        y += height;

        foreach (string s in Locale.Supporters) {
          gui.BoxAutoWidth(x, y, width, height, s, Skin.BoxLeftSkin.Normal);
          y += height;
        }
      }

      gui.BoxAutoWidth(x, y, width, height, Locale.Get("about7"), Skin.BoxLeftSkin.Normal);
      y += height;

      foreach (string author in Locale.Authors) {
        gui.BoxAutoWidth(x, y, width, height, $"  - {author}", Skin.BoxLeftSkin.Normal);
        y += height;
      }

      float mh = gui.MaxContentHeight > gui.ModHeight ? gui.MaxContentHeight : gui.ModHeight;
      if (y < mh) {
        float h = mh - y + Gui.ModTabHeight;
        gui.Box(x, y, width, h, Skin.BoxLeftSkin.Normal);
      }
    }

    private void GuiBadVersion(Gui gui, ref float x, ref float y, float width, float height) {
      gui.BoxAutoWidth(x, y, width, height, Locale.Get("about0v"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.BoxAutoWidth(x, y, width, height, Locale.Get("about1v"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.BoxAutoWidth(x, y, width, height, Locale.Get("about2v"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.BoxAutoWidth(x, y, width, height, Locale.Get("about3v"), Skin.BoxLeftSkin.Normal);
      y += height;

      gui.BoxAutoWidth(x, y, width, height, $"{Locale.Get("about4v")}", Skin.BoxLeftSkin.Normal);
      y += height;

      gui.BoxAutoWidth(x, y, width, height, $"{Locale.Get("about5v")}: {GameVersion.version}", Skin.BoxLeftSkin.Normal);
      y += height;

      gui.BoxAutoWidth(x, y, width, height, $"{Locale.Get("about6v")}: {ModLoader.ClientVersion}", Skin.BoxLeftSkin.Normal);
      y += height;

      float mh = gui.MaxContentHeight > gui.ModHeight ? gui.MaxContentHeight : gui.ModHeight;
      if (y < mh) {
        float h = mh - y + Gui.ModTabHeight;
        gui.Box(x, y, width, h, Skin.BoxLeftSkin.Normal);
      }
    }
  }
}