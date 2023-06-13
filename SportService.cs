using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
[Index(nameof(SportService.Name), IsUnique=true)]
public class SportService
{
    public int         Id     {get; set;}
    public string?     Name   {get; set;}
    public int         Period {get; set;}
    public double      Price  {get; set;}
    public ServiceTime Time   {get; set;}

    [JsonIgnore]
    public List<Subscription> Subscriptions {get;} = new();    
}
public enum ServiceTime
{
    NOT_SPECIFIED,
    MORNING
}
