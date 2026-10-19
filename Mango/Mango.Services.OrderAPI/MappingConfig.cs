using AutoMapper;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;

namespace Mango.Services.OrderAPI;

public class MappingConfig : Profile
{
    public MappingConfig()
    {
        // 1. OrderHeaderDto <-> CartHeaderDto
        CreateMap<OrderHeaderDto, CartHeaderDto>()
            .ForMember(dest => dest.CartTotal, u => u.MapFrom(src => src.OrderTotal))
            .ReverseMap();

        // 2. CartDetailsDto -> OrderDetailsDto
        CreateMap<CartDetailsDto, OrderDetailsDto>()
            .ForMember(dest => dest.ProductName, u => u.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.Price, u => u.MapFrom(src => src.Product != null ? src.Product.Price : 0));

        // 3. OrderHeader <-> OrderHeaderDto
        CreateMap<OrderHeader, OrderHeaderDto>().ReverseMap();

        // 4. OrderDetails <-> OrderDetailsDto
        CreateMap<OrderDetails, OrderDetailsDto>().ReverseMap();
    }
}
