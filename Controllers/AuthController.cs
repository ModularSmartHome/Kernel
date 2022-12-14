using Kernel.Models.Database;
using Kernel.Models.Request;
using Kernel.Modules;
using Kernel.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Kernel.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{

    private readonly DbAccess Db;
    private readonly IAuthorization auth;

    public AuthController(DbAccess db, IAuthorization _auth)
    {
        Db = db;
        auth = _auth;
    }

    /// <summary>
    /// Logs in a user
    /// </summary>
    /// <param name="creds">The login credentials</param>
    /// <returns>The http response that indicates the login status</returns>
    [HttpPost("[action]")]
    public async Task<ActionResult> Login([FromBody] LoginRequest creds)
    {
        var loginSuccessful = await Db.UserRepository.LoginUser(creds);
        if (!loginSuccessful)
        {
            return Unauthorized();
        }

        var user = await Db.UserRepository.FindUserByUsername(creds.Username);
        
        var sessionJwt = auth.GetAuthToken(new AuthClaims(userId: user!.Id));
        var cookieOptions = new CookieOptions()
        {
            HttpOnly = true,
            MaxAge = Constants.SESSION_EXPIRATION,
            Secure = true,
            SameSite = SameSiteMode.Lax
        };
        Response.Cookies.Append(Constants.SESSION_COOKIE_NAME, sessionJwt, cookieOptions);
        Response.Headers.AccessControlExposeHeaders = "Set-Cookie";
        return Ok();
    } 
}