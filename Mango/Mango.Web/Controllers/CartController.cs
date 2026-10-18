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

    public async Task<IActionResult> Remove(int cartDetailsId)
    {
        ResponseDto? response = await _cartService.RemoveFromCartAsync(cartDetailsId);
        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Cart updated successfully";
            return RedirectToAction(nameof(CartIndex));
        }
        TempData["error"] = response?.Message;
        return RedirectToAction(nameof(CartIndex));
    }

    [HttpPost]
    public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
    {
        ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);
        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Cart updated successfully";
            return RedirectToAction(nameof(CartIndex));
        }
        TempData["error"] = response?.Message;
        return RedirectToAction(nameof(CartIndex));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
    {
        cartDto.CartHeader.CouponCode = "";
        ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);
        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Cart updated successfully";
            return RedirectToAction(nameof(CartIndex));
        }
        TempData["error"] = response?.Message;
        return RedirectToAction(nameof(CartIndex));
    }

    [HttpPost]
    [ActionName("EmailCart")]
    public async Task<IActionResult> EmailCart(CartDto cartDto)
    {
        CartDto cart = await LoadCartBasedOnLoggedInUser();

        cart.CartHeader.Email = User.Claims
            .FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email)?.Value;

        ResponseDto? response = await _cartService.EmailCart(cart);

        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Email will be processed and sent shortly.";
            return RedirectToAction(nameof(CartIndex));
        }

        TempData["error"] = response?.Message;
        return RedirectToAction(nameof(CartIndex));
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

