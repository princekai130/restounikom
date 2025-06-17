using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestoUnikom.Data;
using RestoUnikom.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestoUnikom.Data.Tests
{
    [TestClass()]
    public class RepositoriRestoTests
    {
        // Ganti dengan connection string yang sesuai untuk testing
        private const string ConnectionString = "Data Source=Datas/restodb_dev.sqlite;";

        /// <summary>
        /// Membuat opsi DbContext untuk testing.
        /// </summary>
        /// <returns></returns>
        private DbContextOptions<RestoDataContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<RestoDataContext>()
                .UseSqlite(ConnectionString)
                .Options;
        }

        [TestMethod()]
        public async Task GetMenusAsyncTest()
        {
            var options = CreateOptions();
            using (var context = new RestoDataContext(options))
            {
                var repo = new RepositoriResto(context);
                var result = await repo.GetMenusAsync();
                Assert.IsTrue(result.Count > 0, "Database test harus berisi data menu.");
            }
        }

    }
}