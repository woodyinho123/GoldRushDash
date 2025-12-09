using UnityEngine;

public class LadderZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            Debug.Log("Entered LADDER trigger: " + gameObject.name);
            pc.SetOnLadder(true, transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            Debug.Log("Exited LADDER trigger: " + gameObject.name);
            pc.SetOnLadder(false, transform);
        }
    }
}
