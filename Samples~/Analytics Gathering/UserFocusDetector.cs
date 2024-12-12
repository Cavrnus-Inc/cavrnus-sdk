using CavrnusSdk.API;
using CavrnusSdk.PropertySynchronizers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CavrnusPropertiesContainer))]
public class UserFocusDetector : MonoBehaviour
{
    [Header("How far away can the user be to count as \"Focusing On\" this?")]

    public float MaxDistanceFromCamera = 10;

    [Header("How many degrees away from the player's focus can this object be ")]
    public float MaxAngleFromCamera = 15;

    //Poll for user focus every second
    private float pollingTime = 1;
    private float lastPollTime = 0;

    private CavrnusSpaceConnection spaceConn;
    private CavrnusUser localUser;

    private void Start()
    {
        CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn =>
        {
            this.spaceConn = spaceConn;
            CavrnusFunctionLibrary.AwaitLocalUser(spaceConn, localUser =>
            {
                this.localUser = localUser;
            });
        });
    }

    // Update is called once per frame
    void Update()
    {
        //Wait till we are all set up
        if(localUser == null) 
            return;

        //Only check on timer
        if (Time.time < lastPollTime + pollingTime)
            return;
        lastPollTime = Time.time;

        var currUserFocusListRaw = CavrnusFunctionLibrary.GetStringPropertyValue(spaceConn, localUser.ContainerId, "UserFocusList");
        List<string> currUserFocusList = new List<string>();
        if(currUserFocusListRaw != null)
            currUserFocusList = currUserFocusListRaw.Split(',', System.StringSplitOptions.RemoveEmptyEntries).ToList();
        string myContainerId = GetComponent<CavrnusPropertiesContainer>().UniqueContainerName;

        if (LocalUserLookingAtMe())
        {
            if (!currUserFocusList.Contains(myContainerId))
            {
                currUserFocusList.Add(myContainerId);

                string res = string.Join(',', currUserFocusList);

                CavrnusFunctionLibrary.PostStringPropertyUpdate(spaceConn, localUser.ContainerId, "UserFocusList", res);
            }
        }
        else
        {
            if (currUserFocusList.Contains(myContainerId))
            {
                currUserFocusList.Remove(myContainerId);

                string res = string.Join(',', currUserFocusList);

                CavrnusFunctionLibrary.PostStringPropertyUpdate(spaceConn, localUser.ContainerId, "UserFocusList", res);
            }
        }
    }

    //This gets which camera the user is using.
    //By default we assume camera.main, but if your app is different you can change it here.
    private Camera GetUserCamera()
    {
        return Camera.main;
    }

    //This uses two different checks to detect user "focus"
    //First, we gauge the angle between the objects position and the camera view
    //The problem with this is that a large object may have pieces offset from it's root "position"
    //Second we raycast from the camera (if this object has a collider)
    //The problem with this is a tiny or weirdly shaped object may be missed by the raycast
    private bool LocalUserLookingAtMe()
    {
        var cameraToObjectVector = (transform.position - GetUserCamera().transform.position);
        var angleToObject = Vector3.Angle(GetUserCamera().transform.forward, cameraToObjectVector);

        if(angleToObject < MaxAngleFromCamera && cameraToObjectVector.magnitude < MaxDistanceFromCamera)
            return true;

        //Ray who?
        //Ray Skywalker
        var ray = new Ray(GetUserCamera().transform.position, GetUserCamera().transform.forward);
        RaycastHit hit;
        if(GetComponent<Collider>() != null && GetComponent<Collider>().Raycast(ray, out hit, MaxDistanceFromCamera))
        {
            return true;
        }

        return false;
    }
}
