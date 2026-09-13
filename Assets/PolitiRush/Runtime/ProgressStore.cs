using PolitiRush.Core;
using UnityEngine;

namespace PolitiRush.Runtime
{
    /// <summary>Progression persistante dans PlayerPrefs (équivalent de ProgressStore.kt).</summary>
    public static class ProgressStore
    {
        public static MetaProgress Load()
        {
            var m = new MetaProgress { Coins = PlayerPrefs.GetInt("coins", 0), BestScore = PlayerPrefs.GetInt("bestScore", 0), BestWave = PlayerPrefs.GetInt("bestWave", 0), Runs = PlayerPrefs.GetInt("runs", 0) };
            foreach (var u in UpgradeInfo.All) { int l = PlayerPrefs.GetInt("lvl_" + u, 0); if (l > 0) m.Levels[u] = l; }
            return m;
        }

        public static void Save(MetaProgress m)
        {
            PlayerPrefs.SetInt("coins", m.Coins); PlayerPrefs.SetInt("bestScore", m.BestScore); PlayerPrefs.SetInt("bestWave", m.BestWave); PlayerPrefs.SetInt("runs", m.Runs);
            foreach (var u in UpgradeInfo.All) PlayerPrefs.SetInt("lvl_" + u, m.Level(u));
            PlayerPrefs.Save();
        }
    }
}
