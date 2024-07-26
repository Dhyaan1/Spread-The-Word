using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordTimer1 : MonoBehaviour {

	public WordleManager1 wordManager1;

	public float wordDelay = 1.5f;
	private float nextWordTime = 0f;

	private void Update()
	{
		if (Time.time >= nextWordTime)
		{
			wordManager1.AddWord();
			nextWordTime = Time.time + wordDelay;
			wordDelay *= .99f;
		}
	}

}