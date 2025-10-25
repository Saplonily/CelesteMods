using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BeiDanCi;

public record SelectTranslationQuestion(
    string Word,
    IReadOnlyList<string> Selections,
    int CorrectIndex
);

public record AssessmentQuestion(
    string Word, 
    string? Pronunciation, 
    IReadOnlyList<string> Meanings, 
    IReadOnlyList<string>? Sentences
);