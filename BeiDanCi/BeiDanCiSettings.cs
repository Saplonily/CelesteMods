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

    public BeiDanCiTestMode TestMode { get; set; }

    public BeiDanCiRollingMode RollingMode { get; set; }

    [SettingRange(1, 10)]
    public int Possibility { get; set; } = 10;

    public string? EnabledVocabularyLibrary { get; set; }

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