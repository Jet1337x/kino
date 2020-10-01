using System;
using System.Net;
using Octokit;

namespace KN_Updater {
  public class Remote {
    private const string Owner = "trbflxr";
    private const string Repo = "kino";

    public string LatestVersion { get; private set; }

    private GitHubClient github_;
    private Release latest_;
    private ReleaseAsset latestAsset_;

    private bool initialized_;

    public bool Initialize() {
      try {
        github_ = new GitHubClient(new ProductHeaderValue(Repo));

        var releases = github_.Repository.Release.GetAll(Owner, Repo);
        if (releases.Result.Count > 0) {
          latest_ = releases.Result[0];

          if (latest_.Assets != null && latest_.Assets.Count > 0) {
            latestAsset_ = latest_.Assets[0];
          }
          else {
            Log.Write("Bad release, assets count is 0");
            return false;
          }
        }
        else {
          Log.Write("Releases count is 0");
          return false;
        }
      }
      catch (Exception e) {
        Log.Write($"Failed to load releases, {e.Message}");
        return false;
      }

      Log.Write($"Remote initialized. Latest release: {latest_.TagName} ({latest_.Name}), at: {latest_.Url}");
      Log.Write($"Selected asset '{latestAsset_.Name}', download url: {latestAsset_.BrowserDownloadUrl}");

      LatestVersion = latest_.TagName;

      initialized_ = true;

      return true;
    }

    public byte[] DownloadLatestRelease() {
      if (!initialized_) {
        return null;
      }

      using (var client = new WebClient()) {
        try {
          return client.DownloadData(latestAsset_.BrowserDownloadUrl);
        }
        catch (Exception e) {
          Log.Write($"Unable to load release '{latestAsset_.Name}', from '{latestAsset_.BrowserDownloadUrl}' ({e.Message})");
          return null;
        }
      }
    }
  }
}