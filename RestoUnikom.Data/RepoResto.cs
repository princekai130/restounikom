using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestoUnikom.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace RestoUnikom.Data
{
    public interface IRepoRestoFactory
    {
        RepoResto Create();
    }

    public class RepoRestoFactory : IRepoRestoFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public RepoRestoFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public RepoResto Create()
        {
            return _serviceProvider.GetRequiredService<RepoResto>();
        }
    }

    /// <summary>
    /// Repository baru untuk kebutuhan realtime dan alur bisnis modern.
    /// </summary>
    public class RepoResto
    {
        private readonly RestoDataContext _context;

        public RepoResto(RestoDataContext context)
        {
            _context = context;
        }

        // ENUMS
        public enum StatusMeja { Kosong, Dipesan, Ditempati, Disiapkan }
        public enum StatusPesanan { Menunggu, Dibatalkan, Disiapkan, Selesai, Diantarkan, Dibayar }

        /// <summary>
        /// Ambil semua menu beserta stok dan ketersediaan.
        /// </summary>
        public async Task<List<Menu>> GetAllMenusAsync()
        {
            return await _context.Menus.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Update stok menu dan ketersediaan, return menu yang sudah diupdate.
        /// </summary>
        public async Task<Menu?> UpdateMenuStokAsync(int menuId, int stokBaru, int? tersediaKah = null)
        {
            var menu = await _context.Menus.FindAsync(menuId);
            if (menu == null) return null;
            menu.StokTersedia = stokBaru;
            if (tersediaKah.HasValue)
                menu.TersediaKah = tersediaKah.Value;
            await _context.SaveChangesAsync();
            return menu;
        }

        /// <summary>
        /// Ambil semua meja yang kosong.
        /// </summary>
        public async Task<List<Meja>> GetMejasKosongAsync()
        {
            return await _context.Mejas
                .AsNoTracking()
                .Where(m => m.StatusMeja == StatusMeja.Kosong.ToString())
                .ToListAsync();
        }

        /// <summary>
        /// Ambil semua meja yang kosong atau ditempati.
        /// </summary>
        public async Task<List<Meja>> GetMejasKosongAtauDitempatiAsync()
        {
            return await _context.Mejas
                .AsNoTracking()
                .Where(m => m.StatusMeja == StatusMeja.Kosong.ToString() || m.StatusMeja == StatusMeja.Ditempati.ToString())
                .ToListAsync();
        }

        /// <summary>
        /// Ambil meja berdasarkan ID.
        /// </summary>
        public async Task<Meja?> GetMejaByIdAsync(int mejaId)
        {
            return await _context.Mejas
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MejaId == mejaId);
        }

        /// <summary>
        /// Buat pesanan baru.
        /// </summary>
        public async Task<Pesanan?> CreatePesananAsync(int mejaId, int pegawaiId, List<(int menuId, int jumlah, string catatan)> items)
        {
            // Buat pesanan baru
            var pesanan = new Pesanan
            {
                MejaId = mejaId,
                PegawaiId = pegawaiId,
                TanggalPesanan = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                StatusPesanan = StatusPesanan.Menunggu.ToString(),
                DibayarKah = 0
            };
            _context.Pesanans.Add(pesanan);
            await _context.SaveChangesAsync(); // commit Pesanan agar PesananId valid

            // Tambahkan pengecekan validitas menu
            foreach (var item in items)
            {
                var menu = await _context.Menus.FindAsync(item.menuId);
                if (menu == null)
                {
                    throw new Exception($"MenuId {item.menuId} tidak ditemukan.");
                }
                if (menu.StokTersedia < item.jumlah)
                {
                    throw new Exception($"Stok menu {menu.NamaMenu} tidak cukup.");
                }
                menu.StokTersedia -= item.jumlah;
                var detail = new DetailPesanan
                {
                    PesananId = pesanan.PesananId,
                    MenuId = item.menuId,
                    Jumlah = item.jumlah,
                    HargaSatuan = menu.Harga,
                    Catatan = item.catatan ?? ""
                };
                _context.DetailPesanans.Add(detail);
            }

            // Update status meja menjadi Ditempati
            var meja = await _context.Mejas.FindAsync(mejaId);
            if (meja != null)
            {
                meja.StatusMeja = StatusMeja.Ditempati.ToString();
            }

            await _context.SaveChangesAsync();
            return pesanan;
        }

        /// <summary>
        /// Hapus detail pesanan dan kembalikan stok menu.
        /// </summary>
        public async Task<bool> DeleteDetailPesananAsync(int detailPesananId)
        {
            var detail = await _context.DetailPesanans.Include(d => d.Menu).FirstOrDefaultAsync(d => d.DetailPesananId == detailPesananId);
            if (detail == null) return false;
            // Kembalikan stok menu
            if (detail.Menu != null)
                detail.Menu.StokTersedia += detail.Jumlah;
            _context.DetailPesanans.Remove(detail);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Tambah detail pesanan ke pesanan yang sudah ada, dan kurangi stok menu.
        /// </summary>
        public async Task<DetailPesanan?> AddDetailPesananAsync(int pesananId, int menuId, int jumlah, string catatan)
        {
            var menu = await _context.Menus.FindAsync(menuId);
            if (menu == null || menu.StokTersedia < jumlah) return null;
            var detail = new DetailPesanan
            {
                PesananId = pesananId,
                MenuId = menuId,
                Jumlah = jumlah,
                HargaSatuan = menu.Harga,
                Catatan = catatan ?? ""
            };
            menu.StokTersedia -= jumlah;
            _context.DetailPesanans.Add(detail);
            await _context.SaveChangesAsync();
            return detail;
        }

        /// <summary>
        /// Batalkan pesanan (ubah status ke Dibatalkan) dan kembalikan stok semua menu.
        /// </summary>
        public async Task<bool> CancelPesananAsync(int pesananId)
        {
            var pesanan = await _context.Pesanans.Include(p => p.DetailPesanans).ThenInclude(dp => dp.Menu).FirstOrDefaultAsync(p => p.PesananId == pesananId);
            if (pesanan == null || pesanan.StatusPesanan != StatusPesanan.Menunggu.ToString()) return false;
            pesanan.StatusPesanan = StatusPesanan.Dibatalkan.ToString();
            foreach (var detail in pesanan.DetailPesanans)
            {
                if (detail.Menu != null)
                    detail.Menu.StokTersedia += detail.Jumlah;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Ambil semua pesanan berdasarkan ID meja.
        /// </summary>
        public async Task<List<Pesanan>> GetSemuaPesananByMejaIdAsync(int mejaId)
        {
            return await _context.Pesanans
                .Where(p => p.MejaId == mejaId)
                .Include(p => p.DetailPesanans)
                .ThenInclude(dp => dp.Menu)
                .OrderByDescending(p => p.PesananId)
                .ToListAsync();
        }

        /// <summary>
        /// Ambil semua pesanan dari semua meja beserta detail dan info meja.
        /// </summary>
        public async Task<List<Pesanan>> GetSemuaPesananAsync()
        {
            return await _context.Pesanans
                .Include(p => p.Meja)
                .Include(p => p.DetailPesanans)
                .ThenInclude(dp => dp.Menu)
                .OrderByDescending(p => p.PesananId)
                .ToListAsync();
        }

        /// <summary>
        /// Update status pesanan.
        /// </summary>
        public async Task<bool> UpdateStatusPesananAsync(int pesananId, string statusBaru)
        {
            var pesanan = await _context.Pesanans.FindAsync(pesananId);
            if (pesanan == null) return false;
            pesanan.StatusPesanan = statusBaru;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Ambil pegawai berdasarkan nama pengguna.
        /// </summary>
        public async Task<Pegawai?> GetPegawaiByNamaPengguna(string namaPengguna)
        {
            return await _context.Pegawais.FirstOrDefaultAsync(p => p.NamaPengguna.ToLower() == namaPengguna.ToLower());
        }

        /// <summary>
        /// Ambil semua stok bahan.
        /// </summary>
        public async Task<List<StokBahan>> GetAllStokBahanAsync()
        {
            return await _context.StokBahans.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Tambah atau update menu. Return MenuId.
        /// </summary>
        public async Task<int> AddOrUpdateMenuAsync(Menu menu)
        {
            if (menu.MenuId == 0)
            {
                menu.TanggalDitambahkan = DateOnly.FromDateTime(DateTime.Now);
                _context.Menus.Add(menu);
            }
            else
            {
                var existing = await _context.Menus.FindAsync(menu.MenuId);
                if (existing == null) throw new Exception("Menu tidak ditemukan");
                _context.Entry(existing).CurrentValues.SetValues(menu);
            }
            await _context.SaveChangesAsync();
            return menu.MenuId;
        }

        /// <summary>
        /// Tambah atau update MenuBahan untuk menu tertentu.
        /// </summary>
        public async Task AddOrUpdateMenuBahanAsync(int menuId, int bahanId, double jumlahDibutuhkan)
        {
            var menuBahan = await _context.MenuBahans.FirstOrDefaultAsync(mb => mb.MenuId == menuId && mb.BahanId == bahanId);
            if (menuBahan == null)
            {
                menuBahan = new MenuBahan { MenuId = menuId, BahanId = bahanId, JumlahDibutuhkan = jumlahDibutuhkan };
                _context.MenuBahans.Add(menuBahan);
            }
            else
            {
                menuBahan.JumlahDibutuhkan = jumlahDibutuhkan;
            }
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Ambil semua MenuBahan untuk menu tertentu.
        /// </summary>
        public async Task<List<MenuBahan>> GetMenuBahansByMenuIdAsync(int menuId)
        {
            return await _context.MenuBahans.AsNoTracking().Where(mb => mb.MenuId == menuId).ToListAsync();
        }

        /// <summary>
        /// Hapus MenuBahan tertentu dari menu.
        /// </summary>
        public async Task DeleteMenuBahanAsync(int menuId, int bahanId)
        {
            var menuBahan = await _context.MenuBahans.FirstOrDefaultAsync(mb => mb.MenuId == menuId && mb.BahanId == bahanId);
            if (menuBahan != null)
            {
                _context.MenuBahans.Remove(menuBahan);
                await _context.SaveChangesAsync();
            }
        }
    }
}
