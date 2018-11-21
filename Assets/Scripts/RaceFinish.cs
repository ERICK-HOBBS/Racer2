using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceFinish : MonoBehaviour
{

    public GameObject MyCar;
    public GameObject FinishCam;
    public GameObject ViewModes;
    public GameObject LevelMusic;
    public GameObject CompleteTrig;
    public AudioSource FinishMusic;

    void OnTriggerEnter()
    {
        this.GetComponent<BoxCollider>().enabled = false;
        //deactive car object
        MyCar.SetActive(false);
        //deactive finish line trigger
        CompleteTrig.SetActive(false);
        //set speed to 0
        Car.topSpeed = 0.0f;
        //deactive car script
        MyCar.GetComponent<Car>().enabled = false;
        //reactive car object
        MyCar.SetActive(true);
        //active rotating camera
        FinishCam.SetActive(true);
        //deactive level music
        LevelMusic.SetActive(false);
        //deactive camera angles
        ViewModes.SetActive(false);
        //play music
        FinishMusic.Play();
    }

}
