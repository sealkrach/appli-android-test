using UnityEngine;

namespace PolitiRush.Runtime
{
    /// <summary>Doigt ou souris : position horizontale dans [0, 1] tant que le doigt est posé. Flèches au clavier pour le PC.</summary>
    public sealed class TouchInput : MonoBehaviour
    {
        public GameController Controller;

        void Update()
        {
            if (Controller == null) return;
            float? target = null;
            if (Input.touchCount > 0)
            {
                var t = Input.GetTouch(0);
                if (t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled) target = t.position.x / Screen.width;
            }
            else if (Input.GetMouseButton(0)) target = Input.mousePosition.x / Screen.width;
            else if (Input.GetKey(KeyCode.LeftArrow)) target = 0f;
            else if (Input.GetKey(KeyCode.RightArrow)) target = 1f;
            Controller.TargetX = target;
            if (Input.GetKeyDown(KeyCode.Space)) Controller.SpecialRequested = true;
        }
    }
}
