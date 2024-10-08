﻿using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PlatformService.Models;

namespace PlatformService.Data
{
    public static class PrepareDatabase
    {
        public static void PreparePopulation(IApplicationBuilder builder, bool isProduction)
        {
            using (var serviceScope = builder.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), isProduction); 
            }
        }

        private static void SeedData(AppDbContext appDbContext, bool isProduction)
        {
         //   if (isProduction)
            {
                Console.WriteLine("Applying migrations if any...");
                try
                {
                    appDbContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during applying migrations: {ex.Message}");
                }
            }

            if (!appDbContext.Platforms.Any())
            {
                Debug.WriteLine("Seeding!");

                appDbContext.Platforms.AddRange(
                    new Platform()
                    {
                        Name = ".NET",
                        Publisher = "Microsoft",
                        Cost = "Free"
                    },
                    new Platform()
                    {
                        Name = "SQL Server Express",
                        Publisher = "Microsoft",
                        Cost = "Free"
                    },
                    new Platform()
                    {
                        Name = "Kubernetes",
                        Publisher = "Cloud Native Computing Foundation",
                        Cost = "Free"
                    }
                );

                appDbContext.SaveChanges();
            }
            else
            {
                Debug.WriteLine("Nothing to seed. Data is already there!");
            }
        }
    }
}

