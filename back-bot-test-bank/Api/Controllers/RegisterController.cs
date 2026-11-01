

using Api.DTO;
using Microsoft.AspNetCore.Mvc;
using Api.Controllers;

namespace Api.Controllers
{
        
    [ApiController]
    [Route("[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(ILogger<RegisterController> logger)
        {
            _logger = logger;
        }

        [HttpPost(Name = "MakeRegister")]
        public ActionResult MakeRegister(Register register)
        {
            throw new NotImplementedException();
        }

    }
}
