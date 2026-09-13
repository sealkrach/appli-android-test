using System.Collections.Generic;
using System.Linq;

namespace PolitiRush.Core
{
    /// <summary>Catalogue des héros et des cibles du niveau par défaut. Miroir de Roster.kt.</summary>
    public static class Roster
    {
        public static readonly List<Hero> Heroes = new List<Hero>
        {
            new Hero("rambeau", "Jean Rambeau", "Bandeau rouge, marcel, jamais de repos.", 6f, 1, 1.2f, Special.Rafale, 14f),
            new Hero("balbo", "Rocco Balbo", "Le boxeur de Philadelphie. Encaisse, puis frappe.", 3f, 3, 1.0f, Special.Uppercut, 12f),
            new Hero("machete", "El Machette", "Il ne texte pas. Il tranche.", 4f, 2, 1.1f, Special.Lames, 15f),
            new Hero("chauve_souris", "Le Chevalier Chauve-Souris", "Milliardaire nocturne, ceinture pleine de gadgets.", 4.5f, 1, 1.3f, Special.Gadget, 16f),
            new Hero("papa_particulier", "Le Papa Très Particulier", "Ex-agent. Il vous trouvera. Il vous tomatera.", 5f, 2, 1.1f, Special.CompetencesParticulieres, 15f),
            new Hero("transporteur", "Le Transporteur Chauve", "Costume impeccable, règles strictes, livraison garantie.", 5f, 1, 1.4f, Special.Livraison, 18f),
        };

        public static Hero Hero(string id) => Heroes.First(h => h.Id == id);

        public static readonly List<PoliticianType> Politicians = new List<PoliticianType>
        {
            PoliticianType.OfTier("prefet", "Le Préfet", Tier.HautFonctionnaire),
            PoliticianType.OfTier("inspecteur_finances", "L'Inspecteur des Finances", Tier.HautFonctionnaire),
            PoliticianType.OfTier("dir_cabinet", "La Directrice de Cabinet", Tier.HautFonctionnaire),
            PoliticianType.OfTier("depute_base", "Le Député de base", Tier.Depute),
            PoliticianType.OfTier("deputee_marche", "La Députée en marche", Tier.Depute),
            PoliticianType.OfTier("depute_insoumis", "Le Député insoumis", Tier.Depute),
            PoliticianType.OfTier("attal", "Gabriel Attal", Tier.Ministre),
            PoliticianType.OfTier("filippetti", "Aurélie Filippetti", Tier.Ministre),
            PoliticianType.OfTier("le_maire", "Bruno Le Maire", Tier.Ministre),
            PoliticianType.OfTier("darmanin", "Gérald Darmanin", Tier.Ministre),
        };

        public static PoliticianType Boss(string id, string name, Tier tier, int hp, int points, int coins) =>
            new PoliticianType(id, name, tier, hp, hp > 100 ? 0.035f : 0.04f, points, coins, true);

        public static readonly List<PoliticianType> Bosses = new List<PoliticianType>
        {
            Boss("macron", "Emmanuel Macron", Tier.ChefEtat, 60, 500, 25),
            Boss("netanyahou", "Benyamin Netanyahou", Tier.ChefEtat, 80, 600, 30),
            Boss("trump", "Donald Trump", Tier.HyperInfluent, 120, 1000, 50),
            Boss("musk", "Elon Musk", Tier.HyperInfluent, 150, 1200, 60),
            Boss("poutine", "Vladimir Poutine", Tier.HyperInfluent, 160, 1300, 60),
            Boss("zuckerberg", "Mark Zuckerberg", Tier.HyperInfluent, 140, 1100, 55),
        };
    }
}
