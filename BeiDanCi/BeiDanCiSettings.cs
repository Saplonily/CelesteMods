using YamlDotNet.Serialization;

namespace Celeste.Mod.BeiDanCi;

public enum BeiDanCiTestMode
{
    Assessment,
    SelectingENToZH,
    SelectingZHToEN
}

public enum BeiDanCiRollingMode
{
    NewFirst,
    Recall,
    Review
}

public sealed class BeiDanCiSettings : EverestModuleSettings
{
    [SettingSubText("modoptions_beidanci_enabled_desc")]
    public bool Enabled { get; set; } = true;

    public BeiDanCiTestMode TestMode { get; set; }

    public BeiDanCiRollingMode RollingMode { get; set; }

    [SettingRange(1, 10)]
    public int Possibility { get; set; } = 10;

    [SettingRange(1, 15)]
    [SettingSubText("modoptions_beidanci_cooldownwhenskipped_desc")]
    public int CooldownWhenSkipped { get; set; } = 10;

    public string? EnabledVocabularyLibrary { get; set; }

    [SettingSubMenu]
    public class CheckWordsSubMenu
    {
        [SettingIgnore, YamlIgnore]
        public bool IsUnfamiliar { get; set; }

        [YamlIgnore]
        public int NumberOfEntries { get; set; } = 10;

        private List<TextMenu.Button> topLabels = new();

        public CheckWordsSubMenu(bool isUnfamiliar)
            => IsUnfamiliar = isUnfamiliar;

        public void CreateNumberOfEntriesEntry(TextMenuExt.SubMenu menu, bool inGame)
        {
            string label = Dialog.Get("modoptions_beidanci_words_numberofentries");
            menu.Add(new TextMenuExt.IntSlider(label, 5, 30, 10).Change(v =>
            {
                NumberOfEntries = v;
                if (topLabels.Count > 0)
                {
                    foreach (var item in topLabels)
                        menu.Remove(item);
                    topLabels.Clear();
                }
                BuildTops(menu);
            }));

            BuildTops(menu);

            void BuildTops(TextMenuExt.SubMenu menu)
            {
                var dic = IsUnfamiliar ? BeiDanCiModule.SaveData.Unfamiliars : BeiDanCiModule.SaveData.Reviews;
                var tops = dic.OrderBy(p => p.Value).Take(NumberOfEntries);
                foreach (var top in tops)
                {
                    var button = new TextMenu.Button($"{top.Value}: {top.Key}");
                    menu.Add(button);
                    topLabels.Add(button);
                }
            }
        }
    }

    [YamlIgnore]
    public CheckWordsSubMenu UnfamiliarWords { get; set; } = new(true);

    [YamlIgnore]
    public CheckWordsSubMenu ReviewWords { get; set; } = new(false);

    public void CreateEnabledVocabularyLibraryEntry(TextMenu menu, bool inGame)
    {
        string path = BeiDanCiModule.VocabularyPath;
        var files = Directory.EnumerateFiles(path, "*.csv").Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
        string sliderText = Dialog.Get("modoptions_beidanci_enabledvocabularylibrary");
        TextMenu.Slider slider;
        int index = files.IndexOf(EnabledVocabularyLibrary!);
        menu.Add(slider = new TextMenu.Slider(sliderText, i => files[i], 0, files.Count - 1, index));
        slider.Change(i => EnabledVocabularyLibrary = files[i]);
        if (index == -1)
            EnabledVocabularyLibrary = files.Count > 0 ? files[0] : null;

        slider.AddDescription(menu, Dialog.Clean("modoptions_beidanci_vocabularylibrary_tips"));
    }
}