using System.Collections;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

////////////////////////////////////////////////////////////////////////////////////////////////////
builder.Services.AddDbContext<SportDBContext>(opt => opt.UseInMemoryDatabase("sport_club"));
////////////////////////////////////////////////////////////////////////////////////////////////////

// builder.Services.AddDbContext<SportDBContext>( options => options.UseSqlServer(new SqlConnectionStringBuilder()
// {
//     DataSource = builder.Configuration["Db:.DataSource"],
//     UserID     = builder.Configuration["Db:.UserID"    ],
//     Password   = builder.Configuration["Db:.Password"  ]
// }.ConnectionString));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer              = builder.Configuration["Jwt:Issuer"  ],
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

////////////////////////////INITIALIZATION//////////////////////////
var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<SportDBContext>();
if(!db.Initialized)
    db.Initialize();
////////////////////////////INITIALIZATION//////////////////////////

app.MapGet("/services", [AllowAnonymous] async (SportDBContext db) => await db.Services.ToArrayAsync());

app.MapGet("/services/{id}", [AllowAnonymous] async (int id, SportDBContext db) => await db.Services.Where((s) => s.Id == id).ToArrayAsync());

app.MapPost("/services", async (ClaimsPrincipal principal, SportService service, SportDBContext db) => 
{
    var isAdmin = principal.Claims.Where((claim) => claim.Type == "IsAdmin").FirstOrDefault()?.Value;

    if(isAdmin is not null && Boolean.Parse(isAdmin))
    {
        db.Services.Add(service);
        await db.SaveChangesAsync();
        return Results.Created($"/services/{service.Id}", service);
    }
    return Results.Unauthorized();
}).RequireAuthorization();

app.MapPut("/services/{id}", async (int id, ClaimsPrincipal principal, SportService inService, SportDBContext db) =>
{
    var isAdmin = principal.Claims.Where((claim) => claim.Type == "IsAdmin").FirstOrDefault()?.Value;

    if(isAdmin is not null && Boolean.Parse(isAdmin))
    {
        db.Services.Add(inService);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.Unauthorized();
}).RequireAuthorization();;

app.MapDelete("/services/{id}", async (int id, ClaimsPrincipal principal, SportDBContext db) =>
{
    var isAdmin = principal.Claims.Where((claim) => claim.Type == "IsAdmin").FirstOrDefault()?.Value;

    if(isAdmin is not null && Boolean.Parse(isAdmin))
    {
        if (await db.Services.FindAsync(id) is SportService todo)
        {
            db.Services.Remove(todo);
            await db.SaveChangesAsync();
            return Results.Ok(todo);
        }
        return Results.NotFound();
    }
    return Results.Unauthorized();
}).RequireAuthorization();

app.MapPost("/user", [AllowAnonymous] async (User user, SportDBContext db) =>
{
    if(db.Users.Any((u) => u.Email.Equals(user.Email)))
    {
        return Results.Conflict("Account already exists with this email, try to login");
    }

    user.IsAdmin = false;

    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(new {user.Id, user.Name, user.Email});
});

app.MapPost("/login",[AllowAnonymous] (User user, SportDBContext db) =>
{
    if (db.Users.Where((u) => u.Email.Equals(user.Email) && u.Password.Equals(user.Password)).FirstOrDefault() is User dbUser)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var token        = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("Id"                         , dbUser.Id.ToString()),
                new Claim("IsAdmin"                    , dbUser.IsAdmin.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, dbUser.Email),
                new Claim(JwtRegisteredClaimNames.Jti  , Guid.NewGuid().ToString())
             }),
            Expires            = DateTime.UtcNow.AddMonths(1),
            Issuer             = builder.Configuration["Jwt:Issuer"],
            Audience           = builder.Configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials (new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"])),
            SecurityAlgorithms.HmacSha512Signature)
        });
        
        return Results.Ok(tokenHandler.WriteToken(token));
    }
    return Results.Unauthorized();
});

app.MapPost("/purchase/{serviceId}/{resourcePath}", async (string resourcePath, int serviceId, ClaimsPrincipal principal, SportDBContext db) => 
{
    if(principal.Claims.Where((c) => c.Type == "Id").FirstOrDefault()?.Value is String id)
    {
        if (await db.Services.FindAsync(serviceId) is SportService service)
        {
            if(await db.Users.FindAsync(id) is User user)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {db.Token}");
            var response = await client.GetAsync(Path.Combine("https://eu-test.oppwa.com", resourcePath));
            if(response.IsSuccessStatusCode)
            {
                if(await response.Content.ReadFromJsonAsync<ExpandoObject>() is ExpandoObject result)
                {
                    //////////////////////////////////  Need to be reviewed /////////////////////////////////
                    if("000.000.100".Equals(Convert.ToString(((dynamic)result).code)))
                    {
                        IDictionary<String, Object> d = (IDictionary<String, Object>) result;
                        Object records;
                        if(d.TryGetValue("records", out records))
                        {
                            foreach(var record in (IList<dynamic>) records)
                            {
                                if(record.amount == service.Price)
                                    await db.Subscriptions.AddAsync(new Subscription(){
                                        User      = user,
                                        Service   = service,
                                        UserId    = user.Id,
                                        ServiceId = serviceId,
                                    }.PaidAt(DateTime.Now));
                            }
                            await db.SaveChangesAsync();
                            return Results.Ok();
                        }
                    }
                    return Results.NotFound();
                    //////////////////////////////////  Need to be reviewed /////////////////////////////////
                }

            }
            return Results.Problem();
        }
        return Results.Unauthorized();
        }
        return Results.NotFound(); 
    }
    return Results.Unauthorized();
}).RequireAuthorization();

app.MapGet("/subscirptions", async (ClaimsPrincipal principal, SportDBContext db) => 
{
    string? id = null;
    foreach(var claim in principal.Claims)
    {
        if(claim.Type == "IsAdmin")
        {
            if(Boolean.Parse(claim.Value))
            {
                return Results.Ok(await db.Subscriptions.Select((sub) => new{sub.Id, sub.ExpirationDate, sub.IsExpired, sub.ServiceId, sub.UserId}).ToArrayAsync());
            }
        }
        else if(claim.Type == "Id")
        {
            id = claim.Value;
        }
    }

    if(id is not null && await db.Users.FindAsync(int.Parse(id)) is User user)
    {
        return Results.Ok(user.Subscriptions.Select((sub) => new{sub.Id, sub.ExpirationDate, sub.IsExpired, sub.Service.Name}).ToArray());
    }
    return Results.NotFound();
}).RequireAuthorization();

app.MapGet("/subscirptions/{id}", async (int sid, ClaimsPrincipal principal, SportDBContext db) => 
{
    string? uid = null;
    foreach(var claim in principal.Claims)
    {
        if(claim.Type == "IsAdmin")
        {
            if(Boolean.Parse(claim.Value))
            {
                return Results.Ok(await db.Subscriptions.Where((sub) => sub.Id == sid).Select((sub) => new{sub.Id, sub.ExpirationDate, sub.IsExpired, sub.ServiceId, sub.UserId}).ToArrayAsync());
            }
        }
        else if(claim.Type == "Id")
        {
            uid = claim.Value;
        }
    }

    if(uid is not null && await db.Users.FindAsync(int.Parse(uid)) is User user)
    {
        return Results.Ok(user.Subscriptions.Where((sub) => sub.Id == sid).Select((sub) => new{sub.Id, sub.ExpirationDate, sub.IsExpired, sub.Service.Name}).ToArray());
    }
    return Results.NotFound();
}).RequireAuthorization();

app.UseAuthentication();
app.UseAuthorization();
app.Run();
