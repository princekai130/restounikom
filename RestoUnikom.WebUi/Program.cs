using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using RestoUnikom.Data;
using RestoUnikom.Data.Models;
using RestoUnikom.WebUi.Components;

namespace RestoUnikom.WebUi
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

            // Registrasi repository
            builder.Services.AddScoped<RepositoriResto>();

            // Add MudBlazor services
            builder.Services.AddMudServices();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Pastikan folder Datas ada di output directory
            var datasPath = Path.Combine(AppContext.BaseDirectory, "Datas");
            if (!Directory.Exists(datasPath))
            {
                Directory.CreateDirectory(datasPath);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(datasPath),
                RequestPath = "/datas"
            });

            app.Run();
        }

        // Helper method untuk membuat absolute connection string
        static string BuatkanAbsoluteConnectionStringSqlite(string connectionString, string baseDirectory)
        {
            var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);

            if (!Path.IsPathRooted(builder.DataSource))
            {
                var absolutePath = Path.Combine(baseDirectory, builder.DataSource);

                // Pastikan direktori ada
                var directory = Path.GetDirectoryName(absolutePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Cek file database
                if (!File.Exists(absolutePath))
                {
                    throw new FileNotFoundException($"Database tidak ditemukan di: {absolutePath}");
                }

                builder.DataSource = absolutePath;
            }

            return builder.ToString();
        }
    }
}
