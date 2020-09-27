using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace KN_Core {
  public static class Updater {
    public static int Initialize() {
      var data = GetDataFromRemote("aHR0cHM6Ly9yYXcuZ2l0aHVidXNlcmNvbnRlbnQuY29tL3RyYmZseHIva2luby9tYXN0ZXIvdmVyc2lvbg==");
      return data?.Select(line => Convert.ToInt32(line)).FirstOrDefault() ?? 0;
    }

    private static List<string> GetDataFromRemote(string remote) {
      using (var client = new WebClient()) {
        try {
          string uri = Encoding.UTF8.GetString(Convert.FromBase64String(remote));
          string response = client.DownloadString(uri);
          return response.Split('\n').ToList();
        }
        catch (Exception e) {
          Log.Write($"[KN_Update]: Unable to load version data from remote, {e.Message}");
          return null;
        }
      }
    }
  }
}