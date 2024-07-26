using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Threading.Tasks;
using Postgrest.Models;
using Postgrest.Attributes;
using com.example;
using UnityEngine.UI;
using Unity.Netcode;

[Table ("game")]

public class Game : BaseModel
{
    [PrimaryKey("id", false)]
    public int id { get; set; }
    [Column("game_lobby_id")]
    public string GameLobbyId { get; set; }   

    [Column("host_name")]
    public string HostName { get; set; }

    [Column("game_winner")]
    public string GameWinner { get; set; }
}

[Table("words")]
public class Words : BaseModel
{
    [PrimaryKey("id", false)]
    public int id { get; set; }
    [Column("word")]
    public string Word { get; set; }
}
[Table ("games_played")]
public class GamesPlayed : BaseModel
{
    [PrimaryKey("id", false)]
    public int id { get; set; }

    [Column("game_id")]
    public string GameID { get; set; }

    [Column("answer")]
    public string Answer { get; set; }
}

[Table ("winners")]
public class Winner : BaseModel
{
    [PrimaryKey("id", false)]
    public int id { get; set; }
    [Column("game_id")]
    public string GameID { get; set; }
    [Column("username")]
    public string Username { get; set; }
}
[Table ("winning_attempt")]
public class WinningAttempt : BaseModel
{
    [PrimaryKey("id", false)]
    public int id { get; set; }

    [Column("username")]
    public string Username { get; set; }
    [Column("attempt")]
    public int Attempt { get; set; }
}
[Table ("word_score")]
public class WordScore : BaseModel
{
    [PrimaryKey("id", false)]
    public int id { get; set; }

    [Column("username")]
    public string Username { get; set; }
    [Column("guess")]
    public string Guess { get; set; }
    [Column("answer")]
    public string Answer { get; set; }
    [Column("score")]
    public float Score { get; set;}
}
public class Qspwaner : NetworkBehaviour
{
    [SerializeField] private Button StartWordGameButton;
    [SerializeField] private GameObject UICover;
    [SerializeField] private TextMeshProUGUI GameWinnerText;
    [SerializeField] private TextMeshProUGUI PlayerNameText;
    private bool GameState = false;

    [SerializeField] private GameObject wordPrefab;
    [SerializeField] private TMP_InputField WordGuessInputField;
    [SerializeField] private SupabaseManager SupabaseManager = null!;
    public static string playerName = SignUpAndLogin.playerName;
    public static string hostName = AllManager.hostName;
    public static string WordGameWinner;
    private bool gotWordGameWinner = false;
    public static string GameLobbyId = AllManager.joinedLobbyID;
    public string AnswerWord;
    private float xOffset = 1.2f;
    private float yOffset = 1.5f;
    private int wordCount = 5;
    public int rowNumber = 0;
    private static List<string> wordList = new(); 
    
    void ValidateInput(string input)
    {
        // Remove the listener to avoid infinite loop
        WordGuessInputField.onValueChanged.RemoveListener(ValidateInput);

        // Convert input to uppercase
        input = input.ToUpper();

        // Remove any non-alphabetic characters
        string onlyAlphabets = System.Text.RegularExpressions.Regex.Replace(input, @"[^a-zA-Z]", "");

        // Check if the input is a single word of no more than 5 characters
        if (onlyAlphabets.Length <= 5)
        {
            WordGuessInputField.text = onlyAlphabets;
        }
        else
        {
            // If the input is more than 5 characters, keep only the first 5
            WordGuessInputField.text = onlyAlphabets[..5];
        }

        // Add the listener back
        WordGuessInputField.onValueChanged.AddListener(ValidateInput);
    }

    private async Task FetchWords()
    {
        var words = await SupabaseManager.Supabase().From<Words>().Get();
        Debug.Log(words);
        var ListOfWords = words.Models;
        Debug.Log(ListOfWords);
        foreach (var word in ListOfWords)
        {
            // Debug.Log(word.Word);
            wordList.Add(word.Word);
        }
    }

    public static string GetRandomWord ()
        {
            int randomIndex = Random.Range(0, wordList.Count);
            string randomWord = wordList[randomIndex];

            return randomWord;
        }
    public void SpawnLetters (int _rowNumber)
    {
        string UserWord = WordGuessInputField.text;

        for (int i = 0; i < wordCount; i++)
        {
            Vector3 position = new(transform.position.x + i * xOffset, transform.position.y - (_rowNumber * yOffset), transform.position.z);
            GameObject wordObj = Instantiate(wordPrefab, position, Quaternion.identity);
            TextMeshProUGUI textMesh = wordObj.GetComponentInChildren<TextMeshProUGUI>();

            // Check if the TextMeshPro component was found and if the user word is not empty
            if (textMesh != null && i < UserWord.Length)
            {
                textMesh.text = UserWord[i].ToString();
                ChangePrefabColor(wordObj, UserWord, AnswerWord, i);
            }
            else
            {
                Debug.LogError("TextMeshProUGUI component not found in prefab children");
            }
        }
    }

    public void ChangePrefabColor(GameObject prefab, string userWord, string answerWord, int index)
    {
        if (prefab.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            // Default color is red
            Color newColor = Color.red;

            // If the letter is in the answer word
            if (answerWord.Contains(userWord[index]))
            {
                // If the letter is at the same position in the answer word
                if (answerWord[index] == userWord[index])
                {
                    newColor = Color.green;
                }
                else
                {
                    newColor = Color.yellow;
                }
            }

            // Set the color of the SpriteRenderer component
            spriteRenderer.color = newColor;

            // If the Light2D component is found in the children, set its color
            Light2D light2D = prefab.GetComponentInChildren<Light2D>();
            if (light2D != null)
            {
                light2D.color = newColor;
            }
            else
            {
                Debug.LogError("Light2D component not found in prefab or its children");
            }
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found in prefab");
        }
    }
    
    private async void InsertWinningAttempt(int row_number, string player_Name)
    {
        var data = new WinningAttempt
        {
            Username = player_Name,
            Attempt = row_number+1,
        };

        await SupabaseManager.Supabase()!.From<WinningAttempt>().Insert(data);
    }

    public async void InsertGame(string Game_Lobby_Id, string host_Name)
    {
        var data = new Game
        {
            GameLobbyId = Game_Lobby_Id,
            HostName = host_Name,
            GameWinner = "" //need to keep it empty instead of null otherwise trigger doesn't work
        };
        await SupabaseManager.Supabase()!.From<Game>().Insert(data);
    }
    
    private async void UpdateGameWinner(string Game_Lobby_Id, string Game_Winner, string Host_Name)
    {
        Debug.Log("Updating the game winner");
        var update = await SupabaseManager.Supabase()!
        .From<Game>()
        .Where(x=> x.GameLobbyId == Game_Lobby_Id)
        .Where(x=>x.HostName == Host_Name)
        .Set(x=> x.GameWinner, Game_Winner)
        .Update();
    }

    private void ValidateWord(string word)
    {
        if(word == AnswerWord)
        {
            Debug.Log("You have guessed the word correctly");
            InsertWinningAttempt(rowNumber, playerName);
            // Actual winner Name got through the game network
            GameWinnerRpc(playerName);
            if(gotWordGameWinner)
            {
                EndTheWordGame();
            }
        }
        else
        {
            Debug.Log("You have guessed the word incorrectly");
        }
        Debug.Log("Score: " + GetGuessScore(word));
    }

    private float GetGuessScore(string word)
    {
        float score = 0f;
        for(int i=0; i<word.Length; i++)
        {
            if(AnswerWord[i] == word[i])
            {
                score++;
            }
            else if(AnswerWord.Contains(word[i]))
            {
                score +=  0.5f;
            }
        }
        return score;
    }

    private void Awake()
    {
        WordGuessInputField.onValueChanged.AddListener(ValidateInput);
        GameWinnerText.gameObject.SetActive(false);
        PlayerNameText.text = playerName;
        if(hostName == playerName) //replace with the Host and Player Ids
        {
            StartWordGameButton.onClick.AddListener(async () => await StartTheWordGame());
        }
        else
        {
            StartWordGameButton.gameObject.SetActive(false);
        }
    }
    

    private async void InsertWords(string GuessedWord, float word_score, string AnswerWord, string player_Name)
    {
        Debug.Log("Inserting words into the database");
        var data = new WordScore
        {
            Username = player_Name,
            Guess = GuessedWord,
            Answer = AnswerWord,
            Score = word_score,
        };
        await SupabaseManager.Supabase()!.From<WordScore>().Insert(data);
        Debug.Log(data);
    }

    private async Task StartTheWordGame()
    {
        Debug.Log("Starting the word game");
        await FetchWords();
        AnswerWord = GetRandomWord().ToUpper();
        Debug.Log(AnswerWord);
        UICover.SetActive(false);
        GameState = true;
        Debug.Log("Game Lobby ID: " + GameLobbyId);
        Debug.Log("Host Name: " + hostName);
        if(hostName == playerName) //replace with the Host and Player Ids
        {
            InsertGame(GameLobbyId, hostName);
            StartWordGameButton.gameObject.SetActive(false);
            HostHasStartedGameRpc();
        }
    }

    [Rpc (SendTo.NotServer)]
    public void HostHasStartedGameRpc()
    {
        StartCoroutine(StartTheWordGameCoroutine());
    }

    private IEnumerator StartTheWordGameCoroutine()
    {
        // Call StartTheWordGame once and store the resulting Task
        var task = StartTheWordGame();

        // Wait until the task is completed
        yield return new WaitUntil(() => task.IsCompleted);;
    }

    [Rpc (SendTo.Server)]
    public void GameWinnerRpc(string player_Name)
    {
        if(!gotWordGameWinner)
        {
            WordGameWinner = player_Name;
            gotWordGameWinner = true;
            UpdateGameWinner(GameLobbyId, WordGameWinner, hostName);
            TellEveryoneTheWinnerRpc(WordGameWinner);
        }
    }

    [Rpc (SendTo.ClientsAndHost)]
    public void TellEveryoneTheWinnerRpc(string winner_Name)
    {
        WordGameWinner = winner_Name;
        gotWordGameWinner = true;

        if(winner_Name == playerName) //for the player that won the game
        {
            EndTheWordGame();
        }
    }

    private void EndTheWordGame()
    {
        WordGuessInputField.gameObject.SetActive(false);
        GameState = false;
        UICover.SetActive(true);
        GameWinnerText.gameObject.SetActive(true);
        GameWinnerText.text = "The winner is: " + WordGameWinner;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if(WordGuessInputField.text.Length < 5)
            {
                Debug.Log("Word is less than 5 characters");
                return;
            }
            if(!GameState)
            {
                return;
            }
            if(rowNumber ==5)
            {
                EndTheWordGame();
            }
            SpawnLetters(rowNumber);
            ValidateWord(WordGuessInputField.text);
            if(rowNumber == 0)
            {
                InsertWords(WordGuessInputField.text, GetGuessScore(WordGuessInputField.text), AnswerWord, playerName);
            }
            WordGuessInputField.text = ""; 
            rowNumber++;
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key was pressed");
        }
    }
}
