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
        /// Enum untuk status reservasi.
        /// </summary>
        public enum StatusReservasi
        {
            Menunggu,
            Dikonfirmasi,
            Selesai,
            Dibatalkan
        }

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
            Diantarkan,
            Dibatalkan
        }

        /// <summary>
        /// Enum untuk peran pegawai.
        /// </summary>
        public enum PeranPegawai
        {
            Kasir,
            Pelayan,
            Koki,
            Pemilik
        }

        public enum MetodeBayar
        {
            Tunai,
            KartuKredit,
            DompetDigital,
            TransferBank,
            QRIS
        }

        #endregion // ENUMS

        #region PEGAWAI

        /// <summary>
        /// Meng-hash kata sandi pegawai menggunakan SHA256.
        /// </summary>
        /// <param name="kataSandi"></param>
        /// <returns></returns>
        private string HashPassword(string kataSandi)
        {
            // SHA256
            var hashedBytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(kataSandi));
            // Konversi ke string heksadesimal
            var kataSandiHash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
            return kataSandiHash;
        }

        /// <summary>
        /// Mengambil daftar pegawai.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Pegawai>> GetPegawaisAsync()
        {
            return await _context.Pegawais.ToListAsync();
        }

        /// <summary>
        /// Mengambil pegawai berdasarkan ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Pegawai?> GetPegawaiByIdAsync(int id)
        {
            return await _context.Pegawais.FindAsync(id);
        }

        /// <summary>
        /// Mengambil pegawai berdasarkan nama pegawai.
        /// </summary>
        /// <param name="namaPegawai"></param>
        /// <returns></returns>
        public async Task<Pegawai?> GetPegawaiByNamaAsync(string namaPegawai)
        {
            return await _context.Pegawais
                .FirstOrDefaultAsync(p => p.NamaPegawai.ToLower() == namaPegawai.ToLower());
        }

        /// <summary>
        /// Mengambil pegawai berdasarkan nama pengguna (username).
        /// </summary>
        /// <param name="namaPrngguna"></param>
        /// <returns></returns>
        public async Task<Pegawai?> GetPegawaiByNamaPengguna(string namaPrngguna)
        {             
            return await _context.Pegawais
                .FirstOrDefaultAsync(p => p.NamaPengguna.ToLower() == namaPrngguna.ToLower());
        }

        /// <summary>
        /// Mengambil pegawai berdasarkan nama pengguna dan kata sandi.
        /// </summary>
        /// <param name="namaPengguna"></param>
        /// <param name="kataSandi"></param>
        /// <returns></returns>
        public async Task<Pegawai?> GetPegawaiByNamaPenggunaDanKataSandi(string namaPengguna, string kataSandi)
        {
            // Validasi input
            if (string.IsNullOrWhiteSpace(namaPengguna) || string.IsNullOrWhiteSpace(kataSandi))
            {
                return null; 
            }

            // Mencari pegawai berdasarkan nama pengguna
            var pegawai = await _context.Pegawais
                .FirstOrDefaultAsync(p => p.NamaPengguna.ToLower() == namaPengguna.ToLower());

            // Jika pegawai ditemukan, periksa kata sandi
            if (pegawai == null)
            {
                return null; // Pegawai tidak ditemukan
            }

            // Periksa apakah kata sandi cocok
            //var kataSandiHash = HashPassword(kataSandi);

            //if (!pegawai.KataSandi.Equals(kataSandiHash, StringComparison.OrdinalIgnoreCase))
            //{
            //    return null; // Kata sandi tidak cocok
            //}

            // kembalikan pegawai jika nama pengguna dan kata sandi cocok
            return pegawai;
        }

        /// <summary>
        /// Mengambil daftar pegawai berdasarkan peran.
        /// </summary>
        /// <param name="peran"></param>
        /// <returns></returns>
        public async Task<List<Pegawai>> GetPegawaisByPeranAsync(PeranPegawai peran)
        {
            return await _context.Pegawais
                .Where(p => p.PeranPegawai.ToLower() == peran.ToString().ToLower())
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar pegawai berdasarkan apakah mereka aktif atau tidak.
        /// </summary>
        /// <param name="aktifKah"></param>
        /// <returns></returns>
        public async Task<List<Pegawai>> GetPegawaisByAktifKahAsync(bool aktifKah)
        {
            return await _context.Pegawais
                .Where(p => p.AktifKah == (aktifKah ? 1 : 0))
                .ToListAsync();
        }

        /// <summary>
        /// Menambahkan pegawai baru.
        /// </summary>
        /// <param name="pegawaiBaru"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<Pegawai?> SetPegawaiBaruAsync(Pegawai pegawaiBaru)
        {
            if (pegawaiBaru == null)
            {
                throw new ArgumentNullException(nameof(pegawaiBaru), "Pegawai baru tidak boleh null.");
            }
            // Pastikan pegawai baru memiliki nama pengguna unik
            var existingPegawai = await GetPegawaiByNamaPengguna(pegawaiBaru.NamaPengguna);
            if (existingPegawai != null)
            {
                throw new InvalidOperationException($"Pegawai dengan nama pengguna {pegawaiBaru.NamaPengguna} sudah ada.");
            }
            // Hash kata sandi sebelum disimpan
            pegawaiBaru.KataSandi = HashPassword(pegawaiBaru.KataSandi);
            _context.Pegawais.Add(pegawaiBaru);
            await _context.SaveChangesAsync();
            return pegawaiBaru;
        }

        /// <summary>
        /// Mengubah peran pegawai berdasarkan ID.
        /// </summary>
        /// <param name="pegawaiId"></param>
        /// <param name="aktifKah"></param>
        /// <returns></returns>
        public async Task<Pegawai?> SetPegawaiAktifAsync(int pegawaiId, bool aktifKah)
        {
            var pegawai = await GetPegawaiByIdAsync(pegawaiId);
            if (pegawai != null)
            {
                pegawai.AktifKah = aktifKah ? 1 : 0; // 1 untuk aktif, 0 untuk tidak aktif
                _context.Pegawais.Update(pegawai);
                await _context.SaveChangesAsync();
                return pegawai;
            }
            return null;
        }

        /// <summary>
        /// Mengubah peran pegawai berdasarkan ID.
        /// </summary>
        /// <param name="pegawaiId"></param>
        /// <param name="kataSandi"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<Pegawai?> SetPegawaiKataSandi(int pegawaiId, string kataSandi)
        {             
            if (string.IsNullOrWhiteSpace(kataSandi))
            {
                throw new ArgumentNullException(nameof(kataSandi), "Kata sandi tidak boleh kosong.");
            }
            var pegawai = await GetPegawaiByIdAsync(pegawaiId);
            if (pegawai != null)
            {
                pegawai.KataSandi = HashPassword(kataSandi);
                _context.Pegawais.Update(pegawai);
                await _context.SaveChangesAsync();
                return pegawai;
            }
            return null;
        }

        #endregion // PEGAWAI

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

        #region RESERVASI

        /// <summary>
        /// Mengambil daftar reservasi.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Reservasi>> GetReservasisAsync()
        {
            return await _context.Reservasis
                .Include(r => r.Meja)
                .Include(r => r.Pegawai)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil reservasi berdasarkan ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Reservasi?> GetReservasiByIdAsync(int id)
        {
            return await _context.Reservasis
                .Include(r => r.Meja)
                .Include(r => r.Pegawai)
                .FirstOrDefaultAsync(r => r.ReservasiId == id);
        }

        /// <summary>
        /// Mengambil daftar reservasi berdasarkan ID meja.
        /// </summary>
        /// <param name="mejaId"></param>
        /// <returns></returns>
        public async Task<List<Reservasi>> GetReservasisByMejaIdAsync(int mejaId)
        {
            return await _context.Reservasis
                .Include(r => r.Meja)
                .Where(r => r.MejaId == mejaId)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar reservasi berdasarkan ID pegawai.
        /// </summary>
        /// <param name="pegawaiId"></param>
        /// <returns></returns>
        public async Task<List<Reservasi>> GetReservasisByPegawaiIdAsync(int pegawaiId)
        {
            return await _context.Reservasis
                .Include(r => r.Pegawai)
                .Where(r => r.PegawaiId == pegawaiId)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar reservasi berdasarkan tanggal reservasi.
        /// </summary>
        /// <param name="tanggal"></param>
        /// <returns></returns>
        public async Task<List<Reservasi>> GetReservasisByTanggalAsync(DateTime tanggal)
        {
            return await _context.Reservasis
                .Where(r => r.TanggalReservasi == DateOnly.FromDateTime(tanggal.Date))
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar reservasi berdasarkan status reservasi.
        /// </summary>
        /// <param name="statusReservasi"></param>
        /// <returns></returns>
        public async Task<List<Reservasi>> GetReservasisByStatusAsync(StatusReservasi statusReservasi)
        {
            return await _context.Reservasis
                .Where(r => r.StatusReservasi.ToLower() == statusReservasi.ToString().ToLower())
                .ToListAsync();
        }

        /// <summary>
        /// Menambahkan reservasi baru.
        /// </summary>
        /// <param name="reservasi"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<Reservasi?> SetReservasiBaruAsync(Reservasi reservasi)
        {
            if (reservasi == null)
            {
                throw new ArgumentNullException(nameof(reservasi), "Reservasi baru tidak boleh null.");
            }
            // Pastikan meja yang direservasi tersedia
            var meja = await GetMejaByIdAsync(reservasi.MejaId);
            if (meja == null || meja.StatusMeja != StatusMeja.Kosong.ToString())
            {
                throw new InvalidOperationException("Meja tidak tersedia untuk reservasi.");
            }
            // Set status meja menjadi dipesan
            meja.StatusMeja = StatusMeja.Dipesan.ToString();
            _context.Mejas.Update(meja);
            _context.Reservasis.Add(reservasi);
            await _context.SaveChangesAsync();
            return reservasi;
        }

        /// <summary>
        /// Mengubah status reservasi berdasarkan ID.
        /// </summary>
        /// <param name="reservasiId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<Reservasi?> SetReservasiStatusAsync(int reservasiId, StatusReservasi status)
        {
            var reservasi = await GetReservasiByIdAsync(reservasiId);
            if (reservasi != null)
            {
                reservasi.StatusReservasi = status.ToString();
                _context.Reservasis.Update(reservasi);
                await _context.SaveChangesAsync();
                return reservasi;
            }
            return null;
        }

        /// <summary>
        /// Mengubah status reservasi berdasarkan objek Reservasi.
        /// </summary>
        /// <param name="reservasi"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<Reservasi?> SetReservasiStatusAsync(Reservasi reservasi, StatusReservasi status)
        {
            if (reservasi == null)
            {
                throw new ArgumentNullException(nameof(reservasi), "Reservasi tidak boleh null.");
            }
            reservasi.StatusReservasi = status.ToString();
            _context.Reservasis.Update(reservasi);
            await _context.SaveChangesAsync();
            return reservasi;
        }

        /// <summary>
        /// Mengubah meja yang dipesan dalam reservasi berdasarkan ID reservasi dan ID meja.
        /// </summary>
        /// <param name="reservasiId"></param>
        /// <param name="mejaId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<Reservasi?> SetReservasiMejaAsync(int reservasiId, int mejaId)
        {
            var reservasi = await GetReservasiByIdAsync(reservasiId);
            if (reservasi != null)
            {
                var meja = await GetMejaByIdAsync(mejaId);
                if (meja == null || meja.StatusMeja != StatusMeja.Kosong.ToString())
                {
                    throw new InvalidOperationException("Meja tidak tersedia untuk reservasi.");
                }
                // Set status meja menjadi dipesan
                meja.StatusMeja = StatusMeja.Dipesan.ToString();
                _context.Mejas.Update(meja);
                reservasi.MejaId = mejaId;
                _context.Reservasis.Update(reservasi);
                await _context.SaveChangesAsync();
                return reservasi;
            }
            return null;
        }

        /// <summary>
        /// Mengubah meja yang dipesan dalam reservasi berdasarkan objek Reservasi dan ID meja.
        /// </summary>
        /// <param name="reservasi"></param>
        /// <param name="mejaId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<Reservasi?> SetReservasiMejaAsync(Reservasi reservasi, int mejaId)
        {
            if (reservasi == null)
            {
                throw new ArgumentNullException(nameof(reservasi), "Reservasi tidak boleh null.");
            }
            var meja = await GetMejaByIdAsync(mejaId);
            if (meja == null || meja.StatusMeja != StatusMeja.Kosong.ToString())
            {
                throw new InvalidOperationException("Meja tidak tersedia untuk reservasi.");
            }
            // Set status meja menjadi dipesan
            meja.StatusMeja = StatusMeja.Dipesan.ToString();
            _context.Mejas.Update(meja);
            reservasi.MejaId = mejaId;
            _context.Reservasis.Update(reservasi);
            await _context.SaveChangesAsync();
            return reservasi;
        }

        /// <summary>
        /// Mengubah pegawai yang menangani reservasi berdasarkan ID reservasi dan ID pegawai.
        /// </summary>
        /// <param name="reservasiId"></param>
        /// <param name="pegawaiId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<Reservasi?> SetReservasiPegawaiAsync(int reservasiId, int pegawaiId)
        {
            var reservasi = await GetReservasiByIdAsync(reservasiId);
            if (reservasi != null)
            {
                var pegawai = await GetPegawaiByIdAsync(pegawaiId);
                if (pegawai == null)
                {
                    throw new InvalidOperationException("Pegawai tidak ditemukan.");
                }
                reservasi.PegawaiId = pegawaiId;
                _context.Reservasis.Update(reservasi);
                await _context.SaveChangesAsync();
                return reservasi;
            }
            return null;
        }

        #endregion // RESERVASI

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
                    m.Kategori.ToLower() == kategori.ToString().ToLower())
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
                    m.Kategori.ToLower() == kategori.ToString().ToLower() && m.StokTersedia > 0)
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
                    m.Kategori.ToLower() == kategori.ToString().ToLower() && m.StokTersedia >= stokMinimal)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil menu berdasarkan nama menu (pencarian).
        /// </summary>
        /// <param name="nama"></param>
        /// <returns></returns>
        public async Task<List<Menu>> GetMenusByNamaAsync(string nama)
        {
            return await _context.Menus
                .Where(m => m.NamaMenu.ToLower().Contains(nama.ToLower()))
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil menu berdasarkan nama menu (pencarian) dan hanya yang tersedia (stok > 0).
        /// </summary>
        /// <param name="nama"></param>
        /// <returns></returns>
        public async Task<List<Menu>> GetMenusByNamaDanTersediaAsync(string nama)
        {
            return await _context.Menus
                .Where(m => m.NamaMenu.ToLower().Contains(nama.ToLower()) && m.StokTersedia > 0)
                .ToListAsync();
        }

        /// <summary>
        /// MEnambahkan menu baru.
        /// </summary>
        /// <param name="menuBaru"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<Menu?> SetMenuBaruAsync(Menu menuBaru)
        {
            if (menuBaru == null)
            {
                throw new ArgumentNullException(nameof(menuBaru), "Menu baru tidak boleh null.");
            }
            // Pastikan menu baru memiliki nama unik
            var existingMenu = await GetMenusByNamaAsync(menuBaru.NamaMenu);
            if (existingMenu.Any())
            {
                throw new InvalidOperationException($"Menu dengan nama {menuBaru.NamaMenu} sudah ada.");
            }
            _context.Menus.Add(menuBaru);
            await _context.SaveChangesAsync();
            return menuBaru;
        }

        /// <summary>
        /// Mengubah ketersediaan menu yang sudah ada.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tersediaKah"></param>
        /// <returns></returns>
        public async Task<Menu?> SetMenuTersediaAsync(int id, bool tersediaKah)
        {
            int tersedia = tersediaKah ? 1 : 0; // 1 untuk tersedia, 0 untuk tidak tersedia
            var menu = await GetMenuByIdAsync(id);
            if (menu != null)
            {
                menu.TersediaKah = tersedia;
                _context.Menus.Update(menu);
                await _context.SaveChangesAsync();

                return menu;
            }
            return null;
        }

        /// <summary>
        /// Mengubah ketersediaan menu yang sudah ada berdasarkan objek Menu.
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="jumlahStok"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task<Menu?> SetMenuStokTersediaAsync(Menu menu, int jumlahStok)
        {
            if (menu == null)
            {
                throw new ArgumentNullException(nameof(menu), "Menu tidak boleh null.");
            }
            if (jumlahStok < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(jumlahStok), "Stok tidak boleh negatif.");
            }
            menu.StokTersedia = jumlahStok;
            _context.Menus.Update(menu);
            await _context.SaveChangesAsync();
            return menu;
        }

        /// <summary>
        /// Mengubah stok tersedia menu berdasarkan ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="jumlahStok"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task<Menu?> SetMenuStokTersediaAsync(int id, int jumlahStok)
        {
            if (jumlahStok < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(jumlahStok), "Stok tidak boleh negatif.");
            }
            var menu = await GetMenuByIdAsync(id);
            if (menu != null)
            {
                menu.StokTersedia = jumlahStok;
                _context.Menus.Update(menu);
                await _context.SaveChangesAsync();
                return menu;
            }
            return null;
        }

        #endregion // MENUS

        #region MENUBAHAN

        /// <summary>
        /// Mengambil daftar menu bahan.
        /// </summary>
        /// <returns></returns>
        public async Task<List<MenuBahan>> GetMenubahansAsync()
        {
            return await _context.MenuBahans.ToListAsync();
        }

        /// <summary>
        /// Mengambil menu bahan berdasarkan ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<MenuBahan?> GetMenuBahanByIdAsync(int id)
        {
            return await _context.MenuBahans.FindAsync(id);
        }

        /// <summary>
        /// Mengambil daftar menu bahan berdasarkan ID Menu.
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        public async Task<List<MenuBahan>> GetMenuBahansByMenuIdAsync(int menuId)
        {
            return await _context.MenuBahans
                .Where(mb => mb.MenuId == menuId)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar menu bahan berdasarkan nama bahan.
        /// </summary>
        /// <param name="menuBahanBaru"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<MenuBahan?> SetMenuBahanBaruAsync(MenuBahan menuBahanBaru)
        {
            if (menuBahanBaru == null)
            {
                throw new ArgumentNullException(nameof(menuBahanBaru), "Menu bahan baru tidak boleh null.");
            }
            _context.MenuBahans.Add(menuBahanBaru);
            await _context.SaveChangesAsync();
            return menuBahanBaru;
        }

        #endregion // MENUBAHAN

        #region STOKBAHAN

        /// <summary>
        /// Mengambil daftar stok bahan.
        /// </summary>
        /// <returns></returns>
        public async Task<List<StokBahan>> GetStokBahansAsync()
        {
            return await _context.StokBahans.ToListAsync();
        }

        /// <summary>
        /// Mengambil stok bahan berdasarkan ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<StokBahan?> GetStokBahanByIdAsync(int id)
        {
            return await _context.StokBahans.FindAsync(id);
        }

        /// <summary>
        /// Mengambil daftar stok bahan berdasarkan ID Bahan.
        /// </summary>
        /// <param name="bahanId"></param>
        /// <returns></returns>
        public async Task<List<StokBahan>> GetStokBahansByBahanIdAsync(int bahanId)
        {
            return await _context.StokBahans
                .Where(sb => sb.BahanId == bahanId)
                .ToListAsync();
        }

        /// <summary>
        /// Menambahkan stok bahan baru.
        /// </summary>
        /// <param name="stokBahanBaru"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<StokBahan?> SetStokBahanBaruAsync(StokBahan stokBahanBaru)
        {
            if (stokBahanBaru == null)
            {
                throw new ArgumentNullException(nameof(stokBahanBaru), "Stok bahan baru tidak boleh null.");
            }
            _context.StokBahans.Add(stokBahanBaru);
            await _context.SaveChangesAsync();
            return stokBahanBaru;
        }

        /// <summary>
        /// Mengubah jumlah stok bahan berdasarkan ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="jumlahStok"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task<StokBahan?> SetStokBahanAsync(int id, int jumlahStok)
        {
            if (jumlahStok < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(jumlahStok), "Stok tidak boleh negatif.");
            }
            var stokBahan = await GetStokBahanByIdAsync(id);
            if (stokBahan != null)
            {
                stokBahan.JumlahStok = jumlahStok;
                _context.StokBahans.Update(stokBahan);
                await _context.SaveChangesAsync();
                return stokBahan;
            }
            return null;
        }

        /// <summary>
        /// Mengubah ketersediaan stok bahan berdasarkan apakah tersedia atau tidak.
        /// </summary>
        /// <param name="tersediaKah"></param>
        /// <returns></returns>
        public async Task<StokBahan?> SetStokBahanTersediaAsync(bool tersediaKah)
        {
            int tersedia = tersediaKah ? 1 : 0; // 1 untuk tersedia, 0 untuk tidak tersedia
            var stokBahan = await _context.StokBahans.FirstOrDefaultAsync(sb => sb.TersediaKah == tersedia);
            if (stokBahan != null)
            {
                stokBahan.TersediaKah = tersedia;
                _context.StokBahans.Update(stokBahan);
                await _context.SaveChangesAsync();
                return stokBahan;
            }
            return null;
        }

        #endregion // STOKBAHAN

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
            string tanggalStr = tanggal.ToString("yyyy-MM-dd");
            return await _context.Pesanans
                .Include(p => p.Meja)
                .Where(p => p.TanggalPesanan.Substring(0, 10) == tanggalStr)
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
                .Where(p => p.StatusPesanan.ToLower() == statusPesanan.ToString().ToLower())
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
            string tanggalStr = tanggal.ToString("yyyy-MM-dd");
            return await _context.Pesanans
                .Where(p => p.PegawaiId == pegawaiId && p.TanggalPesanan.Substring(0, 10) == tanggalStr)
                .ToListAsync();
        }

        /// <summary>
        /// Menambahkan pesanan baru.
        /// </summary>
        /// <param name="pesananBaru"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<List<Pesanan>> SetPesananBaruAsync(Pesanan pesananBaru)
        {
            if (pesananBaru == null)
            {
                throw new ArgumentNullException(nameof(pesananBaru), "Pesanan baru tidak boleh null.");
            }
            _context.Pesanans.Add(pesananBaru);
            await _context.SaveChangesAsync();
            return await GetPesanansByMejaIdAsync(pesananBaru.MejaId);
        }

        /// <summary>
        /// Mengubah status pesanan berdasarkan ID.
        /// </summary>
        /// <param name="pesananId"></param>
        /// <param name="statusPesanan"></param>
        /// <returns></returns>
        public async Task<Pesanan?> SetPesananStatusAsync(int pesananId, StatusPesanan statusPesanan)
        {
            var pesanan = await GetPesananByIdAsync(pesananId);
            if (pesanan != null)
            {
                pesanan.StatusPesanan = statusPesanan.ToString();
                _context.Pesanans.Update(pesanan);
                await _context.SaveChangesAsync();
                return pesanan;
            }
            return null;
        }

        /// <summary>
        /// Mengubah status pesanan berdasarkan objek Pesanan.
        /// </summary>
        /// <param name="pesananId"></param>
        /// <param name="dibayarKah"></param>
        /// <returns></returns>
        public async Task<Pesanan?> SetPesananDibayarAsync(int pesananId, bool dibayarKah)
        {
            var pesanan = await GetPesananByIdAsync(pesananId);
            if (pesanan != null)
            {
                pesanan.DibayarKah = dibayarKah ? 1 : 0; // 1 untuk sudah dibayar, 0 untuk belum
                _context.Pesanans.Update(pesanan);
                await _context.SaveChangesAsync();
                return pesanan;
            }
            return null;
        }
        #endregion // PESANAN

        #region DETAIL_PESANAN

        /// <summary>
        /// Mengambil daftar detail pesanan.
        /// </summary>
        /// <param name="pesananId"></param>
        /// <returns></returns>
        public async Task<List<DetailPesanan>> GetDetailPesanansByPesananIdAsync(int pesananId)
        {
            return await _context.DetailPesanans
                .Include(dp => dp.Menu)
                .Where(dp => dp.PesananId == pesananId)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil detail pesanan berdasarkan ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DetailPesanan?> GetDetailPesananByIdAsync(int id)
        {
            return await _context.DetailPesanans
                .Include(dp => dp.Menu)
                .FirstOrDefaultAsync(dp => dp.DetailPesananId == id);
        }

        /// <summary>
        /// Mengambil daftar detail pesanan berdasarkan ID Menu.
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        public async Task<List<DetailPesanan>> GetDetailPesanansByMenuIdAsync(int menuId)
        {
            return await _context.DetailPesanans
                .Include(dp => dp.Menu)
                .Where(dp => dp.MenuId == menuId)
                .ToListAsync();
        }

        /// <summary>
        /// Menambahkan detail pesanan baru.
        /// </summary>
        /// <param name="detailPesananBaru"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<DetailPesanan?> SetDetailPesananBaruAsync(DetailPesanan detailPesananBaru)
        {
            if (detailPesananBaru == null)
            {
                throw new ArgumentNullException(nameof(detailPesananBaru), "Detail pesanan baru tidak boleh null.");
            }
            _context.DetailPesanans.Add(detailPesananBaru);
            await _context.SaveChangesAsync();
            return detailPesananBaru;
        }

        /// <summary>
        /// Mengubah jumlah detail pesanan berdasarkan ID.
        /// </summary>
        /// <param name="detailPesananId"></param>
        /// <param name="jumlah"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task<DetailPesanan?> SetDetailPesananJumlahAsync(int detailPesananId, int jumlah)
        {
            if (jumlah < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(jumlah), "Jumlah tidak boleh negatif.");
            }
            var detailPesanan = await GetDetailPesananByIdAsync(detailPesananId);
            if (detailPesanan != null)
            {
                detailPesanan.Jumlah = jumlah;
                _context.DetailPesanans.Update(detailPesanan);
                await _context.SaveChangesAsync();
                return detailPesanan;
            }
            return null;
        }

        /// <summary>
        /// Mengubah harga satuan detail pesanan berdasarkan ID.
        /// </summary>
        /// <param name="detailPesananId"></param>
        /// <param name="hargaSatuan"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task<DetailPesanan?> SetDetailPesananHargaSatuanAsync(int detailPesananId, double hargaSatuan)
        {
            if (hargaSatuan < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hargaSatuan), "Harga tidak boleh negatif.");
            }
            var detailPesanan = await GetDetailPesananByIdAsync(detailPesananId);
            if (detailPesanan != null)
            {
                detailPesanan.HargaSatuan = hargaSatuan;
                _context.DetailPesanans.Update(detailPesanan);
                await _context.SaveChangesAsync();
                return detailPesanan;
            }
            return null;
        }
        #endregion // DETAIL_PESANAN

        #region PEMBAYARAN

        /// <summary>
        /// Mengambil daftar pembayaran.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Pembayaran?> GetPembayaranByIdAsync(int id)
        {
            return await _context.Pembayarans
                .Include(p => p.Pesanan)
                .Include(p => p.Pegawai)
                .FirstOrDefaultAsync(p => p.PembayaranId == id);
        }

        /// <summary>
        /// Mengambil daftar pembayaran berdasarkan ID Pesanan.
        /// </summary>
        /// <param name="pesananId"></param>
        /// <returns></returns>
        public async Task<List<Pembayaran>> GetPembayaransByPesananIdAsync(int pesananId)
        {
            return await _context.Pembayarans
                .Where(p => p.PesananId == pesananId)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar pembayaran berdasarkan ID Pegawai.
        /// </summary>
        /// <param name="pegawaiId"></param>
        /// <returns></returns>
        public async Task<List<Pembayaran>> GetPembayaransByPegawaiIdAsync(int pegawaiId)
        {
            return await _context.Pembayarans
                .Where(p => p.PegawaiId == pegawaiId)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar pembayaran berdasarkan Tanggal Pembayaran.
        /// </summary>
        /// <param name="tanggal"></param>
        /// <returns></returns>
        public async Task<List<Pembayaran>> GetPembayaransByTanggalAsync(DateTime tanggal)
        {
            string tanggalStr = tanggal.ToString("yyyy-MM-dd");
            return await _context.Pembayarans
                .Where(p => p.TanggalBayar.Substring(0, 10) == tanggalStr)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar pembayaran berdasarkan Metode Bayar.
        /// </summary>
        /// <param name="metodeBayar"></param>
        /// <returns></returns>
        public async Task<List<Pembayaran>> GetPembayaransByMetodeBayarAsync(string metodeBayar)
        {
            return await _context.Pembayarans
                .Where(p => p.MetodeBayar.ToLower() == metodeBayar.ToLower())
                .ToListAsync();
        }

        public async Task<List<Pembayaran>> GetPembayaransByBerhasilKahAsync(bool berhasilKah)
        {
            return await _context.Pembayarans
                .Where(p => p.BerhasilKah == (berhasilKah ? 1 : 0))
                .ToListAsync();
        }

        /// <summary>
        /// Menambahkan pembayaran baru.
        /// </summary>
        /// <param name="pembayaranBaru"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<Pembayaran?> SetPembayaranBaruAsync(Pembayaran pembayaranBaru)
        {
            if (pembayaranBaru == null)
            {
                throw new ArgumentNullException(nameof(pembayaranBaru), "Pembayaran baru tidak boleh null.");
            }
            _context.Pembayarans.Add(pembayaranBaru);
            await _context.SaveChangesAsync();
            return pembayaranBaru;
        }

        /// <summary>
        /// Mengubah status pembayaran berdasarkan ID.
        /// </summary>
        /// <param name="pembayaranId"></param>
        /// <param name="berhasilKah"></param>
        /// <returns></returns>
        public async Task<Pembayaran?> SetPembayaranBerhasilAsync(int pembayaranId, bool berhasilKah)
        {
            var pembayaran = await GetPembayaranByIdAsync(pembayaranId);
            if (pembayaran != null)
            {
                pembayaran.BerhasilKah = berhasilKah ? 1 : 0; // 1 untuk berhasil, 0 untuk gagal
                _context.Pembayarans.Update(pembayaran);
                await _context.SaveChangesAsync();
                return pembayaran;
            }
            return null;
        }

        #endregion // PEMBAYARAN

        #region ULASAN

        public async Task<List<Ulasan>> GetUlasansAsync()
        {
            return await _context.Ulasans
                .Include(u => u.Pesanan)
                .ToListAsync();
        }

        public async Task<Ulasan?> GetUlasanByIdAsync(int id)
        {
            return await _context.Ulasans
                .Include(u => u.Pesanan)
                .FirstOrDefaultAsync(u => u.UlasanId == id);
        }

        public async Task<List<Ulasan>> GetUlasansByTanggalAsync(DateTime tanggal)
        {
            string tanggalStr = tanggal.ToString("yyyy-MM-dd");
            return await _context.Ulasans
                .Where(u => u.TanggalUlasan.Substring(0, 10) == tanggalStr)
                .ToListAsync();
        }

        public async Task<Ulasan?> SetUlasanBaruAsync(Ulasan ulasanBaru)
        {
            if (ulasanBaru == null)
            {
                throw new ArgumentNullException(nameof(ulasanBaru), "Ulasan baru tidak boleh null.");
            }
            _context.Ulasans.Add(ulasanBaru);
            await _context.SaveChangesAsync();
            return ulasanBaru;
        }

        #endregion // ULASAN

        #region LOGAKTIVITAS

        /// <summary>
        /// Mengambil daftar log aktivitas.
        /// </summary>
        /// <returns></returns>
        public async Task<List<LogAktivita>> GetLogAktivitasAsync()
        {
            return await _context.LogAktivitas
                .Include(l => l.Pegawai)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil log aktivitas berdasarkan ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<LogAktivita?> GetLogAktivitasByIdAsync(int id)
        {
            return await _context.LogAktivitas
                .Include(l => l.Pegawai)
                .FirstOrDefaultAsync(l => l.LogId == id);
        }

        /// <summary>
        /// Menambahkan log aktivitas baru.
        /// </summary>
        /// <param name="logAktivitasBaru"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<LogAktivita?> SetLogAktivitasBaruAsync(LogAktivita logAktivitasBaru)
        {
            if (logAktivitasBaru == null)
            {
                throw new ArgumentNullException(nameof(logAktivitasBaru), "Log aktivitas baru tidak boleh null.");
            }
            _context.LogAktivitas.Add(logAktivitasBaru);
            await _context.SaveChangesAsync();
            return logAktivitasBaru;
        }

        /// <summary>
        /// Mengambil daftar log aktivitas berdasarkan ID Pegawai.
        /// </summary>
        /// <param name="pegawaiId"></param>
        /// <returns></returns>
        public async Task<List<LogAktivita>> GetLogAktivitasByPegawaiIdAsync(int pegawaiId)
        {
            return await _context.LogAktivitas
                .Where(l => l.PegawaiId == pegawaiId)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar log aktivitas berdasarkan tanggal aktivitas.
        /// </summary>
        /// <param name="tanggal"></param>
        /// <returns></returns>
        public async Task<List<LogAktivita>> GetLogAktivitasByTanggalAsync(DateTime tanggal)
        {
            return await _context.LogAktivitas
                .Where(l => l.TanggalAktivitas.Date == tanggal.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar log aktivitas berdasarkan apakah berhasil atau tidak.
        /// </summary>
        /// <param name="berhasilKah"></param>
        /// <returns></returns>
        public async Task<List<LogAktivita>> GetLogAktivitasByBerhasilKahAsync(bool berhasilKah)
        {
            return await _context.LogAktivitas
                .Where(l => l.BerhasilKah == (berhasilKah ? 1 : 0))
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar log aktivitas berdasarkan ID Pegawai dan Tanggal Aktivitas.
        /// </summary>
        /// <param name="pegawaiId"></param>
        /// <param name="tanggal"></param>
        /// <returns></returns>
        public async Task<List<LogAktivita>> GetLogAktivitasByPegawaiIdDanTanggalAsync(int pegawaiId, DateTime tanggal)
        {
            return await _context.LogAktivitas
                .Where(l => l.PegawaiId == pegawaiId && l.TanggalAktivitas.Date == tanggal.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Mengambil daftar log aktivitas berdasarkan deskripsi aktivitas.
        /// </summary>
        /// <param name="deskripsi"></param>
        /// <returns></returns>
        public async Task<List<LogAktivita>> GetLogAktivitasByDeskripsiAsync(string deskripsi)
        {
            return await _context.LogAktivitas
                .Where(l => l.Deskripsi.ToLower().Contains(deskripsi.ToLower()))
                .ToListAsync();
        }

        #endregion // LOGAKTIVITAS
    }
}
