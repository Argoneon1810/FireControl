using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(TreeMassPlacerOnPCG))]
public class TreeMassPlacerOnPCGEditor : Editor {
    AnimBool showFade;

    void OnEnable() {
        showFade = new AnimBool(false);
        showFade.valueChanged.AddListener(Repaint);
    }

    public override void OnInspectorGUI() {
        TreeMassPlacerOnPCG script = (TreeMassPlacerOnPCG) target;

        script.treeToSpawn = EditorGUILayout.ObjectField("Tree Prefab", script.treeToSpawn, typeof(GameObject), false) as GameObject;

        script.generator = EditorGUILayout.ObjectField("PCG Landmass Generator", script.generator, typeof(PCGCubeMapGenerator), true) as PCGCubeMapGenerator;

        script.numberOfTree = EditorGUILayout.IntField("Desired number of tree to be spawned (quantity is not guaranteed)", script.numberOfTree);
        script.maxTrialPerTree = EditorGUILayout.IntField("Max trial count for spawning individual tree", script.maxTrialPerTree);
        script.radius = EditorGUILayout.FloatField("Collision check radius to prevent trees to overlap", script.radius);
        script.masterSpreadTime = EditorGUILayout.FloatField("Spread time for fire", script.masterSpreadTime);

        script.triggerRegen = GUILayout.Button("Regenerate Trees");
    }
}