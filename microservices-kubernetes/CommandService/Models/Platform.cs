namespace CommandService.Models;

public class Platform
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ExternalId { get; set; }
    public string Name { get; set; }
    public ICollection<Command> Commands { get; set; } = new List<Command>();
}