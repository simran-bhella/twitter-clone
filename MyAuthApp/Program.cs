using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyAuthApp.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. EF Core / SQLite, password hasher, JWT auth (already in place)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<MyAuthApp.Models.User>,
    Microsoft.AspNetCore.Identity.PasswordHasher<MyAuthApp.Models.User>>();
var jwt = builder.Configuration.GetSection("JwtSettings");
var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.Zero
        };
    });

// 2. Add controllers and Swagger services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();        // for minimal APIs; still needed
builder.Services.AddSwaggerGen();                  // generate OpenAPI spec + UI

var app = builder.Build();

// 3. Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// 4. Enable Swagger middleware in Development only (optional)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "swagger";                 // serve at /swagger
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyAuthApp v1");
    });
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
