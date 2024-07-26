using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordleManager1 : MonoBehaviour
{
	public List<Word1> words1;

	public WordSpawner1 wordSpawner1;

	private bool hasActiveWord;
	private Word1 activeWord;

	public void AddWord ()
	{
		Word1 word1 = new Word1(WordGenerator.GetRandomWord(), wordSpawner1.SpawnWord());
		// Debug.Log(word.word);

		words1.Add(word1);
	}

	public void TypeLetter (char letter)
	{
		if (hasActiveWord)
		{
			if (activeWord.GetNextLetter() == letter)
			{
				activeWord.TypeLetter();
			}
		} else
		{
			foreach(Word1 word1 in words1)
			{
				if (word1.GetNextLetter() == letter)
				{
					activeWord = word1;
					hasActiveWord = true;
					word1.TypeLetter();
					break;
				}
			}
		}

		if (hasActiveWord && activeWord.WordTyped())
		{
			hasActiveWord = false;
			words1.Remove(activeWord);
		}
	}

}