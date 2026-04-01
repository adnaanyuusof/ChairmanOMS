using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ChairmanOMS.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ChairmanOMS.Diagnostics
{
    public class DbCheck
    {
        public static void Run(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    var connectionString = "Host=dpg-d71gpdcr85hc73a119i0-a.virginia-postgres.render.com;Database=chairmanoms_db_bqpa;Username=chairmanoms_db_bqpa_user;Password=Ub1Xny0iZQ5NB2sRg2zRFFPepjN8Wrna;Port=5432;SSL Mode=Require;Trust Server Certificate=true";
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseNpgsql(connectionString));
                })
                .Build();

            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var conn = db.Database.GetDbConnection();
                conn.Open();

                foreach(var table in new[] { "IncomingDocuments", "OutgoingDocuments", "Appointments" })
                {
                    Console.WriteLine($"\n--- Columns for [{table}] ---");
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT '\"' || column_name || '\"' FROM information_schema.columns WHERE table_name = '{table}';";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine(reader.GetString(0));
                            }
                        }
                    }
                }
            }
        }
    }
}
