namespace Api_Enhanced.Models;

public class Anime
{
	public int Id { get; set; }
	public string? Title { get; set; }
	public string? Synopsis { get; set; }
	public List<string>? Genre { get; set; }
	public List<string>? Studios { get; set; }
	public string? MainPicture { get; set; }
	public double Score { get; set; }				// MAL Api Uses Mean as its score.

}
