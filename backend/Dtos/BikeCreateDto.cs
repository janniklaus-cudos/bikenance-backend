namespace Backend.Dtos;

public record BikeCreateDto
{
    public string Name { get; init; } = "";
    public string Brand { get; init; } = "";
    public int IconId { get; init; } = 0;
    public List<BikePartCreateDto> Parts { get; init; } = [];
}