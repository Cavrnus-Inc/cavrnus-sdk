using CavrnusSdk.AR;
using UnityEngine;

public class ArTrackerTest : MonoBehaviour
{
    public void AdjustWorld()
    {
        var camPos = Camera.main.transform.position;
        var dir = (transform.position - camPos);
        var rot = Quaternion.LookRotation(dir, Vector3.up);

        rot = Quaternion.Euler(0f, rot.eulerAngles.y, 0f);

        GetComponent<CavrnusArTracker>().TrackArTracker(Vector3.zero, rot);
    }
}
