using southafricantaxtool.DAL;
using southafricantaxtool.DAL.Configuration;
using southafricantaxtool.SARSScraper;
using southafricantaxtool.SARSWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScrapers();

builder.Services.Configure<MongoDbConfiguration>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddDataAccessLayer();

builder.Services.AddHostedService<TaxBracketsWorker>();
builder.Services.AddHostedService<TaxRebatesWorker>();
builder.Services.AddHostedService<ImportantDatesWorker>();

var host = builder.Build();
host.Run();