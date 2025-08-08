using System;
using UnityEngine;

[ExecuteAlways]
public class UniqueID : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private string uniqueId;

    public string ID
    {
        get
        {
            if (string.IsNullOrEmpty(uniqueId))
                Generate();
            return uniqueId;
        }
    }

    private void Awake()
    {
        // 런타임에도 ID가 없으면 생성
        if (string.IsNullOrEmpty(uniqueId))
            Generate();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 에디터에서 붙이거나 복제할 때 ID가 비어 있으면 생성
        if (string.IsNullOrEmpty(uniqueId))
            Generate();
    }
#endif

    private void Generate()
    {
        uniqueId = Guid.NewGuid().ToString();
#if UNITY_EDITOR
        // 에디터 변경 저장
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}