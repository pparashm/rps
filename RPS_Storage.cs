using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RPS_Storage : MonoBehaviour
{

    public List<RPS_Position> positionScript = new List<RPS_Position>(); //List of RPS_Position scripts in the scene
    public List<RPS_PositionSensor> allPositionSensors = new List<RPS_PositionSensor>(); //List of RPS_PositionSensor scripts. This is used by all RPS_Position scripts to calculate their race positions

    public float firstPositionNumber; //These two variables are found in the first frame.
    public float lastPositionNumber; //They are used by RPS_Position scripts to tell when the end of a lap becomes the start of the next lap

    public bool isPointToPoint;

    //All of the other variables are for editor use
    [HideInInspector]
    public bool PositionSensorsSetup = false;

    [HideInInspector]
    public bool hasAddedPSScript = false;

    [HideInInspector]
    public bool hasRemovedPSScript = false;

    [HideInInspector]
    public RPS_PSPlacer PSScript;

    [HideInInspector]
    public List<GameObject> pointChildren = new List<GameObject>();

    public List<GameObject> cuboidarr = new List<GameObject>();

    [HideInInspector]
    public bool is2D = true;

    void Start()
    {
        //Iterates through all PositionSensors to find the lowest and highest position numbers
        float firstNumber = Mathf.Infinity; //Temporary variables changed through the iterations.
        float lastNumber = -Mathf.Infinity;
        foreach (RPS_PositionSensor pos in allPositionSensors)
        {
            if (pos.thisPosition < firstNumber)
            {
                firstNumber = pos.thisPosition; //Finds minimum number
            }
            if (pos.thisPosition > lastNumber)
            {
                lastNumber = pos.thisPosition; //Finds maximum number
            }
        }
        firstPositionNumber = firstNumber;
        lastPositionNumber = lastNumber;

        // Start coroutine to wait for PlayCar objects
        StartCoroutine(WaitForPlayCars(2)); // Wait for 2 PlayCar objects
    }

    IEnumerator WaitForPlayCars(int targetCount)
    {
        int addedCount = 0;
        while (addedCount < targetCount)
        {
            GameObject[] playCars = GameObject.FindGameObjectsWithTag("PlayCar");
            foreach (GameObject playCar in playCars)
            {
                RPS_Position playCarPosition = playCar.GetComponent<RPS_Position>();
                if (playCarPosition != null && !positionScript.Contains(playCarPosition))
                {
                    positionScript.Add(playCarPosition);
                    addedCount++;
                    if (addedCount >= targetCount)
                    {
                        yield break; // Exit the coroutine when target count is reached
                    }
                }
            }
            yield return new WaitForSeconds(1f); // Wait for 1 second before checking again
        }
    }
}