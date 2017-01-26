using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
using ServiceStack.Redis.Generic;

/// <summary>
/// Summary description for GalleryDb
/// </summary>
namespace MurahAje.Web.Entities
{
    public class SocialDb : ISocialData
    {
        public static IRedisClientsManager redisManager;
        public static string RedisConStr { set; get; }
        public SocialDb()
        {
            
            if (redisManager == null)
            {
                redisManager = new PooledRedisClientManager(RedisConStr);
               
            }
            
        }
        /*
        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }
        */
        public bool InsertBulkData<T>(IEnumerable<T> data)
        {

            
            using (var redis = redisManager.GetClient())
            {
                var redisStream = redis.As<T>();
                redisStream.StoreAll(data);
                return true;
            }
        }

        public bool InsertData<T>(T data)
        {
            try
            {
                if (data == null) return false;
                if (data is SocialPost)
                    if (string.IsNullOrEmpty((data as SocialPost).Message)) return false;

                 using (var redis = redisManager.GetClient())
                {
                    var redisStream = redis.As<T>();
                    redisStream.Store(data);
                    return true;
                }
            }
            catch
            {
                //print ke log
                //throw;
                return false;
            }
        }
        public bool ChangeVisibility<T>(long id, bool Value) where T : SocialPost
        {
            try
            {

                
                using (var redis = redisManager.GetClient())
                {
                    var redisStream = redis.As<SocialPost>();
                    var node = redisStream.GetById(id);
                    node.isVisible = Value;
                    redisStream.Store(node);
                    return true;
                }
            }
            catch
            {
                //print ke log
                //throw;
                return false;
            }
        }
        public bool DeleteData<T>(long id)
        {
            try
            {

                 using (var redis = redisManager.GetClient())
                {
                    var redisStream = redis.As<T>();
                    redisStream.DeleteById(id);
                    return true;
                }
            }
            catch
            {
                //print ke log
                //throw;
                return false;
            }
        }
        public bool DeleteAllData<T>()
        {
            try
            {

                 using (var redis = redisManager.GetClient())
                {
                    IRedisTypedClient<T> redis1 = redis as IRedisTypedClient<T>;

                    var datas = redis1.GetAll();
                    foreach (var item in datas)
                    {
                        redis1.Delete(item);
                    }

                    return true;
                }
            }
            catch
            {
                //print ke log
                //throw;
                return false;
            }
        }
        public bool DeleteDataBulk<T>(IEnumerable<T> Ids)
        {
            try
            {

                 using (var redis = redisManager.GetClient())
                {
                    var redisStream = redis.As<T>();
                    redisStream.DeleteByIds(Ids);
                    return true;
                }
            }
            catch
            {
                //print ke log
                //throw;
                return false;
            }
        }

        public T GetDataById<T>(long Id)
        {

             using (var redis = redisManager.GetClient())
            {
                var redisStream = redis.As<T>();

                return redisStream.GetById(Id);

            }
        }
        public List<T> GetDataByIds<T>(params long[] Ids)
        {

             using (var redis = redisManager.GetClient())
            {
                var redisStream = redis.As<T>();
                if (typeof(T) == typeof(SocialPost))
                {
                    var data = from c in redisStream.GetAll()
                               where (c is SocialPost) && (c as SocialPost).isVisible && Ids.Contains((c as SocialPost).Id)
                               select c;
                    return data.ToList();
                }
                else
                {
                    var data = from c in redisStream.GetByIds(Ids)
                               select c;
                    return data.ToList();
                }
            }
        }
        public List<T> GetDataByStartId<T>(int Limit, long StartId)
        {

             using (var redis = redisManager.GetClient())
            {
                var redisStream = redis.As<T>();
                if (typeof(T) == typeof(SocialPost))
                {
                    var data = from c in redisStream.GetAll()
                               where (c is SocialPost) && (c as SocialPost).isVisible && (c as SocialPost).Id >= StartId
                               orderby (c as SocialPost).Id
                               select c;
                    return data.Take(Limit).ToList();
                }
                return null;
                /*
                else
                {
                    var data = from c in redisStream.GetAll()
                               where Convert.ToInt32(GetPropValue(c, "Id")) >= StartId
                               select c;
                    return data.Take(Limit).ToList();
                }*/
            }
        }
        public List<T> GetAllData<T>(int Limit)
        {

             using (var redis = redisManager.GetClient())
            {
                var redisStream = redis.As<T>();
                if (typeof(T) == typeof(SocialPost))
                {
                    var data = from c in redisStream.GetAll()
                               where ((c is SocialPost) && (c as SocialPost).isVisible)
                               orderby (c as SocialPost).Id
                               select c;

                    return data == null ? null : data.TakeLast(Limit).ToList();
                }
                else
                {
                    var data = from c in redisStream.GetAll()
                               select c;

                    return data == null ? null : data.TakeLast(Limit).ToList();
                }
            }
        }
        public List<T> GetAllData<T>()
        {

             using (var redis = redisManager.GetClient())
            {
               
                var redisStream = redis.As<T>();
                if (typeof(T) == typeof(SocialPost))
                {
                    var data = from c in redisStream.GetAll()
                               where (c as SocialPost).isVisible
                               orderby (c as SocialPost).Id
                               select c;
                    return data.ToList();
                }
                else
                {
                    var data = from c in redisStream.GetAll()
                               select c;
                    return data.ToList();
                }
            }
        }
        public List<T> GetDataByMention<T>(int Limit, params string[] Mention) where T : SocialPost
        {

             using (var redis = redisManager.GetClient())
            {
                var SelectedData = new List<T>();
                var redisStream = redis.As<T>();
                var dataPool = redisStream.GetAll();
                foreach (var item in dataPool)
                {
                    if (item is SocialPost)
                    {
                        var newItem = item as SocialPost;
                        if (!newItem.isVisible) continue;

                        foreach (var str in Mention)
                        {
                            if (newItem.Mention.Contains(str, StringComparison.CurrentCultureIgnoreCase))
                            {
                                SelectedData.Add(item);
                                break;
                            }
                        }
                    }

                }
                return SelectedData.OrderBy(x => x.Id).TakeLast(Limit).ToList();
            }

        }
        public List<T> GetDataByHastag<T>(int Limit, params string[] Hashtag) where T : SocialPost
        {

             using (var redis = redisManager.GetClient())
            {
                var SelectedData = new List<T>();
                var redisStream = redis.As<T>();
                var dataPool = redisStream.GetAll();
                foreach (var item in dataPool)
                {
                    if (item is SocialPost)
                    {
                        var newItem = item as SocialPost;
                        if (!newItem.isVisible) continue;
                        if (string.IsNullOrEmpty(newItem.Hashtag)) continue;
                        var newhashtag = newItem.Hashtag + ";";
                        foreach (var str in Hashtag)
                        {
                            if (newhashtag.Contains(str + ";", StringComparison.CurrentCultureIgnoreCase))
                            {
                                SelectedData.Add(item);
                                break;
                            }
                        }
                    }
                }

                return SelectedData.OrderBy(x => x.Id).TakeLast(Limit).ToList(); ;
            }
        }
        public List<T> GetDataByKeyword<T>(int Limit, string Keyword) where T : SocialPost
        {

             using (var redis = redisManager.GetClient())
            {
                var redisStream = redis.As<T>();
                var dataSelected = from c in redisStream.GetAll()
                                   where c is SocialPost && (c as SocialPost).Message.Contains(Keyword, StringComparison.CurrentCultureIgnoreCase) &&
                                   (c as SocialPost).isVisible
                                   orderby (c as SocialPost).Id
                                   select c;
                return dataSelected.TakeLast(Limit).ToList();
            }
        }
        public List<T> GetDataByMime<T>(int Limit, params string[] Mime) where T : SocialPost
        {

             using (var redis = redisManager.GetClient())
            {
                var SelectedData = new List<T>();
                var redisStream = redis.As<T>();
                var dataPool = redisStream.GetAll();
                foreach (var item in dataPool)
                {
                    if (item is SocialPost)
                    {
                        var newItem = item as SocialPost;
                        if (!newItem.isVisible) continue;
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
                return SelectedData.OrderBy(x => x.Id).TakeLast(Limit).ToList();
            }
        }

        public long GetSequence<T>()
        {

             using (var redis = redisManager.GetClient())
            {

                var redisStream = redis.As<T>();
                long Id = redisStream.GetNextSequence();
                return Id;

            }
        }

    }

    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(toCheck)) return false;
            return source.IndexOf(toCheck, comp) >= 0;
        }
    }

}