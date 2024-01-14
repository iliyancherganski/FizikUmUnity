using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorPoint : MonoBehaviour
{
    public LayerMask targetLayer;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
        {
            Vector3 hitPosition = hit.point;
            HitObject(hit.collider.gameObject, hitPosition);
        }
    }

    void HitObject(GameObject hitObject, Vector3 hitPosition)
    {
        transform.position = hitPosition;
    }
}
