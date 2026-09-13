using PolitiRush.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PolitiRush.Runtime
{
    /// <summary>HUD minimal en uGUI : vague, score, pièces, vies, combo, puissance et arme, bouton spécial.</summary>
    public sealed class HudView : MonoBehaviour
    {
        public GameController Controller;
        public Text Wave, Score, Coins, Lives, Combo, Effects, Power, WeaponLabel, Toast;
        public Button SpecialButton;
        float toastTimer;

        void Start()
        {
            if (SpecialButton) SpecialButton.onClick.AddListener(() => Controller.SpecialRequested = true);
            if (Controller) Controller.OnEvent += OnEvent;
        }

        void OnEvent(GameEvent ev)
        {
            switch (ev)
            {
                case GameEvent.GatePassed g: ShowToast($"{g.Op.Label}  →  x{g.NewFirepower}", g.Op.IsGood ? new Color(0.16f, 0.62f, 0.56f) : new Color(0.9f, 0.22f, 0.27f)); break;
                case GameEvent.WaveStarted w: ShowToast("VAGUE " + w.Wave + (w.Wave % GameConfig.BossEveryNWaves == 0 ? " · BOSS" : ""), new Color(0.96f, 0.64f, 0.38f)); break;
                case GameEvent.BonusPicked b: ShowToast(Controller.Level.BonusLabel(b.Type).ToUpper(), new Color(0.61f, 0.36f, 0.9f)); break;
            }
        }

        void ShowToast(string text, Color c) { if (!Toast) return; Toast.text = text; Toast.color = c; Toast.gameObject.SetActive(true); toastTimer = 0.7f; }

        void Update()
        {
            var s = Controller?.State; if (s == null) return;
            if (Wave) Wave.text = "Vague " + s.Wave;
            if (Score) Score.text = s.Score.ToString();
            if (Coins) Coins.text = "Pièces : " + s.Coins;
            if (Lives) Lives.text = "Vies : " + s.HeroHp;
            if (Combo) Combo.text = s.Combo >= 3 ? "COMBO x" + s.Combo : "";
            if (Power) Power.text = "x" + s.Firepower;
            if (WeaponLabel) WeaponLabel.text = s.Weapon.Label().ToUpper();
            if (Effects)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var kv in s.Effects) sb.Append(Controller.Level.BonusLabel(kv.Key)).Append(' ').Append(Mathf.CeilToInt(kv.Value)).Append("s   ");
                Effects.text = sb.ToString();
            }
            if (SpecialButton) { SpecialButton.interactable = s.SpecialReady; SpecialButton.GetComponentInChildren<Text>().text = s.SpecialReady ? "SPÉCIAL" : Mathf.CeilToInt(s.SpecialCooldownRemaining) + "s"; }
            if (toastTimer > 0f) { toastTimer -= Time.deltaTime; if (toastTimer <= 0f && Toast) Toast.gameObject.SetActive(false); }
        }
    }
}
