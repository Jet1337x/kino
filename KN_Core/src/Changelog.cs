using System;
using System.Collections.Generic;
using System.Linq;

namespace KN_Core {
  public static class Changelog {
    private static List<string> data_;

    public static void Initialize() {
      data_ = WebDataLoader.LoadAsList("aHR0cHM6Ly9yYXcuZ2l0aHVidXNlcmNvbnRlbnQuY29tL3RyYmZseHIva2luby9tYXN0ZXIvdmVyc2lvbg==").ToList();
    }

    public static int GetVersion() {
      if (data_ == null || data_.Count <= 0) {
        return 0;
      }
      return Convert.ToInt32(data_[0]);
    }

    public static List<string> GetChangelog() {
      if (data_ == null || data_.Count <= 1) {
        return null;
      }

      var changelog = data_;
      changelog.RemoveAt(0);

      return changelog;
    }
  }
}