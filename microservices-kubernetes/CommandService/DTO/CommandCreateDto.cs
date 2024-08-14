using System.ComponentModel.DataAnnotations;

namespace CommandService.DTO;

public class CommandCreateDto
{
    [Required]
    public string HowTo { get; set; }    
    [Required]
    public string CommandLine { get; set; }    
}