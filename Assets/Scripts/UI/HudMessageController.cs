using System.Collections;
using TMPro;
using UnityEngine;

public class HudMessageController : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (messageText == null)
            messageText = GetComponent<TMP_Text>();
    }

    public void ShowMessage(string message, float durationSeconds = 2f)
    {
        if (messageText == null) return;

        messageText.text = message;
        messageText.gameObject.SetActive(true);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay(durationSeconds));
    }

    public void HideImmediately()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (messageText != null)
            messageText.gameObject.SetActive(false);

        hideCoroutine = null;
    }
}