
namespace southafricantaxtool.Interface.Models;

public class ImportantDate
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public string Url { get; set; }
    public string AllDay { get; set; }
}