using System.Collections.Generic;
using PolitiRush.Core;
using UnityEngine;

namespace PolitiRush.Runtime
{
    /// <summary>
    /// Fait tourner le moteur de règles à pas fixe (60 Hz) et diffuse ses événements.
    /// Aucune logique de jeu ici : tout est dans PolitiRush.Core.
    /// </summary>
    public sealed class GameController : MonoBehaviour
    {
        public string HeroId = "rambeau";
        public string LevelId = "palais";
        public int Seed = 0;

        public GameEngine Engine { get; private set; }
        public GameState State => Engine?.State;
        public Level Level => Engine?.Level;
        public bool Running => Engine != null && State.Status == GameStatus.Running;

        public event System.Action<GameEvent> OnEvent;
        public event System.Action<GameState> OnGameOver;

        public float? TargetX;
        public bool SpecialRequested;

        float accumulator;
        const float Step = 1f / 60f;

        public void StartGame(MetaProgress progress = null)
        {
            var hero = Roster.Hero(HeroId);
            var level = Levels.ById(LevelId);
            var state = progress != null ? progress.StartingState(hero) : new GameState(hero);
            int seed = Seed != 0 ? Seed : (int)(System.DateTime.Now.Ticks & 0x7fffffff);
            Engine = new GameEngine(state, seed, level);
            accumulator = 0f;
        }

        void Update()
        {
            if (!Running) return;
            accumulator += Mathf.Min(Time.deltaTime, 0.1f);
            while (accumulator >= Step)
            {
                var events = Engine.Step(Step, new PlayerInput(TargetX, SpecialRequested));
                SpecialRequested = false;
                accumulator -= Step;
                foreach (var e in events) OnEvent?.Invoke(e);
                if (State.Status == GameStatus.GameOver) { OnGameOver?.Invoke(State); break; }
            }
        }
    }
}
