// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Postgrest.Models;
// using Postgrest.Attributes;

// [Table("game")]

// public class Game : BaseModel
// {
//     [PrimaryKey("id", false)]
//     public int id { get; set; }
//     [Column("game_lobby_id")]
//     public string GameLobbyId { get; set; }

//     [Column("host_name")]
//     public string HostName { get; set; }

//     [Column("game_winner")]
//     public string GameWinner { get; set; }
// }

// [Table("words")]
// public class Words : BaseModel
// {
//     [PrimaryKey("id", false)]
//     public int id { get; set; }
//     [Column("word")]
//     public string Word { get; set; }
// }
// [Table("games_played")]
// public class GamesPlayed : BaseModel
// {
//     [PrimaryKey("id", false)]
//     public int id { get; set; }

//     [Column("game_id")]
//     public string GameID { get; set; }

//     [Column("answer")]
//     public string Answer { get; set; }
// }

// [Table("winners")]
// public class Winner : BaseModel
// {
//     [PrimaryKey("id", false)]
//     public int id { get; set; }
//     [Column("game_id")]
//     public string GameID { get; set; }
//     [Column("username")]
//     public string Username { get; set; }
// }
// [Table("winning_attempt")]
// public class WinningAttempt : BaseModel
// {
//     [PrimaryKey("id", false)]
//     public int id { get; set; }

//     [Column("username")]
//     public string Username { get; set; }
//     [Column("attempt")]
//     public int Attempt { get; set; }
// }
// [Table("word_score")]
// public class WordScore : BaseModel
// {
//     [PrimaryKey("id", false)]
//     public int id { get; set; }

//     [Column("username")]
//     public string Username { get; set; }
//     [Column("guess")]
//     public string Guess { get; set; }
//     [Column("answer")]
//     public string Answer { get; set; }
//     [Column("score")]
//     public float Score { get; set; }
// }