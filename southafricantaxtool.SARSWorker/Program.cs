using southafricantaxtool.SARSWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<TaxBracketsWorker>();

var host = builder.Build();
host.Run();