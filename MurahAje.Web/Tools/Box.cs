using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MurahAje.Web.Entities;

namespace MurahAje.Web.Tools
{
    public class Box
    {
        public  static string GetContentType(string fname)
        {
            // set a default mimetype if not found.
            string contentType = "application/octet-stream";

            try
            {
                // get the registry classes root
                RegistryKey classes = Registry.ClassesRoot;

                // find the sub key based on the file extension
                RegistryKey fileClass = classes.OpenSubKey(Path.GetExtension(fname));
                contentType = fileClass.GetValue("Content Type").ToString();
            }
            catch { }

            return contentType;
        }
        public static List<T> FilterDataByMime<T>(List<T> dataPool, params string[] Mime)
        {
            var SelectedData = new List<T>();
            foreach (var item in dataPool)
            {
                if (item is SocialPost)
                {
                    var newItem = item as SocialPost;
                    foreach (var str in Mime)
                    {
                        if (newItem.mimeType.Contains(str, StringComparison.CurrentCultureIgnoreCase))
                        {
                            SelectedData.Add(item);
                            break;
                        }
                    }
                }
            }
            return SelectedData;

        }
    }
}
