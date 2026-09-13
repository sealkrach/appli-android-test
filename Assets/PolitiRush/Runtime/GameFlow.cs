using PolitiRush.Core;
using UnityEngine;

namespace PolitiRush.Runtime
{
    /// <summary>Enchaînement d'une partie : démarrage, fin, relance. Le menu et la boutique sont l'étape suivante.</summary>
    public sealed class GameFlow : MonoBehaviour
    {
        public GameController Controller;
        public SceneSync Scene;
        public bool AutoStart = true;
        public float RestartDelay = 2.5f;
        MetaProgress progress;

        void Start()
        {
            progress = ProgressStore.Load();
            Controller.OnGameOver += OnGameOver;
            if (AutoStart) Launch();
        }

        public void Launch() { Controller.StartGame(progress); Scene.ResetScene(); }

        void OnGameOver(GameState final)
        {
            progress.AfterRun(final); ProgressStore.Save(progress);
            Debug.Log($"Fin de partie : score {final.Score}, vague {final.Wave}, pièces {final.Coins}. Total pièces : {progress.Coins}");
            Invoke(nameof(Launch), RestartDelay);
        }
    }
}
