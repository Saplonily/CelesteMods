#pragma warning disable CA1416
namespace Celeste.Mod.BeiDanCi;

public sealed class BeiDanCiMenu : TextMenu
{
    private bool skipCooling;
    public event Action? CanReload;

    private BeiDanCiMenu()
    {
        AutoScroll = true;
    }

    public BeiDanCiMenu(SelectTranslationQuestion question)
    {
        AddSelectTranslation(question.Word, question.Selections, question.CorrectIndex);
        FirstSelection();
    }

    public BeiDanCiMenu(AssessmentQuestion question)
    {
        AddAssessment(question);
        FirstSelection();
    }

    public void MarkIsBySkipping()
    {
        foreach (var item in Items)
            if (item is DisabledNoSoundButton button)
                button.Disabled = true;
        skipCooling = true;
        Alarm.Set(this, BeiDanCiModule.Settings.CooldownWhenSkipped / 10f, () =>
        {
            foreach (var item in Items)
                if (item is DisabledNoSoundButton button)
                    button.Disabled = false;
            skipCooling = false;
        });
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        ((Level)scene).Paused = true;
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        ((Level)scene).Paused = false;
    }

    private void AddSelectTranslation(string word, IReadOnlyList<string> selections, int correctIndex)
    {
        Add(new Header(Dialog.Get("beidanci_ui_select_correct_trans")));
        Add(new Button(word) { Selectable = false });
        Add(new LinePadding());
        int index = 0;
        foreach (var selection in selections)
        {
            DisabledNoSoundButton button;
            Add(button = new DisabledNoSoundButton(selection));
            if (index == correctIndex)
                button.OnPressed = () => OnAnswer(true);
            else
                button.OnPressed = () => OnAnswer(false);
            index++;
        }
        Add(new DisabledNoSoundButton(Dialog.Get("beidanci_ui_dontknow")) { OnPressed = () => OnAnswer(false) });

        void OnAnswer(bool correct)
        {
            // uuuuuuuuuuugly
            if (skipCooling)
                return;
            Clear();
            AddSelectTranslationResult(word, selections[correctIndex], correct);
            FirstSelection();
        }
    }

    private void AddSelectTranslationResult(string word, string correctSelection, bool correct)
    {
        Add(new Header(correct ? Dialog.Get("beidanci_ui_answer_correct") : Dialog.Get("beidanci_ui_answer_wrong")));
        Add(new Button(word) { Selectable = false });
        Add(new Button(correctSelection) { Selectable = false });
        Add(new LinePadding());
        Add(new Button(Dialog.Get("beidanci_ui_ok")).Pressed(() => { Close(); CanReload?.Invoke(); }));
    }

    private void AddAssessment(AssessmentQuestion question)
    {
        Add(new Header(Dialog.Get("beidanci_ui_title")));

        Add(new Button(question.Word) { Selectable = false });

        if (question.Pronunciation is not null)
            Add(new Button(question.Pronunciation) { Selectable = false });
        Add(new LinePadding());
        Add(new DisabledNoSoundButton(Dialog.Get("beidanci_ui_check_meaning")).Pressed(CheckMeaning));
        Add(new DisabledNoSoundButton(Dialog.Get("beidanci_ui_listen_again")).Pressed(() => Speak(question.Word)));

        Speak(question.Word);

        void CheckMeaning()
        {
            // uuuuuuuuuuugly
            if (skipCooling)
                return;
            Clear();
            AddAssessmentRevealed(question.Word, question.Pronunciation, question.Meanings, question.Sentences);
            FirstSelection();
        }
    }

    private void AddAssessmentRevealed(
        string word, string? pronunciation,
        IEnumerable<string> meanings,
        IEnumerable<string>? sentences
    )
    {
        Add(new Header(Dialog.Get("beidanci_ui_title")));

        Add(new Button(word) { Selectable = false });
        if (pronunciation is not null)
            Add(new Button(pronunciation) { Selectable = false });

        Add(new Button(Dialog.Get("beidanci_ui_listen")).Pressed(() => Speak(word)));
        Add(new LinePadding());

        Add(new SubHeader(Dialog.Get("beidanci_ui_meaning")) { TopPadding = false });
        foreach (var meaning in meanings)
            Add(new Button(meaning) { Selectable = false });

        if (sentences is not null)
        {
            Add(new SubHeader(Dialog.Get("beidanci_ui_sentences")) { TopPadding = false });
            foreach (var sentence in sentences)
                Add(new Button(sentence) { Selectable = false });
        }

        Add(new LinePadding());

        Add(new Button(Dialog.Get("beidanci_ui_know")).Pressed(() => Result(0)));
        Add(new Button(Dialog.Get("beidanci_ui_uncertain")).Pressed(() => Result(1)));
        Add(new Button(Dialog.Get("beidanci_ui_dontknow")).Pressed(() => Result(2)));


        void Result(int reportValue)
        {
            BeiDanCiModule.SaveData.Report(word, reportValue);
            Clear();
            Close();
            CanReload?.Invoke();
        }
    }

    private static void Speak(string word)
    {
        BeiDanCiModule.Synthesizer?.SpeakAsync(word);
    }
}
