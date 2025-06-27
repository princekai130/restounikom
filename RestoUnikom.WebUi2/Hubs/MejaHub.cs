using Microsoft.AspNetCore.SignalR;

namespace RestoUnikom.WebUi2.Hubs
{
    public class MejaHub : Hub
    {
        // Method ini bisa dipanggil server untuk broadcast update status meja
        public async Task BroadcastStatusMejaChanged(int mejaId)
        {
            await Clients.All.SendAsync("StatusMejaChanged", mejaId);
        }
    }
}
