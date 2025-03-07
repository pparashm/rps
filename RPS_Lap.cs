/*
Race Positioning System by Solution Studios

Script Name: RPS_Lap.cs

Description:
This script can be attached to any object with a RPS_Position script.
It adds a lap system to the object, keeping track of the current lap number.
No objects should have more than one attached.
*/

using UnityEngine;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;

public class RPS_Lap : MonoBehaviour {

	public ObscuredInt startLapNumber = 1; //Lap the object starts on. e.g.It might start before or after the start/finish line, where it should be 0 or 1 respectivley
	public ObscuredBool hasEnd = true; //Is there a final lap
	public ObscuredInt lastLap = 3; //Lap number for the final lap
	public ObscuredBool freezeShownPosAtEnd = true; //Should the calculated race position freeze as crosses finish line or continue calculating the race position after the race has ended

	RPS_Position posScript; //The RPS_Position script attached to this object
	RPS_Lap thisScript; //This instance of this RPS_Lap script

	public bool hasFinished = false; //Variable recording whether the race has finished or not. When the RPS_Position script notices the race has finished, it will change this
	public int finishedRacePosition = 0; //The race position when the object crossed the finish line

	void Start () {
		thisScript = gameObject.GetComponent<RPS_Lap> (); //Assigns this script
		posScript = gameObject.GetComponent<RPS_Position> (); //Assigns the RPS_Position script
		if (posScript == null) {
			Debug.Log ("RPS Error: all RPS_Lap scripts should be attached to an object which has a RPS_Position script");
		} else {
			posScript.currentLapNumber = startLapNumber; //Sets the currentLapNumber of the RPS_Position script
			posScript.lapScript = thisScript;
		}
	}

	void Update () {
		if (posScript == null) {
			posScript = gameObject.GetComponent<RPS_Position> (); //Assigns the RPS_Position script
			if (posScript == null) {
				Debug.Log ("RPS Error: all RPS_Lap scripts should be attached to an object which has a RPS_Position script");
			}
		}
		if (posScript != null) {
            //Modifies variables on the RPS_Position script telling it to include laps when it calculates the current race position
			posScript.useLaps = true;
			posScript.lapScript = thisScript;
		}
	}

	public void raceFinished () { //Called by the RPS_Position script when the race has finished
		hasFinished = true;
		finishedRacePosition = posScript.currentRacePosition; //Stores the finished race position
	}

	//Extra function to get the current lap number (includes laps gone back)
	public int currentLapNumber () {
		if (posScript == null) {
			posScript = gameObject.GetComponent<RPS_Position> (); //Assigns the RPS_Position script
			if (posScript == null) {
				Debug.Log ("RPS Error: all RPS_Lap scripts should be attached to an object which has a RPS_Position script");
			}
		}
		if (posScript != null) {
			return (posScript.currentLapNumber - posScript.lapsGoneBack);
		} else {
			return 0;
		}
	}

	//Extra function to get the current lap number (maximum lap so far as ignores laps gone back)
	public int maxLapNumber () {
		if (posScript == null) {
			posScript = gameObject.GetComponent<RPS_Position> (); //Assigns the RPS_Position script
			if (posScript == null) {
				Debug.Log ("RPS Error: all RPS_Lap scripts should be attached to an object which has a RPS_Position script");
			}
		}
		if (posScript != null) {
			return posScript.currentLapNumber;
		} else {
			return 0;
		}
	}

	//Extra function to get the number of laps gone back
	public int lapsGoneBack () {
		if (posScript == null) {
			posScript = gameObject.GetComponent<RPS_Position> (); //Assigns the RPS_Position script
			if (posScript == null) {
				Debug.Log ("RPS Error: all RPS_Lap scripts should be attached to an object which has a RPS_Position script");
			}
		}
		if (posScript != null) {
			return posScript.lapsGoneBack;
		} else {
			return 0;
		}
	}
}
