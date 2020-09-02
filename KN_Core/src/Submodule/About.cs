namespace KN_Core {

  public class About : BaseMod {
    private readonly bool badVersion_;

    public About(Core core, int version, int clientVersion, bool badVersion) : base(core, "ABOUT", int.MaxValue, version, clientVersion) {
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
      gui.Box(x, y, width, height, "Developed by trbflxr", Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, "Tested by John Sawyer", Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, "If you have any questions or suggestions", Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, "please, contact us at", Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, "trbflxr#8814 or John Sawyer#6915", Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, "or in the mod's discord: discord.gg/jrMReAB", Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, "Also many thanks to Cursed and MRo for their support!", Skin.MainContainerLeft);
      y += height;
    }

    private void GuiBadVersion(Gui gui, ref float x, ref float y, float width, float height) {
      gui.Box(x, y, width, height, "Your CarX client version is different!", Skin.MainContainerRed);
      y += height;

      gui.Box(x, y, width, height, "Please download new version of KiNO mod", Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, "from official discord: discord.gg/jrMReAB", Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, "or contact us at trbflxr#8814 or John Sawyer#6915", Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, $"Current version: {GameVersion.version}", Skin.MainContainerLeft);
      y += height;

      gui.Box(x, y, width, height, $"Target version: {KnConfig.ClientVersion}", Skin.MainContainerLeft);
      y += height;
    }
  }
}