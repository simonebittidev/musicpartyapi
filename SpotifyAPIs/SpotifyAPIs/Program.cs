using System.Net.Http;
using Google.Api;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPIs.Options;
using SpotifyAPIs.Provider;
using static SpotifyAPI.Web.Scopes;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173");
                      });
});


var config = builder.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
FirebaseOptions experienceSettingsTable = config?.GetSection("FirebaseAuth").Get<FirebaseOptions>();


builder.Services.AddSingleton(_ => new FirestoreProvider(
    new FirestoreDbBuilder
    {
        ProjectId = "spotifymusicparty",
        JsonCredentials = JsonConvert.SerializeObject(experienceSettingsTable) // <-- service account json file
    }.Build()
));

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

