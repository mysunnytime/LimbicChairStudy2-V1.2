using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalControl : MonoBehaviour
{
    // singleton
    static public GlobalControl Instance;

    public int initializeStep = 0;
    public Vector3 headZero = new Vector3(), headCurrent = new Vector3();
    public float headXo = 0, headYo = 0, headZo = 0;

    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
}