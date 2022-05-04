using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HelicopterExtinguisher))]
public class HelicopterExtinguisherEditor : Editor {
    bool updateOnPlaymode = false;

    public override void OnInspectorGUI() {
        HelicopterExtinguisher script = (HelicopterExtinguisher) target;

        updateOnPlaymode = EditorGUILayout.Toggle("Update Extinguisher Size In Playmode", updateOnPlaymode);

        EditorGUI.BeginChangeCheck();
        script.extinguisherRadius = EditorGUILayout.FloatField("Extinguisher Radius", script.extinguisherRadius);
        if(EditorGUI.EndChangeCheck())
            if(updateOnPlaymode)
                script.SendMessage("ChangeRadius");

        script.raydistance = EditorGUILayout.FloatField("Ray Maximum Distance for Ground Check", script.raydistance);
        // script._extinguisherMaterial = EditorGUILayout.ObjectField("Material for Extinguisher Sphere", script._extinguisherMaterial, typeof(Material), true) as Material;
        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Water Consumption Amount per Dispatch");
            script.waterConsumptionPerSpray = EditorGUILayout.IntSlider(script.waterConsumptionPerSpray, 0, 100);
        EditorGUILayout.EndHorizontal();
    }
}