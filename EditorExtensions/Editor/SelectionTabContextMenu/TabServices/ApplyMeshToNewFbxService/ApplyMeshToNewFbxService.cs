using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SkyClerik.Develop
{
    public class ApplyMeshToNewFbxService : ISelectionTabService
    {
        public string Title => "Apply Mesh New FBX";
        public string Description => "Создаёт новые FBX из выделенных объектов сцены, применяя им scale и rotation.";

        private Texture2D _icon;
        public Texture2D Icon
        {
            get
            {
                if (_icon == null)
                    _icon = EditorStartupRunner.FindIconByName("sc_editor_exportFBX");
                return _icon;
            }
        }

        private SelectionTabWindow _window;
        private const string _modalWindowTitale = "Результат";

        public bool IsAvailable(UnityEngine.Object[] selection)
        {
            if (selection == null || selection.Length == 0)
                return false;

            // Ищем хоть один объект СЦЕНЫ с MeshFilter+sharedMesh
            foreach (var obj in selection)
            {
                if (obj is GameObject go && go.scene.IsValid()) // сцена, а не prefab-ассет
                {
                    var mf = go.GetComponent<MeshFilter>();
                    if (mf && mf.sharedMesh)
                        return true;
                }
            }

            return false;
        }

        public void Execute(SelectionTabWindow window)
        {
            _window = window;
            var selection = Selection.objects;
            if (selection == null || selection.Length == 0)
            {
                Debug.LogWarning("[ApplyMeshToNewFbx] Ничего не выделено.");
                window.Close();
                return;
            }

            var targets = new List<GameObject>();
            foreach (var obj in selection)
            {
                if (obj is GameObject go && go.scene.IsValid()) // только объекты сцены
                {
                    var mf = go.GetComponent<MeshFilter>();
                    if (mf && mf.sharedMesh)
                        targets.Add(go);
                }
            }

            if (targets.Count == 0)
            {
                EditorUtility.DisplayDialog(_modalWindowTitale, $"Среди выделенных объектов сцены нет ни одного с MeshFilter/sharedMesh.", "OK");
                _window.Close();
                return;
            }

            foreach (var go in targets)
            {
                TryProcessGameObject(go);
            }

            window.Close();
        }

        private void TryProcessGameObject(GameObject go)
        {
            var mf = go.GetComponent<MeshFilter>();
            var mr = go.GetComponent<MeshRenderer>();

            if (!mf || !mf.sharedMesh)
            {
                EditorUtility.DisplayDialog(_modalWindowTitale, $"{go.name}: нет MeshFilter или sharedMesh.", "OK");
                _window.Close();
                return;
            }

            var t = go.transform;
            var scale = t.localScale;
            var rot = t.localRotation;

            Debug.Log($"[_modalWindowTitale] GO={go.name} path={GetHierarchyPath(t)} " +
                      $"pos={t.position} rot={t.localEulerAngles} scale={scale}");

            if (scale == Vector3.one && rot == Quaternion.identity)
            {
                EditorUtility.DisplayDialog(_modalWindowTitale, $"{go.name}: Scale и Rotation уже по умолчанию. Действий не требуется.", "OK");
                _window.Close();
                return;
            }

            Mesh src = mf.sharedMesh;
            Debug.Log($"[_modalWindowTitale] Source mesh: {src.name}, vertices={src.vertexCount}");

            Mesh mesh = UnityEngine.Object.Instantiate(src);
            mesh.name = src.name + "_appliedTR";
            Debug.Log($"[_modalWindowTitale] Clone mesh created: {mesh.name}");

            var vertices = mesh.vertices;
            var normals = mesh.normals;

            Matrix4x4 TR = Matrix4x4.TRS(Vector3.zero, rot, scale);
            Matrix4x4 NTR = TR.inverse.transpose;

            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = TR.MultiplyPoint3x4(vertices[i]);

            for (int i = 0; i < normals.Length; i++)
                normals[i] = NTR.MultiplyVector(normals[i]).normalized;

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.RecalculateBounds();

            Debug.Log($"[ApplyTR] Applied TR to mesh. New bounds={mesh.bounds}");

            var temp = new GameObject(src.name + "_FBXExport");
            var tempMf = temp.AddComponent<MeshFilter>();
            var tempMr = temp.AddComponent<MeshRenderer>();

            temp.transform.position = Vector3.zero;
            temp.transform.rotation = Quaternion.identity;
            temp.transform.localScale = Vector3.one;

            tempMf.sharedMesh = mesh;
            if (mr)
                tempMr.sharedMaterials = mr.sharedMaterials;

            Debug.Log("[ApplyTR] Temp GO created for FBX export.");

            string srcPath = AssetDatabase.GetAssetPath(src);
            if (string.IsNullOrEmpty(srcPath))
                srcPath = "Assets";

            string dir = Path.GetDirectoryName(srcPath);
            string newPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(dir, src.name + "_appliedTR.fbx"));

            Debug.Log($"[ApplyTR] Exporting NEW FBX to: {newPath}");

            if (!TryExportToFbx(newPath, temp))
            {
                UnityEngine.Object.DestroyImmediate(temp);
                return;
            }

            Debug.Log("[ApplyTR] FBX export done.");

            UnityEngine.Object.DestroyImmediate(temp);

            t.localScale = Vector3.one;
            t.localRotation = Quaternion.identity;

            Debug.Log($"[ApplyTR] Finished for {go.name}. New FBX created: {newPath}");
        }

        private bool TryExportToFbx(string path, GameObject go)
        {
            var modelExporterType = Type.GetType("UnityEditor.Formats.Fbx.Exporter.ModelExporter, Unity.Formats.Fbx.Editor");
            if (modelExporterType == null)
            {
                EditorUtility.DisplayDialog(
                    "FBX Exporter не найден",
                    "Инструмент \"Apply Mesh New FBX\" требует пакет \"FBX Exporter\" (com.unity.formats.fbx).\n" +
                    "Установи его через Window > Package Manager.",
                    "OK");
                return false;
            }

            var exportMethod = modelExporterType.GetMethod(
                "ExportObject",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[] { typeof(string), typeof(GameObject) },
                null);

            if (exportMethod == null)
            {
                Debug.LogError("[ApplyTR] Не найден метод ModelExporter.ExportObject(string, GameObject).");
                return false;
            }

            exportMethod.Invoke(null, new object[] { path, go });
            return true;
        }

        private string GetHierarchyPath(Transform t)
        {
            var path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}