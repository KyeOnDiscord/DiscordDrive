namespace DiscordDrive;

internal sealed class DiscordFile
{
    public long FileSize { get; set; }
    public string FileName { get; set; }
    public string BaseURL { get; set; }
    public string SHA256 { get; set; }
    public List<Chunk> Chunks { get; set; } = new();
}
internal sealed class Chunk
{
    public int PartNumber { get; set; }
    public string Route { get; set; }
    public Chunk(int PartNumber, string Route)
    {
        this.PartNumber = PartNumber;
        this.Route = Route;
    }
}