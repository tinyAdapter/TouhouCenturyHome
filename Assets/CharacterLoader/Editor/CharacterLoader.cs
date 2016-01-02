#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;

public class CharacterLoader : EditorWindow {

    string[] clipNames =
    {
        "Bot",
        "Left",
        "Right",
        "Up",
        "BotLeft",
        "BotRight",
        "UpLeft",
        "UpRight"
    };

    Sprite spritesheet;
    Sprite[] sprites;
    
    int framesPerLoop = 4;
    int totalLoops = 8;

    string characterName;
    string directory;

    [MenuItem("Window/Character Loader")]
    static void OpenWindow()
    {
        GetWindow<CharacterLoader>();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Spritesheet");
        spritesheet = EditorGUILayout.ObjectField(spritesheet, typeof(Sprite), false) as Sprite;
        EditorGUILayout.LabelField("Frames per Walk Loop");
        framesPerLoop = EditorGUILayout.IntField(framesPerLoop);
        EditorGUILayout.LabelField("Total Number of Walk Loops");
        totalLoops = EditorGUILayout.IntField(totalLoops);

        if (GUILayout.Button("Load Spritesheet"))
        {
            string sheetPath = AssetDatabase.GetAssetPath(spritesheet);

            characterName = Path.GetFileNameWithoutExtension(sheetPath);
            directory = Path.GetDirectoryName(sheetPath);

            // using linq
            // Since this is editor-only, we don't worry about extra memory allocated by
            // by Linq functions. It's safe, simple, and clear.
            sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToArray();

            // without linq
            // Notice that the first asset AssetDatabase loaded is a Texture2D,
            // instead of a Sprite, even if you correctly set it as a Sprite in editor. 
            // Object[] objs = AssetDatabase.LoadAllAssetsAtPath(path);
            // 
            // sprites = new Sprite[objs.Length - 1];
            // for (int i = 0; i < sprites.Length; i++)
            // {
            //     sprites[i] = objs[i+1] as Sprite;
            // }
            for (int i = 0; i < totalLoops; i++)
            {
                AnimationClip clip = new AnimationClip();
                clip.frameRate = framesPerLoop;
                
                EditorCurveBinding curveBinding = new EditorCurveBinding();
                curveBinding.path = "";
                curveBinding.propertyName = "m_Sprite";
                curveBinding.type = typeof(SpriteRenderer);

                ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[framesPerLoop];
                for (int j = 0; j < framesPerLoop; j++)
                {
                    keys[j] = new ObjectReferenceKeyframe();
                    keys[j].time = (1.0f / framesPerLoop) * j;
                    keys[j].value = sprites[framesPerLoop * i + j];
                }

                AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keys);
                AssetDatabase.CreateAsset(clip, directory + "/" + characterName + "_" + clipNames[i] + ".anim");
            }

            string[] folders = new string[1];
            folders[0] = directory;
            string[] clipPaths = AssetDatabase.FindAssets(characterName, folders);

            List<AnimationClip> clips = new List<AnimationClip>();
            for (int i = 0; i < clipPaths.Length; i++)
            {
                AnimationClip c = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(clipPaths[i]), typeof(AnimationClip)) as AnimationClip;
                if (c != null)
                    clips.Add(c);
            }

            for (int i = 0; i < clips.Count; i++)
            {
                SerializedObject serializedClip = new SerializedObject(clips[i]);
                AnimationClipSettings clipSettings = new AnimationClipSettings(serializedClip.FindProperty("m_AnimationClipSettings"));

                clipSettings.loopTime = true;

                serializedClip.ApplyModifiedProperties();
            }

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(directory + "/" + characterName + "_AnimatorController.controller");
            controller.AddParameter("WalkEnum", AnimatorControllerParameterType.Float);
            
            BlendTree tree;
            controller.CreateBlendTreeInController("Walk", out tree, 0);
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = "WalkEnum";
            tree.useAutomaticThresholds = false;
            for (int i = 0; i < clips.Count; i++)
            {
                tree.AddChild(clips[i], i * (1.0f / totalLoops));
            }

            GameObject go = GameObject.Find(characterName);
            if (go == null)
            {
                go = new GameObject(characterName);
                go.AddComponent<SpriteRenderer>().sprite = sprites[0];
                go.AddComponent<Animator>().runtimeAnimatorController = controller;
            }
            else
            {
                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                if (sr == null)
                    go.AddComponent<SpriteRenderer>().sprite = sprites[0];
                else
                    sr.sprite = sprites[0];
                
                Animator anim = go.GetComponent<Animator>();
                if (anim == null)
                    go.AddComponent<Animator>().runtimeAnimatorController = controller;
                else
                    anim.runtimeAnimatorController = controller;
            }

            PrefabUtility.CreatePrefab(directory + "/" + characterName + ".prefab", go);
        }

    }
}

class AnimationClipSettings
{
    SerializedProperty m_Property;

    private SerializedProperty Get(string property) { return m_Property.FindPropertyRelative(property); }

    public AnimationClipSettings(SerializedProperty prop) { m_Property = prop; }

    public float startTime { get { return Get("m_StartTime").floatValue; } set { Get("m_StartTime").floatValue = value; } }
    public float stopTime { get { return Get("m_StopTime").floatValue; } set { Get("m_StopTime").floatValue = value; } }
    public float orientationOffsetY { get { return Get("m_OrientationOffsetY").floatValue; } set { Get("m_OrientationOffsetY").floatValue = value; } }
    public float level { get { return Get("m_Level").floatValue; } set { Get("m_Level").floatValue = value; } }
    public float cycleOffset { get { return Get("m_CycleOffset").floatValue; } set { Get("m_CycleOffset").floatValue = value; } }

    public bool loopTime { get { return Get("m_LoopTime").boolValue; } set { Get("m_LoopTime").boolValue = value; } }
    public bool loopBlend { get { return Get("m_LoopBlend").boolValue; } set { Get("m_LoopBlend").boolValue = value; } }
    public bool loopBlendOrientation { get { return Get("m_LoopBlendOrientation").boolValue; } set { Get("m_LoopBlendOrientation").boolValue = value; } }
    public bool loopBlendPositionY { get { return Get("m_LoopBlendPositionY").boolValue; } set { Get("m_LoopBlendPositionY").boolValue = value; } }
    public bool loopBlendPositionXZ { get { return Get("m_LoopBlendPositionXZ").boolValue; } set { Get("m_LoopBlendPositionXZ").boolValue = value; } }
    public bool keepOriginalOrientation { get { return Get("m_KeepOriginalOrientation").boolValue; } set { Get("m_KeepOriginalOrientation").boolValue = value; } }
    public bool keepOriginalPositionY { get { return Get("m_KeepOriginalPositionY").boolValue; } set { Get("m_KeepOriginalPositionY").boolValue = value; } }
    public bool keepOriginalPositionXZ { get { return Get("m_KeepOriginalPositionXZ").boolValue; } set { Get("m_KeepOriginalPositionXZ").boolValue = value; } }
    public bool heightFromFeet { get { return Get("m_HeightFromFeet").boolValue; } set { Get("m_HeightFromFeet").boolValue = value; } }
    public bool mirror { get { return Get("m_Mirror").boolValue; } set { Get("m_Mirror").boolValue = value; } }
}

#endif