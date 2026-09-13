using PolitiRush.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PolitiRush.EditorTools
{
    /// <summary>
    /// Construit la scène de jeu en un clic (menu PolitiRush > Construire la scène de jeu),
    /// pour ne pas dépendre d'un fichier .unity écrit à la main. Reproduit la scène
    /// de la maquette 3D : caméra derrière le héros, route, trottoirs, lumière, HUD.
    /// </summary>
    public static class SceneBuilder
    {
        [MenuItem("PolitiRush/Construire la scène de jeu")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Caméra.
            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>(); cam.fieldOfView = 52f; cam.nearClipPlane = 0.5f; cam.farClipPlane = 80f; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.106f, 0.106f, 0.184f);
            camGo.AddComponent<AudioListener>(); camGo.AddComponent<CameraShake>();
            camGo.transform.position = new Vector3(0, 6.6f, 7.2f); camGo.transform.LookAt(new Vector3(0, 0.6f, -8f));
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(0.106f, 0.106f, 0.184f); RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 14f; RenderSettings.fogEndDistance = 34f;
            RenderSettings.ambientLight = new Color(0.35f, 0.35f, 0.45f);

            // Lumière.
            var sun = new GameObject("Soleil").AddComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.1f; sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0);

            // Décor.
            var decor = new GameObject("Décor");
            var road = GameObject.CreatePrimitive(PrimitiveType.Plane); road.name = "Route"; road.transform.SetParent(decor.transform); road.transform.localScale = new Vector3(SceneSync.RoadWidth / 10f, 1, 7f); road.transform.position = new Vector3(0, 0, -25f);
            road.GetComponent<Renderer>().sharedMaterial = CharacterBuilder.Toon(new Color(0.15f, 0.14f, 0.24f));
            foreach (float sx in new[] { -1f, 1f })
            {
                var side = GameObject.CreatePrimitive(PrimitiveType.Cube); side.name = "Trottoir"; side.transform.SetParent(decor.transform); side.transform.localScale = new Vector3(6f, 0.3f, 70f); side.transform.position = new Vector3(sx * (SceneSync.RoadWidth / 2 + 3f), 0.15f, -25f);
                side.GetComponent<Renderer>().sharedMaterial = CharacterBuilder.Toon(new Color(0.2f, 0.19f, 0.31f));
                for (int i = 0; i < 8; i++)
                {
                    var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder); post.name = "Lampadaire"; post.transform.SetParent(decor.transform); post.transform.localScale = new Vector3(0.16f, 1.6f, 0.16f); post.transform.position = new Vector3(sx * (SceneSync.RoadWidth / 2 + 1.6f), 1.9f, -i * 8f + 4f);
                    post.GetComponent<Renderer>().sharedMaterial = CharacterBuilder.Toon(new Color(0.35f, 0.34f, 0.48f));
                    var bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere); bulb.transform.SetParent(post.transform); bulb.transform.localPosition = new Vector3(0, 1.1f, 0); bulb.transform.localScale = new Vector3(3.5f, 0.35f, 3.5f);
                    bulb.GetComponent<Renderer>().sharedMaterial = CharacterBuilder.Toon(new Color(1f, 0.82f, 0.4f));
                    var l = bulb.AddComponent<Light>(); l.color = new Color(1f, 0.82f, 0.4f); l.range = 6f; l.intensity = 0.6f;
                }
            }

            // Jeu.
            var game = new GameObject("Jeu");
            var controller = game.AddComponent<GameController>();
            var input = game.AddComponent<TouchInput>(); input.Controller = controller;
            var world = new GameObject("Monde");
            var sync = game.AddComponent<SceneSync>(); sync.Controller = controller; sync.World = world.transform;
            var flow = game.AddComponent<GameFlow>(); flow.Controller = controller; flow.Scene = sync;

            // HUD.
            var canvasGo = new GameObject("HUD"); var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(405, 720); canvasGo.AddComponent<GraphicRaycaster>();
            var hud = canvasGo.AddComponent<HudView>(); hud.Controller = controller;
            hud.Wave = Label(canvasGo, "Vague", new Vector2(0, 1), new Vector2(12, -12), new Vector2(120, 24), 16, TextAnchor.UpperLeft);
            hud.Score = Label(canvasGo, "Score", new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(160, 40), 30, TextAnchor.UpperCenter);
            hud.Coins = Label(canvasGo, "Pièces", new Vector2(1, 1), new Vector2(-12, -12), new Vector2(120, 24), 16, TextAnchor.UpperRight);
            hud.Lives = Label(canvasGo, "Vies", new Vector2(0, 1), new Vector2(12, -40), new Vector2(120, 24), 16, TextAnchor.UpperLeft); hud.Lives.color = new Color(0.9f, 0.22f, 0.27f);
            hud.Combo = Label(canvasGo, "Combo", new Vector2(1, 1), new Vector2(-12, -40), new Vector2(160, 24), 16, TextAnchor.UpperRight); hud.Combo.color = new Color(0.16f, 0.62f, 0.56f);
            hud.Effects = Label(canvasGo, "Effets", new Vector2(0, 1), new Vector2(12, -66), new Vector2(380, 20), 12, TextAnchor.UpperLeft);
            hud.Power = Label(canvasGo, "Puissance", new Vector2(0, 0), new Vector2(14, 34), new Vector2(160, 44), 40, TextAnchor.LowerLeft);
            hud.WeaponLabel = Label(canvasGo, "Arme", new Vector2(0, 0), new Vector2(14, 12), new Vector2(220, 20), 11, TextAnchor.LowerLeft); hud.WeaponLabel.color = new Color(0.96f, 0.64f, 0.38f);
            hud.Toast = Label(canvasGo, "Toast", new Vector2(0.5f, 0.68f), Vector2.zero, new Vector2(400, 60), 44, TextAnchor.MiddleCenter); hud.Toast.gameObject.SetActive(false);
            hud.SpecialButton = Button(canvasGo, "Spécial", new Vector2(1, 0), new Vector2(-14, 14), new Vector2(84, 84));
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));

            System.IO.Directory.CreateDirectory("Assets/PolitiRush/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/PolitiRush/Scenes/Game.unity");
            Debug.Log("Scène PolitiRush construite : Assets/PolitiRush/Scenes/Game.unity. Appuyez sur Play.");
        }

        static Text Label(GameObject canvas, string name, Vector2 anchor, Vector2 offset, Vector2 size, int fontSize, TextAnchor align)
        {
            var go = new GameObject(name); go.transform.SetParent(canvas.transform, false);
            var rt = go.AddComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = anchor; rt.pivot = anchor; rt.anchoredPosition = offset; rt.sizeDelta = size;
            var t = go.AddComponent<Text>(); t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.fontSize = fontSize; t.fontStyle = FontStyle.Bold; t.alignment = align; t.color = Color.white; t.horizontalOverflow = HorizontalWrapMode.Overflow;
            var outline = go.AddComponent<Outline>(); outline.effectColor = new Color(0.09f, 0.07f, 0.17f); outline.effectDistance = new Vector2(1.5f, -1.5f);
            return t;
        }

        static Button Button(GameObject canvas, string name, Vector2 anchor, Vector2 offset, Vector2 size)
        {
            var go = new GameObject(name); go.transform.SetParent(canvas.transform, false);
            var rt = go.AddComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = anchor; rt.pivot = anchor; rt.anchoredPosition = offset; rt.sizeDelta = size;
            var img = go.AddComponent<Image>(); img.color = new Color(0.16f, 0.62f, 0.56f);
            var b = go.AddComponent<Button>(); b.targetGraphic = img;
            var t = Label(go, "Texte", new Vector2(0.5f, 0.5f), Vector2.zero, size, 13, TextAnchor.MiddleCenter); t.text = "SPÉCIAL";
            return b;
        }
    }
}
