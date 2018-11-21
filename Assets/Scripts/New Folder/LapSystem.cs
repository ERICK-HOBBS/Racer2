using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapSystem : MonoBehaviour {

    public GameObject nextWaypoint;
    public bool finalWaypoint;
    

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            nextWaypoint.SetActive(true);
            this.gameObject.SetActive(false);
        }

        if (finalWaypoint == true)
        {
            other.gameObject.GetComponent<Car>().laps++;
        }
    }
}
