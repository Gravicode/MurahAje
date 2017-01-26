using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MurahAje.Web.Entities
{
    public interface ISocialData
    {
        bool InsertBulkData<T>(IEnumerable<T> data);
        bool InsertData<T>(T data);
        bool DeleteAllData<T>();
        bool DeleteData<T>(long id);
        bool DeleteDataBulk<T>(IEnumerable<T> Ids);
        bool ChangeVisibility<T>(long id, bool Value) where T : SocialPost;
        List<T> GetAllData<T>();
        T GetDataById<T>(long Id);
        List<T> GetDataByIds<T>(params long[] Ids);
        List<T> GetAllData<T>(int Limit);
        List<T> GetDataByMention<T>(int Limit, params string[] Mention) where T : SocialPost;
        List<T> GetDataByHastag<T>(int Limit, params string[] Hashtag) where T : SocialPost;
        List<T> GetDataByMime<T>(int Limit, params string[] Mime) where T : SocialPost;
        List<T> GetDataByKeyword<T>(int Limit, string Keyword) where T : SocialPost;
        List<T> GetDataByStartId<T>(int Limit, long StartId);
        long GetSequence<T>();
    }
}
