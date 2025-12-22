using UnityEngine;
using UnityEngine.SceneManagement;
//  created/edited on branch menu-music-persist 

[RequireComponent(typeof(AudioSource))]
public class MenuMusicController : MonoBehaviour
{
    public static MenuMusicController Instance { get; private set; }

    [Header("Keep music only in these scenes")]
    [SerializeField] private string[] keepAliveSceneNames = { "MainMenuScene", "ScoreboardScene" };

    private AudioSource _src;
    //MATHS CONTENT PRESENT HERE
    private void Awake()
    {
        _src = GetComponent<AudioSource>();

        //  destroy duplicates
        if (Instance != null && Instance != this)
        {
            if (_src != null) _src.Stop();
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        // start music 
        if (_src != null && !_src.isPlaying)
            _src.Play();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            Instance = null;
        }
    }

    private void OnActiveSceneChanged(Scene from, Scene to)
    {
        //  remove this music so it won't play in gameplay scenes**
        if (!IsKeepAliveScene(to.name))
            Destroy(gameObject);
    }

    private bool IsKeepAliveScene(string sceneName)
    {
        if (keepAliveSceneNames == null) return false;

        for (int i = 0; i < keepAliveSceneNames.Length; i++)
            if (keepAliveSceneNames[i] == sceneName)
                return true;

        return false;
    }

    public void StopAndDestroy()
    {
        if (_src != null) _src.Stop();
        Destroy(gameObject);
    }
}
