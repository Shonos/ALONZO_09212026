using FileUploadAndReport.Demo.Api.Repositories;
using FileUploadAndReport.Demo.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("FileUploads")
    ?? throw new InvalidOperationException("The FileUploads connection string is required.");

// singleton because no request state stored, no connection kept open, each operation has its own transaction
builder.Services.AddSingleton<IFileUploadRepository>(_ =>
    new SqliteFileUploadRepository(connectionString));
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

var app = builder.Build();

// initializes the database and creates the table if it does not exist
await app.Services
    .GetRequiredService<IFileUploadRepository>()
    .InitializeAsync();

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
