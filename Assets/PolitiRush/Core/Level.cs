using System.Collections.Generic;
using System.Linq;

namespace PolitiRush.Core
{
    /// <summary>Niveau à thème : cibles, échelle de boss, libellés de bonus. Miroir de Level.kt.</summary>
    public sealed class Level
    {
        public string Id; public string Name; public string Subtitle;
        public List<PoliticianType> Politicians;
        public List<PoliticianType> Bosses;
        public Dictionary<BonusType, string> BonusLabels;

        public Level(string id, string name, string subtitle, List<PoliticianType> politicians, List<PoliticianType> bosses, Dictionary<BonusType, string> bonusLabels = null)
        { Id = id; Name = name; Subtitle = subtitle; Politicians = politicians; Bosses = bosses; BonusLabels = bonusLabels ?? new Dictionary<BonusType, string>(); }

        public PoliticianType BossForWave(int wave)
        {
            int i = wave / GameConfig.BossEveryNWaves - 1;
            if (i < 0) i = 0; if (i > Bosses.Count - 1) i = Bosses.Count - 1;
            return Bosses[i];
        }

        public string BonusLabel(BonusType t) => BonusLabels.TryGetValue(t, out var l) ? l : t.Label();
    }

    public static class Levels
    {
        static PoliticianType T(string id, string name, Tier tier) => PoliticianType.OfTier(id, name, tier);
        static readonly PoliticianType Macron = Roster.Boss("macron", "Emmanuel Macron", Tier.ChefEtat, 60, 500, 25);
        static readonly PoliticianType Trump = Roster.Boss("trump", "Donald Trump", Tier.HyperInfluent, 160, 1300, 60);
        static readonly PoliticianType Musk = Roster.Boss("musk", "Elon Musk", Tier.HyperInfluent, 150, 1200, 60);

        public static readonly Level Palais = new Level("palais", "Le Palais", "La hiérarchie au grand complet", Roster.Politicians, Roster.Bosses);

        public static readonly Level Carburant = new Level("carburant", "Carburant à 2 €", "Le plein de taxes",
            new List<PoliticianType>
            {
                T("inspecteur_taxes", "L'Inspecteur des taxes", Tier.HautFonctionnaire),
                T("dir_dgec", "Le Directeur de la DGEC", Tier.HautFonctionnaire),
                T("conseillere_budget", "La Conseillère budgétaire", Tier.HautFonctionnaire),
                T("depute_taxe_carbone", "Le Député pro-taxe carbone", Tier.Depute),
                T("deputee_malus", "La Députée du malus", Tier.Depute),
                T("le_maire", "Bruno Le Maire", Tier.Ministre),
                T("pannier_runacher", "Agnès Pannier-Runacher", Tier.Ministre),
                T("bechu", "Christophe Béchu", Tier.Ministre),
                T("borne", "Élisabeth Borne", Tier.Ministre),
            },
            new List<PoliticianType> { Macron, Roster.Boss("pouyanne", "Patrick Pouyanné", Tier.HyperInfluent, 130, 1000, 50), Musk, Trump },
            new Dictionary<BonusType, string>
            {
                { BonusType.Sondage, "Ristourne" }, { BonusType.Scandale, "Blocage de raffinerie" }, { BonusType.TomateGeante, "Bidon géant" },
                { BonusType.MotionDeCensure, "Gilets jaunes" }, { BonusType.Meeting, "Chèque carburant" },
            });

        public static readonly Level Aeroport = new Level("aeroport", "L'aéroport bradé", "Privatisation express",
            new List<PoliticianType>
            {
                T("hf_bercy", "Le Haut fonctionnaire de Bercy", Tier.HautFonctionnaire),
                T("expert_participations", "L'Expert des participations", Tier.HautFonctionnaire),
                T("dir_concessions", "La Directrice des concessions", Tier.HautFonctionnaire),
                T("depute_privatisation", "Le Député pro-privatisation", Tier.Depute),
                T("rapporteure_budget", "La Rapporteure du budget", Tier.Depute),
                T("le_maire", "Bruno Le Maire", Tier.Ministre),
                T("borne", "Élisabeth Borne", Tier.Ministre),
                T("djebbari", "Jean-Baptiste Djebbari", Tier.Ministre),
            },
            new List<PoliticianType> { Macron, Roster.Boss("huillard", "Xavier Huillard", Tier.HyperInfluent, 120, 1000, 50), Musk, Trump },
            new Dictionary<BonusType, string>
            {
                { BonusType.Sondage, "Enquête parlementaire" }, { BonusType.Scandale, "Grève des contrôleurs" }, { BonusType.TomateGeante, "Tomate long-courrier" },
                { BonusType.MotionDeCensure, "Référendum" }, { BonusType.Meeting, "Duty free" },
            });

        public static readonly List<Level> All = new List<Level> { Palais, Carburant, Aeroport };
        public static Level ById(string id) => All.First(l => l.Id == id);
    }
}
