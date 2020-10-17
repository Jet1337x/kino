using System.Collections.Generic;
using System.IO;
using System.Linq;
using KN_Core;
using UnityEngine;

namespace KN_Maps {
  public class MapList {
    public delegate void MapLoadCallback(string map, string name);
    public event MapLoadCallback OnMapSelected;

    private float filesListScrollH_;
    private Vector2 filesListScroll_;

    private List<string> maps_;
    private readonly string folder_;
    private string selectedMap_;

    private readonly Timer timer_;

    public MapList() {
      folder_ = KnConfig.MapsDir;
      maps_ = new List<string>();

      timer_ = new Timer(1.0f);
      timer_.Callback += RefreshMaps;
    }

    public void Reset() {
      maps_.Clear();
      selectedMap_ = null;
      timer_.Reset();
    }

    public void Update() {
      timer_.Update();
    }

    public void OnGui(Gui gui, ref float x, ref float y) {
      const float listHeight = 400.0f;
      const float baseWidth = Gui.Width * 2.0f;
      const float baseWidthScroll = baseWidth - 20.0f;

      gui.BeginScrollV(ref x, ref y, baseWidth, listHeight, filesListScrollH_, ref filesListScroll_, $"MAPS {maps_.Count}");
      float sx = x;
      float sy = y;
      const float offset = Gui.ScrollBarWidth / 2.0f;
      bool scrollVisible = filesListScrollH_ > listHeight;
      float width = scrollVisible ? baseWidthScroll - offset : baseWidthScroll + offset;
      foreach (string map in maps_) {
        bool currentMap = map == selectedMap_;
        string dir = Path.GetFileName(map);
        sy += Gui.Offset;
        if (gui.TextButton(ref sx, ref sy, width, Gui.Height, $"{dir}", currentMap ? Skin.ButtonSkin.Active : Skin.ButtonSkin.Normal)) {
          if (!currentMap) {
            OnMapSelected?.Invoke(map, dir);
          }
          selectedMap_ = map;
        }
        sy -= Gui.Offset;
      }
      sy += Gui.Offset;
      filesListScrollH_ = gui.EndScrollV(ref x, ref y, sy);
    }

    private void RefreshMaps() {
      if (string.IsNullOrEmpty(folder_)) {
        return;
      }
      maps_ = Directory.GetDirectories(folder_).ToList();
    }
  }
}