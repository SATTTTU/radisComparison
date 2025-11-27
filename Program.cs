using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Starting Redis vs DB Performance Demo ===");

            using var db = new AppDbContext();
            db.Database.EnsureCreated();

            try
            {
                string key = "bankAccountsCache";

                // STEP 1: Retrieve from DB
                Console.WriteLine("\nFetching from DATABASE...");
                var sw = Stopwatch.StartNew();

                var accountsFromDb = db.BankAccounts.Take(10000).ToList();

                sw.Stop();
                Console.WriteLine($"DB fetch time: {sw.ElapsedMilliseconds} ms");

                // STEP 2: Store in Redis
                Console.WriteLine("Caching 10,000 accounts in Redis...");
                await RedisCache.SetAsync(key, accountsFromDb);
                Console.WriteLine("Cache saved.");

                // STEP 3: Retrieve from Redis
                Console.WriteLine("\nFetching from REDIS cache...");
                sw.Restart();

                var accountsFromRedis = await RedisCache.GetAsync<List<BankAccount>>(key);

                sw.Stop();
                Console.WriteLine($"Redis fetch time: {sw.ElapsedMilliseconds} ms");

                // STEP 4: Print Sample
                Console.WriteLine("\nSample Output:");
                foreach (var acc in accountsFromRedis.Take(5))
                {
                    Console.WriteLine($"{acc.AccountHolder} | {acc.AccountNumber} | Balance: {acc.Balance}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error occurred:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\n=== Demo Completed ===");
        }
    }
}
