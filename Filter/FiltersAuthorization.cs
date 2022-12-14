using Kernel.Controllers;
using Kernel.Modules;
using Kernel.Shared;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Kernel.Filter;

/// <summary>
/// Action filter attribute used to get the authorization token from the
/// requests cookies header and tries to validate this header and sets the
/// values for AuthClaims and AuthorizedUser for the <see cref="AuthorizedControllerBase"/>
/// instance. <br />
/// The request is canceled and an 401 Unauthorized response is returned when
/// either the token is not present in the cookie or if the token validation
/// failed.
/// </summary>
public class FiltersAuthorization : ActionFilterAttribute
{
    private readonly DatabaseContext db;
    private readonly IAuthorization auth;

    public FiltersAuthorization(DatabaseContext _db, IAuthorization _auth)
    {
        db = _db;
        auth = _auth;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controller = context.Controller as AuthorizedControllerBase;
        if (controller == null)
            throw new Exception("the controller is not an AuthorizedController");

        if (!context.HttpContext.Request.Cookies.TryGetValue(Constants.SESSION_COOKIE_NAME, out var token))
        {
            context.Result = controller.Unauthorized();
            return;
        }

        try
        {
            controller.AuthClaims = auth.ValidateAuth(token);
            controller.AuthorizedUser = await db.Users.FindAsync(controller.AuthClaims.UserId);
            if (controller.AuthorizedUser == null)
                throw new Exception("user not found");
        } catch
        {
            context.Result = controller.Unauthorized();
            return;
        }

        await next();
    }
}