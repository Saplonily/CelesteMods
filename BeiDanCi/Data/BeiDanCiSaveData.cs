namespace Celeste.Mod.BeiDanCi;

public sealed class BeiDanCiSaveData
{
    public HashSet<string> Known { get; set; } = new();

    public Dictionary<string, int> Unfamiliars { get; set; } = new();

    public Dictionary<string, int> Reviews { get; set; } = new();

    // 懒得写枚举了
    /// <param name="what">0: 认识, 1: 模糊, 2: 不认识</param>
    public void Report(string word, int what)
    {
        bool inKnown = Known.Contains(word);

        if (what == 0)
        {
            if (inKnown)
                return;
            if (Unfamiliars.TryGetValue(word, out int v1))
            {
                v1++;
                if (v1 >= 7)
                {
                    Unfamiliars.Remove(word);
                    Reviews.Add(word, 0);
                }
                else
                {
                    Unfamiliars[word] = v1;
                }
            }
            else if (Reviews.TryGetValue(word, out int v2))
            {
                v2++;
                Reviews[word] = v2;
            }
            else
            {
                Known.Add(word);
            }
        }
        else if (what == 1)
        {
            if (inKnown)
                throw new ArgumentOutOfRangeException(nameof(what));
            if (Unfamiliars.TryGetValue(word, out int v1))
            {
                // nothing
            }
            else if (Reviews.TryGetValue(word, out int v2))
            {
                // nothing too
            }
            else
            {
                Unfamiliars.Add(word, 0);
            }
        }
        else if (what == 2)
        {
            if (inKnown)
                throw new ArgumentOutOfRangeException(nameof(what));
            if (Unfamiliars.TryGetValue(word, out int v1))
            {
                Unfamiliars[word] = Math.Max(0, v1 - 1);
            }
            else if (Reviews.TryGetValue(word, out int v2))
            {
                if (v2 != 0)
                {
                    Reviews[word] = 0;
                }
                else
                {
                    Reviews.Remove(word);
                    Unfamiliars.Add(word, 2);
                }
            }
            else
            {
                Unfamiliars.Add(word, 0);
            }
        }
    }
}
