using System;
using System.Collections.Generic;
using System.Linq;

namespace KN_Loader {
  public static class Version {
    private static List<string> data_;

    public static void Initialize() {
      data_ = WebDataLoader.LoadAsList("aHR0cHM6Ly9yYXcuZ2l0aHVidXNlcmNvbnRlbnQuY29tL3RyYmZseHIva2luby9tYXN0ZXIvdmVyc2lvbg==").ToList();
    }

    public static int GetVersion() {
      if (data_ == null || data_.Count <= 0) {
        return 0;
      }
      try {
        string version = data_[0].Replace("Version=", "");
        return Convert.ToInt32(version);
      }
      catch (Exception) {
        Log.Write("[KN_Loader::Version]: Unable to parse version");
        return 0;
      }
    }

    public static int GetPatch() {
      if (data_ == null || data_.Count <= 1) {
        return int.MaxValue;
      }
      try {
        string patch = data_[1].Replace("Patch=", "");
        return Convert.ToInt32(patch);
      }
      catch (Exception) {
        Log.Write("[KN_Loader::Version]: Unable to parse patch version");
        return int.MaxValue;
      }
    }

    public static int GetUpdaterVersion() {
      if (data_ == null || data_.Count <= 2) {
        return int.MaxValue;
      }
      try {
        string version = data_[2].Replace("Updater=", "");
        return Convert.ToInt32(version);
      }
      catch (Exception) {
        Log.Write("[KN_Loader::Version]: Unable to parse updater version");
        return int.MaxValue;
      }
    }

    public static List<string> GetChangelog() {
      if (data_ == null || data_.Count <= 2) {
        return null;
      }

      var changelog = data_;
      changelog.RemoveAt(0); // version
      changelog.RemoveAt(1); // patch
      changelog.RemoveAt(2); // updater

      return changelog;
    }
  }
}