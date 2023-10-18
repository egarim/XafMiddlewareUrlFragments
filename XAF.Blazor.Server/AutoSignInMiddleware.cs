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
using System.IdentityModel.Tokens.Jwt;

namespace XAF.Blazor.Server
{
    public interface ITokenService
    {
        TokenDataDTO TokenDecoder(string token);
    }
    public class TokenService : ITokenService
    {
        public TokenDataDTO TokenDecoder(string token)
        {
            try
            {
                var tokenData = new TokenDataDTO();
                var handler = new JwtSecurityTokenHandler();

                var jwtToken = handler.ReadJwtToken(token);

                var jsonPayload = jwtToken.Payload.SerializeToJson();
                var resultPayload = JsonConvert.DeserializeObject<TokenPayload>(jsonPayload);
                tokenData.Payload = resultPayload;

                return tokenData;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error decoding token: " + ex.Message);
                //throw an error exception
                throw;
            }
        }
    }


    public class TokenDataDTO
    {
        public TokenHeader Header { get; set; }
        public TokenPayload Payload { get; set; }
    }

    public class TokenPayload
    {
        public string Ver { get; set; }
        public string Iss { get; set; }
        public string Sub { get; set; }
        public string Aud { get; set; }
        public int Exp { get; set; }
        public string Nonce { get; set; }
        public int Iat { get; set; }
        public int AuthTime { get; set; }
        public string Oid { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public List<string> Emails { get; set; }
        public bool NewUser { get; set; }
        public string Tfp { get; set; }
        public int Nbf { get; set; }
    }

    public class TokenHeader
    {
        public string Alg { get; set; }
        public string Kid { get; set; }
        public string Typ { get; set; }
    }
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
                    var TokenService=new TokenService();
                    var Token= TokenService.TokenDecoder(idToken);
                    using (XPObjectSpaceProvider directProvider = new XPObjectSpaceProvider(configuration.GetConnectionString("ConnectionString")))
                    using (IObjectSpace directObjectSpace = directProvider.CreateObjectSpace())
                    {
                        if (idToken != null)
                        {
                            ApplicationUser User;
                            //if (Token.Payload.NewUser)
                            if (true)
                            {
                                var AdminRol = directObjectSpace.FindObject<ApplicationUser>(CriteriaOperator.Parse("UserName=?", "Admin"));
                                User = directObjectSpace.CreateObject<ApplicationUser>();
                                User.UserName = Token.Payload.Emails[0];
                                User.Roles.Add(AdminRol.Roles[0]);
                                User.IsActive = true;
                                User.SetPassword("");
                                directObjectSpace.CommitChanges();
                            }
                          
                            User = directObjectSpace.FindObject<ApplicationUser>(CriteriaOperator.Parse("UserName=?", Token.Payload.Emails[0]));

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
