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
            director = GetComponent<PlayableDirector>();

        if (director != null && director.playableAsset != null)
        {
            Debug.Log($"SplashTimelineLoader: Timeline='{director.playableAsset.name}', duration={director.duration:0.00}s, updateMode={director.timeUpdateMode}");

            director.extrapolationMode = DirectorWrapMode.Hold;
            director.time = 0;

            // Apply the first frame immediately (important for fade-from-black / alpha 0 start)
            director.Evaluate();
            director.Play();

            // Wait until the timeline actually finishes
            yield return new WaitUntil(() => director.state != PlayState.Playing);
        }
        else
        {
            Debug.LogWarning("SplashTimelineLoader: No PlayableDirector or Timeline asset assigned. Falling back to 4 seconds.");
            yield return new WaitForSecondsRealtime(4f);
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

}
