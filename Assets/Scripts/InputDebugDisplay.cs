using UnityEngine;
using TMPro;   // <-- important for TextMeshPro

public class InputDebugDisplay : MonoBehaviour
{
    public TextMeshProUGUI debugText;

    void Update()
    {
        string keys = "";

        // WASD or arrow keys – add/remove whatever you use
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            keys += "Up ";

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            keys += "Down ";

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            keys += "Left ";

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            keys += "Right ";

        // Show result on screen
        if (string.IsNullOrEmpty(keys))
            debugText.text = "";                // nothing pressed = show nothing
        else
            debugText.text = "Keys: " + keys;   // e.g. "Keys: Up Left"
    }
}
