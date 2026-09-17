using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public class SubastasHub : Hub
{
    public async Task UnirseASubasta(int subastaId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ObtenerNombreDeGrupo(subastaId));
    }
    public async Task SalirDeSubasta(int subastaId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ObtenerNombreDeGrupo(subastaId));
    }
    public static string ObtenerNombreDeGrupo(int subastaId) => $"subasta-{subastaId}";
}