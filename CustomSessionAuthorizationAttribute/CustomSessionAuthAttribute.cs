using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace CustomSessionAuthorizationAttribute
{
    public class CustomSessionAuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string sessionID = context.HttpContext.Session.GetString("SessionID");

            if (string.IsNullOrEmpty(sessionID) || sessionID != context.HttpContext.Session.Id)
            {
                context.Result = new RedirectToActionResult("Login", "Authentication", null);
            }

            base.OnActionExecuting(context);
        }
    }
}