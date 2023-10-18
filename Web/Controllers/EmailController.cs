using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Controllers;


[Route("[controller]")]
[ApiController]
public class EmailController : ControllerBase
{
    private readonly EmailService _emailService;

    public EmailController(EmailService emailService)
    {
        _emailService = emailService;
    }
}