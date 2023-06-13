using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

[Index(nameof(User.Email),IsUnique=true)]
public class User
{
    [JsonIgnore]
    public int     Id       {get; set;}
    public string  Name     {get; set;}
    public string  Email    {get; set;}
    public string  Password {get; set;}
    
    [DefaultValue(false)]
    [JsonIgnore]
    public bool    IsAdmin  {get; set;}
    
    [JsonIgnore]
    public List<Subscription> Subscriptions {get;} = new();


}