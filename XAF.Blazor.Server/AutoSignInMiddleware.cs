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
    //HACK to test use this url https://localhost:44318/LoginPage#id_token=abc123&token_type=bearer
    //HACK to test use this url https://localhost:44318/LoginPage#id_token=Admin&token_type=bearer
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
                   
                    using (XPObjectSpaceProvider directProvider = new XPObjectSpaceProvider(configuration.GetConnectionString("ConnectionString")))
                    using (IObjectSpace directObjectSpace = directProvider.CreateObjectSpace())
                    {
                        if (idToken != null)
                        {
                            var User = directObjectSpace.FindObject<ApplicationUser>(CriteriaOperator.Parse("UserName=?", "Admin"));

                            if (User != null)
                            {
                                var identityCreator = context.RequestServices.GetRequiredService<IStandardAuthenticationIdentityCreator>();
                                ClaimsIdentity id = identityCreator.CreateIdentity(User.Oid.ToString(), User.UserName);
                                await context.SignInAsync(new ClaimsPrincipal(id));
                            }
                            else
                            {
                                await next(context);
                            }
                        }
                       
                        
                      
                       
                        

                    }
                }
                else
                {
                    await next(context);
                }
            }
            else
            {
                await next(context);
            }


           
        }
    }
}
