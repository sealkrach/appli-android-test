using UnityEngine;

namespace PolitiRush.Runtime
{
    /// <summary>
    /// Assemble un personnage cartoon en primitives (grosse tête, buste court, membres
    /// articulés), reproduction de buildCharacter() de la maquette 3D. À remplacer
    /// par les modèles du graphiste : il suffit d'un prefab avec les mêmes noms
    /// d'enfants (ArmL, ArmR, LegL, LegR, Head) pour que CharacterAnimator fonctionne.
    /// </summary>
    public static class CharacterBuilder
    {
        static Material toonMat;
        public static Material Toon(Color c)
        {
            if (toonMat == null)
            {
                var shader = Shader.Find("PolitiRush/Toon");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/Simple Lit");
                if (shader == null) shader = Shader.Find("Standard");
                toonMat = new Material(shader);
            }
            var m = new Material(toonMat); m.color = c;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            return m;
        }

        static GameObject Part(Transform parent, string name, PrimitiveType type, Color color, Vector3 pos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = pos; go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = Toon(color);
            return go;
        }

        public static GameObject Build(Look look, bool angry, float size)
        {
            var root = new GameObject("Character"); var t = root.transform;
            Color suit = look.Suit;
            Part(t, "Body", PrimitiveType.Capsule, suit, new Vector3(0, 1.15f, 0), new Vector3(1.2f, 0.85f, 1.0f));
            if (look.Shirt.HasValue) Part(t, "Shirt", PrimitiveType.Cube, look.Shirt.Value, new Vector3(0, 1.35f, 0.42f), new Vector3(0.36f, 0.8f, 0.12f));
            if (look.Tie.HasValue) Part(t, "Tie", PrimitiveType.Cube, look.Tie.Value, new Vector3(0, 1.3f, 0.5f), new Vector3(0.15f, 0.62f, 0.06f));
            if (look.Tee) Part(t, "Tee", PrimitiveType.Cube, new Color(0.13f, 0.13f, 0.13f), new Vector3(0, 1.55f, 0.44f), new Vector3(0.6f, 0.4f, 0.1f));
            if (look.Scarf)
            {
                Color[] cols = { new Color(0, 0.2f, 0.63f), Color.white, new Color(0.9f, 0.22f, 0.27f) };
                for (int i = 0; i < 3; i++) { var s = Part(t, "Scarf" + i, PrimitiveType.Cube, cols[i], new Vector3((i - 1) * 0.2f - 0.1f, 1.25f, 0.5f), new Vector3(0.2f, 1.2f, 0.06f)); s.transform.localRotation = Quaternion.Euler(0, 0, 32f); }
            }
            if (look.Cape.HasValue) Part(t, "Cape", PrimitiveType.Cube, look.Cape.Value, new Vector3(0, 1.0f, -0.45f), new Vector3(1.3f, 1.8f, 0.08f));

            var head = Part(t, "Head", PrimitiveType.Sphere, look.Skin, new Vector3(0, 2.55f, 0), Vector3.one * 1.56f);
            var h = head.transform;
            Part(h, "EarL", PrimitiveType.Sphere, look.Skin, new Vector3(-0.95f, 0, 0), new Vector3(0.25f, 0.25f, 0.15f));
            Part(h, "EarR", PrimitiveType.Sphere, look.Skin, new Vector3(0.95f, 0, 0), new Vector3(0.25f, 0.25f, 0.15f));
            Part(h, "Nose", PrimitiveType.Sphere, look.Skin, new Vector3(0, -0.12f, 0.95f), new Vector3(0.2f, 0.18f, 0.18f));
            foreach (float sx in new[] { -0.36f, 0.36f })
            {
                var eye = Part(h, "Eye", PrimitiveType.Sphere, Color.white, new Vector3(sx, 0.1f, 0.78f), new Vector3(0.28f, 0.3f, 0.2f));
                Part(eye.transform, "Pupil", PrimitiveType.Sphere, new Color(0.09f, 0.07f, 0.17f), new Vector3(0, -0.05f, 0.75f), new Vector3(0.45f, 0.42f, 0.4f));
                var brow = Part(h, "Brow", PrimitiveType.Cube, look.Hair ?? Color.black, new Vector3(sx, 0.45f, 0.8f), new Vector3(0.4f, 0.09f, 0.08f));
                brow.transform.localRotation = Quaternion.Euler(0, 0, (sx < 0 ? -1 : 1) * (angry ? 26f : 7f));
            }
            Part(h, "Mouth", PrimitiveType.Sphere, new Color(0.48f, 0.16f, 0.23f), new Vector3(0, -0.45f, 0.78f), new Vector3(0.4f, angry ? 0.22f : 0.12f, 0.15f));
            if (look.Mustache) Part(h, "Mustache", PrimitiveType.Cube, new Color(0.1f, 0.1f, 0.1f), new Vector3(0, -0.3f, 0.86f), new Vector3(0.6f, 0.12f, 0.1f));
            if (look.Glasses) foreach (float sx in new[] { -0.36f, 0.36f }) { var g = Part(h, "Glass", PrimitiveType.Cylinder, new Color(0.09f, 0.07f, 0.17f), new Vector3(sx, 0.1f, 0.9f), new Vector3(0.5f, 0.02f, 0.5f)); g.transform.localRotation = Quaternion.Euler(90, 0, 0); }

            if (look.Hair.HasValue && look.Style != "bald")
            {
                var H = look.Hair.Value; string st = look.Style;
                if (st == "quiff") { var q = Part(h, "Hair", PrimitiveType.Sphere, H, new Vector3(0.05f, 0.75f, 0.25f), new Vector3(2.1f, 1.0f, 1.9f)); q.transform.localRotation = Quaternion.Euler(-14f, 0, 0); }
                else if (st == "thin") Part(h, "Hair", PrimitiveType.Sphere, H, new Vector3(0, 0.55f, -0.15f), new Vector3(1.8f, 0.7f, 1.7f));
                else if (st == "cowl") { Part(h, "Hair", PrimitiveType.Sphere, H, new Vector3(0, 0.15f, -0.05f), new Vector3(2.04f, 2.04f, 2.0f)); Part(h, "EarCowlL", PrimitiveType.Capsule, H, new Vector3(-0.5f, 1.15f, 0), new Vector3(0.3f, 0.35f, 0.3f)); Part(h, "EarCowlR", PrimitiveType.Capsule, H, new Vector3(0.5f, 1.15f, 0), new Vector3(0.3f, 0.35f, 0.3f)); }
                else
                {
                    Part(h, "Hair", PrimitiveType.Sphere, H, new Vector3(0, 0.35f, -0.1f), new Vector3(2.04f, 1.5f, 2.0f));
                    if (st == "long" || st == "bob") { float len = st == "long" ? 1.6f : 1.0f; Part(h, "HairL", PrimitiveType.Cube, H, new Vector3(-0.85f, -len / 2 + 0.4f, -0.15f), new Vector3(0.35f, len, 0.7f)); Part(h, "HairR", PrimitiveType.Cube, H, new Vector3(0.85f, -len / 2 + 0.4f, -0.15f), new Vector3(0.35f, len, 0.7f)); }
                }
            }
            if (look.Band.HasValue) { Part(h, "Band", PrimitiveType.Cylinder, look.Band.Value, new Vector3(0, 0.3f, 0), new Vector3(2.08f, 0.11f, 2.08f)); var tail = Part(h, "BandTail", PrimitiveType.Cube, look.Band.Value, new Vector3(-0.9f, 0.2f, -0.6f), new Vector3(0.6f, 0.16f, 0.14f)); tail.transform.localRotation = Quaternion.Euler(0, 34f, 17f); }
            if (look.Cap) { Part(h, "Cap", PrimitiveType.Cylinder, new Color(0.1f, 0.16f, 0.29f), new Vector3(0, 0.95f, 0), new Vector3(1.6f, 0.22f, 1.6f)); Part(h, "CapBrim", PrimitiveType.Cube, new Color(0.1f, 0.16f, 0.29f), new Vector3(0, 0.72f, 0.5f), new Vector3(1.5f, 0.1f, 0.9f)); }
            if (look.RedCap) { Part(h, "RedCap", PrimitiveType.Sphere, new Color(0.9f, 0.22f, 0.27f), new Vector3(0, 0.5f, 0), new Vector3(2.0f, 1.1f, 2.0f)); Part(h, "RedCapBrim", PrimitiveType.Cube, new Color(0.9f, 0.22f, 0.27f), new Vector3(0, 0.35f, 0.9f), new Vector3(1.3f, 0.1f, 0.9f)); }

            foreach (int sx in new[] { -1, 1 })
            {
                var arm = new GameObject(sx < 0 ? "ArmL" : "ArmR"); arm.transform.SetParent(t, false); arm.transform.localPosition = new Vector3(sx * 0.72f, 1.7f, 0);
                Part(arm.transform, "Upper", PrimitiveType.Capsule, suit, new Vector3(0, -0.45f, 0), new Vector3(0.34f, 0.48f, 0.34f));
                Part(arm.transform, "Hand", PrimitiveType.Sphere, look.Gloves ?? look.Skin, new Vector3(0, -0.95f, 0), Vector3.one * (look.Gloves.HasValue ? 0.6f : 0.42f));
                var leg = new GameObject(sx < 0 ? "LegL" : "LegR"); leg.transform.SetParent(t, false); leg.transform.localPosition = new Vector3(sx * 0.3f, 0.72f, 0);
                Part(leg.transform, "Thigh", PrimitiveType.Capsule, new Color(0.17f, 0.15f, 0.25f), new Vector3(0, -0.35f, 0), new Vector3(0.4f, 0.38f, 0.4f));
                Part(leg.transform, "Shoe", PrimitiveType.Cube, new Color(0.1f, 0.1f, 0.1f), new Vector3(0, -0.75f, 0.1f), new Vector3(0.34f, 0.18f, 0.5f));
            }
            root.transform.localScale = Vector3.one * size;
            root.AddComponent<CharacterAnimator>().Size = size;
            return root;
        }
    }

    /// <summary>Cycle de course, squash and stretch, inclinaison et flash de coup.</summary>
    public sealed class CharacterAnimator : MonoBehaviour
    {
        public float Size = 1f;
        Transform armL, armR, legL, legR;
        Renderer[] renderers; Material[] originals; Material white;

        void Awake()
        {
            armL = transform.Find("ArmL"); armR = transform.Find("ArmR"); legL = transform.Find("LegL"); legR = transform.Find("LegR");
            renderers = GetComponentsInChildren<Renderer>();
            originals = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) originals[i] = renderers[i].sharedMaterial;
            white = CharacterBuilder.Toon(Color.white);
        }

        public void Animate(float phase, float squash = -1f, float tilt = 0f)
        {
            float s = Mathf.Sin(phase);
            if (legL) legL.localRotation = Quaternion.Euler(s * 52f, 0, 0);
            if (legR) legR.localRotation = Quaternion.Euler(-s * 52f, 0, 0);
            if (armL) armL.localRotation = Quaternion.Euler(-s * 46f, 0, 0);
            if (armR) armR.localRotation = Quaternion.Euler(s * 46f, 0, 0);
            float sq = squash > 0 ? squash : 1f + Mathf.Cos(phase * 2f) * 0.04f;
            transform.localScale = new Vector3(Size / sq, Size * sq, Size / sq);
            var p = transform.localPosition; p.y = Mathf.Abs(Mathf.Cos(phase)) * 0.12f * Size; transform.localPosition = p;
            transform.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, tilt * Mathf.Rad2Deg);
        }

        public void SetFlash(bool on)
        {
            for (int i = 0; i < renderers.Length; i++) renderers[i].sharedMaterial = on ? white : originals[i];
        }
    }
}
