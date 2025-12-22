using UnityEngine;

[ExecuteInEditMode]
public class FitBoxColliderToChildren : MonoBehaviour
{
    [ContextMenu("Fit BoxCollider To Children")]
    public void Fit()   //MATHS CONTENT PRESENT HERE
    {
        //  add  boxcollider on this object
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
            box = gameObject.AddComponent<BoxCollider>();

        // find all child renderers (ladders)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning("FitBoxColliderToChildren: No child renderers found.", this);
            return;
        }

        // combine their bounds in world space
        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combined.Encapsulate(renderers[i].bounds);
        }

        // convert world bounds to local space of this object
        
        Vector3 centerLocal = transform.InverseTransformPoint(combined.center);
        Vector3 sizeLocal = transform.InverseTransformVector(combined.size);

        // make sizes positive
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
