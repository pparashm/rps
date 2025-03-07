using UnityEngine;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;

public class RPS_Position : MonoBehaviour {

    //The RPS_Storage script for the race. This is used as it has the list of all the PositionSensors which is needed to calculate the race position.
	GameObject storageObject;
	public RPS_Storage storageScript;

	public ObscuredFloat currentNearestPosition; //Position number of the nearest PositionSensor
	public ObscuredFloat currentNearestPositionUnlimited; //Position number of neareast PositionSensor whithout limits from RPS_Checkpoints script
	public ObscuredFloat percentageDist; //Percentage of the way between the last and next PositionSensors. This is used when two RPS_Position scripts are between the same two Position Sensors for higher accuracy.

    //Variables used to help calculate the percentageDist variable
	public ObscuredFloat nextPosition;
	public ObscuredFloat lastPosition;
    
	private RPS_PositionSensor closestSensor;
    private RPS_PositionSensor lastClosestSensor;
	private RPS_PositionSensor nearestLast;
	private RPS_PositionSensor nearestNext;

    //Important Variables:     These are accessed by other scripts when needed
	public ObscuredInt currentRacePosition = 0; //The current calculated race position. 0 = 1st, 1 = 2nd, ... etc
	public ObscuredBool useLaps = false; //Whether there is a RPS_Lap script attached to the object to use laps or not to calculate position
	public ObscuredInt currentLapNumber = 0; //What lap it is currently on if useLaps=true
	public ObscuredInt lapsGoneBack = 0; //Used if a RPS_Position object does a lap backwards and passes the start/finish in the wrong direction. It doesn't change the current lap number, but it does mean the player has to do another lap in the right direction to get back to where they were.
	public RPS_Lap lapScript; //The RPS_Lap script attached to this object if there is one

	private RPS_Position thisScipt; //This instance of this script
	private ObscuredBool useCheckpoints = false; //Whether there is a RPS_Checkpoints script attached to this object. This is checked at runtime.
	private RPS_Checkpoints thisCheckpoints; //The instance of RPS_Checkpoints if attached to this object.

	private ObscuredFloat lastFrameNearestPosition; //The nearest position number in the last frame. If it was the very last position number of a lap and it is the first position number in this frame, it shows a lap has been completed or end of race reached.
    private ObscuredBool isFirstFrame = true; //Prevents the script trying to access the lastFrameNearestPosition variable when one doesn't exist

	public ObscuredBool hasFinished = false; //This is used to compare object positions. When one object has finished the race and the other hasn't, then the one which has finished is ahead.
	private ObscuredBool hasFinishedLastFrame = false;

	private ObscuredBool freezeRacePos = false;

    void Start () {
		thisScipt = gameObject.GetComponent<RPS_Position>(); //Assigns this script to variable
		storageObject = GameObject.Find ("RPS_Storage");
		if (storageObject == null) {
			Debug.Log("RPS Error: Storage object not found in scene. Use the RPS Editor Window to create one");
		} else {
			storageScript = storageObject.gameObject.GetComponent<RPS_Storage>(); //Assigns storage object and RPS_Storage script to variable
			if (storageScript == null) {
				Debug.Log ("RPS Error: Storage object not setup correctly in scene. Use the RPS Editor Window to do this properly");
			}
		}
		RPS_Lap lapscr = gameObject.GetComponent<RPS_Lap>();
		if (lapscr == null) {
			useLaps = false;
		} else {
			useLaps = true;
			lapScript = lapscr; //Assigns the lap script to a variable if one exists
		}

		//Check for (and assign to variable) RPS_Checkpoints script
		RPS_Checkpoints checkScript = gameObject.GetComponent<RPS_Checkpoints>();
		if (checkScript == null) {
			useCheckpoints = false;
		} else {
			useCheckpoints = true;
			thisCheckpoints = checkScript;
		}
	}

	void Update () {
		
		//Check for (and assign to variable) RPS_Checkpoints script
		RPS_Checkpoints checkScript = gameObject.GetComponent<RPS_Checkpoints>();
		if (checkScript == null) {
			useCheckpoints = false;
		} else {
			useCheckpoints = true;
			thisCheckpoints = checkScript;
		}

        //Has the race has finished and is there a Lap script which says the position sensing should freeze and the race position should stop being calculated
		bool shouldFreeze = false;
		if (useLaps == true) {
			if ((lapScript.freezeShownPosAtEnd == true)&&(lapScript.hasFinished == true)) {
				shouldFreeze = true;
			}
		}
		if (freezeRacePos == true) {
			shouldFreeze = true;
		}
		if (shouldFreeze == false) { //If race position should be calculated
			if (storageObject == null) {
				storageObject = GameObject.Find ("RPS_Storage"); //Assign storage object
				if (storageObject == null) {
					Debug.Log("RPS Error: Storage object not found in scene. Use the RPS Editor Window to create one");
				}
			} else {
				if (storageScript == null) {
					storageScript = storageObject.gameObject.GetComponent<RPS_Storage>(); //Assign RPS_Storage script
					if (storageScript == null) {
						Debug.Log ("RPS Error: Storage object not setup correctly in scene. Use the RPS Editor Window to do this properly");
					}
				}
			}

			if ((storageObject != null)&&(storageScript != null)) {
				//Find the closest position sensor by interating through the list on the RPS_Storage script
				bool foundOne = false;
				float sensorDistance = Mathf.Infinity;
				foreach (RPS_PositionSensor sensor in storageScript.allPositionSensors) {
					if (sensor.gameObject.activeSelf == true) {
						foundOne = true;
						float distance = Vector3.Distance(transform.position, sensor.gameObject.transform.position);
						if (distance < sensorDistance) {
							closestSensor = sensor;
							sensorDistance = distance;
						}
					}
				}
				if (foundOne == false) {
                    //If no PositionSensors where found in the scene at all, return error
					Debug.Log ("RPS Error: No active PositionSensors found. Create them in the RPS Editor Window");
				} else {
                    //If closest PositionSensor was found
					currentNearestPosition = closestSensor.thisPosition;
					currentNearestPositionUnlimited = currentNearestPosition;

					//Limit if missed a checkpoint
					if (useCheckpoints == true) {
						float checkLimit = thisCheckpoints.limitedPosition;
						if (currentNearestPosition > checkLimit) { //If the first unpassed checkpoint is behind the object, then limit the objects position around the track to the value by the unpassed checkpoint
							currentNearestPosition = checkLimit;
							percentageDist = 0;
						}
					}

					if ((isFirstFrame == true)&&(useLaps == true)) {
						isFirstFrame = false;
                        //No longer in first frame
					} else {
						if (useLaps == true) {
                            //If the race has laps
                            //If the last position was at the end of the lap and the current position is a the start of a lap, must have completed the lap
							if ((lastFrameNearestPosition == storageScript.lastPositionNumber)&&(currentNearestPosition == storageScript.firstPositionNumber)) {
								//If using checkpoints, make sure all checkpoints have been passed
								bool checksCovered = true;
								if (useCheckpoints == true) {
									checksCovered = thisCheckpoints.CheckCanGoForwardLap();
								}
								if (checksCovered == true) {
									if (lapsGoneBack == 0) {
										if ((lapScript.hasEnd == true)&&((currentLapNumber-lapsGoneBack) == lapScript.lastLap)) {
	                                        //If it was the last lap that was completed
											if (lapScript.freezeShownPosAtEnd == false) {
	                                            //increase the current lap number if the object hasn't done any laps backwards
												currentLapNumber = currentLapNumber + 1;
											}
											lapScript.raceFinished(); //Tell the RPS_Lap script the race has been finished
											hasFinished = true;
											if (useCheckpoints == true) {
												thisCheckpoints.checkpointsForwardLap(); //tell the checkpoint system it has gone forward a lap
											}
										} else {
	                                        //If it wasn't the last lap, increase the current lap number anyway
											currentLapNumber = currentLapNumber + 1;
											if (useCheckpoints == true) {
												thisCheckpoints.checkpointsForwardLap(); //tell the checkpoint system it has gone forward a lap
											}
										}
									} else {
	                                    //If the object has done some laps backwards
										if ((lapScript.hasFinished == true)&&(lapScript.freezeShownPosAtEnd == true)) {
										} else {
											lapsGoneBack = lapsGoneBack - 1;
	                                        //Decrease the number of laps done backwards by one, and don't change the current lap number
											if (useCheckpoints == true) {
												thisCheckpoints.checkpointsForwardLap(); //tell the checkpoint system it has gone forward a lap
											}
										}
									}
								}
							}
                            //If the last position was at the start of a lap and the current position is a the end on a lap, must have gone back a lap (gone the wrong way)
							if ((lastClosestSensor.thisPosition == storageScript.firstPositionNumber)&&(closestSensor.thisPosition == storageScript.lastPositionNumber)) {
								//Checkpoints don't need to be passed to go back a lap so nothing needs to be confirmed with the RPS_Checkpoints script here
								if (lapScript.hasFinished == true) {
									if (lapScript.freezeShownPosAtEnd == false) {
										lapsGoneBack = lapsGoneBack + 1;
                                        //If race has ended, but it is still calculating the race position, increase the number of laps gone back by one, and don't change the current lap number
									}
									if (useCheckpoints == true) {
										thisCheckpoints.checkpointsBackLap(); //tell the checkpoint system it has gone back a lap
									}
								} else {
									lapsGoneBack = lapsGoneBack + 1;
                                    //Increase the number of laps gone back if race hasn't finished
									if (useCheckpoints == true) {
										thisCheckpoints.checkpointsBackLap(); //tell the checkpoint system it has gone back a lap
									}
								}
							}
						}
					}

                    //Find the percentage distance between the next position sensor and the last position sensor:
                    //Firstly find the next and last PositionSensors (before and after the cloest PositionSensor)
					float thingToFindLast = closestSensor.lastPosition; //position number of the next PositionSensor after the closest one
					bool foundThingOne = false;
					float distanceOne = Mathf.Infinity;
					float thingToFindNext = closestSensor.nextPosition; //position number of the previous PositionSensor before the closest one
					bool foundThingTwo = false;
					float distanceTwo = Mathf.Infinity;
					if (closestSensor.isFirst == true) { //If the nearest position sensor is the one with the lowest position number
						thingToFindLast = storageScript.lastPositionNumber; //then the previous position sensor is the one with the highest position number
					}
					if (closestSensor.isLast == true) { //If the nearest position sensor is the one with the highest position number
						thingToFindNext = storageScript.firstPositionNumber; //then the next position sensor is the one with the lowest position number
					}
					foreach (RPS_PositionSensor sens in storageScript.allPositionSensors) { //Iterate through the list of all PositionSensors
						if (sens.thisPosition == thingToFindLast) {
							foundThingOne = true; //The previous position sensor was found
							float anotherDist = Vector3.Distance(transform.position, sens.gameObject.transform.position);
							if (anotherDist < distanceOne) {
								nearestLast = sens;
								distanceOne = anotherDist; //Finds distance to the previous position sensor
							}
						}
						if (sens.thisPosition == thingToFindNext) {
							foundThingTwo = true; //The next position sensor was found
							float anotherDistt = Vector3.Distance(transform.position, sens.gameObject.transform.position);
							if (anotherDistt < distanceTwo) {
								nearestNext = sens;
								distanceTwo = anotherDistt; //finds the distance to the next position sensor
							}
						}
					}
					if ((foundThingOne == false)||(foundThingTwo == false)) {
                        //If the next or previous PositionSensors were not found then the next and previous position numbers were wrong
                        //So return a error
						Debug.Log("RPS Error: Position Sensors last and next positions do not exist in some cases");
					} else {
                        //We now have three position sensors:
                        //The closest PositionSensor, the previous one before the closest one (the distance to it being distanceOne), and the next one after the closest one (the distance to it being distanceTwo)
                        //We need to narrow this down to the two PositionSensors the object is between. We know the closest PositionSensor must be one of these, so we need to work out which of the other two PostionSensors, the other one is.
                        if (distanceOne < distanceTwo) { //If the previous sensor is closest to the object, it must be that one
							nextPosition = currentNearestPosition; //Object is before the closest PositionSensor
							lastPosition = thingToFindLast; //Object is after the previous PositionSensor
						}
						if (distanceTwo < distanceOne) { //If the next sensor is closest to the object, it must be that one
							nextPosition = thingToFindNext; //Object is before the next PositionSensor
							lastPosition = currentNearestPosition; //Object is after the closest PositionSensor
						}
						if (distanceOne == distanceTwo) { //If the next and previous sensors are the same distance away
							nextPosition = currentNearestPosition; //Just say the object is before the closest and also after the closest to prevent bugs
							lastPosition = currentNearestPosition;
						}
                        //Now we know which two PositionSensors the object is between
                        //To caluclate the percentage of the way between them we need to find the total distance between them:
						float totalDist = Vector3.Distance(nearestNext.gameObject.transform.position, nearestLast.gameObject.transform.position);
                        //And the distance between the object and the next PositionSensor:
                        float progressDist = Vector3.Distance(transform.position, nearestNext.gameObject.transform.position);
                        //Calculat the percentage distance the object is between them
                        if (storageScript.isPointToPoint == false) {
                            //If there are laps
							percentageDist = (totalDist-progressDist) / totalDist;
						} else {
                            //If there are not laps it is different if the closest sensor is also the final one
							if (closestSensor.isFirst == true) {
								percentageDist = (totalDist-progressDist) / totalDist;
							} else {
								if (closestSensor.isLast == true) {
                                    //This means, if objects go past the final sensor, the furthest one from it (in the correct direction) is furthest infront. It prevents bugs.
									percentageDist = progressDist / totalDist;
								} else {
									percentageDist = (totalDist-progressDist) / totalDist;
								}
							}
						}

                        //Now we know the Current Lap Number, and the number of Laps gone back (if we are using laps). And the closest PositionSensor and the percentage distance between the previous and next PositionSensors
                        //This is enough to compare with the other RPS_Position scripts in the List in the RPS_Storage script
                        //This will give us the current race position
						int racePosition = 0; //We start by saying we are first. We can add 1 for each object behind us.
						bool useLastFrames = false;
						foreach (RPS_Position car in storageScript.positionScript) { //Iterates through array of other RPS_Position scripts
							if (thisScipt != car) {
								if (useLaps == true) {
									//If this object has finished the race and the other hasn't then this object is infront
									if ((hasFinished == true)&&(car.hasFinished == false)) {
									} else {
										if ((hasFinishedLastFrame == false)&&(car.hasFinished == true)) {
											//If the other object has finished and this one hasn't then this object is behind
											racePosition = racePosition + 1;
										} else {
		                                    //If we are using laps, we should compare those first
											if ((currentLapNumber-lapsGoneBack) <(car.currentLapNumber-car.lapsGoneBack)) {
												racePosition = racePosition + 1; //If the other object has done more laps, this object is behind so add 1
											} else {
												if ((currentLapNumber-lapsGoneBack) == (car.currentLapNumber-car.lapsGoneBack)) {
		                                            //If we have done the same number of laps
													if (currentNearestPosition < car.currentNearestPosition) {
														racePosition = racePosition + 1; //If the other object is at a higher position around the track, this object is behind so add 1
													} else {
														if (currentNearestPosition == car.currentNearestPosition) {
		                                                    //If the other object is at the same position around the track, compare the percentage distances between the previous and next sensors
															if (percentageDist < car.percentageDist) {
																racePosition = racePosition + 1; //If the other object's percentage is higher, this object is behind so add 1
															} else {
																if (percentageDist == car.percentageDist) {
		                                                            //If even the percentages are the same (which is very unlikley), just assume they we are behind so add 1
																	useLastFrames = true;
																	racePosition = racePosition + 1;
																}
															}
														}
													}
												}
											}
										}
									}
								} else {
                                    //If laps are not being used
									if (currentNearestPosition < car.currentNearestPosition) {
										racePosition = racePosition + 1; //If the other object is at a higher position around the track, this object is behind so add 1
                                    } else {
										if (currentNearestPosition == car.currentNearestPosition) {
                                            //If the other object is at the same position around the track, compare the percentage distances between the previous and next sensors
                                            if (percentageDist < car.percentageDist) {
												racePosition = racePosition + 1; //If the other object's percentage is higher, this object is behind so add 1
                                            } else {
												if (percentageDist == car.percentageDist) {
                                                    //If even the percentages are the same (which is very unlikley), just assume they we are behind so add 1
                                                    useLastFrames = true;
													racePosition = racePosition + 1;
												}
											}
										}
									}
								}
							}
						}
						if (useLastFrames == false) {
							currentRacePosition = racePosition; //set the currentRacePosition variable which other scripts can access as the race position just calculated
						}
					}
				}
			}
			lastFrameNearestPosition = currentNearestPosition; //remember the position calculated in this frame for use in the next frame
            lastClosestSensor = closestSensor;
        }
		hasFinishedLastFrame = hasFinished; //Remembers whether the object had finished the race in the last frame
	}

	//Extra Function to freeze the race position (probably most useful at end of race with sprint/PointToPoint race types where a RPS_Lap script cannot be used
	public void freezePosition () {
		freezeRacePos = true;
	}

	//Extra Function to unfreeze the race position
	public void unfreezePosition () {
		freezeRacePos = false;
	}

	//Extra funtion to say the race has finished (normally controlled by RPS_Lap when being used)
	public void raceFinished () {
		hasFinished = true;
	}

	//Extra Function to return the race position (frozen or unfrozen using two functions above)
	public int getRacePosition () {
		return currentRacePosition;
	}

	//Extra Function to get the value of the nearest PositionSensor
	public float neareastPosition () {
		return currentNearestPositionUnlimited;
	}

	//Extra Function to get the value of the nearest PositionSensor including limits by RPS_Checkpoints
	public float neareastPositionLimited () {
		return currentNearestPosition;
	}

}