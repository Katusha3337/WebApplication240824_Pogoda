using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    var city = context.Request.Query["city"].ToString();
    if (string.IsNullOrEmpty(city))
    {
        await context.Response.WriteAsync("<form method='get'><input type='text' name='city' placeholder='������� �������� ������' /><button type='submit'>�����</button></form><h3>���������� ������</h3><ul><li><a href='/?city=Kyiv'>����</a></li><li><a href='/?city=Odesa'>������</a></li><li><a href='/?city=Lviv'>�����</a></li></ul>", Encoding.UTF8);
        return;
    }

    var apiKey = builder.Configuration["WeatherApiKey"];
    var httpClient = new HttpClient();
    var response = await httpClient.GetStringAsync($"http://api.weatherapi.com/v1/current.json?key={apiKey}&q={city}");
    var weatherJson = JsonDocument.Parse(response);

    if (weatherJson.RootElement.TryGetProperty("error", out _))
    {
        await context.Response.WriteAsync("<p>����� �� ������.</p>", Encoding.UTF8);
        return;
    }

    var weather = new WeatherData
    {
        City = weatherJson.RootElement.GetProperty("location").GetProperty("name").GetString(),
        Temperature = weatherJson.RootElement.GetProperty("current").GetProperty("temp_c").GetDecimal() + "�C",
        Condition = weatherJson.RootElement.GetProperty("current").GetProperty("condition").GetProperty("text").GetString()
    };

    await context.Response.WriteAsync($"<h2>������ � {weather.City}</h2><p>�����������: {weather.Temperature}</p><p>���������: {weather.Condition}</p>", Encoding.UTF8);
});

app.Run();
public class WeatherData
{
    public string City { get; set; }
    public string Temperature { get; set; }
    public string Condition { get; set; }
}