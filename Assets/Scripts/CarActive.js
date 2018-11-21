var Car : GameObject;
var Dreamcar01 : GameObject;
var Dreamcar02 : GameObject;

function Start () {
    Car.GetComponent("Car").enabled = true;
    Dreamcar01.GetComponent("CarAIControl").enabled = true;
    Dreamcar02.GetComponent("CarAIControl").enabled = true;
	
}