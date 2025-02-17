using Microsoft.AspNetCore.Mvc;

namespace SkillBridgeAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SignInController : ControllerBase
    {
        public SignInController() 
        {
            
        }
        [HttpPost]
        public async Task<IResult> Post()
        {

            return Results.Ok();
        }
    }
}
