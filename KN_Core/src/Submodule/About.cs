namespace KN_Core {

  public class About : BaseMod {
    private readonly bool badVersion_;

    public About(Core core, int version, int clientVersion, bool badVersion) : base(core, "about", int.MaxValue, version, clientVersion) {
      badVersion_ = badVersion;
    }

    public override void OnGUI(int id, Gui gui, ref float x, ref float y) {
      float defaultWidth = badVersion_ ? 352.0f : 380.0f;

      float tabsWidth = Core.GuiTabsWidth - Gui.OffsetGuiX * 2.0f;
      float width = defaultWidth > tabsWidth ? defaultWidth : tabsWidth;
      const float height = Gui.Height;

      x += Gui.OffsetSmall;

      if (badVersion_) {
        GuiBadVersion(gui, ref x, ref y, width, height);
      }
      else {
        GuiAbout(gui, ref x, ref y, width, height);
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

      gui.Box(x, y, width, height, Locale.Get("about6"), Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, Locale.Get("about7"), Skin.MainContainerLeft);
      y += height;
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

      gui.Box(x, y, width, height, $"{Locale.Get("about4v")}: {GameVersion.version}", Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, $"{Locale.Get("about5v")}: {KnConfig.ClientVersion}", Skin.MainContainerLeft);
      y += height;
    }
  }
}