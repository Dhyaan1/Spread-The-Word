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
using Supabase.Gotrue;
using UnityEngine.SceneManagement;

[Table("users")]
public class Users : BaseModel
{
    [PrimaryKey("id", false)]
    public int id { get; set; }
    [Column("name")]
    public string Name { get; set; }

    [PrimaryKey("wins", false)]
    public string Wins { get; set; }
}
public class SignUpAndLogin : MonoBehaviour
{

    [SerializeField] private SupabaseManager SupabaseManager = null!;
    [SerializeField] private Button LogInButton;
    [SerializeField] private TMP_InputField UserNameInputField;
    private string NextSceneName= "LobbyScene";

    public static string playerName;
    private bool Authenticated = false;

    private void Awake()
    {
        LogInButton.onClick.AddListener(() => 
        { 
            if (UserNameInputField.text != "")
            {
                LogInPageButtonClicked(UserNameInputField.text);
            }
        });
    }

    private async void LogInPageButtonClicked(string text)
    {
        await DoLogin(text);
        // After login, load the next scene
        if (Authenticated)
        {
            Debug.Log("Next Scene");
            SceneManager.LoadScene(NextSceneName);
        }
        else
        {
            Debug.Log("User not authenticated");
        }
    }

    private async Task DoLogin(string User_Name)
    {
        try
        {
            var data = new Users
            {
                Name = User_Name
            };
            Debug.Log("User Being Added");
            await SupabaseManager.Supabase()!.From<Users>().Insert(data);
            Debug.Log("User added successfully");
            playerName = User_Name;
            Debug.Log("Player Name: " + playerName);
            Authenticated = true;
            Debug.Log("Authenticated: " + Authenticated);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            if (e.Message.Contains("23505"))
            {
                Authenticated = true;
                Debug.Log("Authenticated: " + Authenticated);
                playerName = User_Name;
                Debug.Log("Player Name: " + playerName);
            }
        }
    }
}
