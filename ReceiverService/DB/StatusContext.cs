using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using DB.Entities;

namespace DbProviders;

public class StatusContext : DbContext
{
    public DbSet<ModuleStatistics> ModuleStatistics { get; set; }
    public DbSet<PacketStatistics> PacketStatistics { get; set; }

    public string DbPath { get; }

    public StatusContext()
    {
        DbPath = System.IO.Path.Join(AppDomain.CurrentDomain.BaseDirectory, "DB", "stat.db");
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath};");
    }
}



