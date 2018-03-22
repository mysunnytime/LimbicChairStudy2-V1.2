using UnityEngine;
using System.Collections;

public enum Interface
{
	Standing,
	NormalChair,
	LimbicChair}
;

public class Flying : MonoBehaviour
{
    /******************************************************************************************************************************/
    /**********                         UI Display: Display UI messages on screen when calibrating                      ***********/
    /******************************************************************************************************************************/
    public GameObject calibrationDisplay;

    /******************************************************************************************************************************/
    /**********             Speed Parameters: You can change them to move faster/slower toward each direction           ***********/
    /******************************************************************************************************************************/
    public Interface locomotionInterface;
	public float speedLimit = 10;
	public float speedSensitivity = 15;

	/******************************************************************************************************************************/
	/******   Vive Objects: Whenever you add this script to any project, drag vive controller objects into these variables    *****/
	/******************************************************************************************************************************/
	public GameObject viveCameraEye;
	public GameObject viveLeftController;
	public GameObject viveRightController;

	/******************************************************************************************************************************/
	/**** 								Internal Variables: Don't change these variables										***/
	/******************************************************************************************************************************/
	//float locomotionDirection = 0;
	Vector3 LocomotionEuler = new Vector3 ();
	Quaternion LocomotionQuat = new Quaternion ();
    bool interfaceIsReady = false;
    public bool viveRightControllerTriggerStatus = false;
    public bool viveLeftControllerTriggerStatus = false;
    bool handBrakeActivated = false, viveControllerPadStatus = false;
	float handBrakeDecceleratrion = 3;
	Vector3 headZero, headCurrent;
	float headXo = 0, headYo = 0, headZo = 0, headWidth = .09f, headHeight = .07f;
	float exponentialTransferFuntionPower = 1.53f;
	public int initializeStep = 0;

	//0 = before printing PressSpace message, 1 = after PressSpace message waiting for space, 2 = after space press and when the user can fly

	// Use this for initialization
	void Start ()
	{

	}

	void FixedUpdate ()
	{
		ReadControllerData (); //Read Vive Controller data and store them inside internal variables
	}

	// Update is called once per frame
	void Update ()
	{
		FlyingLocomotion (); //Apply the Vive Controller data to the user position in Virtual Environment
	}

	// *** Call this method in FixedUpdate() *** (updates the position of Vivev HMD and Controller at each frame inside internal variables)
	void ReadControllerData ()
	{

		//Print the initial message on screen
		if (initializeStep == 0)
        {
            Debug.Log ("Ask the user to sit straight and look forward and then press the right controller trigger");
			initializeStep = 1;
		}
        else if (initializeStep == 1)
        {
            calibrationDisplay.SetActive(true);
        }

        //Check if the user pressed trigger
        if (!viveRightControllerTriggerStatus && (viveRightController.GetComponent<SteamVR_TrackedController>().triggerPressed))
        {
            Debug.Log ("Right Controller trigger is pressed!");
            if (initializeStep == 1)
            {
                //Read the Vive Controller data to calculate the neck position
                float headYaw = viveCameraEye.transform.localRotation.eulerAngles.y;
                headXo = viveCameraEye.transform.localPosition.x - headWidth * Mathf.Sin(headYaw * Mathf.PI / 180); //Calculate the Neck x Position
                headYo = viveCameraEye.transform.localPosition.y + headHeight * Mathf.Sin(viveCameraEye.transform.rotation.eulerAngles.x * Mathf.PI / 180); //Calculate the Neck y Position;
                headZo = viveCameraEye.transform.localPosition.z - headWidth * Mathf.Cos(headYaw * Mathf.PI / 180); //Calculate the Neck y Position
                headZero = new Vector3(headXo, headYo, headZo);
                calibrationDisplay.SetActive(false);
                //Debug.Log("Great! Now the user can fly");
                initializeStep = 2;
            } else if (initializeStep == 2)
            {
                //Debug.Log("Reseted status.");
                initializeStep = 0;
            }
            viveRightControllerTriggerStatus = true;
        }

        //Check if the user released trigger
        if (viveRightControllerTriggerStatus && !(viveRightController.GetComponent<SteamVR_TrackedController>().triggerPressed))
        {
            Debug.Log ("Right Controller pad is released!");
            viveRightControllerTriggerStatus = false;
        }
        Debug.Log(viveRightControllerTriggerStatus);
    }


	// *** Call this method in update() *** (Uses the Vive HMD & Controller data stored in internal variables to move the player in Virtual Environment)
	void FlyingLocomotion ()
	{

		// ***************************  Calculate  neck displacement  *******************************************
		float headYaw = viveCameraEye.transform.localRotation.eulerAngles.y;	
		float headY = viveCameraEye.transform.localPosition.y + headHeight * Mathf.Sin (viveCameraEye.transform.rotation.eulerAngles.x * Mathf.PI / 180); //Calculate the Neck y Position
		float headX = viveCameraEye.transform.localPosition.x - headWidth * Mathf.Sin (headYaw * Mathf.PI / 180); //Calculate the Neck x Position
		float headZ = viveCameraEye.transform.localPosition.z - headWidth * Mathf.Cos (headYaw * Mathf.PI / 180); //Calculate the Neck y Position

		headCurrent = new Vector3 (headX, headY, headZ);

		// **************************** Calculate polar coordinates of the neck deflection ***********************************************
		float deltaX = (headX - headXo) * speedSensitivity;
		float deltaY = (headY - headYo) * speedSensitivity;
		float deltaZ = (headZ - headZo) * speedSensitivity;
		float radious = Vector3.Distance (headCurrent, headZero) * speedSensitivity; // polar r (radious)
		float Fi = (radious == 0) ? 0 : Mathf.Asin ((float)(deltaY / radious)); // Fi in radian
		float Tetta = (deltaX == 0 && deltaZ == 0) ? 0 : Mathf.Atan2 (deltaZ, deltaX); //Tetta in radian

		// **************************** Apply exponential transfer function ***********************************************
		float radiousExp = Mathf.Pow (radious, exponentialTransferFuntionPower);

		// **************************** Limiting the speed if needed ***********************************************
		if (speedLimit >= 0 && radiousExp > speedLimit)
			radiousExp = speedLimit;
		// **************************** Calculate velocities of of the neck ***********************************************
		float velocityX = radiousExp * Mathf.Cos (Fi) * Mathf.Cos (Tetta);
		float velocityY = radiousExp * Mathf.Sin (Fi);
		float velocityZ = radiousExp * Mathf.Cos (Fi) * Mathf.Sin (Tetta);

		Vector3 velocity = new Vector3 (velocityX, velocityY, velocityZ);

		if (locomotionInterface == Interface.Standing || locomotionInterface == Interface.NormalChair) {
			Vector3 translate = velocity * Time.deltaTime;
			translate.y = (transform.position.y + translate.y < 0) ? 0 : translate.y; // the player should not go beneath the ground

			//  ****************************** Apply velocities to the user position *****************************************
			if (initializeStep == 2) {
				//Debug.Log ("Exp Distance = " + radiousExp + " - velocity = " + velocity.ToString ());

				Vector3 pos = new Vector3 (transform.position.x + translate.x, transform.position.y + translate.y, transform.position.z + translate.z);
				transform.position = pos;
			}
		} else {
			float forwardVelocity = velocity.z * Time.deltaTime;
			Vector3 translate = new Vector3 (forwardVelocity * Mathf.Sin (LocomotionEuler.y * Mathf.PI / 180), velocity.y * Time.deltaTime, forwardVelocity * Mathf.Cos (LocomotionEuler.y * Mathf.PI / 180));

			translate.y = (transform.position.y + translate.y < 0) ? 0 : translate.y; // the player should not go beneath the ground
	
			//Debug.Log (" Velocity: x = " + velocity.x + " - y = " + velocity.y + " - z = " + velocity.z);


			//  ****************************** Apply velocities to the user position *****************************************
			if (initializeStep == 2) {
				//Debug.Log ("Exp Distance = " + radiousExp + " - velocity = " + velocity.ToString ());

				Vector3 pos = new Vector3 (transform.position.x + translate.x, transform.position.y + translate.y, transform.position.z + translate.z);
				transform.position = pos;

				if (speedLimit >= 0) {
					if (velocity.x > speedLimit / 5) {
						velocity.x = speedLimit / 5;
					}

					if (velocity.x < -speedLimit / 5) {
						velocity.x = -speedLimit / 5;
					}
				}
				float adjustedYaw = velocity.x * speedSensitivity / 3 * Time.deltaTime;

				LocomotionEuler.y += adjustedYaw;
				LocomotionQuat.eulerAngles = LocomotionEuler;
				transform.rotation = LocomotionQuat;
			}

		}
	}


    /******************************************************************************************************************************/
    /******                                          save and fetch data across scenes                                        *****/
    /******************************************************************************************************************************/
    public void saveCalibration()
    {
        GlobalControl.Instance.initializeStep = initializeStep;
        GlobalControl.Instance.headZero = headZero;
        GlobalControl.Instance.headCurrent = headCurrent;
        GlobalControl.Instance.headXo = headXo;
        GlobalControl.Instance.headYo = headYo;
        GlobalControl.Instance.headZo = headZo;
    }

    public void fetchCalibration()
    {
        headZero = GlobalControl.Instance.headZero;
        headCurrent = GlobalControl.Instance.headCurrent;
        headXo = GlobalControl.Instance.headXo;
        headYo = GlobalControl.Instance.headYo;
        headZo = GlobalControl.Instance.headZo;
        initializeStep = GlobalControl.Instance.initializeStep;
    }
}
