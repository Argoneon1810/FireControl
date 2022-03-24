using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(TreeMassPlacerOnTerrain))]
public class TreeMassPlacerOnTerrainEditor : Editor {
    AnimBool showFade;

    void OnEnable() {
        showFade = new AnimBool(false);
        showFade.valueChanged.AddListener(Repaint);
    }

    public override void OnInspectorGUI() {
        TreeMassPlacerOnTerrain script = (TreeMassPlacerOnTerrain) target;

        script.treeToSpawn = EditorGUILayout.ObjectField("Tree Prefab", script.treeToSpawn, typeof(GameObject), true) as GameObject;
        using (var check = new EditorGUI.ChangeCheckScope()) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_mTerrains"), true);
            if(check.changed)
                serializedObject.ApplyModifiedProperties();
        }

        showFade.target = EditorGUILayout.Foldout(showFade.target, "Easy Load Terrains Fields");
        if(EditorGUILayout.BeginFadeGroup(showFade.faded)) {
            script.terrains = EditorGUILayout.ObjectField("Easy Load Terrains", script.terrains, typeof(Transform), true) as Transform;
            script.numberOfTerrainsToPlantTrees = EditorGUILayout.IntField("Number of Terrains to load", script.numberOfTerrainsToPlantTrees);
        }
        EditorGUILayout.EndFadeGroup();

        script.numberOfTree = EditorGUILayout.IntField("Desired number of tree to be spawned (quantity is not guaranteed)", script.numberOfTree);
        script.maxTrialPerTree = EditorGUILayout.IntField("Max trial count for spawning individual tree", script.maxTrialPerTree);
        script.radius = EditorGUILayout.FloatField("Collision check radius to prevent trees to overlap", script.radius);
        script.masterSpreadTime = EditorGUILayout.FloatField("Spread time for fire", script.masterSpreadTime);

        script.triggerRegen = GUILayout.Button("Regenerate Trees");
    }
}