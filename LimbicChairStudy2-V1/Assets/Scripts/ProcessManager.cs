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

    void SetupNewState(ProcessState s)
    {
        switch (s)
        {
            case ProcessState.PREPAREATION_1:
                flying.fetchCalibration();
                flying.speedLimit /= 50;
                tourInfo.SetActive(true);
                timeDisplay.gameObject.SetActive(false);
                chest.gameObject.SetActive(false);
                chestInfo.SetActive(false);
                break;
            case ProcessState.TASK_1:
                if(flying.speedLimit < 1) flying.speedLimit *= 50;
                time = 0;
                tourInfo.SetActive(false);
                chest.gameObject.SetActive(false);
                break;
            case ProcessState.PREPAREATION_2:
                chestInfo.SetActive(true);
                break;
            case ProcessState.TASK_2:
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
        WriteString("\nParticipant ID: " + ParticipantID + "\tDate: " + System.DateTime.Now);
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

}
