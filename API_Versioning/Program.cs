using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new QueryStringApiVersionReader("version");
});
builder.Services.AddSwaggerGen(options=>
{
    options.SwaggerDoc("1.0", new OpenApiInfo { Title = "My API V1", Version="1.0" });
    options.SwaggerDoc("2.0", new OpenApiInfo { Title = "My API V2", Version = "2.0" });
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    options.DocInclusionPredicate((version,apiDesc)=>
    {
        if (!apiDesc.TryGetMethodInfo(out MethodInfo method))
            return false;
        var methodVersions = method.GetCustomAttributes(true)
        .OfType<ApiVersionAttribute>()
        .SelectMany(attr => attr.Versions);
        var controllerVersions = method.DeclaringType?.GetCustomAttributes(true)
        .OfType<ApiVersionAttribute>().
        SelectMany(attr => attr.Versions);
        var allVersions = methodVersions.Union(controllerVersions).Distinct();
        return allVersions.Any(v => v.ToString() == version);
    });
});
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options=>
    {
        options.SwaggerEndpoint("/swagger/1.0/swagger.json", "My API V1");
        options.SwaggerEndpoint("/swagger/2.0/swagger.json", "My API V2");
    });
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();