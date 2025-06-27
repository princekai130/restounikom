using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using RestoUnikom.Data;
using RestoUnikom.Data.Models;
using RestoUnikom.WebUi2.Authentication;
using RestoUnikom.WebUi2.Components;
using RestoUnikom.WebUi2.Hubs;

namespace RestoUnikom.WebUi2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Ambil connection string dari konfigurasi dan buat absolute path
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            var absoluteConnectionString = BuatkanAbsoluteConnectionStringSqlite(connectionString, AppContext.BaseDirectory);

            // Registrasi DbContext RestoDataContext
            builder.Services.AddDbContext<RestoDataContext>(options =>
                options.UseSqlite(absoluteConnectionString));

            // Registrasi repositori
            builder.Services.AddScoped<RepositoriResto>();

            // Tambahkan ProtectedSessionStorage
            builder.Services.AddScoped<ProtectedSessionStorage>();

            // Tambahkan IHttpContextAccessor (opsional, tidak digunakan lagi di CustomAuthenticationStateProvider)
            builder.Services.AddHttpContextAccessor();

            // Add MudBlazor services
            builder.Services.AddMudServices();

            // Add services to the container
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Tambahkan CascadingAuthenticationState untuk mendukung autentikasi Blazor
            builder.Services.AddCascadingAuthenticationState();

            // OTENTIKASI
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "CustomScheme";
                options.DefaultChallengeScheme = "CustomScheme";
            }).AddCookie("CustomScheme");

            // Registrasi CustomAuthenticationStateProvider
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
            builder.Services.AddScoped<CustomAuthenticationStateProvider>();

            // Tambahkan SignalR
            builder.Services.AddSignalR();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Registrasi endpoint SignalR
            app.MapHub<MejaHub>("/mejahub");

            // Pastikan folder Datas dan GambarMenu ada di output directory
            var datasPath = Path.Combine(AppContext.BaseDirectory, "Datas");
            if (!Directory.Exists(datasPath))
            {
                Directory.CreateDirectory(datasPath);
            }
            var gambarMenuPath = Path.Combine(datasPath, "GambarMenu");
            if (!Directory.Exists(gambarMenuPath))
            {
                Directory.CreateDirectory(gambarMenuPath);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(gambarMenuPath),
                RequestPath = "/datas/GambarMenu"
            });

            app.Run();
        }

        private static string BuatkanAbsoluteConnectionStringSqlite(string connectionString, string baseDirectory)
        {
            var builder = new SqliteConnectionStringBuilder(connectionString);

            if (!Path.IsPathRooted(builder.DataSource))
            {
                var absolutePath = Path.Combine(baseDirectory, builder.DataSource);

                var directory = Path.GetDirectoryName(absolutePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(absolutePath))
                {
                    throw new FileNotFoundException($"Database tidak ada di: {absolutePath}");
                }

                builder.DataSource = absolutePath;
            }

            return builder.ToString();
        }
    }
}
