
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class RecuperatorioStartupScene
{
    private const string ScenePath =
        "Assets/Parcial1-IA-Recuperatorio.unity";

    static RecuperatorioStartupScene()
    {
        EditorApplication.delayCall += OpenScene;
    }

    private static void OpenScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (EditorSceneManager.GetActiveScene().path == ScenePath)
            return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (System.IO.File.Exists(ScenePath))
        {
            EditorSceneManager.OpenScene(ScenePath);
        }
        else
        {
            Debug.LogWarning("No se encontró la escena del recuperatorio.");
        }
    }
}
