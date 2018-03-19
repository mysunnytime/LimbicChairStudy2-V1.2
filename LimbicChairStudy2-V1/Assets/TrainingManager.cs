using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrainingManager : MonoBehaviour {
    public enum Section
    {
        SECTION_1,
        SECTION_2,
        SECTION_3
    };

    public enum TrainingState {
        TRAINING,
        CONFIRNMING // comfirm to move to the real experiment scene
    }

    public Section section;
    public Text stateDisplay;
    TrainingState state;

	// Use this for initialization
	void Start () {
        state = TrainingState.TRAINING;
	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log(state);
        stateDisplay.text = state.ToString();
		if (Input.GetKeyDown("space"))
        {
            if(state == TrainingState.TRAINING)
            {
                Debug.Log("Sure to go to next scene?");
                state = TrainingState.CONFIRNMING;
            }
            else if(state == TrainingState.CONFIRNMING)
            {
                Debug.Log("transfer to new scene.");
                switch (section)
                {
                    case Section.SECTION_1:
                        SceneManager.LoadScene("SnowMountain", LoadSceneMode.Single);
                        break;
                    case Section.SECTION_2:
                        SceneManager.LoadScene("MountainLake", LoadSceneMode.Single);
                        break;
                    case Section.SECTION_3:
                        SceneManager.LoadScene("Dezert", LoadSceneMode.Single);
                        break;
                }
            }
        }
	}
}
