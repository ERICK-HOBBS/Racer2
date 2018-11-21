using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfPointTrigger : MonoBehaviour
{

    public GameObject LapCompleteTrig;
    public GameObject HalfLapTrig;

    void OnTriggerEnter()
    {
        //active finish line trigger when passed through by player
        LapCompleteTrig.SetActive(true);
        //deactive half point trigger
        HalfLapTrig.SetActive(false);
    }
}