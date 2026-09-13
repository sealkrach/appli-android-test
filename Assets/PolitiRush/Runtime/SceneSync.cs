using System.Collections.Generic;
using PolitiRush.Core;
using UnityEngine;

namespace PolitiRush.Runtime
{
    /// <summary>
    /// Fait correspondre la scène Unity à l'état du moteur, image par image.
    /// Reproduction de syncScene() de la maquette 3D : mêmes repères, même caméra.
    /// </summary>
    public sealed class SceneSync : MonoBehaviour
    {
        public GameController Controller;
        public Transform World;
        public const float RoadWidth = 5.2f, Depth = 26f;

        readonly Dictionary<int, EnemyView> enemies = new Dictionary<int, EnemyView>();
        readonly Dictionary<int, GateView> gates = new Dictionary<int, GateView>();
        readonly Dictionary<int, GameObject> bonuses = new Dictionary<int, GameObject>();
        readonly List<GameObject> tomatoPool = new List<GameObject>();
        readonly List<GameObject> coinPool = new List<GameObject>();
        readonly List<Corpse> corpses = new List<Corpse>();
        HeroView hero;
        ParticleSystem splash;
        Material tomatoMat, stemMat, coinMat, bonusMat;

        sealed class Corpse { public GameObject Go; public float T; public Vector3 V; public float Spin; }

        public static float WX(float x) => (x - 0.5f) * RoadWidth;
        public static float WZ(float y) => -(y - GameConfig.HeroY) * Depth;

        void Awake()
        {
            if (World == null) World = new GameObject("World").transform;
            tomatoMat = CharacterBuilder.Toon(new Color(0.9f, 0.22f, 0.27f));
            stemMat = CharacterBuilder.Toon(new Color(0.16f, 0.62f, 0.56f));
            coinMat = CharacterBuilder.Toon(new Color(1f, 0.82f, 0.4f));
            bonusMat = CharacterBuilder.Toon(new Color(0.61f, 0.36f, 0.9f));
            splash = BuildSplash();
        }

        void OnEnable() { if (Controller != null) { Controller.OnEvent += HandleEvent; } }
        void OnDisable() { if (Controller != null) { Controller.OnEvent -= HandleEvent; } }

        public void ResetScene()
        {
            foreach (var e in enemies.Values) Destroy(e.gameObject); enemies.Clear();
            foreach (var g in gates.Values) Destroy(g.gameObject); gates.Clear();
            foreach (var b in bonuses.Values) Destroy(b); bonuses.Clear();
            foreach (var c in corpses) Destroy(c.Go); corpses.Clear();
            if (hero != null) Destroy(hero.gameObject);
            hero = HeroView.Create(World, Controller.State.Hero);
        }

        void HandleEvent(GameEvent ev)
        {
            switch (ev)
            {
                case GameEvent.EnemyKilled k:
                    splash.transform.position = new Vector3(WX(k.X), 1f, WZ(k.Y));
                    splash.Emit(k.Type.IsBoss ? 60 : 24);
                    if (enemies.TryGetValue(k.EnemyId, out var view))
                    {
                        enemies.Remove(k.EnemyId); view.HideLabel();
                        corpses.Add(new Corpse { Go = view.gameObject, T = 0.9f, V = new Vector3(Random.Range(-2f, 2f), Random.Range(6f, 9f), Random.Range(-9f, -5f)), Spin = Random.Range(300f, 600f) });
                    }
                    CameraShake.Instance?.Shake(k.Type.IsBoss ? 0.5f : 0f);
                    break;
                case GameEvent.HeroHurt _: CameraShake.Instance?.Shake(0.4f); break;
                case GameEvent.GatePassed g: hero?.OnGatePassed(g); break;
            }
        }

        void LateUpdate()
        {
            var s = Controller?.State; if (s == null) return;
            float t = s.Elapsed, dt = Time.deltaTime;
            // Héros.
            if (hero == null) hero = HeroView.Create(World, s.Hero);
            hero.Sync(s, Controller.TargetX.HasValue, t);
            // Cibles.
            bool frozen = s.HasEffect(BonusType.Scandale) || (s.Hero.Special == Special.Gadget && s.SpecialActiveRemaining > 0f);
            var seen = new HashSet<int>();
            foreach (var e in s.Enemies)
            {
                seen.Add(e.Id);
                if (!enemies.TryGetValue(e.Id, out var v)) { v = EnemyView.Create(World, e); enemies[e.Id] = v; }
                v.Sync(e, t, frozen);
            }
            foreach (var id in new List<int>(enemies.Keys)) if (!seen.Contains(id)) { Destroy(enemies[id].gameObject); enemies.Remove(id); }
            // Portes.
            var seenG = new HashSet<int>();
            foreach (var g in s.Gates)
            {
                if (g.Consumed) continue; seenG.Add(g.Id);
                if (!gates.TryGetValue(g.Id, out var gv)) { gv = GateView.Create(World, g); gates[g.Id] = gv; }
                gv.transform.position = new Vector3(g.Side == 0 ? -RoadWidth / 4 : RoadWidth / 4, 0, WZ(g.Y));
            }
            foreach (var id in new List<int>(gates.Keys)) if (!seenG.Contains(id)) { Destroy(gates[id].gameObject); gates.Remove(id); }
            // Bonus.
            var seenB = new HashSet<int>();
            foreach (var b in s.Bonuses)
            {
                seenB.Add(b.Id);
                if (!bonuses.TryGetValue(b.Id, out var bo))
                {
                    bo = GameObject.CreatePrimitive(PrimitiveType.Cube); bo.name = "Bonus " + Controller.Level.BonusLabel(b.Type); Destroy(bo.GetComponent<Collider>());
                    bo.GetComponent<Renderer>().sharedMaterial = bonusMat; bo.transform.SetParent(World, false); bo.transform.localScale = new Vector3(0.6f, 0.8f, 0.6f);
                    bonuses[b.Id] = bo;
                }
                bo.transform.position = new Vector3(WX(b.X), 0.9f + Mathf.Sin(t * 5f) * 0.1f, WZ(b.Y)); bo.transform.rotation = Quaternion.Euler(45f, t * 170f, 45f);
            }
            foreach (var id in new List<int>(bonuses.Keys)) if (!seenB.Contains(id)) { Destroy(bonuses[id]); bonuses.Remove(id); }
            // Tomates (pool).
            SyncPool(tomatoPool, s.Projectiles.Count, () => MakeTomato(), (go, i) =>
            {
                var p = s.Projectiles[i];
                float arc = p.Arc ? Mathf.Sin(Mathf.Clamp01((p.Y - GameConfig.HeroY) / 0.45f) * Mathf.PI) * 1.3f : 0f;
                go.transform.position = new Vector3(WX(p.X), 0.9f + arc, WZ(p.Y));
                go.transform.localScale = Vector3.one * (p.Piercing ? 0.44f : 0.3f);
            });
            // Pièces (pool).
            SyncPool(coinPool, s.CoinsOnTrack.Count, () => MakeCoin(), (go, i) =>
            {
                var c = s.CoinsOnTrack[i];
                go.transform.position = new Vector3(WX(c.X), 0.5f + Mathf.Sin(t * 6f + c.Id) * 0.08f, WZ(c.Y)); go.transform.rotation = Quaternion.Euler(90f, t * 230f, 0);
            });
            // KO.
            for (int i = corpses.Count - 1; i >= 0; i--)
            {
                var c = corpses[i]; c.T -= dt; c.Go.transform.position += c.V * dt; c.V.y -= 16f * dt; c.Go.transform.Rotate(-c.Spin * dt, 0, c.Spin * 0.3f * dt);
                if (c.T <= 0f) { Destroy(c.Go); corpses.RemoveAt(i); }
            }
        }

        static void SyncPool(List<GameObject> pool, int count, System.Func<GameObject> make, System.Action<GameObject, int> place)
        {
            while (pool.Count < count) pool.Add(make());
            for (int i = 0; i < pool.Count; i++) { bool on = i < count; if (pool[i].activeSelf != on) pool[i].SetActive(on); if (on) place(pool[i], i); }
        }

        GameObject MakeTomato()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere); go.name = "Tomate"; Destroy(go.GetComponent<Collider>()); go.transform.SetParent(World, false);
            go.GetComponent<Renderer>().sharedMaterial = tomatoMat;
            var stem = GameObject.CreatePrimitive(PrimitiveType.Capsule); Destroy(stem.GetComponent<Collider>()); stem.transform.SetParent(go.transform, false);
            stem.transform.localPosition = new Vector3(0, 0.6f, 0); stem.transform.localScale = new Vector3(0.25f, 0.2f, 0.25f); stem.GetComponent<Renderer>().sharedMaterial = stemMat;
            return go;
        }

        GameObject MakeCoin()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); go.name = "Pièce"; Destroy(go.GetComponent<Collider>()); go.transform.SetParent(World, false);
            go.transform.localScale = new Vector3(0.44f, 0.06f, 0.44f); go.GetComponent<Renderer>().sharedMaterial = coinMat; return go;
        }

        ParticleSystem BuildSplash()
        {
            var go = new GameObject("Splash"); go.transform.SetParent(World, false);
            var ps = go.AddComponent<ParticleSystem>(); var main = ps.main; main.playOnAwake = false; main.loop = false; main.startLifetime = 0.9f; main.startSpeed = 5f; main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startColor = new Color(0.9f, 0.22f, 0.27f); main.gravityModifier = 1.4f; main.maxParticles = 500;
            var em = ps.emission; em.rateOverTime = 0f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.3f;
            var r = go.GetComponent<ParticleSystemRenderer>(); r.material = tomatoMat; r.renderMode = ParticleSystemRenderMode.Mesh; r.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            return ps;
        }
    }
}
