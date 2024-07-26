using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordSpawner1 : MonoBehaviour {

	public GameObject wordPrefab;

	// public GameObject MissilePrefab;
	public Transform wordCanvas;

	public WordDisplay2 SpawnWord ()
	{
		Vector3 randomPosition = new Vector3(Random.Range(-2.5f, 2.5f), 7f);
		Vector3 MrandomPosition = new Vector3(Random.Range(-7.5f, 7.5f), Random.Range(-7.5f, 7.5f));

		GameObject wordObj = Instantiate(wordPrefab, randomPosition, Quaternion.identity, wordCanvas);
		// GameObject MissilePrefab = Instantiate(MissilePrefab, MrandomPosition, Quaternion.identity, wordCanvas);
		// GameObject missile = Instantiate(MissilePrefab, MrandomPosition, Quaternion.identity, wordCanvas);
		WordDisplay2 wordDisplay2 = wordObj.GetComponent<WordDisplay2>();

		return wordDisplay2;
	}

}