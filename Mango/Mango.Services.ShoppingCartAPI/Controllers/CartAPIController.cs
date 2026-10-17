using AutoMapper;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Controllers;

[Route("api/cart")]
[ApiController]
public class CartAPIController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private ResponseDto _response;

    public CartAPIController(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
        _response = new ResponseDto();
    }

    [HttpPost("CartUpsert")]
    public async Task<ResponseDto> CartUpsert([FromBody] CartDto cartDto)
    {
        try
        {
            var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking().FirstOrDefaultAsync(
                u => u.UserId == cartDto.CartHeader.UserId);

            if (cartHeaderFromDb == null)
            {
                // Create CartHeader and CartDetails
                CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                _db.CartHeaders.Add(cartHeader);
                await _db.SaveChangesAsync();

                cartDto.CartDetails!.First().CartHeaderId = cartHeader.CartHeaderId;
                CartDetails cartDetails = _mapper.Map<CartDetails>(cartDto.CartDetails!.First());
                _db.CartDetails.Add(cartDetails);
                await _db.SaveChangesAsync();
            }
            else
            {
                // CartHeader exists, check if CartDetails contains this product
                var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                    u => u.ProductId == cartDto.CartDetails!.First().ProductId &&
                         u.CartHeaderId == cartHeaderFromDb.CartHeaderId);

                if (cartDetailsFromDb == null)
                {
                    // Create CartDetails for existing CartHeader
                    cartDto.CartDetails!.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                    CartDetails cartDetails = _mapper.Map<CartDetails>(cartDto.CartDetails!.First());
                    _db.CartDetails.Add(cartDetails);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    // Update existing CartDetails quantity
                    cartDto.CartDetails!.First().Count += cartDetailsFromDb.Count;
                    cartDto.CartDetails!.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                    cartDto.CartDetails!.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;
                    CartDetails cartDetails = _mapper.Map<CartDetails>(cartDto.CartDetails!.First());
                    _db.CartDetails.Update(cartDetails);
                    await _db.SaveChangesAsync();
                }
            }

            _response.Result = cartDto;
        }
        catch (Exception ex)
        {
            _response.Message = ex.Message.ToString();
            _response.IsSuccess = false;
        }

        return _response;
    }

    [HttpPost("RemoveCart")]
    public async Task<ResponseDto> RemoveCart([FromBody] int cartDetailsId)
    {
        try
        {
            CartDetails cartDetails = await _db.CartDetails.FirstAsync(u => u.CartDetailsId == cartDetailsId);

            int totalCountOfCartItems = _db.CartDetails.Where(u => u.CartHeaderId == cartDetails.CartHeaderId).Count();
            _db.CartDetails.Remove(cartDetails);

            if (totalCountOfCartItems == 1)
            {
                var cartHeaderToRemove = await _db.CartHeaders
                    .FirstOrDefaultAsync(u => u.CartHeaderId == cartDetails.CartHeaderId);

                _db.CartHeaders.Remove(cartHeaderToRemove!);
            }
            await _db.SaveChangesAsync();

            _response.Result = true;
            _response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _response.Message = ex.Message.ToString();
            _response.IsSuccess = false;
        }

        return _response;
    }
}

