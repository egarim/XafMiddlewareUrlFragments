using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using XAF.Module.BusinessObjects;
using DevExpress.ExpressApp.Security.Authentication.Internal;
using Newtonsoft.Json;

namespace XAF.Blazor.Server
{
    public class AutoSignInMiddleware
    {
        private readonly RequestDelegate next;
        private IConfiguration configuration;
        public AutoSignInMiddleware(IConfiguration config, RequestDelegate next)
        {
            configuration = config;
            this.next = next;
        }
  
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(new PathString("/api/save-token")))
            {
                // Read JSON payload
                var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);

                if (payload != null && payload.TryGetValue("id_token", out string idToken))
                {
                    var test = idToken;
                    // Now you have the ID token on the server side
                    // Do something with it, like validation or saving it for further use
                }
            }
            else
            {
                await next(context);
            }


            //string userId = context.Request.Query["UserID"];
            //Guid userOid = Guid.Empty;
            //ApplicationUser myUser = null;
            //if (Guid.TryParse(userId, out userOid))
            //{
            //    if (!(context.User?.Identity.IsAuthenticated ?? false) && !string.IsNullOrEmpty(userId))
            //    {
            //        bool autoLoginOK = false;
            //        if (configuration.GetConnectionString("ConnectionString") != null)
            //        {
            //            using (XPObjectSpaceProvider directProvider = new XPObjectSpaceProvider(configuration.GetConnectionString("ConnectionString")))
            //            using (IObjectSpace directObjectSpace = directProvider.CreateObjectSpace())
            //            {
            //                 myUser = directObjectSpace.FindObject<ApplicationUser>(CriteriaOperator.Parse("Oid=?", userOid));
            //                if (myUser != null)
            //                    if (myUser.AutoLoginByURL) 
            //                    {
            //                        autoLoginOK = true;
            //                    }
            //            }
            //        }

            //        if (autoLoginOK)
            //        {


            //            var identityCreator = context.RequestServices.GetRequiredService<IStandardAuthenticationIdentityCreator>();
            //            ClaimsIdentity id = identityCreator.CreateIdentity(myUser.Oid.ToString(), myUser.UserName);
            //            await context.SignInAsync(new ClaimsPrincipal(id));
            //            context.Response.Redirect("/");

            //            //ClaimsIdentity id = new ClaimsIdentity(SecurityDefaults.DefaultClaimsIssuer);
            //            //Claim claim = new Claim(ClaimTypes.NameIdentifier, userId, ClaimValueTypes.String, SecurityDefaults.Issuer);
            //            //id.AddClaim(claim);
            //            //await context.SignInAsync(new ClaimsPrincipal(id));
            //            //context.Response.Redirect("/");
            //        }
            //        else
            //            await next(context);
            //    }
            //    else
            //        await next(context);
            //}
            //else
            //{
            //    await next(context);
            //}
        }
    }
}
