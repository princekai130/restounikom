using Microsoft.EntityFrameworkCore;
using RestoUnikom.Data.Models;

namespace RestoUnikom.Data
{
    public class RepositoriResto
    {
        private readonly RestoDataContext _context;
        public RepositoriResto(RestoDataContext context)
        {
            _context = context;
        }

        #region ENUMS

        /// <summary>
        /// Enum untuk status meja.
        /// </summary>
        public enum StatusMeja
        {
            Kosong,
            Ditempati,
            Dipesan
        }

        /// <summary>
        /// Enum untuk kategori menu.
        /// </summary>
        public enum KategoriMenu
        {
            Makanan,
            Minuman,
            Camilan
        }

        /// <summary>
        /// Enum untuk status pesanan.
        /// </summary>
        public enum StatusPesanan
        {
            Menunggu,
            Disiapkan,
            Selesai,
            Dibatalkan
        }

        #endregion // ENUMS

        #region MEJAS

        /// <summary>
        /// Mengambil daftar meja.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Meja>> GetMejasAsync()
        {
            return await _context.Mejas.ToListAsync();
        }

        /// <summary>
        /// Mengambil meja berdasarkan ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Meja?> GetMejaByIdAsync(int id)
        {
            return await _context.Mejas.FindAsync(id);
        }

        /// <summary>
        /// Mengambil meja berdasarkan statusnya (kosong, ditempati, dipesan).
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<Meja?> GetMejaByStatusAsync(StatusMeja status)
        {
            return await _context.Mejas
                .FirstOrDefaultAsync(m => m.StatusMeja == status.ToString());
        }

        /// <summary>
        /// Mengambil daftar meja yang kosong (tidak ditempati).
        /// </summary>
        /// <returns></returns>
        public async Task<List<Meja>> GetMejasKosongAsync()
        {
            return await _context.Mejas
                .Where(m => m.StatusMeja == StatusMeja.Kosong.ToString())
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar meja yang sedang ditempati.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Meja>> GetMejasDitempatiAsync()
        {
            return await _context.Mejas
                .Where(m => m.StatusMeja == StatusMeja.Ditempati.ToString())
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar meja yang sedang dipesan (belum ditempati).
        /// </summary>
        /// <returns></returns>
        public async Task<List<Meja>> GetMejasDipesanAsync()
        {
            return await _context.Mejas
                .Where(m => m.StatusMeja == StatusMeja.Dipesan.ToString())
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil meja berdasarkan nomor meja (nomor unik).
        /// </summary>
        /// <param name="nomor">Nomor Meja</param>
        /// <returns></returns>
        public async Task<Meja?> GetMejaByNomorAsync(string nomor)
        {
            return await _context.Mejas
                .FirstOrDefaultAsync(m => m.NomorMeja == nomor);
        }

        /// <summary>
        /// Mengubah status meja berdasarkan ID.
        /// </summary>
        /// <param name="mejaId">ID</param>
        /// <param name="status">Enum StatusMeja</param>
        /// <returns></returns>
        public async Task<Meja?> SetStatusMejaAsync(int mejaId, StatusMeja status)
        {
            var meja = await GetMejaByIdAsync(mejaId);
            if (meja != null)
            {
                meja.StatusMeja = status.ToString();
                await _context.SaveChangesAsync();
            }
            return meja;
        }

        /// <summary>
        /// Mengubah status meja berdasarkan nomor meja.
        /// </summary>
        /// <param name="nomorMeja">Nomor Meja</param>
        /// <param name="status">Enum StatusMeja</param>
        /// <returns></returns>
        public async Task<Meja?> SetStatusMejaAsync(string nomorMeja, StatusMeja status)
        {
            var meja = await GetMejaByNomorAsync(nomorMeja);
            if (meja != null)
            {
                meja.StatusMeja = status.ToString();
                await _context.SaveChangesAsync();
            }
            return meja;
        }

        /// <summary>
        /// Mengubah status meja dengan objek Meja yang sudah ada.
        /// </summary>
        /// <param name="meja">Objek Meja</param>
        /// <param name="status">Enum StatusMeja</param>
        /// <returns></returns>
        public async Task<Meja?> SetStatusMejaAsync(Meja meja, StatusMeja status)
        {
            meja.StatusMeja = status.ToString();
            _context.Mejas.Update(meja);
            await _context.SaveChangesAsync();
            return meja;
        }

        /// <summary>
        /// Mengubah status meja dengan objek Meja yang sudah ada.
        /// </summary>
        /// <param name="meja">Objek Meja</param>
        /// <returns></returns>
        public async Task<Meja?> SetStatusMejaAsync(Meja meja)
        {
            _context.Mejas.Update(meja);
            await _context.SaveChangesAsync();
            return meja;
        }

        /// <summary>
        /// Menambahkan meja baru.
        /// </summary>
        /// <param name="mejaBaru">Objek Meja</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<Meja?> SetMejaBaruAsync(Meja mejaBaru)
        {
            if (mejaBaru == null)
            {
                throw new ArgumentNullException(nameof(mejaBaru), "Meja baru tidak boleh null.");
            }
            // Pastikan meja baru memiliki nomor unik
            var existingMeja = await GetMejaByNomorAsync(mejaBaru.NomorMeja);
            if (existingMeja != null)
            {
                throw new InvalidOperationException($"Meja dengan nomor {mejaBaru.NomorMeja} sudah ada.");
            }
            _context.Mejas.Add(mejaBaru);
            await _context.SaveChangesAsync();
            return mejaBaru;
        }

        /// <summary>
        /// Mengubah status aktif meja (aktif atau tidak aktif).
        /// </summary>
        /// <param name="mejaId">ID</param>
        /// <param name="aktifKah">Aktif(True/False)</param>
        /// <returns></returns>
        public async Task<Meja?> SetStatusAktifMeja(int mejaId, bool aktifKah)
        {
            var meja = await GetMejaByIdAsync(mejaId);
            if (meja != null)
            {
                meja.AktifKah = aktifKah ? 1 : 0; // 1 untuk aktif, 0 untuk tidak aktif
                _context.Mejas.Update(meja);
                await _context.SaveChangesAsync();
            }
            return meja;
        }

        /// <summary>
        /// Mengubah status aktif meja (aktif atau tidak aktif) berdasarkan objek Meja.
        /// </summary>
        /// <param name="meja"></param>
        /// <param name="aktifKah">Aktif(True/False)</param>
        /// <returns></returns>
        public async Task<Meja?> SetStatusAktifMeja(Meja meja, bool aktifKah)
        {
            meja.AktifKah = aktifKah ? 1 : 0; // 1 untuk aktif, 0 untuk tidak aktif
            _context.Mejas.Update(meja);
            await _context.SaveChangesAsync();
            return meja;
        }

        #endregion // MEJAS

        #region MENUS

        /// <summary>
        /// Mengambil daftar menu/restoran. 
        /// </summary>
        /// <returns></returns>
        public async Task<List<Menu>> GetMenusAsync()
        {
            return await _context.Menus.ToListAsync();
        }

        /// <summary>
        /// Mengambil menu berdasarkan ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Menu?> GetMenuByIdAsync(int id)
        {
            return await _context.Menus.FindAsync(id);
        }

        /// <summary>
        /// Mengambil menu yang bisa dipesan. Tersedia dan stoknya masih ada.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Menu>> GetMenusStokTersediaAsync()
        {
            return await _context.Menus
                .Where(m => m.TersediaKah == 1 && m.StokTersedia > 0)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil menu berdasarkan kategori (makanan, minuman, camilan)
        /// </summary>
        /// <param name="kategori"></param>
        /// <returns></returns>
        public async Task<List<Menu>> GetMenusByKategoriAsync(KategoriMenu kategori)
        {
            return await _context.Menus
                .Where(m => 
                    m.Kategori.Equals(kategori.ToString(), 
                    StringComparison.OrdinalIgnoreCase))
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil menu berdasarkan kategori dan hanya yang tersedia (stok > 0). 
        /// </summary>
        /// <param name="kategori"></param>
        /// <returns></returns>
        public async Task<List<Menu>> GetMenusByKategoriDanTersediaAsync(KategoriMenu kategori)
        {
            return await _context.Menus
                .Where(m => 
                    m.Kategori.Equals(kategori.ToString(), 
                    StringComparison.OrdinalIgnoreCase) && m.StokTersedia > 0)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil menu berdasarkan kategori dan stok minimal yang tersedia.
        /// </summary>
        /// <param name="kategori"></param>
        /// <param name="stokMinimal"></param>
        /// <returns></returns>
        public async Task<List<Menu>> GetMenusByKategoriDanTersediaAsync(KategoriMenu kategori, int stokMinimal)
        {
            return await _context.Menus
                .Where(m => 
                    m.Kategori.Equals(kategori.ToString(), 
                    StringComparison.OrdinalIgnoreCase) && m.StokTersedia >= stokMinimal)
                .ToListAsync();
        }

        #endregion // MENUS

        #region PESANAN

        /// <summary>
        /// Mengambil daftar pesanan.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Pesanan?> GetPesananByIdAsync(int id)
        {
            return await _context.Pesanans
                .Include(p => p.Meja)
                .Include(p => p.DetailPesanans)
                .FirstOrDefaultAsync(p => p.PesananId == id);
        }

        /// <summary>
        /// Mengambil daftar pesanan berdasarkan ID Meja.
        /// </summary>
        /// <param name="mejaId"></param>
        /// <returns></returns>
        public async Task<List<Pesanan>> GetPesanansByMejaIdAsync(int mejaId)
        {
            return await _context.Pesanans
                .Include(p => p.Meja)
                .Where(p => p.Meja.MejaId == mejaId)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar pesanan berdasarkan  Nomor Meja.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Pesanan>> GetPesanansByNomorMejaAsync(string nomorMeja)
        {
            return await _context.Pesanans
                .Include(p => p.Meja)
                .Where(p => p.Meja.NomorMeja == nomorMeja)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar pesanan berdasarkan Tanggal.
        /// </summary>
        /// <param name="tanggal"></param>
        /// <returns></returns>
        public async Task<List<Pesanan>> GetPesanansByTanggalAsync(DateTime tanggal)
        { 
            return await _context.Pesanans
                .Where(p => DateTime.Parse(p.TanggalPesanan).Date == tanggal.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar pesanan berdasarkan Status Pesanan.
        /// </summary>
        /// <param name="statusPesanan"></param>
        /// <returns></returns>
        public async Task<List<Pesanan>> GetPesanansByStatusAsync(StatusPesanan statusPesanan)
        {
            return await _context.Pesanans
                .Where(p => p.StatusPesanan.Equals(statusPesanan.ToString(), StringComparison.OrdinalIgnoreCase))
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar pesanan berdasarkan apakah sudah dibayar atau belum.
        /// </summary>
        /// <param name="dibayarKah"></param>
        /// <returns></returns>
        public async Task<List<Pesanan>> GetPesanansByDibayarkahAsync(bool dibayarKah)
        {
            return await _context.Pesanans
                .Where(p => p.DibayarKah == (dibayarKah ? 1 : 0))
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar pesanan berdasarkan Pegawai ID dan Tanggal Pesanan.
        /// </summary>
        /// <param name="pegawaiId"></param>
        /// <param name="tanggal"></param>
        /// <returns></returns>
        public async Task<List<Pesanan>> GetPesanansByPegawaiIdDanTanggalAsync(int pegawaiId, DateTime tanggal)
        {
            return await _context.Pesanans
                .Where(p => p.PegawaiId == pegawaiId && DateTime.Parse(p.TanggalPesanan).Date == tanggal.Date)
                .ToListAsync();
        }
        #endregion // PESANAN
    }
}
