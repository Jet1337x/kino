namespace KN_Core.Submodule {
  public class Settings : BaseMod {
    public Settings(Core core) : base(core, "SETTINGS", int.MaxValue - 1) { }

    public override void OnGUI(int id, Gui gui, ref float x, ref float y) {
      float width = Core.GuiTabsWidth - Gui.OffsetGuiX * 2.0f;
      const float height = Gui.Height;

      x += Gui.OffsetSmall;

      gui.Box(x, y, width, height, "Developed by trbflxr", Skin.MainContainerLeft);
      y += height;
    }
  }
}