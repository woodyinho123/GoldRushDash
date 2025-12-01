using UnityEngine;
using TMPro;   //needed to add this for textmeshpro to work

public class InputDebugDisplay : MonoBehaviour
{
    public TextMeshProUGUI debugText;

    void Update()
    {
        string keys = "";

        // getting arrow keys input
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            keys += "Up ";

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            keys += "Down ";

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            keys += "Left ";

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            keys += "Right ";

        // shows result on screen
        if (string.IsNullOrEmpty(keys))
            debugText.text = "";                // nothing pressed = show nothing
        else
            debugText.text = "Keys: " + keys;  
    }
}
