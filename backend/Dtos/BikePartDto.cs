
using Backend.Models;

namespace Backend.Dtos;

public record BikePartDto(
	Guid Id,
	string Name,
	BikePartPosition Position,
	Guid BikeId
);