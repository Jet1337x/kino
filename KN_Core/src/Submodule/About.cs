using System.Linq;
using KN_Loader;

namespace KN_Core {

  public class About : BaseMod {
    private readonly bool badVersion_;

    public About(Core core, int version, int patch, int clientVersion, bool badVersion) :
      base(core, "about", Skin.PlusSkin, int.MaxValue, version, patch, clientVersion) {
      badVersion_ = badVersion;
    }

    public override void OnGUI(int id, Gui gui, ref float x, ref float y) {
      x += Gui.OffsetSmall;

      float width = Core.GuiTabsWidth - Gui.OffsetGuiX * 2.0f;

      if (badVersion_) {
        GuiBadVersion(gui, ref x, ref y, width, Gui.Height);
      }
      else {
        GuiAbout(gui, ref x, ref y, width, Gui.Height);
      }
    }

    private void GuiAbout(Gui gui, ref float x, ref float y, float width, float height) {
      gui.Box(x, y, width, height, Locale.Get("about0"), Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, Locale.Get("about1"), Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, Locale.Get("about2"), Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, Locale.Get("about3"), Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, Locale.Get("about4"), Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, Locale.Get("about5"), Skin.MainContainerLeft);
      y += height;

      string supporters = Locale.Supporters.Aggregate("", (current, s) => current + $"{s}, ");
      if (!string.IsNullOrEmpty(supporters)) {
        supporters = supporters.Substring(0, supporters.Length - 2);

        gui.Box(x, y, width, height, $"{Locale.Get("about6")} {supporters} {Locale.Get("about7")}", Skin.MainContainerLeft);
        y += height;
      }

      gui.Box(x, y, width, height, Locale.Get("about8"), Skin.MainContainerLeft);
      y += height;

      foreach (string author in Locale.Authors) {
        gui.Box(x, y, width, height, $"  - {author}", Skin.MainContainerLeft);
        y += height;
      }
    }

    private void GuiBadVersion(Gui gui, ref float x, ref float y, float width, float height) {
      gui.Box(x, y, width, height, Locale.Get("about0v"), Skin.MainContainerRed);
      y += height;

      gui.Box(x, y, width, height, Locale.Get("about1v"), Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, Locale.Get("about2v"), Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, Locale.Get("about3v"), Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, $"{Locale.Get("about4v")}", Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, $"{Locale.Get("about5v")}: {GameVersion.version}", Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, $"{Locale.Get("about6v")}: {ModLoader.ClientVersion}", Skin.MainContainerLeft);
      y += height;
    }
  }
}