using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Api_Enhanced.Services;

namespace Api_Enhanced;

public static class Program
{
	public static void Main(string[] args)
	{

		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.

		// Attribute Routing is in line 17.
		builder.Services.AddControllers();
		builder.Services.AddHttpClient<MyAnimeListService>();
		builder.Services.AddTransient<MALActor>();

		builder.Services.AddSingleton(provider =>
			new MyAnimeListService(provider.GetRequiredService<HttpClient>()));

		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();

		app.UseAuthorization();

		// Map controllers to routes.
		app.MapControllers();

		app.Run();

	}

	
}
  