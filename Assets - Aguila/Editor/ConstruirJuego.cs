using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using AnimatorController = UnityEditor.Animations.AnimatorController;
using InternalEditorUtility = UnityEditorInternal.InternalEditorUtility;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Arma toda la escena del juego (águila, escenario con colliders, diamantes, cerezas, puntos).
// Se ejecuta sola una vez si no existe Assets/Scenes/Juego.unity, o desde el menú:
// Rana Ninja ▸ Construir juego.
public static class ConstruirJuego
{
    const string EscenaPath = "Assets/Scenes/Juego.unity";
    const string AnimDir = "Assets/Animations/";

    [InitializeOnLoadMethod]
    static void AutoEjecutar()
    {
        if (SessionState.GetBool("RanaNinja_AutoBuild", false)) return;
        SessionState.SetBool("RanaNinja_AutoBuild", true);
        if (File.Exists(EscenaPath)) return;
        EditorApplication.delayCall += () =>
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) Construir();
        };
    }

    [MenuItem("Rana Ninja/Construir juego")]
    public static void Construir()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        AsegurarTag("gema");
        AsegurarTag("cereza");

        // ── Animaciones ──
        var clipIdle = CrearClip("aguila_state", "Assets/Sprites/eagle-attack-state.png", 0.15f);
        var clipVuelo = CrearClip("aguila_left", "Assets/Sprites/eagle-attack-west.png", 0.08f);
        var clipGema = CrearClip("gema", "Assets/Sprites/gem.png", 0.12f);
        var clipCereza = CrearClip("cereza", "Assets/Sprites/cherry.png", 0.12f);

        var ctrlAguila = CrearControllerAguila(clipIdle, clipVuelo);
        var ctrlGema = CrearControllerSimple("gem_0", clipGema);
        var ctrlCereza = CrearControllerSimple("cherry_0", clipCereza);

        // ── Escena nueva ──
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) Object.DestroyImmediate(l.gameObject);

        var cam = Camera.main;
        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.42f, 0.68f, 0.33f);
        if (cam.GetComponent<UniversalAdditionalCameraData>() == null) cam.gameObject.AddComponent<UniversalAdditionalCameraData>();

        var blanco = SpriteBlanco();
        var arena = CargarSprite("Assets/Sprites/StaticTiles.png", "StaticTiles_34");
        var fisica = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Scripts/SinFriccion.physicsMaterial2D");

        // Marco del escenario (visible y sólido): interior de 24 x 13
        Muro("Muro Arriba", new Vector2(0, 7f), new Vector2(26f, 1f), blanco);
        Muro("Muro Abajo", new Vector2(0, -7f), new Vector2(26f, 1f), blanco);
        Muro("Muro Izquierda", new Vector2(-12.5f, 0), new Vector2(1f, 15f), blanco);
        Muro("Muro Derecha", new Vector2(12.5f, 0), new Vector2(1f, 15f), blanco);

        // Obstáculos de arena (sólidos)
        foreach (var p in new[] { new Vector2(-6f, 2.5f), new Vector2(5f, -2.5f), new Vector2(-2f, -4f), new Vector2(7f, 4f) })
            Obstaculo("Obstaculo", p, arena, blanco);

        // Águila
        var aguila = new GameObject("Aguila");
        aguila.transform.position = Vector3.zero;
        var sr = aguila.AddComponent<SpriteRenderer>();
        sr.sprite = CargarSprites("Assets/Sprites/eagle-attack-state.png")[0];
        sr.sortingOrder = 10;
        var rb = aguila.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var box = aguila.AddComponent<BoxCollider2D>();
        box.size = new Vector2(0.8f, 0.6f);
        box.sharedMaterial = fisica;
        var anim = aguila.AddComponent<Animator>();
        anim.runtimeAnimatorController = ctrlAguila;
        aguila.AddComponent<MovementController>();
        aguila.AddComponent<RecogerObjeto>();

        // Diamantes (suman) y cerezas (restan)
        var posGemas = new[] { new Vector2(-9.5f, 5f), new Vector2(-2f, 4f), new Vector2(2.5f, 5.2f), new Vector2(10f, -1f),
                               new Vector2(-10f, -3f), new Vector2(1f, -1f), new Vector2(9.5f, -5f), new Vector2(-6f, -0.5f) };
        var posCerezas = new[] { new Vector2(-4f, 0.5f), new Vector2(3f, 1f), new Vector2(8f, 0.5f), new Vector2(-8f, -5f), new Vector2(2f, -4.5f) };
        foreach (var p in posGemas) Recolectable("Gema", "gema", p, CargarSprites("Assets/Sprites/gem.png")[0], ctrlGema);
        foreach (var p in posCerezas) Recolectable("Cereza", "cereza", p, CargarSprites("Assets/Sprites/cherry.png")[0], ctrlCereza);

        new GameObject("GameManager").AddComponent<GameManager>();

        // ── Guardar y registrar en Build Settings ──
        EditorSceneManager.SaveScene(scene, EscenaPath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(EscenaPath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("[Rana Ninja] Juego construido en " + EscenaPath + ". Pulsa Play.");
    }

    // ── Escenario ──
    static void Muro(string nombre, Vector2 pos, Vector2 tam, Sprite blanco)
    {
        var go = new GameObject(nombre);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(tam.x, tam.y, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = blanco;
        sr.color = new Color(0.36f, 0.23f, 0.12f);
        go.AddComponent<BoxCollider2D>().size = Vector2.one;
    }

    static void Obstaculo(string nombre, Vector2 pos, Sprite sprite, Sprite blanco)
    {
        var go = new GameObject(nombre);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite != null ? sprite : blanco;
        if (sprite == null) go.transform.localScale = new Vector3(3, 3, 1);
        var col = go.AddComponent<BoxCollider2D>();
        col.size = sr.sprite.bounds.size;
        col.offset = sr.sprite.bounds.center;
    }

    static void Recolectable(string nombre, string tag, Vector2 pos, Sprite sprite, RuntimeAnimatorController ctrl)
    {
        var go = new GameObject(nombre);
        go.tag = tag;
        go.transform.position = pos;
        go.transform.localScale = new Vector3(1.3f, 1.3f, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 5;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = Mathf.Min(sprite.bounds.extents.x, sprite.bounds.extents.y);
        go.AddComponent<Animator>().runtimeAnimatorController = ctrl;
    }

    // ── Assets ──
    static Sprite SpriteBlanco()
    {
        const string path = "Assets/Sprites/Blanco.png";
        if (!File.Exists(path))
        {
            var tex = new Texture2D(4, 4);
            tex.SetPixels(Enumerable.Repeat(Color.white, 16).ToArray());
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Sprite;
            imp.spritePixelsToUnits = 4; // 1 píxel-bloque = 1 unidad
            imp.filterMode = FilterMode.Point;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Sprite[] CargarSprites(string path)
    {
        return AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>()
            .OrderBy(s => int.TryParse(s.name.Substring(s.name.LastIndexOf('_') + 1), out int n) ? n : 0).ToArray();
    }

    static Sprite CargarSprite(string path, string nombre) => CargarSprites(path).FirstOrDefault(s => s.name == nombre);

    static AnimationClip CrearClip(string nombre, string spritesPath, float segPorFrame)
    {
        string path = AnimDir + nombre + ".anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
        }
        var frames = CargarSprites(spritesPath);
        var keys = frames.Select((s, i) => new ObjectReferenceKeyframe { time = i * segPorFrame, value = s }).ToArray();
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        settings.stopTime = frames.Length * segPorFrame;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    static AnimatorController CrearControllerAguila(AnimationClip idle, AnimationClip vuelo)
    {
        string path = AnimDir + "eagle-attack-west_3.controller";
        AssetDatabase.DeleteAsset(path);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        ctrl.AddParameter("AnimationState", AnimatorControllerParameterType.Int);
        var sm = ctrl.layers[0].stateMachine;
        var sIdle = sm.AddState("aguila_state");
        sIdle.motion = idle;
        var sVuelo = sm.AddState("aguila_left");
        sVuelo.motion = vuelo;
        sm.defaultState = sIdle;
        Transicion(sIdle, sVuelo, 1);
        Transicion(sVuelo, sIdle, 0);
        return ctrl;
    }

    static void Transicion(AnimatorState desde, AnimatorState hasta, int valor)
    {
        var t = desde.AddTransition(hasta);
        t.hasExitTime = false;
        t.duration = 0f;
        t.AddCondition(AnimatorConditionMode.Equals, valor, "AnimationState");
    }

    static AnimatorController CrearControllerSimple(string nombre, AnimationClip clip)
    {
        string path = AnimDir + nombre + ".controller";
        AssetDatabase.DeleteAsset(path);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        ctrl.layers[0].stateMachine.AddState(clip.name).motion = clip;
        return ctrl;
    }

    static void AsegurarTag(string tag)
    {
        if (!InternalEditorUtility.tags.Contains(tag)) InternalEditorUtility.AddTag(tag);
    }
}
