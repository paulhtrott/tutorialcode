﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GraphAPIDemo.TokenStorage;
using GraphAPIDemo.Models;
using Microsoft.Owin.Security.Cookies;
using System.Security.Claims;

namespace GraphAPIDemo.Controllers
{
    public abstract class BaseController : Controller
    {
        protected void Flash(string message, string debug = null)
        {
            var alerts = TempData.ContainsKey(Alert.AlertKey) ?
                (List<Alert>)TempData[Alert.AlertKey] :
                new List<Alert>();

            alerts.Add(new Alert
            {
                Message = message,
                Debug = debug
            });

            TempData[Alert.AlertKey] = alerts;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Request.IsAuthenticated)
            {
                // Get the signed in user's id and create a token cache
                string signedInUserId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
                SessionTokenStore tokenStore = new SessionTokenStore(signedInUserId, HttpContext);

                if (tokenStore.HasData())
                {
                    // Add the user to the view bag
                    ViewBag.User = tokenStore.GetUserDetails();
                }
                else
                {
                    // The session has lost data. This happens often
                    // when debugging. Log out so the user can log back in
                    Request.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
                    filterContext.Result = RedirectToAction("Index", "Home");
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}