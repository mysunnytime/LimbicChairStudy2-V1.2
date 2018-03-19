using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestControl : MonoBehaviour
{
	public bool found = false;
	public bool foundTrigger = false;
	public Collider player;
	public GameObject chest_open;
	public GameObject chest_closed;
	public Transform position;

	// Use this for initialization
	void Start ()
	{
        Close();
	}
	
	// Update is called once per frame
	void Update ()
	{
		
		if ((!found && chest_closed.activeSelf) || (found && chest_open.activeSelf))
			return;
		else {
			ToggleOpeness ();
		}
	}

	void OnTriggerEnter (Collider other)
	{
		if (other == player) {
			found = true;
			foundTrigger = true;
		}
	}

	public void Open ()
	{
		chest_open.SetActive (true);
		chest_closed.SetActive (false);
	}

	public void Close ()
	{
		chest_open.SetActive (false);
		chest_closed.SetActive (true);
	}

	public void ToggleOpeness ()
	{
		chest_open.SetActive (!chest_open.activeSelf);
		chest_closed.SetActive (!chest_closed.activeSelf);
	}
}
