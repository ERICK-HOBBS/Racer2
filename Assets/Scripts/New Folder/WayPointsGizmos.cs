using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointsGizmos : MonoBehaviour {

    public float size = 1f;
    public Transform[] waypoints;
    private void OnDrawGizmos()
    {
        waypoints = gameObject.GetComponentsInChildren<Transform>();
        Vector3 last = waypoints[waypoints.Length - 1].position;
        for (int i = 1; i < waypoints.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(waypoints[i].position, size);
            Gizmos.DrawLine(last, waypoints[i].position);
            last = waypoints[i].position;
        }
    }

    // Use this for initialization
    void Start () { 
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
