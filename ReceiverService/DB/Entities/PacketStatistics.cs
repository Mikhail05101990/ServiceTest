using System.ComponentModel.DataAnnotations;

namespace DB.Entities;

public class PacketStatistics
{
    [Key]
    public string PacketID { get; set; } = "";
    public int AvailabilityPercent { get; set; }
}