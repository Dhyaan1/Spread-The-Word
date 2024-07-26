using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WordDisplay2 : MonoBehaviour {

	public TextMeshProUGUI text;
	public float fallSpeed = 0.7f;

	public void SetWord (string word)
	{
		text.text = word;
	}

	public void RemoveLetter ()
	{
		text.text = text.text.Remove(0, 1);
		text.color = Color.red;
	}

	//write a function to add tag of "player" to the word
	public void AddTag ()
	{
		gameObject.tag = "Player";
        Debug.Log("Tag is "+text.tag);
	}

	public void RemoveWord ()
	{
		Destroy(gameObject);
        //give me a debug log of the tag of the word
        
	}

	private void Update()
	{
		transform.Translate(0f, -fallSpeed * Time.deltaTime, 0f);
	}

}