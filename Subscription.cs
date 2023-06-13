using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Subscription
{
    [JsonIgnore]
    public int          Id             {get; set;}
    public DateTime     ExpirationDate {get; set;}

    [JsonIgnore]
    public int          UserId         {get; set;}
    [JsonIgnore]
    public int          ServiceId      {get; set;}
    public User         User           {get; set;} = null!;
    public SportService Service        {get; set;} = null!;

    [NotMapped]
    public bool         IsExpired      {get => DateTime.Now < ExpirationDate;}

    public Subscription PaidAt(DateTime date)
    {
        ExpirationDate = date;
        ExpirationDate.AddDays(Service.Period);
        return this;
    }


}