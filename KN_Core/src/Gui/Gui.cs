using UnityEngine;

namespace KN_Core {
  public class Gui {
    private const float MinModWidth = 470.0f;
    private const float MinModHeight = 340.0f;

    public const float ModIconSize = 50.0f;
    public const float ModTabHeight = 25.0f;
    public const float Height = 20.0f;
    public const float Width = 160.0f;
    public const float WidthScroll = 140.0f;
    public const float ScrollBarWidth = 8.0f;
    public const float WidthSlider = 16.0f;

    public const float Offset = 10.0f;
    public const float OffsetSmall = 5.0f;

    public float ModHeight { get; private set; }

    public float MaxContentWidth { get; private set; }
    public float MaxContentHeight { get; private set; }

    private float scrollX_;
    private float scrollY_;
    private float scrollVisibleHeight_;

    private bool begin_;

    private float x_;
    private float y_;

    private float width_;
    private float height_;

    public Gui() {
      ModHeight = MinModHeight;

      ResetSize();
    }

    public void UpdateMinModHeight(int modsCount) {
      const int maxMods = (int) (MinModHeight / ModIconSize) - 1;

      if (modsCount >= maxMods) {
        int c = modsCount - maxMods + 1;
        ModHeight = MinModHeight + ModIconSize * c;
      }
    }

    public void ResetSize() {
      width_ = MinModWidth + ModIconSize;
      height_ = ModHeight + ModTabHeight;

      MaxContentWidth = width_;
      MaxContentHeight = height_;
    }

    public void Begin(float x, float y) {
      begin_ = true;

      x_ = x;
      y_ = y;

      width_ = MinModWidth + ModIconSize;
      height_ = ModHeight + ModTabHeight;
    }

    public void End() {
      begin_ = false;

      MaxContentWidth = width_ - (x_ + ModIconSize);
      MaxContentHeight = height_ - (y_ + ModTabHeight);
    }

    public bool BaseButton(ref float x, ref float y, float width, float height, string text, GUISkin skin) {
      var old = GUI.skin;

      GUI.skin = skin;
      bool res = GUI.Button(new Rect(x, y, width, height), text);
      y += height + Offset;
      GUI.skin = old;

      EnsureContentSize(x, y, width);
      return res;
    }

    public bool TabButton(ref float x, ref float y, float width, float height, string text, GUISkin skin) {
      var old = GUI.skin;

      GUI.skin = skin;
      bool res = GUI.Button(new Rect(x, y, width, height), text);
      x += width;
      GUI.skin = old;

      EnsureContentSize(x, y, 0.0f);
      return res;
    }

    public bool ImageButton(ref float x, ref float y, float width, float height, GUISkin skin) {
      var old = GUI.skin;

      GUI.skin = skin;
      bool res = GUI.Button(new Rect(x, y, width, height), "");
      GUI.skin = old;

      EnsureContentSize(x, y, width);
      return res;
    }

    public bool ImageButton(ref float x, ref float y, GUISkin skin) {
      return ImageButton(ref x, ref y, Gui.ModIconSize, Gui.ModIconSize, skin);
    }

    public bool TextButton(ref float x, ref float y, string text, GUISkin skin) {
      return BaseButton(ref x, ref y, Width, Height, text, skin);
    }

    public bool TextButton(ref float x, ref float y, float width, float height, string text, GUISkin skin) {
      return BaseButton(ref x, ref y, width, height, text, skin);
    }

    public void BoxAutoWidth(float x, float y, float width, float height, string text, GUISkin skin) {
      var old = GUI.skin;

      float textWidth = TextWidth(text, skin.box.font);
      float w = width > 0.0f ? width : textWidth;

      GUI.skin = skin;
      GUI.Box(new Rect(x, y, w, height), text);
      GUI.skin = old;

      EnsureContentSize(x, y, textWidth);
    }

    public void Box(float x, float y, float width, float height, string text, GUISkin skin) {
      var old = GUI.skin;

      GUI.skin = skin;
      GUI.Box(new Rect(x, y, width, height), text);
      GUI.skin = old;

      EnsureContentSize(x, y, width);
    }

    public void Box(float x, float y, float width, float height, GUISkin skin) {
      Box(x, y, width, height, "", skin);
    }

    public void Line(float x, float y, float width, float height, Color color) {
      var old = GUI.color;

      GUI.color = color;
      GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
      GUI.color = old;
    }

    public bool SliderH(ref float x, ref float y, float width, ref float value, float low, float high, string text, GUISkin skin) {
      var old = GUI.skin;
      GUI.skin = skin;

      float result = GUI.HorizontalSlider(new Rect(x, y, width, Height), value, low, high);

      x += OffsetSmall;
      GUI.Label(new Rect(x, y, width, Height), text);
      x -= OffsetSmall;

      y += Height + Offset;

      GUI.skin = old;

      EnsureContentSize(x, y, width);

      if (Mathf.Abs(value - result) < 0.0001f) {
        return false;
      }

      value = result;
      return true;
    }

    public bool SliderH(ref float x, ref float y, ref float value, float low, float high, string text) {
      return SliderH(ref x, ref y, Width, ref value, low, high, text, Skin.SliderSkin.Normal);
    }

    public bool SliderH(ref float x, ref float y, float width, ref float value, float low, float high, string text) {
      return SliderH(ref x, ref y, width, ref value, low, high, text, Skin.SliderSkin.Normal);
    }

    public void BeginScrollV(ref float x, ref float y, float width, float visibleHeight, float contentHeight, ref Vector2 scrollPos, string caption) {
      var old = GUI.skin;
      GUI.skin = Skin.ScrollSkin.Normal;

      GUI.Box(new Rect(x, y, width, Height), caption);
      y += Height;

      scrollVisibleHeight_ = visibleHeight;
      scrollX_ = x;
      scrollY_ = y;

      scrollPos = GUI.BeginScrollView(new Rect(scrollX_, scrollY_, width, visibleHeight), scrollPos,
        new Rect(scrollX_, scrollY_, width, contentHeight), false, false);

      GUI.skin = old;

      x += ScrollBarWidth;

      EnsureContentSize(x, y, width);
    }

    public void BeginScrollV(ref float x, ref float y, float visibleHeight, float contentHeight, ref Vector2 scrollPos, string caption) {
      BeginScrollV(ref x, ref y, Width, visibleHeight, contentHeight, ref scrollPos, caption);
    }

    public float EndScrollV(ref float x, ref float y, float contentY) {
      y += scrollVisibleHeight_ + Offset;
      GUI.EndScrollView();

      x -= Offset;

      EnsureContentSize(x, y, 0.0f);

      //content height
      return contentY - scrollY_;
    }

    public bool ScrollViewButton(ref float x, ref float y, float width, float height, string text, out bool delete, GUISkin skin, GUISkin deleteSkin) {
      // this should only be used in box container / scroll view

      var old = GUI.skin;
      GUI.skin = skin;

      float w = width - (Height + OffsetSmall);

      y += Offset;
      bool result = GUI.Button(new Rect(x, y, w, height), text);

      //delete
      x += w + OffsetSmall;

      GUI.skin = deleteSkin;
      delete = GUI.Button(new Rect(x, y, Height, Height), "X");

      x -= w + OffsetSmall;
      y += height;

      GUI.skin = old;
      return result || delete;
    }

    public bool ScrollViewButton(ref float x, ref float y, string text, out bool delete, GUISkin skin, GUISkin deleteSkin) {
      return ScrollViewButton(ref x, ref y, WidthScroll, Height, text, out delete, skin, deleteSkin);
    }

    private void EnsureContentSize(float x, float y, float width) {
      if (!begin_) {
        return;
      }

      float elementX = x + width;
      if (width_ < elementX) {
        width_ = elementX;
      }

      if (height_ < y) {
        height_ = y;
      }
    }

    private static int TextWidth(string text, Font font) {
      int width = 0;
      foreach (char c in text) {
        font.GetCharacterInfo(c, out var characterInfo, font.fontSize);
        width += characterInfo.advance;
      }
      return width + (int) (OffsetSmall * 1.5f);
    }
  }
}