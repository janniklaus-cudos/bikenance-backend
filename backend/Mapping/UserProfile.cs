using AutoMapper;
using Backend.Dtos;
using Backend.Models;

namespace Backend.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();

    }
}