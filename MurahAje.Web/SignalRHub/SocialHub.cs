using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;
using System.Linq;
using System.Collections.Generic;
using MoreLinq;
using MurahAje.Web.Entities;
using MurahAje.Web.Tools;

namespace MurahAje.Web
{
    [HubName("SocialHub")]
    public class SocialHub : Hub
    {
        public static ISocialData db = null;
        public SocialHub()
        {
            if (db == null)
            {
                /* # INVERSION OF CONTROL SCANNER/CONTAINER # uncomment this when u deploy in production area
                Gravicode.Transformer.LibraryScanner scanner = new Gravicode.Transformer.LibraryScanner();
                scanner.ScanLibrary<IDataContext>(ConfigurationManager.AppSettings["LibraryPath"]);
                foreach (string a in scanner.GetLibraryList())
                {
                    Console.WriteLine("Nama fungsi :" + a);
                }
                IDataContext redis = scanner.getInstance<IDataContext>(ConfigurationManager.AppSettings["StorageLibrary"]);
                db = redis;
                */
                db = new SocialDb();
            }
        }

        #region Social Post
        /*
          [HubMethodName("GetPostByUser")]
        public void GetPostByUser(int Limit, string LoginName)
        {
            HashSet<long> hash = new HashSet<long>();
            var mythread = (from c in db.GetAllData<SocialPost>()
                            where c.LoginName == LoginName
                            select c).ToList();
            mythread.ForEach(x => hash.Add(x.Id));

            var myfollow = from c in db.GetAllData<SocialFollow>()
                           where c.LoginName == LoginName
                           select c;
            foreach (var item in myfollow)
            {
                if (item.Hashtags != null)
                {
                    var threadbyhashtag = db.GetDataByHastag<SocialPost>(1000, item.Hashtags.ToArray());
                    threadbyhashtag.ToList().ForEach(x =>
                    {
                        if (!hash.Contains(x.Id))
                        {
                            mythread.Add(x);
                            hash.Add(x.Id);
                        }
                    });
                }
                if (item.Users != null)
                {
                    HashSet<string> followinguser = new HashSet<string>();
                    item.Users.ToList().ForEach(x => followinguser.Add(x));

                    var threadbyuser = from c in db.GetAllData<SocialPost>()
                                       where followinguser.Contains(c.LoginName)
                                       select c;

                    threadbyuser.ToList().ForEach(x =>
                    {
                        if (!hash.Contains(x.Id))
                        {
                            mythread.Add(x);
                            hash.Add(x.Id);
                        }
                    });
                }
            }
            Clients.Caller.DisplayPost(mythread.OrderBy(x => x.Id).TakeLast(Limit));
        }
         */
        [HubMethodName("GetPostByUser")]
        public void GetPostByUser(int Limit, string LoginName)
        {
            HashSet<long> hash = new HashSet<long>();
            var mythread = (from c in db.GetAllData<SocialPost>()
                            where c.LoginName == LoginName
                            select c).ToList();
            mythread.ForEach(x => hash.Add(x.Id));

            var seluser = (from c in db.GetAllData<SocialUser>()
                           where c.LoginName == LoginName
                           select c).SingleOrDefault();
            var item = seluser.Follow;
            if (item != null)
            {
                if (item.Hashtags != null)
                {
                    var threadbyhashtag = db.GetDataByHastag<SocialPost>(1000, item.Hashtags.ToArray());
                    threadbyhashtag.ToList().ForEach(x =>
                    {
                        if (!hash.Contains(x.Id))
                        {
                            mythread.Add(x);
                            hash.Add(x.Id);
                        }
                    });
                }
                if (item.Users != null)
                {
                    var threadbyuser = from c in db.GetAllData<SocialPost>()
                                       where item.Users.Contains(c.LoginName)
                                       select c;

                    threadbyuser.ToList().ForEach(x =>
                    {
                        if (!hash.Contains(x.Id))
                        {
                            mythread.Add(x);
                            hash.Add(x.Id);
                        }
                    });
                }
            }
            Clients.Caller.DisplayPost(mythread.OrderBy(x => x.Id).TakeLast(Limit));
        }

        [HubMethodName("GetLatestPost")]
        public void GetLatestPost(int Limit)
        {
            var datas = db.GetAllData<SocialPost>(Limit).OrderByDescending(x => x.CreatedDate).ToList();
            // Call the broadcastMessage method to update clients.
            Clients.Caller.DisplayPost(datas);
        }

        [HubMethodName("GetPostByStartId")]
        public void GetPostByStartId(int Limit, int StartId)
        {
            var datas = db.GetDataByStartId<SocialPost>(Limit, StartId);
            // Call the broadcastMessage method to update clients.
            Clients.Caller.DisplayPost(datas);
        }

        [HubMethodName("GetPostByMention")]
        public void GetPostByMention(int Limit, string mention)
        {
            var datas = db.GetDataByMention<SocialPost>(Limit, mention.Split(';'));
            // Call the broadcastMessage method to update clients.
            Clients.Caller.DisplayPost(datas);
        }

        [HubMethodName("GetPostByHashtag")]
        public void GetPostByHashtag(int Limit, string hashtag)
        {
            var datas = db.GetDataByHastag<SocialPost>(Limit, hashtag.Split(';')).OrderByDescending(x => x.CreatedDate).ToList();
            // Call the broadcastMessage method to update clients.
            Clients.Caller.DisplayPost(datas);
        }

        [HubMethodName("GetPostById")]
        public SocialPost GetPostById(long PostId)
        {
            var datas = from c in db.GetAllData<SocialPost>()
                        where c.Id == PostId
                        orderby c.Id
                        select c;
            return datas.SingleOrDefault();
        }

        [HubMethodName("GetPostByCategory")]
        public void GetPostByCategory(int Limit, long CategoryId)
        {
            var selCategory = from c in db.GetAllData<SocialCategory>()
                              where c.Id == CategoryId
                              select c;
            foreach (var item in selCategory)
            {
                var datas = db.GetDataByHastag<SocialPost>(Limit, item.Hashtags.ToArray()).OrderByDescending(x => x.CreatedDate).ToList();
                // Call the broadcastMessage method to update clients.
                Clients.Caller.DisplayPost(datas);
                break;
            }
        }

        [HubMethodName("GetPostByHashtagAndMime")]
        public void GetPostByHashtagAndMime(int Limit, string hashtag, string mime)
        {
            var datas = db.GetDataByHastag<SocialPost>(Limit, hashtag);
            datas = Box.FilterDataByMime<SocialPost>(datas, mime);
            // Call the broadcastMessage method to update clients.
            Clients.Caller.DisplayPost(datas);
        }

        [HubMethodName("GetPostByKeyword")]
        public void GetPostByKeyword(int Limit, string keyword)
        {
            var datas = db.GetDataByKeyword<SocialPost>(Limit, keyword).OrderByDescending(x => x.CreatedDate).ToList();
            // Call the broadcastMessage method to update clients.
            Clients.Caller.DisplayPost(datas);
        }

        [HubMethodName("GetPost")]
        public void GetPost()
        {
            var datas = db.GetAllData<SocialPost>().OrderByDescending(x => x.CreatedDate).ToList();

            // Call the broadcastMessage method to update clients.
            Clients.Caller.DisplayPost(datas.ToList());
        }

        [HubMethodName("DeletePost")]
        public void DeletePost(int id)
        {
            var item = db.GetDataById<SocialPost>(id);
            if (!string.IsNullOrEmpty(item.Hashtag))
            {
                DeleteHashtagStat(item.Hashtag.Split(';'));
            }
            if (!string.IsNullOrEmpty(item.FilePath))
            {
                var fPath = item.FilePath; //HttpContext.Current.Server.MapPath("~") + item.Url;
                if (System.IO.File.Exists(fPath))
                {
                    System.IO.File.Delete(fPath);
                }
            }
            var hasil = db.DeleteData<SocialPost>(id);
            Clients.Caller.Notify("delete post [" + id + "] : " + hasil);
        }

        [HubMethodName("SendSocialPost")]
        public void SendSocialPost(string LoginName, string Name, string Pesan, string ImageUrl, string PathFile)
        {
            try
            {
                SocialPost Data = new SocialPost() { Name = Name, Message = Pesan, CreatedDate = DateTime.Now, isVisible = true, LoginName = LoginName, Id = db.GetSequence<SocialPost>(), FilePath = PathFile };

                Data = Validator.parseMessage(Data);
                if (!string.IsNullOrEmpty(ImageUrl))
                {
                    Data.Url = ImageUrl;
                    Data.mimeType = Box.GetContentType(Data.Url);
                }
                var hasil = db.InsertData<SocialPost>(Data);
                if (!string.IsNullOrEmpty(Data.Hashtag))
                {
                    UpdateHashtagStat(Data.Hashtag.Split(';'));
                }
                Clients.Caller.Notify("SocialPost data : " + hasil);

            }
            catch (Exception ex)
            {
                Logs.WriteLog("Insert gagal - " + ex.Message + "_" + ex.StackTrace);
            }
        }
        [HubMethodName("ModifySocialPost")]
        public void ModifySocialPost(int Id, string Pesan)
        {
            try
            {
                SocialPost Data = db.GetDataById<SocialPost>(Id);

                if (!string.IsNullOrEmpty(Data.Hashtag))
                {
                    DeleteHashtagStat(Data.Hashtag.Split(';'));
                    Data.Hashtag = string.Empty;
                }
                if (!string.IsNullOrEmpty(Data.Mention))
                {
                    Data.Mention = string.Empty;
                }
                Data.Message = Pesan;

                Data = Validator.parseMessage(Data);

                var hasil = db.InsertData<SocialPost>(Data);

                if (!string.IsNullOrEmpty(Data.Hashtag))
                {
                    UpdateHashtagStat(Data.Hashtag.Split(';'));
                }

                Clients.Caller.Notify("update SocialPost data [" + Id + "] : " + hasil);
            }
            catch (Exception ex)
            {
                Logs.WriteLog("Update gagal - " + ex.Message + "_" + ex.StackTrace);
            }
        }

        [HubMethodName("GetPostByLoginName")]
        public List<SocialPost> GetPostByLoginName(string LoginName)
        {
            try
            {
                var datas = from c in db.GetAllData<SocialPost>()
                            where c.LoginName == LoginName
                            select c;
                return datas.ToList();
                //Clients.Caller.DisplayPost(datas.ToList());

            }
            catch (Exception ex)
            {
                Logs.WriteLog("Insert gagal - " + ex.Message + "_" + ex.StackTrace);
            }

            return default(List<SocialPost>);
        }
        #endregion

        #region Hashtag
        public void DeleteHashtagStat(string[] Hashtag)
        {
            var datas = db.GetAllData<SocialHashtag>();
            var hash = new Dictionary<string, bool>();
            Hashtag.ForEach(x => { if (!hash.ContainsKey(x.ToLower())) hash.Add(x.ToLower(), false); });

            //only existing hashtag
            foreach (var item in datas)
            {
                if (hash.ContainsKey(item.Hashtag))
                {
                    item.Count--;
                    db.InsertData<SocialHashtag>(item);
                    hash[item.Hashtag] = true;
                }
            }

        }

        public void UpdateHashtagStat(string[] Hashtag)
        {
            var datas = db.GetAllData<SocialHashtag>();
            var hash = new Dictionary<string, bool>();
            Hashtag.ForEach(x => { if (!hash.ContainsKey(x.ToLower())) hash.Add(x.ToLower(), false); });

            //for existing hashtag
            foreach (var item in datas)
            {
                if (hash.ContainsKey(item.Hashtag))
                {
                    item.Count++;
                    db.InsertData<SocialHashtag>(item);
                    hash[item.Hashtag] = true;
                }
            }
            //for new hashtag
            foreach (KeyValuePair<string, bool> entry in hash)
            {
                if (!entry.Value)
                {
                    var newitem = new SocialHashtag() { Hashtag = entry.Key, Count = 1, Id = db.GetSequence<SocialHashtag>() };
                    db.InsertData<SocialHashtag>(newitem);
                }
            }
        }

        [HubMethodName("GetHashtagStat")]
        public void GetHashtagStat(int Limit)
        {
            try
            {
                var datas = from c in db.GetAllData<SocialHashtag>(10000)
                            where c.Count > 0
                            orderby c.Count descending
                            select c;
                Clients.Caller.DisplayHashtagStat(datas.Take(Limit).ToList());
            }
            catch (Exception ex)
            {
                Logs.WriteLog("Get hashtag gagal - " + ex.Message + "_" + ex.StackTrace);
            }
        }
        #endregion

        #region Comments
        /*
        [HubMethodName("PostComment")]
        public void PostComment(long ItemId, string LoginName, string Message)
        {
            SocialComment comment = new SocialComment() { Id = db.GetSequence<SocialComment>(), CreatedDate = DateTime.Now, ItemId = ItemId, LoginName = LoginName, Message = Message };
            bool hasil = db.InsertData<SocialComment>(comment);
            Clients.Caller.Notify("post comment : " + hasil);
        }

        [HubMethodName("GetComments")]
        public void GetComments(long ItemId)
        {
            var datas = from c in db.GetAllData<SocialComment>()
                        where c.ItemId == ItemId
                        orderby c.Id
                        select c;
            Clients.Caller.DisplayComments(datas.ToList());
        }

        [HubMethodName("DeleteComment")]
        public void DeleteComment(long CommentId)
        {
            bool hasil = db.DeleteData<SocialComment>(CommentId);
            Clients.Caller.Notify("delete comment : " + hasil);
        }*/
        [HubMethodName("PostComment")]
        public void PostComment(long ItemId, string LoginName, string Name, string Message)
        {
            try
            {
                var selpost = db.GetDataById<SocialPost>(ItemId);
                var comments = selpost.Comments;
                long counter = 0;
                if (comments == null)
                {
                    selpost.Comments = new List<SocialComment>();
                }
                else
                {
                    if (selpost.Comments.Count <= 0) counter = 0;
                    else
                        counter = selpost.Comments.Max(x => x.Id);
                }
                counter++;
                SocialComment comment = new SocialComment() { Name = Name, Id = counter, CreatedDate = DateTime.Now, LoginName = LoginName, Message = Message };
                selpost.Comments.Add(comment);

                bool hasil = db.InsertData<SocialPost>(selpost);

                Clients.Caller.DisplayComments(selpost.Comments.ToList());
            }
            catch
            {

            }
            //Clients.Caller.Notify("post comment : " + hasil);
        }

        [HubMethodName("GetComments")]
        public void GetComments(long ItemId)
        {
            var selpost = db.GetDataById<SocialPost>(ItemId);
            if (selpost.Comments != null)
                Clients.Caller.DisplayComments(selpost.Comments.ToList());
        }

        [HubMethodName("DeleteComment")]
        public void DeleteComment(long IdPost, long CommentId)
        {
            try
            {
                bool hasil = false;
                var selpost = db.GetDataById<SocialPost>(IdPost);
                var comments = selpost.Comments;
                if (comments != null)
                {
                    var selitem = (from c in comments
                                   where c.Id == CommentId
                                   select c).SingleOrDefault();
                    if (selitem != null)
                    {
                        comments.Remove(selitem);
                        selpost.Comments = comments;
                        hasil = db.InsertData<SocialPost>(selpost);
                        Clients.Caller.DisplayComments(selpost.Comments.ToList());
                    }
                }

            }
            catch
            {

            }
            //Clients.Caller.Notify("delete comment : " + hasil);
        }
        #endregion

        #region Likes
        /*
        [HubMethodName("Like")]
        public void Like(long ItemId, string LoginName)
        {
            SocialLike Like = new SocialLike() { Id = db.GetSequence<SocialLike>(), CreatedDate = DateTime.Now, ItemId = ItemId, LoginName = LoginName };
            bool hasil = db.InsertData<SocialLike>(Like);
            Clients.Caller.Notify("post Like : " + hasil);
        }

        [HubMethodName("GetLikes")]
        public void GetLikes(long ItemId)
        {
            var datas = from c in db.GetAllData<SocialLike>()
                        where c.ItemId == ItemId
                        orderby c.Id
                        select c;
            Clients.Caller.DisplayLikes(datas.ToList());
        }

        [HubMethodName("UnLike")]
        public void UnLike(long LikeId)
        {
            bool hasil = db.DeleteData<SocialLike>(LikeId);
            Clients.Caller.Notify("UnLike : " + hasil);
        }*/

        [HubMethodName("LikeUnLike")]
        public void LikeUnLike(long ItemId, string LoginName, string Name)
        {
            long counter = 0;
            var selpost = db.GetDataById<SocialPost>(ItemId);
            var likes = selpost.Likes;
            if (likes == null)
            {
                likes = new List<SocialLike>();
            }
            else
            {
                if (selpost.Likes.Count <= 0) counter = 0;
                else
                    counter = selpost.Likes.Max(x => x.Id);
            }
            counter++;
            var hash = new HashSet<string>();
            likes.ForEach(x => hash.Add(x.LoginName));

            if (!hash.Contains(LoginName))
            {
                SocialLike Like = new SocialLike() { Id = counter, CreatedDate = DateTime.Now, Name = Name, LoginName = LoginName };
                likes.Add(Like);
                selpost.Likes = likes;
                bool hasil = db.InsertData<SocialPost>(selpost);
            }
            else
            {
                var selitem = (from c in likes
                               where c.LoginName == LoginName
                               select c).SingleOrDefault();
                if (selitem != null)
                {
                    likes.Remove(selitem);
                    selpost.Likes = likes;
                    bool hasil = db.InsertData<SocialPost>(selpost);

                }
            }
            Clients.Caller.DisplayLikes(selpost.Likes.ToList());
            //Clients.Caller.Notify("post Like : " + hasil);
        }

        [HubMethodName("Like")]
        public void Like(long ItemId, string LoginName, string Name)
        {
            long counter = 0;
            var selpost = db.GetDataById<SocialPost>(ItemId);
            var likes = selpost.Likes;
            if (likes == null)
            {
                likes = new List<SocialLike>();
            }
            else
            {
                if (selpost.Likes.Count <= 0) counter = 0;
                else
                    counter = selpost.Likes.Max(x => x.Id);
            }
            counter++;
            var hash = new HashSet<string>();
            likes.ForEach(x => hash.Add(x.LoginName));

            if (!hash.Contains(LoginName))
            {
                SocialLike Like = new SocialLike() { Id = counter, CreatedDate = DateTime.Now, Name = Name, LoginName = LoginName };
                likes.Add(Like);
                selpost.Likes = likes;
                bool hasil = db.InsertData<SocialPost>(selpost);
            }
            Clients.Caller.DisplayLikes(selpost.Likes.ToList());
            //Clients.Caller.Notify("post Like : " + hasil);
        }

        [HubMethodName("GetLikes")]
        public void GetLikes(long ItemId)
        {
            var datas = (from c in db.GetAllData<SocialPost>()
                         where c.Id == ItemId
                         orderby c.Id
                         select c).SingleOrDefault();
            if (datas != null)
            {
                Clients.Caller.DisplayLikes(datas.Likes.ToList());
            }
        }

        [HubMethodName("UnLike")]
        public void UnLike(long ItemId, long LikeId)
        {
            var selpost = db.GetDataById<SocialPost>(ItemId);
            var likes = selpost.Likes;
            if (likes != null)
            {
                var selitem = (from c in likes
                               where c.Id == LikeId
                               select c).SingleOrDefault();
                if (selitem != null)
                {
                    likes.Remove(selitem);
                    selpost.Likes = likes;
                    bool hasil = db.InsertData<SocialPost>(selpost);
                    Clients.Caller.DisplayLikes(selpost.Likes.ToList());
                }
            }

        }
        #endregion

        #region Follow
        [HubMethodName("GetTotalFollowers")]
        public int GetTotalFollowers(string LoginName)
        {
            var datas = from c in db.GetAllData<SocialUser>()
                        where (c.Follow != null && c.Follow.Users != null && c.Follow.Users.Contains(LoginName))
                        select c;
            if (datas != null && datas.Count() > 0)
            {
                return datas.Count();
            }
            return 0;
        }
        [HubMethodName("GetTotalFollowing")]
        public int GetTotalFollowing(string LoginName)
        {
            var datas = (from c in db.GetAllData<SocialUser>()
                         where c.LoginName == LoginName
                         select c).SingleOrDefault();
            if (datas != null)
            {
                return datas.Follow == null || datas.Follow.Users == null ? 0 : datas.Follow.Users.Count;
            }
            return 0;
        }

        [HubMethodName("FollowUnfollowUser")]
        public FollowResult FollowUnfollowUser(string LoginName, string UserToFollow)
        {
            int hasil = 0;
            var follows = (from c in db.GetAllData<SocialUser>()
                           where c.LoginName == LoginName
                           select c).SingleOrDefault();
            if (follows != null && follows.Follow != null && follows.Follow.Users != null)
            {
                if (!follows.Follow.Users.Contains(UserToFollow))
                {
                    follows.Follow.Users.Add(UserToFollow);
                    hasil = 1;
                }
                else
                {
                    follows.Follow.Users.Remove(UserToFollow);
                    hasil = 2;
                }
                var result = db.InsertData<SocialUser>(follows);
            }
            return new FollowResult() { LoginName = UserToFollow, Hasil = hasil };
            //Clients.Caller.Notify("Follow Unfollow user : " + hasil);
        }


        [HubMethodName("FollowUser")]
        public void FollowUser(string LoginName, string UserToFollow)
        {
            bool hasil = false;
            var follows = (from c in db.GetAllData<SocialUser>()
                           where c.LoginName == LoginName
                           select c).SingleOrDefault();
            if (follows != null && follows.Follow != null && follows.Follow.Users != null)
            {
                if (!follows.Follow.Users.Contains(UserToFollow))
                {
                    follows.Follow.Users.Add(UserToFollow);
                    hasil = db.InsertData<SocialUser>(follows);
                }
            }
            Clients.Caller.Notify("Follow user : " + hasil);
        }

        [HubMethodName("FollowHashtag")]
        public void FollowHashtag(string LoginName, string HashtagToFollow)
        {
            bool hasil = false;
            var follows = (from c in db.GetAllData<SocialUser>()
                           where c.LoginName == LoginName
                           select c).SingleOrDefault();
            if (follows != null && follows.Follow != null && follows.Follow.Hashtags != null)
            {
                if (!follows.Follow.Hashtags.Contains(HashtagToFollow))
                {
                    follows.Follow.Hashtags.Add(HashtagToFollow);
                    hasil = db.InsertData<SocialUser>(follows);
                }
            }
            Clients.Caller.Notify("Follow hashtag : " + hasil);
        }

        [HubMethodName("UnFollowUser")]
        public void UnFollowUser(string LoginName, string UnfollowUser)
        {
            bool hasil = false;
            var item = (from c in db.GetAllData<SocialUser>()
                        where c.LoginName == LoginName
                        select c).SingleOrDefault();

            if (item != null && item.Follow != null && item.Follow.Users != null)
            {
                if (item.Follow.Users.Contains(UnfollowUser))
                {
                    item.Follow.Users.Remove(UnfollowUser);
                    hasil = db.InsertData<SocialUser>(item);
                }
            }

            Clients.Caller.Notify("UnFollow user : " + hasil);
        }

        [HubMethodName("UnFollowHashtag")]
        public void UnFollowHashtag(string LoginName, string UnFollowHashtag)
        {
            bool hasil = false;
            var item = (from c in db.GetAllData<SocialUser>()
                        where c.LoginName == LoginName
                        select c).SingleOrDefault();

            if (item != null && item.Follow != null && item.Follow.Hashtags != null)
            {
                if (item.Follow.Hashtags.Contains(UnFollowHashtag))
                {
                    item.Follow.Hashtags.Remove(UnFollowHashtag);
                    hasil = db.InsertData<SocialUser>(item);
                }
            }

            Clients.Caller.Notify("UnFollow user : " + hasil);
        }

        [HubMethodName("GetFollowers")]
        public List<string> GetFollowers(string LoginName)
        {
            var datas = from c in db.GetAllData<SocialUser>()
                        where c.LoginName == LoginName
                        select c;
            if (datas != null && datas.Count() > 0)
            {
                return datas.SingleOrDefault().Follow.Users.ToList();
                //Clients.Caller.DisplayFollows(datas.SingleOrDefault().Follow.Users.ToList());
            }
            return null;
        }
        /*
        [HubMethodName("GetTotalFollowers")]
        public int GetTotalFollowers(string LoginName)
        {
            int counter = 0;
            var datas = from c in db.GetAllData<SocialFollow>()
                          select c;
            if (datas != null && datas.Count() > 0)
            {
                foreach (var item in datas)
                {
                    foreach (var usr in item.Users)
                    {                        
                        if (usr == LoginName)
                        {
                            counter++;
                            break;
                        }
                    }
                }
            }
            return counter;
        }
        [HubMethodName("GetTotalFollowing")]
        public int GetTotalFollowing(string LoginName)
        {
            var datas = from c in db.GetAllData<SocialFollow>()
                        where c.LoginName == LoginName
                        select c;
            if (datas != null && datas.Count() > 0)
            {
                return datas.SingleOrDefault().Users == null ? 0 : datas.SingleOrDefault().Users.Count;
            }
            return 0;
        }
        [HubMethodName("FollowUser")]
        public void FollowUser(string LoginName, string UserToFollow)
        {
            bool hasil = false;
            var follows = from c in db.GetAllData<SocialFollow>()
                          where c.LoginName == LoginName
                          select c;
            if (follows != null && follows.Count() > 0)
            {
                foreach (var item in follows)
                {
                    HashSet<string> hash = new HashSet<string>();
                    item.Users.ForEach(x => hash.Add(x));
                    if (!hash.Contains(UserToFollow))
                    {
                        item.Users.Add(UserToFollow);
                        db.DeleteData<SocialFollow>(item.Id);
                        hasil = db.InsertData<SocialFollow>(item);
                        break;
                    }

                }
            }
            Clients.Caller.Notify("Follow user : " + hasil);
        }

        [HubMethodName("FollowHashtag")]
        public void FollowHashtag(string LoginName, string HashtagToFollow)
        {
            bool hasil = false;
            var follows = from c in db.GetAllData<SocialFollow>()
                          where c.LoginName == LoginName
                          select c;
            if (follows != null && follows.Count() > 0)
            {
                foreach (var item in follows)
                {
                    HashSet<string> hash = new HashSet<string>();
                    item.Hashtags.ForEach(x => hash.Add(x));
                    if (!hash.Contains(HashtagToFollow))
                    {
                        item.Hashtags.Add(HashtagToFollow);
                        db.DeleteData<SocialFollow>(item.Id);
                        hasil = db.InsertData<SocialFollow>(item);
                        break;
                    }

                }
            }
            Clients.Caller.Notify("Follow hashtag : " + hasil);
        }

        [HubMethodName("UnFollowUser")]
        public void UnFollowUser(string LoginName, string UnfollowUser)
        {
            bool hasil = false;
            var follows = from c in db.GetAllData<SocialFollow>()
                          where c.LoginName == LoginName
                          select c;
            if (follows != null && follows.Count() > 0)
            {
                foreach (var item in follows)
                {
                    HashSet<string> hash = new HashSet<string>();
                    item.Users.ForEach(x => hash.Add(x));
                    if (hash.Contains(UnfollowUser))
                    {
                        item.Users.Remove(UnfollowUser);
                        db.DeleteData<SocialFollow>(item.Id);
                        hasil = db.InsertData<SocialFollow>(item);
                        break;
                    }

                }
            }
            Clients.Caller.Notify("UnFollow user : " + hasil);
        }

        [HubMethodName("UnFollowHashtag")]
        public void UnFollowHashtag(string LoginName, string UnFollowHashtag)
        {
            bool hasil = false;
            var follows = from c in db.GetAllData<SocialFollow>()
                          where c.LoginName == LoginName
                          select c;
            if (follows != null && follows.Count() > 0)
            {
                foreach (var item in follows)
                {
                    HashSet<string> hash = new HashSet<string>();
                    item.Hashtags.ForEach(x => hash.Add(x));
                    if (hash.Contains(UnFollowHashtag))
                    {
                        item.Hashtags.Remove(UnFollowHashtag);
                        db.DeleteData<SocialFollow>(item.Id);
                        hasil = db.InsertData<SocialFollow>(item);
                        break;
                    }

                }
            }
            Clients.Caller.Notify("UnFollow hashtag : " + hasil);
        }

        [HubMethodName("GetFollowers")]
        public void GetFollowers(string LoginName)
        {
            var datas = from c in db.GetAllData<SocialFollow>()
                        where c.LoginName == LoginName
                        orderby c.Id
                        select c;
            if (datas == null || datas.Count() <= 0)
                Clients.Caller.DisplayFollows(new SocialFollow() { Users = null, Hashtags = null, LoginName = LoginName });
            else
                Clients.Caller.DisplayFollows(datas.SingleOrDefault());
        }
        */
        #endregion

        #region Category
        [HubMethodName("PostCategory")]
        public void PostCategory(string Title, string Description, string Hashtag)
        {
            SocialCategory Category = new SocialCategory() { Id = db.GetSequence<SocialCategory>(), Title = Title, Description = Description, Hashtags = Hashtag.Split(';').ToList() };
            bool hasil = db.InsertData<SocialCategory>(Category);
            Clients.Caller.Notify("post Category : " + hasil);
        }

        [HubMethodName("ModifyCategory")]
        public void ModifyCategory(int Id, string Title, string Description, string Hashtag)
        {
            SocialCategory Category = new SocialCategory() { Id = Id, Title = Title, Description = Description, Hashtags = Hashtag.Split(';').ToList() };
            bool hasil = db.InsertData<SocialCategory>(Category);
            Clients.Caller.Notify("Modify Category : " + hasil);
        }

        [HubMethodName("GetCategories")]
        public void GetCategories()
        {
            var datas = from c in db.GetAllData<SocialCategory>()
                        orderby c.Id
                        select c;
            Clients.Caller.DisplayCategory(datas.ToList());
        }

        [HubMethodName("GetCategoryById")]
        public SocialCategory GetCategoryById(long CategoryId)
        {
            var datas = from c in db.GetAllData<SocialCategory>()
                        where c.Id == CategoryId
                        orderby c.Id
                        select c;
            return datas.SingleOrDefault();
        }

        [HubMethodName("GetCategoryByName")]
        public void GetCategoryByName(string CategoryName)
        {
            var datas = from c in db.GetAllData<SocialCategory>()
                        where c.Title == CategoryName
                        orderby c.Id
                        select c;
            Clients.Caller.DisplayCategory(datas.ToList());
        }

        [HubMethodName("DeleteCategory")]
        public void DeleteCategory(long CategoryId)
        {
            bool hasil = db.DeleteData<SocialCategory>(CategoryId);
            Clients.Caller.Notify("delete Category : " + hasil);
        }
        #endregion

        #region User
        [HubMethodName("VerifyUser")]
        public int VerifyUser(string LoginName, string Name, string Phone, string Email, string PicUrl, string AuthType)
        {

            var datas = from c in db.GetAllData<SocialUser>()
                        where c.LoginName == LoginName
                        select c;
            if (datas != null && datas.Count() > 0) return 1;

            var hasil = db.InsertData<SocialUser>(new SocialUser() { Id = db.GetSequence<SocialUser>(), AuthType = AuthType, Email = Email, LoginName = LoginName, FullName = Name, PicUrl = PicUrl, Phone = Phone, Follow = new SocialFollow() { Hashtags = new HashSet<string>(), Users = new HashSet<string>() } });
            return hasil ? 1 : 0;
        }
        [HubMethodName("GetUserList")]
        public void GetUserList(string LoginName)
        {
            var currentuser = db.GetAllData<SocialUser>().Where(x => x.LoginName == LoginName).SingleOrDefault();
            var datas = from c in db.GetAllData<SocialUser>()
                        orderby c.LoginName
                        select new People() { Id = c.Id, Phone = c.Phone, Email = c.Email, Follow = c.Follow, LoginName = c.LoginName, MySiteUrl = c.MySiteUrl, PicUrl = c.PicUrl, FullName = c.FullName, TotalFollowers = GetTotalFollowers(c.LoginName), TotalFollowing = GetTotalFollowing(c.LoginName), TotalPost = GetPostByLoginName(c.LoginName).Count, IsFollowing = currentuser.Follow.Users.Contains(c.LoginName) };
            Console.WriteLine("hasil=" + datas.Count());
            Clients.Caller.DisplayUsers(datas.ToList());
        }
        [HubMethodName("GetUsers")]
        public void GetUsers()
        {
            var datas = from c in db.GetAllData<SocialUser>()
                        orderby c.LoginName
                        select c;
            Clients.Caller.DisplayUsers(datas.ToList());
        }
        [HubMethodName("GetUsersByKeyword")]
        public void GetUsersByKeyword(string Keyword)
        {

            var datas = from c in db.GetAllData<SocialUser>()
                        where c.LoginName.ToLower().Contains(Keyword.ToLower())
                        orderby c.LoginName
                        select new People() { Id = c.Id, Phone = c.Phone, Email = c.Email, Follow = c.Follow, LoginName = c.LoginName, MySiteUrl = c.MySiteUrl, PicUrl = c.PicUrl, FullName = c.FullName, TotalFollowers = GetTotalFollowers(c.LoginName), TotalFollowing = GetTotalFollowing(c.LoginName), TotalPost = GetPostByLoginName(c.LoginName).Count };
            Clients.Caller.DisplayUsers(datas.ToList());
        }
        [HubMethodName("GetUserById")]
        public SocialUser GetUserById(long UserId)
        {
            var datas = from c in db.GetAllData<SocialUser>()
                        where c.Id == UserId
                        select c;
            return datas.SingleOrDefault();
        }

        [HubMethodName("GetUserByEmail")]
        public SocialUser GetUserByEmail(string Mail)
        {
            var datas = from c in db.GetAllData<SocialUser>()
                        where c.Email.Equals(Mail, StringComparison.CurrentCultureIgnoreCase)
                        select c;
            return datas.SingleOrDefault();
        }
        #endregion

        #region Store
        [HubMethodName("GetAllStores")]
        public IEnumerable<Business> GetAllStores()
        {
            var datas = db.GetAllData<Business>().OrderBy(x => x.Title);
            return datas;
        }

        [HubMethodName("GetStoresByID")]
        public IEnumerable<Business> GetStoresById(long sId)
        {
            var datas = db.GetAllData<Business>().Where(x => x.Id == sId);
            return datas;
        }


        [HubMethodName("GetStores")]
        public IEnumerable<Business> GetStores(string SortBy, string sStoreCategory, double sHighestPrice, double sLowestPrice, int [] iFasility)
        {
            var datas = db.GetAllData<Business>().Where(x => x.Id != null);

            if (!string.IsNullOrEmpty(sStoreCategory))
            {
                datas = db.GetAllData<Business>().Where(x => x.Category.Trim() == sStoreCategory);
            }
            foreach (int option in iFasility)
            {
                switch (option)
                {
                    case 0:
                        {
                            datas = datas.Where(x => x.Facilities.Contains("Parkir Luas"));
                        }
                        break;
                    case 1:
                        {
                            datas = datas.Where(x => x.Facilities.Contains("Porsi Besar"));
                        }
                        break;
                    case 2:
                        {
                            datas = datas.Where(x => x.Facilities.Contains("Prasmanan"));
                        }
                        break;
                }
            }
            if (!string.IsNullOrEmpty(SortBy))
            {
                switch (SortBy)
                {
                    case "Terdekat":
                        {
                            datas.OrderBy(x => x.Title);
                        }
                        break;
                    case "Terjauh":
                        {
                            datas = datas.OrderBy(x => x.Title);
                        }
                        break;
                    case "Termurah":
                        {
                            datas = datas.OrderBy(x => x.LowestPrice);
                        }
                        break;
                    case "Termahal":
                        {
                            datas = datas.OrderByDescending(x => x.HighestPrice);
                        }
                        break;
                    case "Terpopuler":
                        {
                            datas = datas.OrderByDescending(x => x.Visitors);
                        }
                        break;
                    default:
                        {
                            datas.OrderBy(x => x.Title);
                        }
                        break;
                }
            }
            if (sHighestPrice > 0 && sLowestPrice > 0)
            {
                datas = datas.Where(x => x.LowestPrice >= sLowestPrice && x.HighestPrice <= sHighestPrice);
            }

            return datas;
        }

        [HubMethodName("GetStoreByKeyword")]
        public IEnumerable<Business> GetStoreByKeyword(string Keyword)
        {
            var datas = from x in db.GetAllData<Business>()
                        where x.Title.Contains(Keyword, StringComparison.CurrentCultureIgnoreCase)
                        orderby x ascending
                        select x;
            return datas;
        }

        [HubMethodName("GetProductByIDStore")]
        public IEnumerable<Product> GetProductByIDStore(long sIDStore,string sProductCategory)
        {
            var datas = from x in db.GetAllData<Product>()
                        where x.IDBusiness == sIDStore && x.ProductCategory == sProductCategory
                        orderby x ascending
                        select x;
            return datas;
        }
        [HubMethodName("AddProduct")]
        public OutputData AddProduct(long sIdStore, string sTitle, string sDesc, double sPrice)
        {
            //insert facilities
           //endf
            string Username = Context.User.Identity.Name;
            var node = new Product()
            {
                Id = db.GetSequence<Product>(),
                Title = sTitle,
                Desc = sDesc,
                IDBusiness = sIdStore,
                Price = sPrice
            };
            db.InsertData<Product>(node);
            return new OutputData() { Data = node, IsSucceed = true };
        }

        [HubMethodName("AddStore")]
        public OutputData AddStore(string sTitle, string sDesc, string sStoreCategory, string sCity, string[] sFacilities, string sImageUrl)
        {
            //insert facilities
            string value = "";
            var sf = new HashSet<string>();
            foreach (string option in sFacilities)
            {
                value = option;
                sf.Add(value);
            }
            //end

            string Username = Context.User.Identity.Name;
            var node = new Business()
            {
                LoginName = Username,
                Id = db.GetSequence<Business>(),
                Title = sTitle,
                Desc = sDesc,
                Address = new SocialAddress()
                {
                    City = sCity
                },
                PriceMeter = new List<SocialRating>(),
                Recommendation = new List<SocialRecommendation>(),
                Experience = new List<SocialRating>(),
                Comments = new List<SocialComment>(),
                Visitors = new List<SocialCheckIn>(),
                Facilities = sf,
                ImageUrl = sImageUrl,
                Category = sStoreCategory
            };
            db.InsertData<Business>(node);
            return new OutputData() { Data = node, IsSucceed = true };
        }

        [HubMethodName("UpdateStore")]
        public OutputData UpdateStore(int IdStore, string Title, string Category, string Desc, string Address, string City, double HighP, double LowP)
        {
            Business temp = null;
            var datas = from x in db.GetAllData<Business>()
                        where x.Id == IdStore
                        select x;
            foreach (var node in datas)
            {
                node.Title = Title;
                node.Desc = Desc;
                node.Address.Location = Address;
                node.Address.City = City;
                node.HighestPrice = HighP;
                node.LowestPrice = LowP;
                node.Category = Category;
                db.InsertData<Business>(node);
                temp = node;
                return new OutputData() { Data = temp, IsSucceed = true };
            }
            return new OutputData() { Data = temp, IsSucceed = false };
        }
        [HubMethodName("UpdateStoreComment")]
        public OutputData UpdateStoreComment(int IdStore, string sMessage, int sMurahMeter, int sKenikmatan)
        {
            string Username = Context.User.Identity.Name;
            long ids = db.GetSequence<SocialRating>();
            SocialComment sc = new SocialComment()
            {
                Id = ids,
                Message = sMessage,
                CreatedDate = DateTime.Now,
                LoginName = Username,
                Name = Context.User.Identity.Name
            };
            SocialRating mm = new SocialRating()
            {
                Id  = ids,
                LoginName = Username,
                RatingValue = sMurahMeter
            };
            SocialRating km = new SocialRating()
            {
                Id = ids,
                LoginName = Username,
                RatingValue = sKenikmatan
            };

            Business temp = null;
            var datas = from x in db.GetAllData<Business>()
                        where x.Id == IdStore
                        select x;
            foreach (var node in datas)
            {
                node.PriceMeter.Add(mm);
                node.Experience.Add(km);
                node.Comments.Add(sc);
                db.InsertData<Business>(node);
                temp = node;
                return new OutputData() { Data = temp, IsSucceed = true };
            }
            return new OutputData() { Data = temp, IsSucceed = false };
        }
        //public OutputData UpdateChildStoreComment()
        //{

        //}
        [HubMethodName("DeleteStore")]
        public OutputData DeleteStore(int IdStore)
        {
            var hasil = db.DeleteData<Business>(IdStore);
            return new OutputData() { Data = IdStore, IsSucceed = hasil };
        }

        [HubMethodName("DeleteAllStore")]
        public OutputData DeleteAllStore()
        {
            var hasil = db.DeleteAllData<Business>();
            return new OutputData() { IsSucceed = hasil };
        }
        #endregion

        #region Others
        [HubMethodName("GetServerTime")]
        public string GetServerTime()
        {
            return DateTime.Now.ToString("dd MM yyyy HH:mm:ss");
        }

        [HubMethodName("GetTest")]
        public string GetTest()
        {
            return "test boss";
        }
        #endregion
    }
}
