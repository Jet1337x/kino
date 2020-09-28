using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace KN_Core {
  public static class WebDataLoader {
    public static byte[] LoadAsBytes(string remote, string module = "") {
      using (var client = new WebClient()) {
        string uri = Encoding.UTF8.GetString(Convert.FromBase64String(remote));
        try {
          return client.DownloadData(uri);
        }
        catch (Exception e) {
          Log.Write(module.Length > 0
            ? $"[KN_Loader]: Unable to load data for module '{module}' from remote ({uri}), {e.Message}"
            : $"[KN_Loader]: Unable to load data from remote ({uri}), {e.Message}");
          return null;
        }
      }
    }

    public static IEnumerable<string> LoadAsList(string remote, string module = "") {
      using (var client = new WebClient()) {
        string uri = Encoding.UTF8.GetString(Convert.FromBase64String(remote));
        try {
          string response = client.DownloadString(uri);
          return response.Split('\n').ToList();
        }
        catch (Exception e) {
          Log.Write(module.Length > 0
            ? $"[KN_Loader]: Unable to load data for module '{module}' from remote ({uri}), {e.Message}"
            : $"[KN_Loader]: Unable to load data from remote ({uri}), {e.Message}");
          return null;
        }
      }
    }
  }
}