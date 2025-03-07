/*
Race Positioning System by Solution Studios

Script Name: RPS_LapUI.cs

Description:
This script can be attached to any object with a RPS_Lap script.
It is used to display the current lap number of an object through an object with a Unity UI Text component.
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;
using Mirror;

public class RPS_LapUI : NetworkBehaviour
{

    public Text textObj; //The UI Text component to assign the lap string to
    public ObscuredBool showTotalLaps = true; //Whether the total number of laps should be added onto the end of the string e.g. '1/3' instead of '1' if true
    public ObscuredBool changeWhenFinished = true; //At the end of the race, should the lap string be changed or still show the final lap number e.g. '3/3' or '3'
    public ObscuredString changeToText = "Finished"; //If the lap string should change, what should it change to
    public ObscuredBool changeFontSize = false; //Should the font size change at the end of the race if the string changes e.g. Change to lower font size if the word 'Finished' cannot fit on the screen
    public ObscuredInt newFontSize = 0; //Font size to change to

    private RPS_Position posScript; //The RPS_Position script on this object
    private RPS_Lap lapScript; //The RPS_Lap script on this object

    private void Awake()
    {
        textObj = GameObject.Find("Lap").GetComponent<Text>();
    }

    void Start()
    {
        posScript = gameObject.GetComponent<RPS_Position>(); //Assign posScript
        if (posScript == null)
        {
            Debug.Log("RPS Error: RPS_LapUI must be on a object where a RPS_Position script is also attached");
        }
        lapScript = gameObject.GetComponent<RPS_Lap>(); //Assign lapScript
        if (lapScript == null)
        {
            Debug.Log("RPS Error: RPS_LapUI must be on a object where a RPS_Lap script is also attached");
        }
    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (posScript == null)
        {
            posScript = gameObject.GetComponent<RPS_Position>(); //Assign posScript
            if (posScript == null)
            {
                Debug.Log("RPS Error: RPS_LapUI must be on a object where a RPS_Position script is also attached");
            }
        }
        if (lapScript == null)
        {
            lapScript = gameObject.GetComponent<RPS_Lap>(); //Assign lapScript
            if (lapScript == null)
            {
                Debug.Log("RPS Error: RPS_LapUI must be on a object where a RPS_Lap script is also attached");
            }
        }
        if ((posScript != null) && (lapScript != null))
        {
            if (lapScript.hasEnd == false)
            {
                showTotalLaps = false;
            }
            if (lapScript.hasFinished == true)
            {
                if (changeWhenFinished == true)
                { //If the race has ended and the text is meant to change at the end of the race
                    textObj.text = changeToText; //Change the text
                    if (changeFontSize == true)
                    {
                        textObj.fontSize = newFontSize; //And if the font size is meant to change, change the font size
                    }
                }
                else
                {
                    if (showTotalLaps == true)
                    {
                        //As the race has finished show lastlap/lastlap e.g. 3/3
                        textObj.text = lapScript.lastLap + "/" + lapScript.lastLap;
                    }
                    else
                    {
                        //Otherwise, just lastlap
                        textObj.text = "" + lapScript.lastLap;
                    }
                }
            }
            else
            {
                if (showTotalLaps == true)
                {
                    //Race hasn't finished so use the current lap number
                    //If should show the total number of laps e.g. '1/3' instead of '3', add the '/3' to the end of the string
                    textObj.text = "" + posScript.currentLapNumber + "/" + lapScript.lastLap;
                }
                else
                {
                    //Otherwise, don't
                    textObj.text = "" + posScript.currentLapNumber;
                }
            }
        }
    }
}