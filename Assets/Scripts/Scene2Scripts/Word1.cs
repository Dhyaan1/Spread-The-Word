using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Word1 {

	public string word1;
	private int typeIndex;

WordDisplay2 display2;
bool isTagAdded = false; // Add this line

public Word1 (string _word1, WordDisplay2 _display2)
{
    word1 = _word1;
    typeIndex = 0;

    display2 = _display2;
    display2.SetWord(word1);
}

public char GetNextLetter ()
{
    return word1[typeIndex];
}

public void TypeLetter ()
{
    typeIndex++;
    if (!isTagAdded) // Add this line
    {
        display2.AddTag();
        isTagAdded = true; // Add this line
    }
    display2.RemoveLetter();
}

public bool WordTyped ()
{
    bool wordTyped = typeIndex >= word1.Length;
    if (wordTyped)
    {
        display2.RemoveWord();
    }
    return wordTyped;
}

}