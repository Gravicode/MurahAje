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
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MurahAje.Web.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AuthController : Controller
    {
        // GET: api/values
        [ActionName("SetAuth")]
        [HttpGet]
        public OutputData SetAuth(string uname, bool persist=true)
        {
            var aes = System.Security.Cryptography.Aes.Create();
            Enkripsi en = new Enkripsi();

            WriteCookies("uname", en.Encrypt(uname), persist);
            return new OutputData() { Data = "", IsSucceed = true };
        }
        [ActionName("GetAuth")]
        [HttpGet]
        public OutputData GetAuth()
        {
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

        [ActionName("LogOut")]
        [HttpGet]
        public OutputData LogOut()
        {
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

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
