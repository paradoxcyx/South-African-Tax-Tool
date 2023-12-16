using southafricantaxtool.BL;
using southafricantaxtool.Caching;
using southafricantaxtool.DAL;
using southafricantaxtool.DAL.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.InstanceName = builder.Configuration["RedisCache:InstanceName"];
    options.Configuration = builder.Configuration["RedisCache:Connection"];
});

builder.Services.Configure<MongoDbConfiguration>(builder.Configuration.GetSection("MongoDb"));

builder.Services.AddBusinessLogicServices();
builder.Services.AddCaching();
builder.Services.AddDataAccessLayer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
