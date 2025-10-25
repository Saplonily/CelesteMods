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
            if (item is CanBeDisabledButton button)
                button.Disabled = true;
        skipCooling = true;
        Alarm.Set(this, 1.5f, () =>
        {
            foreach (var item in Items)
                if (item is CanBeDisabledButton button)
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
        Add(new Header("选择正确的翻译"));
        Add(new Button(word) { Selectable = false });
        Add(new LinePadding());
        int index = 0;
        foreach (var selection in selections)
        {
            CanBeDisabledButton button;
            Add(button = new CanBeDisabledButton(selection));
            if (index == correctIndex)
                button.OnPressed = AnswerCorrect;
            else
                button.OnPressed = AnswerWrong;
            index++;
        }
        Add(new CanBeDisabledButton("不认识") { OnPressed = AnswerWrong });

        void AnswerCorrect()
        {
            // uuuuuuuuuuugly
            if (skipCooling)
                return;
            Clear();
            AddSelectTranslationResult(word, selections[correctIndex], true);
            FirstSelection();
        }

        void AnswerWrong()
        {
            // uuuuuuuuuuugly
            if (skipCooling)
                return;
            Clear();
            AddSelectTranslationResult(word, selections[correctIndex], false);
            FirstSelection();
        }
    }

    private void AddSelectTranslationResult(string word, string correctSelection, bool correct)
    {
        Add(new Header(correct ? "√ 回答正确" : "× 回答错误"));
        Add(new Button(word) { Selectable = false });
        Add(new Button(correctSelection) { Selectable = false });
        Add(new LinePadding());
        Add(new Button("确认").Pressed(() => { Close(); CanReload?.Invoke(); }));
    }

    private void AddAssessment(AssessmentQuestion question)
    {
        Add(new Header("背单词"));

        Add(new Button(question.Word) { Selectable = false });

        if (question.Pronunciation is not null)
            Add(new Button(question.Pronunciation) { Selectable = false });
        Add(new LinePadding());
        Add(new CanBeDisabledButton("查看释义").Pressed(CheckMeaning));
        Add(new CanBeDisabledButton("再次发音").Pressed(() => Speak(question.Word)));

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
        Add(new Header("背单词"));

        Add(new Button(word) { Selectable = false });
        if (pronunciation is not null)
            Add(new Button(pronunciation) { Selectable = false });

        Add(new Button("发音").Pressed(() => Speak(word)));
        Add(new LinePadding());

        Add(new SubHeader("释义") { TopPadding = false });
        foreach (var meaning in meanings)
            Add(new Button(meaning) { Selectable = false });

        if (sentences is not null)
        {
            Add(new SubHeader("例句") { TopPadding = false });
            foreach (var sentence in sentences)
                Add(new Button(sentence) { Selectable = false });
        }

        Add(new LinePadding());

        Add(new Button("认识").Pressed(() => Result(0)));
        Add(new Button("模糊").Pressed(() => Result(1)));
        Add(new Button("不认识").Pressed(() => Result(2)));


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
