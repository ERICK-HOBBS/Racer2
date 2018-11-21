using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpedoMeter : MonoBehaviour {

    //min rotation angle for needle on z axis
    static float minAngle = 42.0f;
    //max rotation angle for needle on z axis
    static float maxAngle = -225.0f;

    //create instance to access from function
    static SpedoMeter thisSpeedo;


	// Use this for initialization
	void Start () {

        thisSpeedo = this;
		
	}
	
	public static void ShowSpeed(float speed, float min, float max)
    {
        //calculating min/max needle angle based off cars min/max speed
        float ang = Mathf.Lerp(minAngle, maxAngle, Mathf.InverseLerp(min, max, speed));
        //z angle rotation on needle
        thisSpeedo.transform.eulerAngles = new Vector3(0, 0, ang);
    }
}
