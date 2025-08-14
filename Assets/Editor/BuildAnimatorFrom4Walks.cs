#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Aliases para evitar ambiguidade
using AnimController = UnityEditor.Animations.AnimatorController;
using AnimBlendTree  = UnityEditor.Animations.BlendTree;
using ACPType        = UnityEngine.AnimatorControllerParameterType;

public static class BuildAnimatorFrom4Walks
{
    // caminhos de saída (ajuste se quiser)
    const string AnimFolder  = "Assets/Animations/Player";
    const string CtrlPath    = AnimFolder + "/Player_Default.controller";
    const string ClassFolder = "Assets/ScriptableObjects/Classes";
    const string ClassPath   = ClassFolder + "/Class_Default.asset";

    [MenuItem("Tools/Player/Build Animator (4 Walk Clips)")]
    public static void Build()
    {
        EnsureFolder("Assets/Animations");
        EnsureFolder(AnimFolder);
        EnsureFolder("Assets/ScriptableObjects");
        EnsureFolder(ClassFolder);

        // 1) localizar os 4 walk clips por nome
        var walkR = FindClip("Walk_Right");
        var walkL = FindClip("Walk_Left");
        var walkU = FindClip("Walk_Up");
        var walkD = FindClip("Walk_Down");

        if (!walkR || !walkL || !walkU || !walkD)
        {
            EditorUtility.DisplayDialog("Faltam clips",
                "Certifique-se de que existem Animation Clips com os nomes exatos:\n" +
                "Walk_Right, Walk_Left, Walk_Up, Walk_Down.",
                "OK");
            return;
        }

        // 2) gerar 4 idles (um frame) a partir do 1º frame de cada walk
        var idleR = CreateIdleFromFirstFrame(walkR, AnimFolder + "/Idle_Right.anim");
        var idleL = CreateIdleFromFirstFrame(walkL, AnimFolder + "/Idle_Left.anim");
        var idleU = CreateIdleFromFirstFrame(walkU, AnimFolder + "/Idle_Up.anim");
        var idleD = CreateIdleFromFirstFrame(walkD, AnimFolder + "/Idle_Down.anim");

        // 3) criar/pegar AnimatorController (Editor.Animations)
        var controller = AssetDatabase.LoadAssetAtPath<AnimController>(CtrlPath);
        if (!controller)
            controller = AnimController.CreateAnimatorControllerAtPath(CtrlPath);

        EnsureParam(controller, "Speed", ACPType.Float);
        EnsureParam(controller, "MoveX", ACPType.Float);
        EnsureParam(controller, "MoveY", ACPType.Float);

        var layer = controller.layers[0];
        var sm = layer.stateMachine;

        // 4) criar BlendTree Idle (2D direcional)
        var idleTree = new AnimBlendTree
        {
            name = "IdleTree",
            blendType = BlendTreeType.FreeformDirectional2D,
            useAutomaticThresholds = false,
            blendParameter = "MoveX",
            blendParameterY = "MoveY"
        };
        AssetDatabase.AddObjectToAsset(idleTree, controller);
        idleTree.AddChild(idleR, new Vector2( 1f,  0f));
        idleTree.AddChild(idleL, new Vector2(-1f,  0f));
        idleTree.AddChild(idleU, new Vector2( 0f,  1f));
        idleTree.AddChild(idleD, new Vector2( 0f, -1f));

        // 5) criar BlendTree Walk (2D direcional)
        var walkTree = new AnimBlendTree
        {
            name = "WalkTree",
            blendType = BlendTreeType.FreeformDirectional2D,
            useAutomaticThresholds = false,
            blendParameter = "MoveX",
            blendParameterY = "MoveY"
        };
        AssetDatabase.AddObjectToAsset(walkTree, controller);
        walkTree.AddChild(walkR, new Vector2( 1f,  0f));
        walkTree.AddChild(walkL, new Vector2(-1f,  0f));
        walkTree.AddChild(walkU, new Vector2( 0f,  1f));
        walkTree.AddChild(walkD, new Vector2( 0f, -1f));

        // 6) criar estados + transições
        var idleState = sm.states.FirstOrDefault(s => s.state.name == "Idle").state;
        if (idleState == null) idleState = sm.AddState("Idle", new Vector3(200, 100, 0));
        idleState.motion = idleTree;

        var walkState = sm.states.FirstOrDefault(s => s.state.name == "Walk").state;
        if (walkState == null) walkState = sm.AddState("Walk", new Vector3(500, 100, 0));
        walkState.motion = walkTree;

        sm.defaultState = idleState;

        // limpa transições antigas
        foreach (var t in idleState.transitions.ToArray()) idleState.RemoveTransition(t);
        foreach (var t in walkState.transitions.ToArray()) walkState.RemoveTransition(t);


        var toWalk = idleState.AddTransition(walkState);
        toWalk.hasExitTime = false;
        toWalk.duration = 0.05f;
        toWalk.AddCondition(AnimatorConditionMode.Greater, 0.05f, "Speed");

        var toIdle = walkState.AddTransition(idleState);
        toIdle.hasExitTime = false;
        toIdle.duration = 0.05f;
        toIdle.AddCondition(AnimatorConditionMode.Less, 0.05f, "Speed");

        EditorUtility.SetDirty(controller);

        // 7) criar/atualizar ScriptableObject da classe
        var playerClass = AssetDatabase.LoadAssetAtPath<PlayerClass>(ClassPath);
        if (!playerClass)
        {
            playerClass = ScriptableObject.CreateInstance<PlayerClass>();
            playerClass.classId = "default";
            AssetDatabase.CreateAsset(playerClass, ClassPath);
        }
        playerClass.animator = controller;
        playerClass.defaultIdleSprite = TryExtractFirstSprite(idleD) ?? TryExtractFirstSprite(walkD);
        EditorUtility.SetDirty(playerClass);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 8) se tiver um Animator selecionado (ex.: seu "Visual"), atribui controller
        var sel = Selection.activeGameObject;
        if (sel)
        {
            var anim = sel.GetComponent<Animator>();
            if (anim)
            {
                anim.runtimeAnimatorController = controller;
                EditorUtility.SetDirty(anim);
            }
        }

        EditorUtility.DisplayDialog("Pronto!",
            "Animator (Idle/Walk com Blend Tree 2D) e Class_Default gerados/atualizados.\n" +
            "Se um objeto com Animator estava selecionado, o controller foi atribuído.",
            "OK");
    }

    // ------------------- helpers -------------------
    static void EnsureFolder(string path)
    {
        var parts = path.Split('/');
        string cur = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }

    static void EnsureParam(AnimController ctrl, string name, ACPType type)
    {
        if (!ctrl.parameters.Any(p => p.name == name))
            ctrl.AddParameter(name, type);
    }

    static AnimationClip FindClip(string name)
    {
        var guids = AssetDatabase.FindAssets($"{name} t:AnimationClip");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip && clip.name == name) return clip;
        }
        return null;
    }

    static AnimationClip CreateIdleFromFirstFrame(AnimationClip sourceWalk, string savePath)
    {
        var sprite = TryExtractFirstSprite(sourceWalk);
        if (!sprite)
        {
            Debug.LogWarning($"Não foi possível extrair 1º frame de {sourceWalk.name}.");
            return null;
        }

        var idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath);
        if (!idle)
        {
            idle = new AnimationClip { frameRate = 12f, name = System.IO.Path.GetFileNameWithoutExtension(savePath) };
            AssetDatabase.CreateAsset(idle, savePath);
        }

        var curveBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };
        var key = new ObjectReferenceKeyframe { time = 0f, value = sprite };
        AnimationUtility.SetObjectReferenceCurve(idle, curveBinding, new[] { key });

        EditorUtility.SetDirty(idle);
        return idle;
    }

    static Sprite TryExtractFirstSprite(AnimationClip clip)
    {
        if (!clip) return null;
        var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
        foreach (var b in bindings)
        {
            if (b.type == typeof(SpriteRenderer) && b.propertyName == "m_Sprite")
            {
                var keys = AnimationUtility.GetObjectReferenceCurve(clip, b);
                if (keys != null && keys.Length > 0)
                    return keys[0].value as Sprite;
            }
        }
        return null;
    }
}
#endif
