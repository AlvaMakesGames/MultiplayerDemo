using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraCollision : MonoBehaviour
{
    private float minDist = 1f;
    private float maxDist = 3f;
    private float smoothing = 10f;
    private Vector3 dollyDir;
    private float distance;

    // Start is called before the first frame update
    void Start()
    {
        dollyDir = transform.localPosition.normalized;
        distance = transform.localPosition.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 desiredCamPos = transform.parent.TransformPoint(dollyDir * maxDist);
        RaycastHit hit;

        if (Physics.Linecast(transform.parent.position, desiredCamPos, out hit))
        {
            distance = Mathf.Clamp(hit.distance, minDist, maxDist);
        }
        else
        {
            distance = maxDist;
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, dollyDir * distance, Time.deltaTime * smoothing);
    }
}
