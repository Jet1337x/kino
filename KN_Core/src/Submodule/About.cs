namespace KN_Core.Submodule {

  public class About : BaseMod {
    public About(Core core) : base(core, "ABOUT", int.MaxValue) { }

    public override void OnGUI(int id, Gui gui, ref float x, ref float y) {
      float width = Core.GuiTabsWidth - Gui.OffsetGuiX * 2.0f ;
      const float height = Gui.Height;

      x += Gui.OffsetSmall;

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

      gui.Box(x, y, width, height, "Also many thanks to Cursed and MRo for their support!", Skin.MainContainerLeft);
      y += height;
    }
  }
}