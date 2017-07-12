using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MurahAje.Web;
using MurahAje.Web.ManageViewModels;
using MurahAje.Web.Services;
using Gravicode.AspNetCore.Identity.Redis;
using System.IO;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage        

using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace MurahAje.Web.Controllers
{
	[Produces("application/json")]
    [Route("api/Media")]
    public class MediaControlller : Controller
    {
        [HttpPost]
        public async Task<IActionResult> PostFiles(IFormCollection collection)
        {
            try
            {
                System.Console.WriteLine("You received the call!");

                var ms = new MemoryStream();
                var files = collection.Files;
                string url ="";
                string ext = "";
                foreach (var formFile in files)
                {
                    ext = System.IO.Path.GetExtension(formFile.FileName);
                    if (formFile.Length > 0)
                    {
                        
                        await formFile.CopyToAsync(ms);
                        //upload ms to azure storage      

                        var bytes = ms.ToArray();
                        var uniq = Guid.NewGuid().ToString();
                        CloudStorageAccount storageAccount = new CloudStorageAccount(
                        new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                        "storagemurahaje",
                        "NU2f/5suzFgLyGYplR6ydXQ+6L8STLCRviDqJf+MS8bVWsO3L5VWFK3qaUltdPNwdd092st0eJWQIBvLI0WI1A=="), true);
                        // Create the blob client.
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                        // Retrieve reference to a previously created container.
                        CloudBlobContainer container = blobClient.GetContainerReference("products");

                        // Retrieve reference to a blob named "myblob".
                        CloudBlockBlob blockBlob = container.GetBlockBlobReference(uniq + ext);

                        // Create or overwrite the "myblob" blob with contents from a local file.

                        await blockBlob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
                        url = blockBlob.Uri.AbsoluteUri;
                        
                    }
                }

                // process uploaded files

                return Ok(new {
                    count = files.Count,
                    Url = url
                });
            }
            catch (Exception exp)
            {
                System.Console.WriteLine("Exception generated when uploading file - " + exp.Message);

                string message = $"file / upload failed!";
                return Json(message);
            }
        }

        public static async void uploadAzureAsync(MemoryStream ms)
        {
            var bytes = ms.ToArray();
            var uniq = Guid.NewGuid().ToString();
            CloudStorageAccount storageAccount = new CloudStorageAccount(
            new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            "storagemurahaje",
            "NU2f/5suzFgLyGYplR6ydXQ+6L8STLCRviDqJf+MS8bVWsO3L5VWFK3qaUltdPNwdd092st0eJWQIBvLI0WI1A=="), true);
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("products");

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(uniq +".PNG");

            // Create or overwrite the "myblob" blob with contents from a local file.
          
                await blockBlob.UploadFromByteArrayAsync(bytes,0,bytes.Length);
            
        }


    }
}
