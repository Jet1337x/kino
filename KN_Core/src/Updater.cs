using System;
using System.Linq;

namespace KN_Core {
  public static class Updater {
    public static int Initialize() {
      var data = WebDataLoader.LoadAsList("aHR0cHM6Ly9yYXcuZ2l0aHVidXNlcmNvbnRlbnQuY29tL3RyYmZseHIva2luby9tYXN0ZXIvdmVyc2lvbg==");
      return data?.Select(line => Convert.ToInt32(line)).FirstOrDefault() ?? 0;
    }
  }
}