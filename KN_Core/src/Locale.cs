using System;
using System.Collections.Generic;
using System.Xml;

namespace KN_Core {
  public static class Locale {
    public static readonly string[] ActiveLocales = {"en", "ru", "fr"};

    public static string CurrentLocale { get; private set; }

    private static Dictionary<string, string> currentLocale_;
    private static Dictionary<string, string> defaultLocale_;
    private static Dictionary<string, Dictionary<string, string>> locales_;

    private static Core core_;

    private static int localeIndex_;

    public static void Initialize(string locale, Core core) {
      CurrentLocale = locale;
      core_ = core;
      locales_ = new Dictionary<string, Dictionary<string, string>>();

      foreach (string l in ActiveLocales) {
        LoadLocale(l);
      }

      defaultLocale_ = locales_["en"];

      bool found = false;
      foreach (var loc in locales_) {
        if (loc.Key == CurrentLocale) {
          currentLocale_ = loc.Value;
          for (int i = 0; i < ActiveLocales.Length; i++) {
            if (ActiveLocales[i] == loc.Key) {
              localeIndex_ = i;
            }
          }
          found = true;
          break;
        }
      }

      if (!found) {
        Log.Write($"[KN_Core]: Unable to find locale '{CurrentLocale}', init default");
        currentLocale_ = locales_["en"];
        CurrentLocale = "en";
        localeIndex_ = 0;
      }
    }

    private static void LoadLocale(string locale) {
      Log.Write($"[KN_Core]: Loading locale '{locale}' ...");
      var stream = Core.LoadCoreFile($"Locale.{locale}.xml");
      if (stream == null) {
        return;
      }

      string id = "";
      var dictionary = new Dictionary<string, string>();
      using (var reader = XmlReader.Create(stream)) {
        while (reader.Read()) {
          if (reader.NodeType == XmlNodeType.Element) {
            if (!reader.HasAttributes) {
              continue;
            }

            string idAttribute = reader.GetAttribute("loc");
            if (!string.IsNullOrEmpty(idAttribute)) {
              id = idAttribute;
              continue;
            }

            string key = reader.GetAttribute("id");
            string value = reader.GetAttribute("value");
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) {
              continue;
            }

            try {
              dictionary.Add(key, value);
            }
            catch (Exception e) {
              Log.Write($"[KN_Core]: Failed to add locale entry: {key} -> {value}, {e.Message}");
            }
          }
        }
      }
      locales_.Add(id, dictionary);
    }

    public static void SelectNextLocale() {
      if (++localeIndex_ >= ActiveLocales.Length) {
        localeIndex_ = 0;
      }
      CurrentLocale = ActiveLocales[localeIndex_];
      currentLocale_ = locales_[CurrentLocale];
    }

    public static string Get(string id) {
      if (core_ == null || core_.DisplayTextAsId || defaultLocale_ == null) {
        return id;
      }
      try {
        return currentLocale_[id];
      }
      catch (Exception) {
        try {
          return defaultLocale_[id];
        }
        catch (Exception) {
          // ignored
        }
        // ignored
      }
      return id;
    }
  }
}