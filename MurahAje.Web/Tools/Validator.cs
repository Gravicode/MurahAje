using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MurahAje.Web.Entities;

namespace MurahAje.Web.Tools
{
    public class Validator
    {
        public static SocialPost parseMessage(SocialPost data)
        {
            data.mimeType = "text/text";
            int counter = 0;
            //@"\b\@\w+"
            var mentions = Regex.Matches(data.Message, @"@\w+", RegexOptions.IgnoreCase);
            foreach (Match item in mentions)
            {
                data.Mention += (counter > 0 ? ";" : string.Empty) + item.Value;
                counter++;
            }
            counter = 0;
            //@"\b#\w+"
            var hashtags = Regex.Matches(data.Message, @"\#\w+", RegexOptions.IgnoreCase);
            foreach (Match item in hashtags)
            {
                data.Hashtag += (counter > 0 ? ";" : string.Empty) + item.Value;
                counter++;
            }
            counter = 0;
            /*
            var urls = Regex.Matches(data.Message, @"\bhttps?://[-\w]+(\.\w[-\w]*)+(:\d+)?(/[^.!,?;""\'<>()\[\]\{\}\s\x7F-\xFF]*([.!,?]+[^.!,?;""\'<>\(\)\[\]\{\}\s\x7F-\xFF]+)*)?\b", RegexOptions.IgnoreCase);
          
            foreach (Match item in urls)
            {
                data.Url += (counter > 0 ? ";" : string.Empty) + item.Value;
                var mime = Box.GetContentType(item.Value);
                if (!string.IsNullOrEmpty(mime))
                {
                    data.mimeType = mime;
                }
                counter++;
            }*/

            return data;

        }
    }
}
