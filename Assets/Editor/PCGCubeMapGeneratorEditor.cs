using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(PCGCubeMapGenerator))]
public class PCGCubeMapGeneratorEditor : Editor {
    PCGCubeMapGenerator script;
    AnimBool showFadeNoisemap, showFadeMesh, showFadeTexture, showFadeAnim;
    bool changed = false;

    void OnEnable() {
        showFadeNoisemap = new AnimBool(false);
        showFadeNoisemap.valueChanged.AddListener(Repaint);

        showFadeMesh = new AnimBool(false);
        showFadeMesh.valueChanged.AddListener(Repaint);

        showFadeTexture = new AnimBool(false);
        showFadeTexture.valueChanged.AddListener(Repaint);

        showFadeAnim = new AnimBool(false);
        showFadeAnim.valueChanged.AddListener(Repaint);
    }

    public override void OnInspectorGUI() {
        if(script == null) script = (PCGCubeMapGenerator) target;

        script.autoUpdate        = EditorGUILayout.ToggleLeft("Auto Update", script.autoUpdate);

        GUILayout.Space(5);

        #region Noise Map Generation Fade Group
        showFadeNoisemap.target  = EditorGUILayout.Foldout(showFadeNoisemap.target, "Noise Map Generation Fields");
        if(EditorGUILayout.BeginFadeGroup(showFadeNoisemap.faded)) {
            EditorGUI.BeginChangeCheck();
        
            ++EditorGUI.indentLevel;

            script.seed          = EditorGUILayout.IntField("Noisemap Seed", script.seed);
            script.offset        = EditorGUILayout.Vector2Field("Noisemap Offset", script.offset);

            GUILayout.Space(10);

            script.mapChunkSize  = EditorGUILayout.IntField("Noisemap Resolution", script.mapChunkSize);
            script.matchTransformScaleToNoiseTexture
                                 = EditorGUILayout.Toggle("Match Transform Scale to NoiseTexture", script.matchTransformScaleToNoiseTexture);

            GUILayout.Space(10);

            script.noiseScale    = EditorGUILayout.FloatField("Scale of Noisemap", script.noiseScale);
            script.octaves       = EditorGUILayout.IntField("Octave of Noisemap", script.octaves);
            script.persistance   = EditorGUILayout.FloatField("Persistance of Noisemap", script.persistance);
            script.lacunarity    = EditorGUILayout.FloatField("Lacunarity of Noisemap", script.lacunarity);

            --EditorGUI.indentLevel;

            if (EditorGUI.EndChangeCheck())
                changed = true;
        }
        EditorGUILayout.EndFadeGroup();
        #endregion

        GUILayout.Space(5);

        #region Mesh Generation Fade Group
        showFadeMesh.target      = EditorGUILayout.Foldout(showFadeMesh.target, "Mesh Generation Fields");
        if(EditorGUILayout.BeginFadeGroup(showFadeMesh.faded)) {
            EditorGUI.BeginChangeCheck();

            ++EditorGUI.indentLevel;

            script.useFalloff    = EditorGUILayout.ToggleLeft("Use Falloff", script.useFalloff);
            script.clampFalloff  = EditorGUILayout.ToggleLeft("Clamp Falloff Result to 0-1 Scale", script.clampFalloff);
            GUILayout.Space(5);
            script.falloffCurve  = EditorGUILayout.CurveField("Falloff Curve", script.falloffCurve);

            GUILayout.Space(10);

            script.heightMultiplier
                                 = EditorGUILayout.FloatField("Height Multiplier", script.heightMultiplier);
            script.heightGain    = EditorGUILayout.FloatField("Height Gain", script.heightGain);
            GUILayout.Label("Height Gain is a value to add from the result height of noise height times multiplier.");

            --EditorGUI.indentLevel;

            if (EditorGUI.EndChangeCheck())
                changed = true;
        }
        EditorGUILayout.EndFadeGroup();
        #endregion
            
        GUILayout.Space(5);

        #region Texture Generation Fade Group
        showFadeTexture.target   = EditorGUILayout.Foldout(showFadeTexture.target, "Texture Generation Fields");
        if(EditorGUILayout.BeginFadeGroup(showFadeTexture.faded)) {
            EditorGUI.BeginChangeCheck();

            ++EditorGUI.indentLevel;

            script.colorCurve    = EditorGUILayout.CurveField("Color Interpolation Curve", script.colorCurve); 
            script.topColor      = EditorGUILayout.ColorField("Color of the mountain", script.topColor);
            script.bottomColor   = EditorGUILayout.ColorField("Color at the edge", script.bottomColor);

            --EditorGUI.indentLevel;

            if (EditorGUI.EndChangeCheck())
                changed = true;
        }
        EditorGUILayout.EndFadeGroup();
        #endregion

        GUILayout.Space(5);

        #region Texture Generation Fade Group
        showFadeAnim.target   = EditorGUILayout.Foldout(showFadeAnim.target, "Animation Fields");
        if(EditorGUILayout.BeginFadeGroup(showFadeAnim.faded)) {
            ++EditorGUI.indentLevel;

            script.treeMassPlacerOnPCG
                                 = EditorGUILayout.ObjectField(script.treeMassPlacerOnPCG, typeof(TreeMassPlacerOnPCG), true) as TreeMassPlacerOnPCG;
            script.sinkHeight    = EditorGUILayout.FloatField("Sink amount of land pre-animation", script.sinkHeight);
            script.riseDuration  = EditorGUILayout.FloatField("Time length of land rising back to position", script.riseDuration);
            script.riseCurve     = EditorGUILayout.CurveField("Rise Motion Curve", script.colorCurve); 

            --EditorGUI.indentLevel;
        }
        EditorGUILayout.EndFadeGroup();
        #endregion

        if(changed) {
            changed = false;
            script.changed = true;
            if(script.autoUpdate) {
                script.Generate();
            }
        }
            
        GUILayout.Space(10);

        if(GUILayout.Button("Generate"))
            script.Generate();
        
        if(GUILayout.Button("Generate Random")) {
            script.RandomizeSeed();
            script.Generate();
        }
    }
}