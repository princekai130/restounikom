using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RestoUnikom.WebUi2.Hubs
{
    public class MenuHub : Hub
    {
        // Broadcast ke semua client jika stok menu berubah
        public async Task BroadcastMenuStokChanged()
        {
            await Clients.All.SendAsync("MenuStokChanged");
        }
    }
}
