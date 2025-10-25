using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using NReco.Csv;

namespace Celeste.Mod.BeiDanCi;

public enum VocabularyDataFormat
{
    IndexWordPronunciationMeanings,
    WordLinedMeanings
}

public sealed partial record Vocabulary(
    string Word,
    string? Pronunciation,
    IReadOnlyList<string> Meanings,
    IReadOnlyList<string>? Sentences
)
{
    public static List<Vocabulary> ReadFrom(StreamReader reader, VocabularyDataFormat format)
    {
        List<Vocabulary> vocabularies = new();
        CsvReader csvReader = new(reader);
        switch (format)
        {
        case VocabularyDataFormat.IndexWordPronunciationMeanings:
            while (csvReader.Read())
            {
                string meaningsString = csvReader[3];
                var meaningsMatches = GetSplitMeaningRegex().Matches(meaningsString);
                vocabularies.Add(
                    new Vocabulary(
                        csvReader[1],
                        csvReader[2],
                        meaningsMatches.Select(m => m.Value).ToList(),
                        null
                    )
                );
            }
            break;
        case VocabularyDataFormat.WordLinedMeanings:
            while (csvReader.Read())
            {
                vocabularies.Add(new Vocabulary(csvReader[0], null, csvReader[1].Split("\n"), null));
            }
            break;
        }
        return vocabularies;
    }

    public static AssessmentQuestion RollAssessmentQuestion(
        IReadOnlyList<Vocabulary> vocabularies,
        BeiDanCiSaveData saveData,
        BeiDanCiRollingMode mode
    )
    {
        Random random = Random.Shared;

        int s1 = 10, s2 = 10;

        if (mode == BeiDanCiRollingMode.NewFirst)
        {
            s1 = 7;
            s2 = 9;
        }
        else if (mode == BeiDanCiRollingMode.Recall)
        {
            s1 = 0;
            s2 = 7;
        }
        else if (mode == BeiDanCiRollingMode.Review)
        {
            s1 = 0;
            s2 = 0;
        }

    Reroll:
        Vocabulary vocabulary;
        int r = random.Next(0, 10);
        // 新单词
        if (r < s1)
        {
            // TODO any better way...?
            var freshVocabularies = vocabularies.Where(v =>
                !saveData.Known.Contains(v.Word) &&
                !saveData.Unfamiliars.ContainsKey(v.Word) &&
                !saveData.Reviews.ContainsKey(v.Word)
            ).ToList();

            vocabulary = freshVocabularies[random.Next(0, freshVocabularies.Count)];
        }
        // 不熟悉单词
        else if (r >= s1 && r < s2)
        {
            int count = saveData.Unfamiliars.Count;
            if (count == 0)
                goto Reroll;
            var pair = saveData.Unfamiliars.ElementAt(random.Next(0, count));
            vocabulary = vocabularies.First(v => v.Word == pair.Key);
        }
        // 复习的单词
        else if (r >= s2)
        {
            int count = saveData.Reviews.Count;
            if (count == 0)
                goto Reroll;
            var pair = saveData.Reviews.ElementAt(random.Next(0, count));
            vocabulary = vocabularies.First(v => v.Word == pair.Key);
        }
        else
        {
            throw new UnreachableException();
        }
        return new AssessmentQuestion(vocabulary.Word, vocabulary.Pronunciation, vocabulary.Meanings, vocabulary.Sentences);
    }

    public static SelectTranslationQuestion RollSelectingENToZHQuestion(
        IReadOnlyList<Vocabulary> vocabularies
    )
    {
        Random random = Random.Shared;

        Span<int> choices = stackalloc int[4];
        RollIndexes(random, choices, 0, vocabularies.Count);
        List<string> selections = new();
        foreach (var i in choices)
            selections.Add(string.Join(' ', vocabularies[i].Meanings));

        int correctIndex = random.Next(0, 4);

        return new SelectTranslationQuestion(vocabularies[choices[correctIndex]].Word, selections, correctIndex);
    }

    public static SelectTranslationQuestion RollSelectingZHToENQuestion(IReadOnlyList<Vocabulary> vocabularies)
    {
        Random random = Random.Shared;

        Span<int> choices = stackalloc int[4];
        RollIndexes(random, choices, 0, vocabularies.Count);
        List<string> selections = new();
        foreach (var i in choices)
            selections.Add(vocabularies[i].Word);

        int correctIndex = random.Next(0, 4);

        return new SelectTranslationQuestion(string.Join(' ', vocabularies[choices[correctIndex]].Meanings), selections, correctIndex);
    }

    [GeneratedRegex(@"[a-z]+\.\s*[^a-z]*(?=[a-z]+\.\s*|$)", RegexOptions.IgnoreCase)]
    private static partial Regex GetSplitMeaningRegex();

    private static void RollIndexes(Random random, Span<int> destinationSpan, int minValue, int maxValue)
    {
        if (destinationSpan.Length > maxValue - minValue)
            throw new ArgumentOutOfRangeException(nameof(destinationSpan));

        for (int i = 0; i < destinationSpan.Length; i++)
        {
        ReRoll:
            int value = random.Next(minValue, maxValue);

            for (int j = 0; j < i; j++)
                if (destinationSpan[j] == value)
                    goto ReRoll;

            destinationSpan[i] = value;
        }
    }
}