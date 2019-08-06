using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

public class MyLightingShaderGUI : ShaderGUI {

    Material target;
    MaterialEditor editor;
    MaterialProperty[] properties;
    
    MaterialProperty FindProperty(string name) {
        return FindProperty(name, properties);
    }

    static GUIContent staticLabel = new GUIContent();

    static GUIContent MakeLabel(MaterialProperty property, string tooltip = null) {
        staticLabel.text = property.displayName;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }

    public override void OnGUI (
        MaterialEditor editor, MaterialProperty[] properties) {

        this.target = editor.target as Material;
        this.editor = editor;
        this.properties = properties;
        DoMain();
    }

    void DoMain() {
        GUILayout.Label("Main Maps", EditorStyles.boldLabel);

        MaterialProperty mainTex = FindProperty("_MainTex");
        editor.TexturePropertySingleLine(
            MakeLabel(mainTex, "Albedo (RGB)"), mainTex, FindProperty("_Tint"));
        DoNormals();
        editor.TextureScaleOffsetProperty(mainTex);
    }

    void RecordAction(string label) {
        editor.RegisterPropertyChangeUndo(label);
    }

    void DoNormals() {
        MaterialProperty map = FindProperty("_NormalMap");
        Texture tex = map.textureValue;
        EditorGUI.BeginChangeCheck();
        editor.TexturePropertySingleLine(
            MakeLabel(map), map, 
            tex ? FindProperty("_BumpScale") : null);
    }
}
