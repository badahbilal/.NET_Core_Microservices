using AutoMapper;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Controllers;

[Route("api/cart")]
[ApiController]
public class CartAPIController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IProductService _productService;
    private ResponseDto _response;

    public CartAPIController(AppDbContext db, IMapper mapper, IProductService productService)
    {
        _db = db;
        _mapper = mapper;
        _productService = productService;
        _response = new ResponseDto();
    }

    [HttpGet("GetCart/{userId}")]
    public async Task<ResponseDto> GetCart(string userId)
    {
        try
        {
            CartDto cart = new()
            {
                CartHeader = _mapper.Map<CartHeaderDto>(await _db.CartHeaders.FirstAsync(u => u.UserId == userId))
            };

            cart.CartDetails = _mapper.Map<IEnumerable<CartDetailsDto>>(
                await _db.CartDetails.Where(u => u.CartHeaderId == cart.CartHeader.CartHeaderId).ToListAsync());

            IEnumerable<ProductDto> productDtos = await _productService.GetProducts();

            foreach (var item in cart.CartDetails)
            {
                item.Product = productDtos.FirstOrDefault(u => u.ProductId == item.ProductId);
                if (item.Product != null)
                {
                    cart.CartHeader.CartTotal += (item.Count * item.Product.Price);
                }
            }

            _response.Result = cart;
            _response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message.ToString();
        }

        return _response;
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

    [HttpPost("ApplyCoupon")]
    public async Task<ResponseDto> ApplyCoupon([FromBody] CartDto cartDto)
    {
        try
        {
            var cartFromDb = await _db.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
            cartFromDb.CouponCode = cartDto.CartHeader.CouponCode;
            _db.CartHeaders.Update(cartFromDb);
            await _db.SaveChangesAsync();

            _response.Result = true;
            _response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message.ToString();
        }
        return _response;
    }

    [HttpPost("RemoveCoupon")]
    public async Task<ResponseDto> RemoveCoupon([FromBody] CartDto cartDto)
    {
        try
        {
            var cartFromDb = await _db.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
            cartFromDb.CouponCode = "";
            _db.CartHeaders.Update(cartFromDb);
            await _db.SaveChangesAsync();

            _response.Result = true;
            _response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message.ToString();
        }
        return _response;
    }
}

