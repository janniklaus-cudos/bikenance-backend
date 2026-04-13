namespace Backend.Dtos;

public record BikeDto(
	Guid Id,
	string Name,
	string Brand,
	int IconId,
	DateTime CreatedAtUtc,
	List<BikePartDto> Parts
);