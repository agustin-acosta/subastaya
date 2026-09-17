using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

const int subastaId = 1;
const string baseUrl = "https://localhost:7094";

const string emailA = "comprador1@test.com";
const string emailB = "comprador2@test.com";
const string password = "password123";
const decimal montoOfertado = 46000;

var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
};
using var client = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };

async Task<string> Login(string email, string password)
{
    var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
    var contenido = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        throw new Exception($"No se pudo iniciar sesion con {email}: HTTP {(int)response.StatusCode} | {contenido}");
    }

    using var json = JsonDocument.Parse(contenido);
    return json.RootElement.GetProperty("token").GetString()!;
}

async Task<string> Ofertar(string token, decimal monto)
{
    using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/subastas/{subastaId}/pujas")
    {
        Content = JsonContent.Create(new { monto })
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var response = await client.SendAsync(request);
    var contenido = await response.Content.ReadAsStringAsync();
    return $"HTTP {(int)response.StatusCode} {response.StatusCode} | {contenido}";
}

Console.WriteLine("Iniciando sesion con los dos compradores...\n");

var tokenA = await Login(emailA, password);
var tokenB = await Login(emailB, password);

Console.WriteLine("Sesiones iniciadas. Disparando las dos ofertas al mismo tiempo...\n");

var tareaA = Ofertar(tokenA, montoOfertado);
var tareaB = Ofertar(tokenB, montoOfertado);

var resultados = await Task.WhenAll(tareaA, tareaB);

Console.WriteLine($"{emailA} -> {resultados[0]}");
Console.WriteLine($"{emailB} -> {resultados[1]}");

Console.WriteLine("\nPresiona una tecla para salir.");
Console.ReadKey();