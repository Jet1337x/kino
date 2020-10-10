using System.Text.RegularExpressions;
using UnityEngine;

namespace KN_Core {
  public class Gui {
    public const float MinTabsWidth = 520.0f;

    public const float ModIconSize = 50.0f;
    public const float ModTabHeight = 25.0f;
    public const float Height = 20.0f;
    public const float ScrollBarWidth = 8.0f;
    public const float WidthSlider = 16.0f;

    public const float Offset = 10.0f;
    public const float OffsetSmall = 5.0f;


    public const float Width = 160.0f;
    public const float WidthScroll = 140.0f;
    public const float HeightTimeline = 10.0f;
    public const float SmallSize = Height;
    public const float IconSize = 40.0f;

    public const float TabButtonWidth = 80.0f;
    public const float TabButtonHeight = 23.0f;

    public const float OffsetY = 10.0f;
    public const float OffsetGuiX = 10.0f;

    public int SelectedTab { get; set; }

    private float scrollX_;
    private float scrollY_;
    private float scrollVisibleHeight_;

    private float tabsX_;
    private float tabsY_;
    private float tabsWidth_;
    private float tabsHeight_;
    private float tabsWidthButtons_;

    public float TabsMaxWidth { get; private set; }
    public float TabsMaxHeight { get; private set; }

    public bool BaseButton(ref float x, ref float y, float width, float height, string text, GUISkin skin) {
      var old = GUI.skin;

      GUI.skin = skin;
      bool res = GUI.Button(new Rect(x, y, width, height), text);
      y += height + Offset;
      GUI.skin = old;

      return res;
    }

    public bool TabButton(ref float x, ref float y, float width, float height, string text, GUISkin skin) {
      var old = GUI.skin;

      GUI.skin = skin;
      bool res = GUI.Button(new Rect(x, y, width, height), text);
      x += width;
      GUI.skin = old;

      return res;
    }

    public bool ImageButton(ref float x, ref float y, float width, float height, GUISkin skin) {
      var old = GUI.skin;

      GUI.skin = skin;
      bool res = GUI.Button(new Rect(x, y, width, height), "");
      GUI.skin = old;

      return res;
    }

    public bool ImageButton(ref float x, ref float y, GUISkin skin) {
      return ImageButton(ref x, ref y, IconSize, IconSize, skin);
    }

    public bool TextButton(ref float x, ref float y, string text, GUISkin skin) {
      return BaseButton(ref x, ref y, Width, Height, text, skin);
    }

    public bool TextButton(ref float x, ref float y, float width, float height, string text, GUISkin skin) {
      return BaseButton(ref x, ref y, width, height, text, skin);
    }

    public void Box(float x, float y, float width, float height, string text, GUISkin skin) {
      var old = GUI.skin;

      GUI.skin = skin;
      GUI.Box(new Rect(x, y, width, height), text);
      GUI.skin = old;
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
    }

    public void BeginScrollV(ref float x, ref float y, float visibleHeight, float contentHeight, ref Vector2 scrollPos, string caption) {
      BeginScrollV(ref x, ref y, Width, visibleHeight, contentHeight, ref scrollPos, caption);
    }

    public float EndScrollV(ref float x, ref float y, float contentY) {
      y += scrollVisibleHeight_ + OffsetSmall;
      GUI.EndScrollView();

      x -= Offset;

      //content height
      return contentY - scrollY_;
    }

    // old ----------------------------
    public void Tabs(ref float x, ref float y, string[] tabs, ref int selected) {
      TabsMaxWidth = MinTabsWidth;
      TabsMaxHeight = 0.0f;

      y += OffsetSmall + TabButtonHeight;

      var oldColor = GUI.color;
      var old = GUI.skin;
      GUI.skin = Skin.MainContainer;
      GUI.color = Skin.ContainerAlpha;
      GUI.Box(new Rect(x, y, tabsWidth_, tabsHeight_), "");

      tabsX_ = x;
      tabsY_ = y;

      y -= TabButtonHeight;

      float tx = x;
      int i;
      GUI.color = Skin.ElementAlpha;
      for (i = 0; i < tabs.Length; i++) {
        GUI.skin = SelectedTab == i ? Skin.ButtonActiveTab : Skin.ButtonTab;
        if (GUI.Button(new Rect(tx, y, TabButtonWidth, TabButtonHeight), tabs[i])) {
          selected = i;
          SelectedTab = i;
        }
        tx += TabButtonWidth + OffsetSmall;
      }
      tabsWidthButtons_ = tx - x - OffsetSmall;
      GUI.color = oldColor;
      GUI.skin = old;

      x += OffsetSmall;
      y += TabButtonHeight + OffsetY;
    }

    public void EndTabs(ref float x, ref float y) {
      tabsWidth_ = x - tabsX_;
      tabsHeight_ = y - tabsY_;

      if (tabsWidth_ < tabsWidthButtons_) {
        tabsWidth_ = tabsWidthButtons_;
      }

      TabsMaxWidth += OffsetGuiX;
      if (tabsWidth_ < TabsMaxWidth) {
        tabsWidth_ = TabsMaxWidth;
      }
      else {
        TabsMaxWidth = tabsWidth_;
      }

      TabsMaxHeight += OffsetY;
      if (tabsHeight_ < TabsMaxHeight) {
        tabsHeight_ = TabsMaxHeight;
      }
      else {
        TabsMaxHeight = tabsHeight_;
      }

      x -= OffsetGuiX;
    }

    public bool ScrollViewButton(ref float x, ref float y, float width, float height, string text, out bool delete, GUISkin skin, GUISkin deleteSkin) {
      //this should only be used in box container / scroll view

      var oldColor = GUI.color;
      var old = GUI.skin;
      GUI.skin = skin;

      float w = width - (SmallSize + OffsetSmall);

      y += OffsetY;
      GUI.color = Skin.ElementAlpha;
      bool result = GUI.Button(new Rect(x, y, w, height), text);

      //delete
      x += w + OffsetSmall;

      GUI.skin = deleteSkin;
      delete = GUI.Button(new Rect(x, y, SmallSize, SmallSize), "X");

      x -= w + OffsetSmall;
      y += height;

      GUI.color = oldColor;
      GUI.skin = old;
      return result || delete;
    }

    public bool ScrollViewButton(ref float x, ref float y, string text, out bool delete, GUISkin skin, GUISkin deleteSkin) {
      return ScrollViewButton(ref x, ref y, WidthScroll, Height, text, out delete, skin, deleteSkin);
    }

    public void Label(ref float x, ref float y, float width, float height, string text) {
      var oldColor = GUI.color;
      var old = GUI.skin;
      GUI.skin = Skin.TextField;
      GUI.color = Skin.TextAlpha;
      GUI.Label(new Rect(x, y, width, height), text);
      GUI.color = oldColor;
      GUI.skin = old;
      y += OffsetY;
    }

    public void Label(ref float x, ref float y, string text) {
      Label(ref x, ref y, Width, Height, text);
    }

    public bool TextField(ref float x, ref float y, float width, ref string text, string caption, int maxLength, string regex) {
      EnsureTabsSize(x, y, width, Height);
      var oldColor = GUI.color;
      var old = GUI.skin;
      GUI.skin = Skin.TextField;

      y -= OffsetY;
      GUI.color = Skin.TextAlpha;
      GUI.Label(new Rect(x, y, width, Height), caption);
      y += Height;

      string buff = text;
      GUI.color = Skin.ElementAlpha;
      text = GUI.TextField(new Rect(x, y, width, Height), text, maxLength);
      y += Height + OffsetY;

      if (!string.IsNullOrEmpty(text)) {
        if (!Regex.Match(text, regex).Success) {
          text = buff;
        }
      }

      GUI.color = oldColor;
      GUI.skin = old;

      return text != buff;
    }

    public bool TextField(ref float x, ref float y, ref string text, string caption, int maxLength, string regex) {
      return TextField(ref x, ref y, Width, ref text, caption, maxLength, regex);
    }

    private void EnsureTabsSize(float x, float y, float width, float height) {
      float currWidth = x - tabsX_ + width;
      float currHeight = y - tabsY_ + height;
      if (TabsMaxWidth < currWidth) {
        TabsMaxWidth = currWidth;
      }
      if (TabsMaxHeight < currHeight) {
        TabsMaxHeight = currHeight;
      }
    }
  }
}