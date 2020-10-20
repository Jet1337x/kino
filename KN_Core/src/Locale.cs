using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using KN_Loader;

namespace KN_Core {
  internal class NameData {
    public string Name;
    public int Size;
  }

  public static class Locale {
    private const int NameSize = 11;

    public static readonly string[] ActiveLocales = {"en", "ru", "fr", "nl", "pl", "jp", "ita", "de", "zn"};

    public static List<string> Authors { get; private set; }
    public static List<string> Supporters { get; private set; }

    public static string CurrentLocale { get; private set; }

    public static bool DisplayTextAsId { get; set; }

    private static Dictionary<string, string> currentLocale_;
    private static Dictionary<string, string> defaultLocale_;
    private static Dictionary<string, Dictionary<string, string>> locales_;

    private static int localeIndex_;

    public static void Initialize(string locale) {
      CurrentLocale = locale;
      locales_ = new Dictionary<string, Dictionary<string, string>>();

      Authors = new List<string>();
      Supporters = new List<string>();

      foreach (string l in ActiveLocales) {
        LoadLocale(l);
      }

      try {
        LoadSupporters();
      }
      catch (Exception) {
        // ignored
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
        Log.Write($"[KN_Core::Locale]: Unable to find locale '{CurrentLocale}', init default");
        currentLocale_ = locales_["en"];
        CurrentLocale = "en";
        localeIndex_ = 0;
      }
    }

    private static void LoadLocale(string locale) {
      const int locSize = 5;

      Log.Write($"[KN_Core::Locale]: Loading locale '{locale}' ...");
      var stream = Embedded.LoadEmbeddedFile($"Locale.{locale}.xml");
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

              string author = reader.GetAttribute("author");
              if (!string.IsNullOrEmpty(author)) {
                string loc = $"({idAttribute.ToUpper()})";
                Authors.Add($"{author,-NameSize} {loc,-locSize}");
              }

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
              Log.Write($"[KN_Core::Locale]: Failed to add locale entry: {key} -> {value}, {e.Message}");
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
      if (DisplayTextAsId || id.Length == 0 || defaultLocale_ == null) {
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

    private static void LoadSupporters() {
      var stream = Embedded.LoadEmbeddedFile("supporters.xml");
      if (stream == null) {
        return;
      }

      var names = new List<NameData>();
      using (var reader = XmlReader.Create(stream)) {
        while (reader.Read()) {
          if (reader.NodeType == XmlNodeType.Element) {
            if (!reader.HasAttributes) {
              continue;
            }

            string name = reader.GetAttribute("name");
            if (string.IsNullOrEmpty(name)) {
              continue;
            }

            string nameSizeStr = reader.GetAttribute("size");
            int size = 0;
            if (!string.IsNullOrEmpty(nameSizeStr)) {
              int.TryParse(nameSizeStr, out size);
            }

            names.Add(new NameData {
                Name = name,
                Size = size
              }
            );
          }
        }
      }

      const int columns = 3;
      const int add = 1;

      int toAdd = names.Count % columns + add;
      for (int i = 0; i < toAdd; ++i) {
        names.Add(new NameData {Name = "", Size = 0});
      }

      for (int i = 0; i < names.Count; i += columns) {
        string n0 = GetFormattedName(names[i]);
        string n1 = GetFormattedName(names[i + 1]);
        string n2 = GetFormattedName(names[i + 2]);

        try {
          Supporters.Add($"  | {n0} | {n1} | {n2} |");
        }
        catch (Exception) {
          // ignored
        }
      }
    }

    private static string GetFormattedName(NameData data) {
      string s = new string(' ', data.Size == 0 ? NameSize : data.Size);
      var builder = new StringBuilder(s);
      builder.Remove(0, data.Name.Length);
      builder.Insert(0, data.Name);
      return builder.ToString();
    }
  }
}