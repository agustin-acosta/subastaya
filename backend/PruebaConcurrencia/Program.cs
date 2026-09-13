using System.Net.Http.Json;

const int subastaId = 10;
const string baseUrl = "https://localhost:7094"; 

const int compradorAId = 6;
const int compradorBId = 7;
const decimal montoOfertado = 46000;

var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true 
};
using var client = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };

async Task<string> Ofertar(int compradorId, decimal monto)
{
    var body = new { compradorId, monto };
    var response = await client.PostAsJsonAsync($"/api/subastas/{subastaId}/pujas", body);
    var contenido = await response.Content.ReadAsStringAsync();
    return $"Comprador {compradorId} -> HTTP {(int)response.StatusCode} {response.StatusCode} | {contenido}";
}

Console.WriteLine("Disparando las dos ofertas al mismo tiempo...\n");

var tareaA = Ofertar(compradorAId, montoOfertado);
var tareaB = Ofertar(compradorBId, montoOfertado);

var resultados = await Task.WhenAll(tareaA, tareaB);

foreach (var r in resultados)
{
    Console.WriteLine(r);
}

Console.WriteLine("\nPresioná una tecla para salir.");
Console.ReadKey();