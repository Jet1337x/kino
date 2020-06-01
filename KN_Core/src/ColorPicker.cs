using UnityEngine;

namespace KN_Core {
  public class ColorPicker {
    public bool IsPicking { get; set; }
    public bool IsForceClosed { get; set; }

    public Color PickedColor { get; private set; }

    public void Reset() {
      PickedColor = Color.white;
      IsPicking = false;
      IsForceClosed = false;
    }

    public void Pick(Color initialColor) {
      PickedColor = initialColor;
      IsPicking = true;
    }

    public void OnGui(Gui gui, ref float x, ref float y) {
      const float width = Gui.Width * 1.5f;
      const float boxWidth = width + Gui.OffsetGuiX * 2.0f;
      const float boxHeight = Gui.Height * 5.0f + Gui.OffsetY * 6.0f;

      float yBegin = y;

      gui.Box(x, y, boxWidth, Gui.Height, "COLOR PICKER", Skin.MainContainerDark);
      y += Gui.Height;

      gui.Box(x, y, boxWidth, boxHeight, Skin.MainContainer);
      y += Gui.OffsetY;
      x += Gui.OffsetGuiX;

      float r = PickedColor.r;
      if (gui.SliderH(ref x, ref y, width, ref r, 0.0f, 1.0f, $"RED: {r:F}")) {
        PickedColor = new Color(r, PickedColor.g, PickedColor.b, PickedColor.a);
      }

      float g = PickedColor.g;
      if (gui.SliderH(ref x, ref y, width, ref g, 0.0f, 1.0f, $"GREEN: {g:F}")) {
        PickedColor = new Color(PickedColor.r, g, PickedColor.b, PickedColor.a);
      }

      float b = PickedColor.b;
      if (gui.SliderH(ref x, ref y, width, ref b, 0.0f, 1.0f, $"GLUE: {b:F}")) {
        PickedColor = new Color(PickedColor.r, PickedColor.g, b, PickedColor.a);
      }

      float a = PickedColor.a;
      if (gui.SliderH(ref x, ref y, width, ref a, 0.0f, 1.0f, $"ALPHA: {a:F}")) {
        PickedColor = new Color(PickedColor.r, PickedColor.g, PickedColor.b, a);
      }

      if (gui.Button(ref x, ref y, width, Gui.Height, "CLOSE", Skin.Button)) {
        IsPicking = false;
        IsForceClosed = true;
      }

      x += boxWidth + Gui.OffsetGuiX;
      y = yBegin;
    }
  }
}