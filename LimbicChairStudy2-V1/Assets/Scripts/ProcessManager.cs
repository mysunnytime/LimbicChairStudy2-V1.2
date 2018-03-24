using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using UnityEditor;
using System.IO;
using UnityEngine.UI;

public class ProcessManager : MonoBehaviour
{
    public enum ProcessState
    {
        PREPAREATION_1,
        TASK_1, // take two pictures
        PREPAREATION_2,
        TASK_2, // find the treasure chest
        END,
    }

    public ProcessState state = ProcessState.PREPAREATION_1;
    float time = 0.0f;
    public string ParticipantID = "____";
    public ChestControl chest;
    public Flying flying;
    public Text stateDisplay;
    public Text timeDisplay;
    public GameObject tourInfo;
    public GameObject chestInfo;
    public GameObject cameraPreviewObj;
    public RenderTexture cameraPreview;
    public Text cameraInfo;
    int pictureID = 1;

    //public Text debugInfo;

    void SetupNewState(ProcessState s)
    {
        switch (s)
        {
            case ProcessState.PREPAREATION_1:
                WriteString("\nParticipant ID: " + ParticipantID + "\tDate: " + System.DateTime.Now);
                //Debug.Log("Setting up preparation 1"); debugInfo.text = "Setting up preparation 1";
                if (GlobalControl.Instance != null) flying.fetchCalibration();
                flying.speedLimit /= 50;
                tourInfo.SetActive(true);
                timeDisplay.gameObject.SetActive(false);
                chest.gameObject.SetActive(false);
                chestInfo.SetActive(false);
                cameraInfo.gameObject.SetActive(false);
                cameraPreviewObj.SetActive(false);
                break;
            case ProcessState.TASK_1:
                //Debug.Log("Setting up task 1"); debugInfo.text = "Setting up task 1";
                if (flying.speedLimit < 1) flying.speedLimit *= 50;
                time = 0;
                tourInfo.SetActive(false);
                chest.gameObject.SetActive(false);
                cameraInfo.gameObject.SetActive(true);
                cameraPreviewObj.SetActive(true);
                break;
            case ProcessState.PREPAREATION_2:
                //Debug.Log("Setting up preparation 2"); debugInfo.text = "Setting up preparation 2";
                chestInfo.SetActive(true);
                cameraInfo.gameObject.SetActive(false);
                cameraPreviewObj.SetActive(false);
                break;
            case ProcessState.TASK_2:
                //Debug.Log("Setting up task 2"); debugInfo.text = "Setting up task 2";
                time = 0;
                chestInfo.SetActive(false);
                timeDisplay.gameObject.SetActive(true);
                chest.gameObject.SetActive(true);
                chest.Open();
                chest.transform.position = chest.position.position;
                chest.transform.rotation = chest.position.rotation;
                break;
        }

    }

    void Start()
    {
        SetupNewState(ProcessState.PREPAREATION_1);
    }

    void Update()
    {
        Debug.Log("state: " + state);
        stateDisplay.text = state.ToString();

        // reset to task one or task two
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetupNewState(ProcessState.PREPAREATION_1);
            state = ProcessState.PREPAREATION_1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetupNewState(ProcessState.PREPAREATION_2);
            state = ProcessState.PREPAREATION_2;
        }

        // transition between states
        if (state == ProcessState.PREPAREATION_1) {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SetupNewState(ProcessState.TASK_1);
                state = ProcessState.TASK_1;
            }
        }
        else if (state == ProcessState.TASK_1)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SetupNewState(ProcessState.PREPAREATION_2);
                state = ProcessState.PREPAREATION_2;
            }

            switch (pictureID) {
                case 1:
                    cameraInfo.text = "Take your first picture";
                    break;
                case 2:
                    cameraInfo.text = "Take your second picture";
                    break;
                default:
                    cameraInfo.gameObject.SetActive(false);
                    cameraPreviewObj.SetActive(false);
                    return;
            }

            //Check if the user pressed trigger
            if (!flying.viveLeftControllerTriggerStatus && (flying.viveLeftController.GetComponent<SteamVR_TrackedController>().triggerPressed))
            {
                TakePicture();
                flying.viveLeftControllerTriggerStatus = true;
            }

            //Check if the user released trigger
            if (flying.viveLeftControllerTriggerStatus && !(flying.viveLeftController.GetComponent<SteamVR_TrackedController>().triggerPressed || flying.viveRightController.GetComponent<SteamVR_TrackedController>().triggerPressed))
            {
                //Debug.Log ("Right Controller pad is released!");
                flying.viveLeftControllerTriggerStatus = false;
            }
        }
        else if (state == ProcessState.PREPAREATION_2)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SetupNewState(ProcessState.TASK_2);
                state = ProcessState.TASK_2;
            }
        }
        else if (state == ProcessState.TASK_2)
        {
            time += Time.deltaTime;
            timeDisplay.text = StringfyTime(180 - (int)time);
            if (chest.foundTrigger)
            {
                WriteString("time to find the chest: " + (int)time + " s.");
                state = ProcessState.END;
                chest.foundTrigger = false;
            }
            else if(time > 180)
            {
                WriteString("did not find the chest in " + (int)time + " s.");
                chest.gameObject.SetActive(false);
                state = ProcessState.END;
            }
        }
    }

    void OnApplicationQuit()
    {
        WriteString("\n\n");
    }

    void WriteString (string s)
	{
		string path = "Data/Data.txt";
		StreamWriter writer = new StreamWriter (path, true);
		writer.WriteLine (s);
		writer.Close ();
    }

    string StringfyTime(float time)
    {
        string minute = "" + (int)time / 60;
        string second = "" + (int)time % 60;
        if ((int)time / 60 < 10) minute = "0" + minute;
        if ((int)time % 60 < 10) second = "0" + second;
        return minute + ":" + second;
    }

    void TakePicture() {
        //Debug.Log("taking picture..."); debugInfo.text = "taking picture...";
        Texture2D tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);

        // Read screen contents into the texture
        RenderTexture.active = cameraPreview;
        tex.ReadPixels(new Rect(0, 0, cameraPreview.width, cameraPreview.height), 0, 0);
        tex.Apply();

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Object.Destroy(tex);
        File.WriteAllBytes("Data/" + System.DateTime.Now.ToString("MMddHHmmss-") + "0" + pictureID + ".png", bytes);
        pictureID++;
    }

}
