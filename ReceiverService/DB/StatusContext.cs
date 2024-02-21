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
        var path = "DB";
        DbPath = System.IO.Path.Join(path, "stat.db");
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source=/home/misha/Документы/ServiceTest/ReceiverService/bin/Debug/net8.0/DB/stat.db;");
    }
}



