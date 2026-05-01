using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace SkyClerik.Develop
{
    [CustomEditor(typeof(EditorComponentMeshReplacer))]
    public class MeshReplacerEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Кнопка
            var replaceBtn = new Button(() =>
            {
                serializedObject.Update();
                ((EditorComponentMeshReplacer)target).ReplaceMeshes();
                serializedObject.ApplyModifiedProperties();
            })
            {
                text = "ЗАМЕНИТЬ МЕШИ",
                style = {
                    height = 45,
                    backgroundColor = new Color(0, 0.8f, 0, 1),
                    color = Color.white,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14,
                    marginBottom = 10
                }
            };
            root.Add(replaceBtn);

            root.Add(new HelpBox("Drag & Drop в списки ниже", HelpBoxMessageType.Info));

            // Source GO - PropertyField (работает!)
            var sourceProp = serializedObject.FindProperty("sourceGOs");
            root.Add(new PropertyField(sourceProp)
            {
                style = { marginBottom = 15 }
            });

            // FBX Assets - PropertyField (работает!)
            var fbxProp = serializedObject.FindProperty("fbxAssets");
            root.Add(new PropertyField(fbxProp));

            // Log toggle
            var logProp = serializedObject.FindProperty("logDetails");
            root.Add(new PropertyField(logProp));

            return root;
        }
    }
}
