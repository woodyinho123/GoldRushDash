#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]  // runs as soon as Unity loads the editor
public static class AutoSaveScene
{
    // interval (seconds) between autosaves
    private static float saveInterval = 300f;    // 5 minutes
    private static double nextSaveTime;

    static AutoSaveScene()
    {
        nextSaveTime = EditorApplication.timeSinceStartup + saveInterval;
        EditorApplication.update += AutoSave;
    }

    private static void AutoSave()
    {
        // Never autosave while game is playing or about to change playmode
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (EditorApplication.timeSinceStartup < nextSaveTime)
            return;

        SaveScene();
        nextSaveTime = EditorApplication.timeSinceStartup + saveInterval;
    }

    private static void SaveScene()
    {
        // Use EditorSceneManager in the editor
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.isDirty)
            return; // nothing changed

        bool sceneSaved = EditorSceneManager.SaveScene(scene);
        if (sceneSaved)
        {
            Debug.Log("Scene auto-saved at: " + System.DateTime.Now);
        }
        else
        {
            Debug.LogError("Failed to auto-save scene.");
        }
    }
}
#endif
