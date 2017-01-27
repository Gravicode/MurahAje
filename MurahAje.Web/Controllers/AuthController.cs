using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using MurahAje.Web.Entities;
using MurahAje.Web.Tools;
using Microsoft.AspNetCore.SignalR.Hubs;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net;
using Microsoft.AspNetCore.Http.Authentication;
using System.Security.Claims;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MurahAje.Web.Controllers
{
   
    public class AuthController : Controller
    {
        // GET: api/values
        [SwaggerOperation("SetAuth")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [ActionName("SetAuth")]
        [HttpGet]
        public async Task<OutputData> SetAuth(string uname, bool persist = true)
        {
            const string Issuer = "https://murahaje.com";
          
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, uname, ClaimValueTypes.String, Issuer)
                };

            var userIdentity = new ClaimsIdentity(claims, "Passport");
            var userPrincipal = new ClaimsPrincipal(userIdentity);
            
            await HttpContext.Authentication.SignInAsync("Cookie", userPrincipal,
                new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                    IsPersistent = false,
                    AllowRefresh = false
                });

            var aes = System.Security.Cryptography.Aes.Create();
            Enkripsi en = new Enkripsi();

            WriteCookies("uname", en.Encrypt(uname), persist);
            return new OutputData() { Data = "", IsSucceed = true };
            /*
             Cara pake session di netcore
             HttpContext.Session.SetString(SessionKeyName, "Rick");
             var name = HttpContext.Session.GetString(SessionKeyName);
             */
        }

        [SwaggerOperation("GetAuth")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [ActionName("GetAuth")]
        [HttpGet]
        public OutputData GetAuth()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return new OutputData() { Data = "", IsSucceed = false };
            }
            var nama = ReadCookies("uname");
            Enkripsi en = new Enkripsi();
            if (nama != null)
            {
                SocialHub hub = ObjectContainer.Get<SocialHub>();
                nama = en.Decrypt(nama);
                var node = hub.GetUserByEmail(nama);
                return new OutputData() { Data = node, IsSucceed = true };
            }
            else
            {
                return new OutputData() { Data = nama, IsSucceed = false };
            }
           
        }

        [SwaggerOperation("LogOut")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [ActionName("LogOut")]
        [HttpGet]
        public async Task<OutputData> LogOut()
        {
            await HttpContext.Authentication.SignOutAsync("Cookie", 
              new AuthenticationProperties
              {
                  ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                  IsPersistent = false,
                  AllowRefresh = false
              });
            Response.Cookies.Delete("uname");
            return new OutputData() { Data = "", IsSucceed = true };
        }
        public void WriteCookies(string setting,
             string settingValue, bool isPersistent)
        {

            if (isPersistent)
            {
                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddDays(1);
                Response.Cookies.Append(setting, settingValue, options);
            }
            else
            {
                Response.Cookies.Append(setting, settingValue);
            }
        }

        public string ReadCookies(string Setting)
        {
            var Vals = Request.Cookies[Setting];

            if (!string.IsNullOrEmpty(Vals))
            {
                return Vals;
            }
            return null;
        }
    }
}
