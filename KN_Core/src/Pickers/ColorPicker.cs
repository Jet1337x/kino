using UnityEngine;

namespace KN_Core {
  public class ColorPicker {
    public bool IsPicking { get; set; }

    public Color PickedColor { get; private set; }

    private bool alpha_ = true;

    public void Reset() {
      PickedColor = Color.white;
      IsPicking = false;
      alpha_ = true;
    }

    public void Toggle(Color initialColor, bool pickAlpha = true) {
      IsPicking = !IsPicking;
      if (IsPicking) {
        Pick(initialColor, pickAlpha);
      }
      else {
        Reset();
      }
    }

    public void Pick(Color initialColor, bool pickAlpha = true) {
      PickedColor = initialColor;
      IsPicking = true;
      alpha_ = pickAlpha;
    }

    public void OnGui(Gui gui, ref float x, ref float y) {
      const float width = Gui.Width * 1.5f;
      const float boxWidth = width + Gui.OffsetGuiX * 2.0f;

      float boxHeight = Gui.Height * 4.0f + Gui.OffsetY * 5.0f;
      if (alpha_) {
        boxHeight += Gui.Height + Gui.OffsetY;
      }

      float yBegin = y;

      gui.Box(x, y, boxWidth, Gui.Height, Locale.Get("cp_title"), Skin.BoxSkin.Normal);
      y += Gui.Height;

      gui.Box(x, y, boxWidth, boxHeight, Skin.MainContainer);
      y += Gui.OffsetY;
      x += Gui.OffsetGuiX;

      float r = PickedColor.r;
      if (gui.SliderH(ref x, ref y, width, ref r, 0.0f, 1.0f, $"{Locale.Get("red")}: {r:F}")) {
        PickedColor = new Color(r, PickedColor.g, PickedColor.b, PickedColor.a);
      }

      float g = PickedColor.g;
      if (gui.SliderH(ref x, ref y, width, ref g, 0.0f, 1.0f, $"{Locale.Get("green")}: {g:F}")) {
        PickedColor = new Color(PickedColor.r, g, PickedColor.b, PickedColor.a);
      }

      float b = PickedColor.b;
      if (gui.SliderH(ref x, ref y, width, ref b, 0.0f, 1.0f, $"{Locale.Get("blue")}: {b:F}")) {
        PickedColor = new Color(PickedColor.r, PickedColor.g, b, PickedColor.a);
      }

      if (alpha_) {
        float a = PickedColor.a;
        if (gui.SliderH(ref x, ref y, width, ref a, 0.0f, 1.0f, $"{Locale.Get("alpha")}: {a:F}")) {
          PickedColor = new Color(PickedColor.r, PickedColor.g, PickedColor.b, a);
        }
      }

      if (gui.TextButton(ref x, ref y, width, Gui.Height, Locale.Get("close"), Skin.ButtonSkin.Normal)) {
        IsPicking = false;
        alpha_ = true;
      }

      x += boxWidth + Gui.OffsetGuiX;
      y = yBegin;
    }
  }
}