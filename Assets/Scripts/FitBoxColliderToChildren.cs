using UnityEngine;

[ExecuteInEditMode]
public class FitBoxColliderToChildren : MonoBehaviour
{
    [ContextMenu("Fit BoxCollider To Children")]
    public void Fit()
    {
        // Get or add a BoxCollider on this object
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
            box = gameObject.AddComponent<BoxCollider>();

        // Find all child renderers (your ladders)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning("FitBoxColliderToChildren: No child renderers found.", this);
            return;
        }

        // Combine their bounds in world space
        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combined.Encapsulate(renderers[i].bounds);
        }

        // Convert world bounds to local space of this object
        // IMPORTANT: works best if parent has no rotation and scale = 1
        Vector3 centerLocal = transform.InverseTransformPoint(combined.center);
        Vector3 sizeLocal = transform.InverseTransformVector(combined.size);

        // Make sizes positive
        sizeLocal = new Vector3(
            Mathf.Abs(sizeLocal.x),
            Mathf.Abs(sizeLocal.y),
            Mathf.Abs(sizeLocal.z)
        );

        box.center = centerLocal;
        box.size = sizeLocal;
        box.isTrigger = true;
    }
}
