using PolitiRush.Core;
using UnityEngine;

namespace PolitiRush.Runtime
{
    /// <summary>Le héros et ses quatre armes (main, lance-pierre, canon, tank).</summary>
    public sealed class HeroView : MonoBehaviour
    {
        CharacterAnimator anim; GameObject slingshot, cannon, tank; Transform character;

        public static HeroView Create(Transform world, Hero hero)
        {
            var go = new GameObject("Héros " + hero.Name); go.transform.SetParent(world, false);
            var v = go.AddComponent<HeroView>();
            var ch = CharacterBuilder.Build(Look.For(hero.Id, Tier.HautFonctionnaire), false, 0.5f); ch.transform.SetParent(go.transform, false); ch.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            v.character = ch.transform; v.anim = ch.GetComponent<CharacterAnimator>();
            v.slingshot = Weapons.Slingshot(go.transform); v.cannon = Weapons.Cannon(go.transform); v.tank = Weapons.Tank(go.transform);
            return v;
        }

        public void Sync(GameState s, bool running, float t)
        {
            transform.position = new Vector3(SceneSync.WX(s.HeroX), 0, 0);
            var w = s.Weapon;
            slingshot.SetActive(w == Weapon.LancePierre); cannon.SetActive(w == Weapon.Canon); tank.SetActive(w == Weapon.Tank);
            if (w == Weapon.Tank) { character.localPosition = new Vector3(0, 1.15f, 0); anim.Animate(t * 3f, 1f); }
            else { character.localPosition = Vector3.zero; anim.Animate(running ? t * 12f : t * 3f); }
        }

        public void OnGatePassed(GameEvent.GatePassed g) { CameraShake.Instance?.Punch(); }
    }

    /// <summary>Une cible : personnage + étiquette (nom, rang, barre de vie).</summary>
    public sealed class EnemyView : MonoBehaviour
    {
        CharacterAnimator anim; TextMesh label; Transform hpBar; float size;

        public static EnemyView Create(Transform world, Enemy e)
        {
            var go = new GameObject(e.Type.Name); go.transform.SetParent(world, false);
            var v = go.AddComponent<EnemyView>();
            v.size = e.Type.IsBoss ? 1.05f : e.Type.Tier == Tier.Ministre ? 0.62f : e.Type.Tier == Tier.Depute ? 0.55f : 0.5f;
            var ch = CharacterBuilder.Build(Look.For(e.Type.Id, e.Type.Tier), true, v.size); ch.transform.SetParent(go.transform, false);
            v.anim = ch.GetComponent<CharacterAnimator>();
            var lgo = new GameObject("Label"); lgo.transform.SetParent(go.transform, false); lgo.transform.localPosition = new Vector3(0, 3.9f * v.size + 0.4f, 0);
            v.label = lgo.AddComponent<TextMesh>(); v.label.text = (e.Type.IsBoss ? e.Type.Tier.Label().ToUpper() + "\n" : "") + e.Type.Name;
            v.label.anchor = TextAnchor.LowerCenter; v.label.alignment = TextAlignment.Center; v.label.characterSize = 0.08f; v.label.fontSize = 48; v.label.color = Color.white;
            lgo.AddComponent<Billboard>();
            var bar = GameObject.CreatePrimitive(PrimitiveType.Cube); Destroy(bar.GetComponent<Collider>()); bar.transform.SetParent(go.transform, false);
            bar.transform.localPosition = new Vector3(0, 3.9f * v.size + 0.25f, 0); bar.GetComponent<Renderer>().sharedMaterial = CharacterBuilder.Toon(new Color(0.9f, 0.22f, 0.27f));
            v.hpBar = bar.transform;
            return v;
        }

        public void Sync(Enemy e, float t, bool frozen)
        {
            transform.position = new Vector3(SceneSync.WX(e.X), 0, SceneSync.WZ(e.Y));
            anim.Animate(frozen ? 0f : t * 10f + e.Id, e.Hurt > 0f ? 0.85f : -1f, e.Hurt > 0f ? 0.15f : 0f);
            anim.SetFlash(e.Hurt > 0f);
            float frac = Mathf.Clamp01(e.Hp / (float)e.MaxHp);
            hpBar.localScale = new Vector3(1.6f * size * frac, 0.08f, 0.08f);
            hpBar.localPosition = new Vector3(-0.8f * size * (1f - frac), 3.9f * size + 0.25f, 0);
        }

        public void HideLabel() { if (label) label.gameObject.SetActive(false); if (hpBar) hpBar.gameObject.SetActive(false); }
    }

    /// <summary>Panneau de porte : couleur selon le signe, texte de l'opération.</summary>
    public sealed class GateView : MonoBehaviour
    {
        public static GateView Create(Transform world, Gate g)
        {
            var go = new GameObject("Porte " + g.Op.Label); go.transform.SetParent(world, false);
            var v = go.AddComponent<GateView>();
            float w = SceneSync.RoadWidth / 2 - 0.4f;
            var panel = GameObject.CreatePrimitive(PrimitiveType.Cube); Destroy(panel.GetComponent<Collider>()); panel.transform.SetParent(go.transform, false);
            panel.transform.localPosition = new Vector3(0, 1.0f, 0); panel.transform.localScale = new Vector3(w, 1.5f, 0.12f);
            var col = g.Op.IsGood ? new Color(0.16f, 0.62f, 0.56f, 0.8f) : new Color(0.9f, 0.22f, 0.27f, 0.8f);
            var m = CharacterBuilder.Toon(col); panel.GetComponent<Renderer>().sharedMaterial = m;
            foreach (float sx in new[] { -1f, 1f }) { var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder); Destroy(post.GetComponent<Collider>()); post.transform.SetParent(go.transform, false); post.transform.localPosition = new Vector3(sx * w / 2, 0.5f, 0); post.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f); post.GetComponent<Renderer>().sharedMaterial = CharacterBuilder.Toon(new Color(0.54f, 0.53f, 0.66f)); }
            var tgo = new GameObject("Texte"); tgo.transform.SetParent(go.transform, false); tgo.transform.localPosition = new Vector3(0, 1.0f, 0.1f); tgo.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            var tm = tgo.AddComponent<TextMesh>(); tm.text = g.Op.Label; tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center; tm.characterSize = 0.25f; tm.fontSize = 64; tm.fontStyle = FontStyle.Bold; tm.color = Color.white;
            return v;
        }
    }

    /// <summary>Fait face à la caméra (étiquettes).</summary>
    public sealed class Billboard : MonoBehaviour
    {
        void LateUpdate() { if (Camera.main) transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position); }
    }

    /// <summary>Secousse et zoom de caméra. La caméra reste à la position de la maquette 3D.</summary>
    public sealed class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }
        public Vector3 BasePosition = new Vector3(0, 6.6f, 7.2f);
        public Vector3 LookAt = new Vector3(0, 0.6f, -8f);
        public float BaseFov = 52f;
        float shake, fov; Camera cam;

        void Awake() { Instance = this; cam = GetComponent<Camera>(); fov = BaseFov; }
        public void Shake(float s) { shake = Mathf.Max(shake, s); }
        public void Punch() { fov = 62f; shake = Mathf.Max(shake, 0.25f); }

        void LateUpdate()
        {
            shake = Mathf.Max(0f, shake - Time.deltaTime);
            fov = Mathf.Lerp(fov, BaseFov, 0.08f); if (cam) cam.fieldOfView = fov;
            transform.position = BasePosition + (shake > 0f ? new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0) * shake * 0.6f : Vector3.zero);
            transform.LookAt(LookAt);
        }
    }

    /// <summary>Accessoires : lance-pierre, canon, tank, en primitives.</summary>
    public static class Weapons
    {
        static GameObject P(Transform parent, PrimitiveType type, Color c, Vector3 pos, Vector3 scale, Vector3? rot = null)
        {
            var go = GameObject.CreatePrimitive(type); Object.Destroy(go.GetComponent<Collider>()); go.transform.SetParent(parent, false);
            go.transform.localPosition = pos; go.transform.localScale = scale; if (rot.HasValue) go.transform.localRotation = Quaternion.Euler(rot.Value);
            go.GetComponent<Renderer>().sharedMaterial = CharacterBuilder.Toon(c); return go;
        }
        static readonly Color Wood = new Color(0.54f, 0.35f, 0.16f), Metal = new Color(0.23f, 0.23f, 0.28f), Tomato = new Color(0.9f, 0.22f, 0.27f), Khaki = new Color(0.36f, 0.42f, 0.25f);

        public static GameObject Slingshot(Transform parent)
        {
            var g = new GameObject("Lance-pierre"); g.transform.SetParent(parent, false); g.transform.localPosition = new Vector3(0.55f, 0.55f, -0.3f);
            P(g.transform, PrimitiveType.Cylinder, Wood, new Vector3(0, 0.45f, 0), new Vector3(0.14f, 0.3f, 0.14f));
            P(g.transform, PrimitiveType.Cylinder, Wood, new Vector3(-0.22f, 0.95f, 0), new Vector3(0.12f, 0.28f, 0.12f), new Vector3(0, 0, 30f));
            P(g.transform, PrimitiveType.Cylinder, Wood, new Vector3(0.22f, 0.95f, 0), new Vector3(0.12f, 0.28f, 0.12f), new Vector3(0, 0, -30f));
            P(g.transform, PrimitiveType.Sphere, Tomato, new Vector3(0, 0.95f, 0), Vector3.one * 0.28f);
            g.SetActive(false); return g;
        }

        public static GameObject Cannon(Transform parent)
        {
            var g = new GameObject("Canon"); g.transform.SetParent(parent, false); g.transform.localPosition = new Vector3(0.95f, 0, 0);
            P(g.transform, PrimitiveType.Cylinder, Wood, new Vector3(0, 0.35f, 0), new Vector3(0.76f, 0.07f, 0.76f), new Vector3(0, 0, 90f));
            P(g.transform, PrimitiveType.Cylinder, Metal, new Vector3(0, 0.55f, -0.35f), new Vector3(0.48f, 0.65f, 0.48f), new Vector3(70f, 0, 0));
            P(g.transform, PrimitiveType.Sphere, Tomato, new Vector3(0, 0.95f, -0.95f), Vector3.one * 0.32f);
            g.SetActive(false); return g;
        }

        public static GameObject Tank(Transform parent)
        {
            var g = new GameObject("Tank"); g.transform.SetParent(parent, false);
            P(g.transform, PrimitiveType.Cube, Metal, new Vector3(0, 0.3f, 0), new Vector3(1.9f, 0.5f, 1.4f));
            P(g.transform, PrimitiveType.Cube, Khaki, new Vector3(0, 0.72f, 0), new Vector3(1.4f, 0.4f, 1.0f));
            P(g.transform, PrimitiveType.Cylinder, new Color(0.29f, 0.35f, 0.2f), new Vector3(0, 1.05f, 0), new Vector3(0.9f, 0.18f, 0.9f));
            P(g.transform, PrimitiveType.Cylinder, new Color(0.54f, 0.12f, 0.17f), new Vector3(0, 1.1f, -0.9f), new Vector3(0.2f, 0.7f, 0.2f), new Vector3(90f, 0, 0));
            P(g.transform, PrimitiveType.Sphere, Tomato, new Vector3(0, 1.1f, -1.65f), Vector3.one * 0.36f);
            for (int sx = -1; sx <= 1; sx += 2) for (int i = 0; i < 4; i++) P(g.transform, PrimitiveType.Cylinder, new Color(0.42f, 0.42f, 0.48f), new Vector3(sx * 0.95f, 0.22f, -0.55f + i * 0.37f), new Vector3(0.4f, 0.06f, 0.4f), new Vector3(0, 0, 90f));
            g.SetActive(false); return g;
        }
    }
}
