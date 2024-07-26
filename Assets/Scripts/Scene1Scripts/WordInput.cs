using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordInput : MonoBehaviour {

	public WordleManager wordleManager;

	// Update is called once per frame
	void Update () {
		foreach (char letter in Input.inputString)
		{
			wordleManager.TypeLetter(letter);
			// Debug.Log(letter);
		}
	}

}