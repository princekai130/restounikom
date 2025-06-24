using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using RestoUnikom.Data;
using RestoUnikom.Data.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RestoUnikom.WebUi2.Authentication
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly RepositoriResto _repo;
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(ProtectedSessionStorage sessionStorage, RepositoriResto repo)
        {
            _sessionStorage = sessionStorage;
            _repo = repo;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userSessionResult = await _sessionStorage.GetAsync<UserSession>("UserSession");
                if (userSessionResult.Success && userSessionResult.Value != null && !string.IsNullOrEmpty(userSessionResult.Value.NamaPengguna))
                {
                    var peran = userSessionResult.Value.PeranPegawai?.Trim() ?? string.Empty;
                    if (string.Equals(peran, "pelayan", StringComparison.OrdinalIgnoreCase))
                        peran = "Pelayan";
                    else if (string.Equals(peran, "admin", StringComparison.OrdinalIgnoreCase))
                        peran = "Admin";

                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, userSessionResult.Value.NamaPengguna),
                        new Claim(ClaimTypes.Role, peran)
                    };
                    var identity = new ClaimsIdentity(claims, "CustomAuth");
                    Console.WriteLine($"GetAuthenticationStateAsync: User = {userSessionResult.Value.NamaPengguna}, Role = {peran}");
                    return new AuthenticationState(new ClaimsPrincipal(identity));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAuthenticationStateAsync Error: {ex.Message}");
                return new AuthenticationState(_anonymous);
            }
            Console.WriteLine("GetAuthenticationStateAsync: Returning anonymous state");
            return new AuthenticationState(_anonymous);
        }

        public async Task LoginAsync(string namaPengguna, string kataSandi)
        {
            try
            {
                var pegawai = await _repo.GetPegawaiByNamaPenggunaDanKataSandi(namaPengguna, kataSandi);
                if (pegawai != null)
                {
                    var peran = pegawai.PeranPegawai?.Trim() ?? string.Empty;
                    if (string.Equals(peran, "pelayan", StringComparison.OrdinalIgnoreCase))
                        peran = "Pelayan";
                    else if (string.Equals(peran, "admin", StringComparison.OrdinalIgnoreCase))
                        peran = "Admin";

                    var userSession = new UserSession
                    {
                        NamaPengguna = pegawai.NamaPengguna,
                        PeranPegawai = peran
                    };
                    await _sessionStorage.SetAsync("UserSession", userSession);
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, pegawai.NamaPengguna),
                        new Claim(ClaimTypes.Role, peran)
                    };
                    var identity = new ClaimsIdentity(claims, "CustomAuth");
                    Console.WriteLine($"LoginAsync: User = {pegawai.NamaPengguna}, Role = {peran}");
                    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
                }
                else
                {
                    await _sessionStorage.DeleteAsync("UserSession");
                    Console.WriteLine("LoginAsync: Invalid credentials");
                    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
                    throw new Exception("Invalid credentials");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoginAsync Error: {ex.Message}");
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _sessionStorage.DeleteAsync("UserSession");
                Console.WriteLine("LogoutAsync: Session cleared");
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LogoutAsync Error: {ex.Message}");
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
            }
        }
    }

    public class UserSession
    {
        public string NamaPengguna { get; set; } = string.Empty;
        public string PeranPegawai { get; set; } = string.Empty;
    }
}