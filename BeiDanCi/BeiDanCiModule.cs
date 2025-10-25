using System.Speech.Synthesis;
using System.Text;
using System.Text.Json;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Celeste.Mod.BeiDanCi;

public sealed class BeiDanCiModule : EverestModule
{
    public const string SkippedDynamicField = "bdc_Skipped";

    public static BeiDanCiModule Instance { get; private set; } = null!;

    public override Type SettingsType => typeof(BeiDanCiSettings);
    public static BeiDanCiSettings Settings => (BeiDanCiSettings)Instance._Settings;

    public static string? CurrentVocabularyLibraryName;
    public static IReadOnlyList<Vocabulary>? CurrentVocabularyLibrary;
    public readonly static SpeechSynthesizer? Synthesizer;
    public static string VocabularyPath => Path.Combine(Everest.PathEverest, "Vocabulary");

    public static BeiDanCiSaveData SaveData = null!;

    static BeiDanCiModule()
    {
        Synthesizer = OperatingSystem.IsWindows() ? new SpeechSynthesizer() : null;
    }

    public override void Load()
    {
        Instance = this;
        Directory.CreateDirectory(VocabularyPath);
        IL.Celeste.PlayerDeadBody.Update += PlayerDeadBody_Update;
        On.Celeste.PlayerDeadBody.End += PlayerDeadBody_End;
        On.Celeste.UserIO._SerializeModSave += UserIO__SerializeModSave;
    }

    public override void Unload()
    {
        IL.Celeste.PlayerDeadBody.Update -= PlayerDeadBody_Update;
        On.Celeste.PlayerDeadBody.End -= PlayerDeadBody_End;
        On.Celeste.UserIO._SerializeModSave -= UserIO__SerializeModSave;
    }

    public override void LoadContent(bool firstLoad)
    {
        base.LoadContent(firstLoad);
        LoadVocabularyLibrary();
    }

    private static void LoadVocabularyLibrary()
    {
        // TODO uglyyyyyyyyyyy
        if (Settings.EnabledVocabularyLibrary != CurrentVocabularyLibraryName)
        {
            SaveCurrentLibrarySave();
            string file = Path.Combine(VocabularyPath, Settings.EnabledVocabularyLibrary + ".csv");
            if (!File.Exists(file))
                return;
            using FileStream fs = new(file, FileMode.Open, FileAccess.Read);
            using StreamReader srPreview = new(fs, Encoding.UTF8, leaveOpen: true);
            string? firstLine = srPreview.ReadLine();
            bool isIwpm = firstLine is "I";
            if (!isIwpm)
                fs.Seek(0, SeekOrigin.Begin);
            using StreamReader sr = new(fs, Encoding.UTF8, leaveOpen: true);
            if (isIwpm)
                sr.ReadLine();
            CurrentVocabularyLibrary = Vocabulary.ReadFrom(
                sr,
                firstLine is "I" ?
                VocabularyDataFormat.IndexWordPronunciationMeanings :
                VocabularyDataFormat.WordLinedMeanings
            );
            CurrentVocabularyLibraryName = Settings.EnabledVocabularyLibrary;
            LoadCurrentLibrarySave();
        }
    }

    private static void LoadCurrentLibrarySave()
    {
        string savePath = Path.Combine(VocabularyPath, CurrentVocabularyLibraryName + ".save.json");
        if (File.Exists(savePath))
        {
            using FileStream fs = new(savePath, FileMode.Open, FileAccess.Read);
            SaveData = JsonSerializer.Deserialize<BeiDanCiSaveData>(fs) ?? new();

        }
        else
        {
            SaveData = new();
        }
    }

    private static void SaveCurrentLibrarySave()
    {
        if (CurrentVocabularyLibraryName is null)
            return;
        string savePath = Path.Combine(VocabularyPath, CurrentVocabularyLibraryName + ".save.json");
        using FileStream fs = new(savePath, FileMode.Create, FileAccess.Write);
        JsonSerializer.Serialize(fs, SaveData);
    }

    public override void SaveSettings()
    {
        base.SaveSettings();
        LoadVocabularyLibrary();
    }

    private void UserIO__SerializeModSave(On.Celeste.UserIO.orig__SerializeModSave orig)
    {
        orig();
        SaveCurrentLibrarySave();
    }

    private static void PlayerDeadBody_Update(ILContext il)
    {
        ILCursor cur = new(il);
        cur.GotoNext(MoveType.Before, ins => ins.MatchLdarg0(), ins => ins.MatchCallvirt<PlayerDeadBody>("End"));
        cur.EmitLdarg0();
        cur.EmitDelegate(static (PlayerDeadBody body) => DynamicData.For(body).Set(SkippedDynamicField, true));
    }

    private static void PlayerDeadBody_End(On.Celeste.PlayerDeadBody.orig_End orig, PlayerDeadBody self)
    {
        if (
            CurrentVocabularyLibrary is null ||
            Random.Shared.Next(0, 10) > Settings.Possibility ||
            (Settings.TestMode == BeiDanCiTestMode.Assessment && !AbleToRollAssessment(SaveData, Settings.RollingMode)) ||
            (Settings.TestMode > BeiDanCiTestMode.Assessment && !AbleToRollSelecting(SaveData, Settings.RollingMode))
        )
        {
            orig(self);
            return;
        }
        Level level = self.SceneAs<Level>();
        if (!self.finished)
        {
            self.finished = true;

#pragma warning disable CS8524
            BeiDanCiMenu menu = Settings.TestMode switch
            {
                BeiDanCiTestMode.Assessment
                    => new(Vocabulary.RollAssessmentQuestion(CurrentVocabularyLibrary, SaveData, Settings.RollingMode)),
                BeiDanCiTestMode.SelectingENToZH
                    => new(Vocabulary.RollSelectingENToZHQuestion(CurrentVocabularyLibrary)),
                BeiDanCiTestMode.SelectingZHToEN
                    => new(Vocabulary.RollSelectingZHToENQuestion(CurrentVocabularyLibrary))
            };
#pragma warning restore CS8524

            menu.CanReload += () => level.DoScreenWipe(false, self.DeathAction ?? level.Reload, false);
            if (DynamicData.For(self).Get<bool?>(SkippedDynamicField) is true)
                menu.MarkIsBySkipping();
            level.Add(menu);
        }
    }

    private static bool AbleToRollAssessment(BeiDanCiSaveData saveData, BeiDanCiRollingMode mode) => mode switch
    {
        BeiDanCiRollingMode.NewFirst => CurrentVocabularyLibrary!.Count > 0,
        BeiDanCiRollingMode.Recall => saveData.Unfamiliars.Count > 0 || saveData.Reviews.Count > 0,
        BeiDanCiRollingMode.Review => saveData.Reviews.Count > 0,
        _ => false
    };

    private static bool AbleToRollSelecting(BeiDanCiSaveData saveData, BeiDanCiRollingMode mode) => mode switch
    {
        BeiDanCiRollingMode.NewFirst => CurrentVocabularyLibrary!.Count > 4,
        BeiDanCiRollingMode.Recall => saveData.Unfamiliars.Count > 4 || saveData.Reviews.Count > 4,
        BeiDanCiRollingMode.Review => saveData.Reviews.Count > 4,
        _ => false
    };
}