using System.ComponentModel.DataAnnotations;

namespace DB.Entities;

public class ModuleStatistics
{
    [Key]
    public string ModuleCategoryID { get; set; } = "";
    public string ModuleState { get; set; } = "";
}