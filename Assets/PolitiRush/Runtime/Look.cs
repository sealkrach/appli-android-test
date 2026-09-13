using System.Collections.Generic;
using UnityEngine;

namespace PolitiRush.Runtime
{
    /// <summary>
    /// Traits de caricature d'un personnage. Placeholder pour les vrais modèles :
    /// tant que le graphiste n'a pas livré, CharacterBuilder assemble des primitives.
    /// </summary>
    public sealed class Look
    {
        public Color Skin = C(0xF0D0B0);
        public Color? Hair = C(0x3b2a1a);
        public string Style = "short"; // short, long, bob, quiff, thin, cowl, bald
        public Color Suit = C(0x444444);
        public Color? Shirt, Tie, Band, Gloves, Cape;
        public bool Scarf, Glasses, Mustache, Cap, RedCap, Tee;

        static Color C(int hex) => new Color(((hex >> 16) & 255) / 255f, ((hex >> 8) & 255) / 255f, (hex & 255) / 255f);

        public static readonly Dictionary<string, Look> ById = new Dictionary<string, Look>
        {
            // Héros.
            { "rambeau", new Look { Skin = C(0xD9A066), Hair = C(0x2b1d14), Style = "long", Suit = C(0x4a5a3a), Band = C(0xE63946) } },
            { "balbo", new Look { Skin = C(0xE0B08A), Hair = C(0x1a1a1a), Style = "short", Suit = C(0x7d7d7d), Gloves = C(0xE63946) } },
            { "machete", new Look { Skin = C(0xB5773F), Hair = C(0x111111), Style = "long", Suit = C(0x2b2b2b), Mustache = true } },
            { "chauve_souris", new Look { Skin = C(0xE0B08A), Hair = C(0x111111), Style = "cowl", Suit = C(0x1f1f2e), Cape = C(0x111111) } },
            { "papa_particulier", new Look { Skin = C(0xE8C0A0), Hair = C(0x9a9a9a), Style = "short", Suit = C(0x3a3a3a) } },
            { "transporteur", new Look { Skin = C(0xE0B08A), Hair = null, Style = "bald", Suit = C(0x111111), Shirt = Color.white, Tie = C(0x111111) } },
            // Cibles nommées (mêmes traits que la maquette 3D).
            { "macron", new Look { Skin = C(0xF0D0B0), Hair = C(0x4a3728), Style = "short", Suit = C(0x1f3a5f), Tie = C(0x1f3a5f), Shirt = Color.white } },
            { "le_maire", new Look { Skin = C(0xF0D0B0), Hair = C(0x9a8a7a), Style = "short", Suit = C(0x2c3e50), Tie = C(0x3a6ea5), Shirt = Color.white } },
            { "attal", new Look { Skin = C(0xF0D0B0), Hair = C(0x3b2a1a), Style = "short", Suit = C(0x1f3a5f), Tie = C(0x1f3a5f), Shirt = Color.white } },
            { "filippetti", new Look { Skin = C(0xF0D0B0), Hair = C(0x2a1a12), Style = "long", Suit = C(0x8e1b3c) } },
            { "darmanin", new Look { Skin = C(0xF0D0B0), Hair = C(0x2b1d14), Style = "short", Suit = C(0x1a1a2e), Tie = C(0x0033A0), Shirt = Color.white } },
            { "borne", new Look { Skin = C(0xF0D0B0), Hair = C(0x6b5a4a), Style = "bob", Suit = C(0x2c2c3c), Glasses = true } },
            { "pannier_runacher", new Look { Skin = C(0xF0D0B0), Hair = C(0x3a2a1a), Style = "bob", Suit = C(0x3a3a5a) } },
            { "bechu", new Look { Skin = C(0xF0D0B0), Hair = C(0x2a1a12), Style = "short", Suit = C(0x2c3e50), Tie = C(0x2A9D8F), Shirt = Color.white } },
            { "djebbari", new Look { Skin = C(0xE8C0A0), Hair = C(0x1a1a1a), Style = "short", Suit = C(0x2c3e50), Tie = C(0x1f3a5f), Shirt = Color.white } },
            { "netanyahou", new Look { Skin = C(0xE8C0A0), Hair = C(0xbbbbbb), Style = "short", Suit = C(0x222222), Tie = C(0x3a6ea5), Shirt = Color.white } },
            { "trump", new Look { Skin = C(0xF2B07A), Hair = C(0xF5D76E), Style = "quiff", Suit = C(0x1a1a2e), Tie = C(0xE63946), Shirt = Color.white, RedCap = true } },
            { "musk", new Look { Skin = C(0xF0D0B0), Hair = C(0x3b2a1a), Style = "short", Suit = C(0x111111), Tee = true } },
            { "poutine", new Look { Skin = C(0xF0D0B0), Hair = C(0xd9d0c0), Style = "thin", Suit = C(0x2b2b2b), Tie = C(0x8b0000), Shirt = Color.white } },
            { "zuckerberg", new Look { Skin = C(0xF5DCC5), Hair = C(0x6b4a2a), Style = "short", Suit = C(0x444444), Tee = true } },
            { "pouyanne", new Look { Skin = C(0xF0D0B0), Hair = C(0x9a9a9a), Style = "thin", Suit = C(0x1a1a2e), Tie = C(0xE63946), Shirt = Color.white, Glasses = true } },
            { "huillard", new Look { Skin = C(0xF0D0B0), Hair = C(0xcccccc), Style = "short", Suit = C(0x2c3e50), Tie = C(0x1f3a5f), Shirt = Color.white } },
        };

        /// <summary>Archétype par défaut selon le rang, avec écharpe pour les élus et casquette pour le préfet.</summary>
        public static Look For(string id, Core.Tier tier)
        {
            if (ById.TryGetValue(id, out var l)) return l;
            var look = new Look { Skin = C(0xE8C0A0), Hair = C(0x5a4632), Style = id.Contains("directrice") || id.Contains("deputee") || id.Contains("conseillere") || id.Contains("rapporteure") ? "bob" : "short", Suit = C(0x34495e), Shirt = Color.white, Tie = C(0x777777) };
            if (tier == Core.Tier.Depute) { look.Scarf = true; look.Tie = C(0xE63946); }
            if (id == "prefet") look.Cap = true;
            if (id.Contains("inspecteur") || id.Contains("expert")) look.Glasses = true;
            return look;
        }
    }
}
