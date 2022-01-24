using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

public class MyCoroutineHandler
{
    public static void StartCoroutine(IEnumerator routine, MonoBehaviour behaviour, object ownerInEditor)
    {
        if (Application.isEditor)
            EditorCoroutineUtility.StartCoroutine(routine, ownerInEditor);
        else
            behaviour.StartCoroutine(routine);
    }
}
