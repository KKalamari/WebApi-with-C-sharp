using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    //automatically open the Swagger UI page in the default browser
    var swaggerUiUrl = "http://localhost:5065/swagger/index.html"; 
    Process.Start(new ProcessStartInfo
    {
        FileName = swaggerUiUrl,
        UseShellExecute = true
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers(); 
app.Run();
