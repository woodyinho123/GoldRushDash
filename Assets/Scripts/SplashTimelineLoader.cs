using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class SplashTimelineLoader : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";
    [SerializeField] private float fallbackDuration = 4f; // if Unity reports 0

    private IEnumerator Start()
    {
        Time.timeScale = 1f;

        if (director == null)
        {
            yield return new WaitForSecondsRealtime(fallbackDuration);
            SceneManager.LoadScene(mainMenuSceneName);
            yield break;
        }

        // Force the director to run in real seconds (ignores any timescale weirdness)
        director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;

        // Restart cleanly from time 0
        director.Stop();
        director.time = 0;
        director.Evaluate(); // applies the first keyframe (start state)
        director.Play();

        float dur = fallbackDuration;

        // Prefer TimelineAsset.duration (more reliable than director.duration)
        if (director.playableAsset is TimelineAsset ta && ta.duration > 0.01)
            dur = (float)ta.duration;

        Debug.Log($"SplashTimelineLoader: waiting {dur:0.00}s then loading {mainMenuSceneName}");

        yield return new WaitForSecondsRealtime(dur);

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
