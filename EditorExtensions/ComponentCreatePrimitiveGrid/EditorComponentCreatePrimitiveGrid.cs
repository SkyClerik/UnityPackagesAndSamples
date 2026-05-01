using System.Collections.Generic;
using UnityEngine;

public class EditorComponentCreatePrimitiveGrid : MonoBehaviour
{
    [SerializeField] private GameObject _prefabCubeGameObject;
    [SerializeField] private Material _materialA;
    [SerializeField] private Material _materialB;

    [SerializeField] private int _width = 10;
    [SerializeField] private int _height = 10;
    [SerializeField] private Vector2 _spacing = new Vector2(1f, 1f);

    [SerializeField] private bool _autoGenerateOnStart = false;

    private readonly List<GameObject> _instances = new();

    private void Start()
    {
        if (_autoGenerateOnStart)
            GenerateGrid();
    }

    public void GenerateGrid()
    {
        ClearGrid();

        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                Vector3 position = new Vector3(
                    x * _spacing.x,
                    0f,
                    z * _spacing.y
                );

                GameObject go;

                if (_prefabCubeGameObject != null)
                {
                    go = Instantiate(_prefabCubeGameObject, position, Quaternion.identity, transform);
                }
                else
                {
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube); // запасной куб
                    go.transform.SetPositionAndRotation(position, Quaternion.identity);
                    go.transform.SetParent(transform);
                }

                var renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    bool isEven = (x + z) % 2 == 0;
                    var mat = isEven ? _materialA : _materialB;

                    if (mat != null)
                        renderer.material = mat;
                    // если материалов нет — пусть остаётся дефолтный материал Unity
                }

                _instances.Add(go);
            }
        }
    }

    public void ClearGrid()
    {
        foreach (var go in _instances)
        {
            if (go != null)
#if UNITY_EDITOR
                DestroyImmediate(go);
#else
                Destroy(go);
#endif
        }
        _instances.Clear();
    }
}