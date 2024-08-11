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
		builder.Services.AddControllers();

		// Attribute Routing is in line 17.
		// Add HttpClient for MyAnimeListService
		builder.Services.AddHttpClient<MyAnimeListService>();

		// builder.Services.AddTransient<MALActor>();
		// Register IMALActor and MALActor
		builder.Services.AddTransient<IMALActor, MALActor>();

		// Register MyAnimeListService with singleton scope.
		builder.Services.AddSingleton(provider =>
			new MyAnimeListService(provider.GetRequiredService<HttpClient>()));

		// Register IMALAnimeScrape and its implementation
		builder.Services.AddTransient<IMALAnimeScrape, MALAnimeScrape>();

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
  