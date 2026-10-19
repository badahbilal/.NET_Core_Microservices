//117. Asynchronous Communication in Action
using Mango.Services.EmailAPI.Models.Dto;

namespace Mango.Services.EmailAPI.Services;

public interface IEmailService
{
    Task EmailCartAndLog(CartDto cartDto);

    // Step 1 : Register user email and log
    Task RegisterUserEmailAndLog(string email);
}

