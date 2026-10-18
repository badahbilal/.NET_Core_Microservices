using System.IdentityModel.Tokens.Jwt;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<IActionResult> CartIndex()
    {
        return View(await LoadCartBasedOnLoggedInUser());
    }

    private async Task<CartDto> LoadCartBasedOnLoggedInUser()
    {
        var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
        ResponseDto? response = await _cartService.GetCartByUserIdAsync(userId!);

        if (response != null && response.IsSuccess)
        {
            CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result)) ?? new CartDto();
            return cartDto;
        }

        return new CartDto();
    }
}

