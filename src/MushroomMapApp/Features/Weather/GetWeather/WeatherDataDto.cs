namespace MushroomMapApp.Features.Weather.GetWeather;

public class WeatherDataDto
{
    public string Time { get; set; }
    public double Temperature { get; set; }
    public double Precipitation { get; set; }
    public double WindSpeed { get; set; }
    public int WeatherCode { get; set; }
}
