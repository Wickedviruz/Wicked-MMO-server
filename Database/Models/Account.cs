using System.Data.Common;

namespace GameCore.Database.Models;

// account model 
public class Account
{
    public int Id {get;set;}
    public string Username {get;set;} ="";
    public string PasswordHash {get;set;} ="";
    public string? Email {get;set;} ="";
    public DateTime CreatedAt {get;set;}
    public DateTime? LastLogin {get;set;} 
}