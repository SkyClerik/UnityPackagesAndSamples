using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SkyClerik.Develop
{
    public class EditorComponentMeshReplacer : MonoBehaviour
    {
        [Header("Source GO (GO с MeshFilter для замены)")]
        [SerializeField] public List<GameObject> sourceGOs = new List<GameObject>();

        [Header("FBX Assets (FBX модели с новыми мешами)")]
        [SerializeField] public List<GameObject> fbxAssets = new List<GameObject>();

        [Header("Настройки")]
        [SerializeField] private bool logDetails = true;

        [ContextMenu("Заменить меши")]
        public void ReplaceMeshes()
        {
            int success = 0, failed = 0;

            foreach (var sourceGO in sourceGOs.Where(go => go))
            {
                var meshFilter = sourceGO.GetComponent<MeshFilter>();
                string meshName = meshFilter == null ? meshFilter.sharedMesh.name : sourceGO.name;

                // Поиск по имени меша в FBX
                var targetMesh = fbxAssets
                    .Where(fbx => fbx)
                    .Select(fbx => fbx.GetComponent<MeshFilter>())
                    .Where(mf => mf?.sharedMesh?.name == meshName)
                    .FirstOrDefault()?.sharedMesh;

                if (targetMesh)
                {
                    Undo.RecordObject(meshFilter, $"Replace {meshName}");
                    meshFilter.sharedMesh = targetMesh;
                    EditorUtility.SetDirty(meshFilter);
                    success++;

                    if (logDetails)
                        Debug.Log($"[{sourceGO.name}] {meshName} → заменен");
                }
                else
                {
                    failed++;
                    Debug.LogWarning($"[{sourceGO.name}] {meshName} — не найден");
                }
            }

            Debug.Log($"Замена завершена: {success} успеха, {failed} пропущено");
        }
    }

}
