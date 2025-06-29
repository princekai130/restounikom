using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RestoUnikom.WebUi2.Hubs
{
    public class RestoHub : Hub
    {
        // Broadcast ke semua client jika stok menu berubah
        public async Task BroadcastMenuStokChanged()
        {
            await Clients.All.SendAsync("MenuStokChanged");
        }

        // Broadcast update status meja
        public async Task BroadcastStatusMejaChanged(int mejaId)
        {
            await Clients.All.SendAsync("StatusMejaChanged", mejaId);
        }

        // Broadcast update pesanan
        public async Task BroadcastPesananChanged(int pesananId)
        {
            await Clients.All.SendAsync("PesananChanged", pesananId);
        }
    }
}
