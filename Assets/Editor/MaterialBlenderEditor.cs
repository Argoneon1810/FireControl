using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(MaterialBlender))]
public class MaterialBlenderEditor : Editor {
    AnimBool showFade;

    void OnEnable() {
        showFade = new AnimBool(false);
        showFade.valueChanged.AddListener(Repaint);
    }

    public override void OnInspectorGUI() {
        MaterialBlender script = (MaterialBlender) target;

        script.transitionLength = EditorGUILayout.FloatField("Transition Length (in second)", script.transitionLength);
        script.mRenderer = EditorGUILayout.ObjectField("Renderer", script.mRenderer, typeof(Renderer), true) as Renderer;
        script.reverse = EditorGUILayout.Toggle("Reverse Transition Order", script.reverse);

        showFade.target = EditorGUILayout.Foldout(showFade.target, "Show Material Array Fields");

        if(EditorGUILayout.BeginFadeGroup(showFade.faded)) {
            ++EditorGUI.indentLevel;
            GUILayout.Space(10);
            GUILayout.Label("!!! Make sure it matches the original material order (check renderer material) !!!");
            GUILayout.Space(10);
            using (var check = new EditorGUI.ChangeCheckScope()) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_fromMaterials"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_toMaterials"), true);
                if(check.changed)
                    serializedObject.ApplyModifiedProperties();
            }
            --EditorGUI.indentLevel;
        }
        EditorGUILayout.EndFadeGroup();
    }
}