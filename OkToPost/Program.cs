using Microsoft.EntityFrameworkCore;
using OkToPost.Data;
using OkToPost.Repositories;
using OkToPost.Services;
using OkToPost.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();


// Services
// TODO: Replace with actual implementation class
builder.Services.AddScoped<IUrlRepository, SqlUrlRepository>(); // Or your EF-based repository
builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();
builder.Services.AddSingleton<ICodeGenerator, RandomCodeGenerator>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUrlRepository, SqlUrlRepository>();

builder.WebHost.UseUrls("http://+:80");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}


// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "API is alive. go to: http://localhost:8080/swagger/");
app.Run();
