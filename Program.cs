using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SearchTool_ServerSide.Authentication;
using SearchTool_ServerSide.Data;
using SearchTool_ServerSide.Repository;
using SearchTool_ServerSide.Services;
using ServerSide;

var builder = WebApplication.CreateBuilder(args);
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();
if (jwtOptions == null)
{
    throw new ArgumentNullException(nameof(jwtOptions));
}
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin",
    builder => { builder.RequireRole("SuperAdmin"); });
    options.AddPolicy("Admin",
    builder => { builder.RequireRole("Admin", "SuperAdmin"); });
    options.AddPolicy("Pharmacist",
    builder => { builder.RequireRole("Pharmacist", "Admin", "SuperAdmin"); });

});

builder.Services.AddAuthentication()
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            };

        });
builder.Services.AddDbContext<SearchToolDBContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("SearchTool")));
builder.Services.AddScoped<UserAccessToken>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<DrugRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<LogRepository>();
builder.Services.AddScoped<BranchRepository>();
builder.Services.AddScoped<DrugService>();
builder.Services.AddScoped<UserSevice>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpContextAccessor();

var allowedOrigins = new List<string>
{
    "https://medisearchtool.com",
    "http://localhost:5173",
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

app.MapControllers();
app.Run();
