using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordInput1 : MonoBehaviour {


	public GameObject MissilePrefab;
	public WordleManager1 wordleManager1;
	public Transform wordCanvas;

	// Update is called once per frame
	void Update () {
		foreach (char letter in Input.inputString)
		{

			wordleManager1.TypeLetter(letter);
			// Debug.Log(letter);
		Vector3 MrandomPosition = new Vector3(Random.Range(-7.5f, 7.5f), Random.Range(-7.5f, 7.5f));

		// GameObject MissilePrefab = Instantiate(MissilePrefab, MrandomPosition, Quaternion.identity, wordCanvas);
		Instantiate(MissilePrefab, MrandomPosition, Quaternion.identity, wordCanvas);
		}
	}

}