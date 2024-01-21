using Google.Api;
using Google.Cloud.Firestore;
using Newtonsoft.Json;
using SpotifyAPIs.Options;
using SpotifyAPIs.Provider;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173");
                          policy.WithOrigins("http://localhost:3000");
                      });
});

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));


var config = builder.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();

FirestoreOptions? experienceSettingsTable = config?.GetSection("FirestoreAuth")?.Get<FirestoreOptions>();

var json = JsonConvert.SerializeObject(experienceSettingsTable);
builder.Services.AddSingleton(_ => new FirestoreProvider(
    new FirestoreDbBuilder
    {
        ProjectId = "spotifymusicparty",
        JsonCredentials = json // <-- service account json file
    }.Build()
));


//Important step for In-Memory Caching
builder.Services.AddEasyCaching(options =>
{
    // use memory cache with a simple way
    options.UseInMemory("inmemory");
});

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseRateLimiter();

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

