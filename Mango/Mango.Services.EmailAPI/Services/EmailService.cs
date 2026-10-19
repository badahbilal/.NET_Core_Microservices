//117. Asynchronous Communication in Action
using System.Text;
using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.EmailAPI.Services;

public class EmailService : IEmailService
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public EmailService(DbContextOptions<AppDbContext> dbOptions)
    {
        _dbOptions = dbOptions;
    }

    public async Task EmailCartAndLog(CartDto cartDto)
    {
        StringBuilder message = new();
        message.AppendLine("<br/>Cart Email Requested");
        message.AppendLine($"<br/>Total: {cartDto.CartHeader.CartTotal:c}");
        message.AppendLine("<br/>");
        message.AppendLine("<ul>");
        if (cartDto.CartDetails != null)
        {
            foreach (var item in cartDto.CartDetails)
            {
                message.AppendLine($"<li>{item.Product?.Name} x {item.Count}</li>");
            }
        }
        message.AppendLine("</ul>");

        await LogAndEmail(message.ToString(), cartDto.CartHeader.Email!);
    }

    private async Task<bool> LogAndEmail(string message, string email)
    {
        try
        {
            await using var db = new AppDbContext(_dbOptions);
            EmailLogger emailLog = new()
            {
                Email = email,
                EmailSent = DateTime.Now,
                Message = message
            };
            await db.EmailLoggers.AddAsync(emailLog);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
