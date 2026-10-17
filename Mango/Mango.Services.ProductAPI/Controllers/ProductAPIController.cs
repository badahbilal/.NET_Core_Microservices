using AutoMapper;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ProductAPI.Controllers;

[Route("api/product")]
[ApiController]
public class ProductAPIController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private ResponseDto _response;

    public ProductAPIController(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
        _response = new ResponseDto();
    }

    [HttpGet]
    public async Task<ResponseDto> Get()
    {
        try
        {
            IEnumerable<Product> objList = await _db.Products.ToListAsync();
            _response.Result = _mapper.Map<IEnumerable<ProductDto>>(objList);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message;
        }

        return _response;
    }

    [HttpGet("{id:int}")]
    public async Task<ResponseDto> Get(int id)
    {
        try
        {
            Product? obj = await _db.Products.FirstOrDefaultAsync(u => u.ProductId == id);
            if (obj == null)
            {
                _response.IsSuccess = false;
                _response.Message = "Product not found";
            }
            else
            {
                _response.Result = _mapper.Map<ProductDto>(obj);
            }
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message;
        }

        return _response;
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<ResponseDto> Post([FromBody] ProductDto productDto)
    {
        try
        {
            Product product = _mapper.Map<Product>(productDto);
            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            _response.Result = _mapper.Map<ProductDto>(product);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message;
        }

        return _response;
    }

    [HttpPut]
    [Authorize(Roles = "ADMIN")]
    public async Task<ResponseDto> Put([FromBody] ProductDto productDto)
    {
        try
        {
            Product product = _mapper.Map<Product>(productDto);
            _db.Products.Update(product);
            await _db.SaveChangesAsync();

            _response.Result = _mapper.Map<ProductDto>(product);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message;
        }

        return _response;
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ResponseDto> Delete(int id)
    {
        try
        {
            Product? product = await _db.Products.FirstOrDefaultAsync(u => u.ProductId == id);
            if (product == null)
            {
                _response.IsSuccess = false;
                _response.Message = "Product not found";
            }
            else
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message;
        }

        return _response;
    }
}

