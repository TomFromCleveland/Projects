﻿using Capstone.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Capstone.Web.Filters
{
    public class BaseAuthorizationFilter : ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            // Check to see if we have a username in the url
            if (filterContext.ActionParameters.ContainsKey("username"))
            {

                //gets the username from the url
                var impliedUsername = (string)filterContext.ActionParameters["username"];
                var controller = (BaseController)filterContext.Controller;
                var actualUsername = controller.CurrentUser;

                // If the user is not logged in, then take them to the login page
                if (!controller.IsAuthenticated)
                {
                    // then redirect to login page
                    var routeValue = new RouteValueDictionary(new
                    {
                        controller = "Users",
                        action = "Login",
                        landingPage = filterContext.HttpContext.Request.Url
                    });
                    filterContext.Result = new RedirectToRouteResult(routeValue);
                }
                else
                {
                    if (impliedUsername.ToLower() != actualUsername.ToLower()) //They're liars
                    {
                        filterContext.Result = new HttpStatusCodeResult(403);
                    }
                }

            }

            // Get the username from session as well
            //If the session username and url username match -> good
            //Else Send the user to a 403 page


            base.OnActionExecuting(filterContext);
        }
    }
}