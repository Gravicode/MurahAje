using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MurahAje.Web.Entities
{
    public class Store
    {
        public string LoginName { set; get; }
        public long Id { set; get; }
        public string Title { set; get; }
        public string Desc { set; get; }
        public string StoreCategory { set; get; }
        public double LowestPrice { set; get; }
        public double HighestPrice { set; get; }
        public SocialAddress Address { set; get; }
        public List<SocialRating> Ratings { set; get; }
        public List<SocialComment> Comments { set; get; }
    }

    public class SocialAddress
    {
        public string Location  {set;get;}
        public double Longitude {set;get;}
        public double Latitude  {set;get;}
        public string City      {set;get;}
        public string Country { set; get; }
    }
    public class Product
    {
        public long Id { set; get; }
        public string IDStore { set; get; }
        public string Title { set; get; }
        public string Desc { set; get; }
        public string ProductCategory { set; get; }

        public List<SocialRating> Ratings { set; get; }

        public List<SocialComment> Comments { set; get; }

    }

    public class SocialRating
    {
        public long Id { set; get; }
        public string LoginName { set; get; }
        public int RatingValue { set; get; }

    }
    public class SocialPost
    {

        public long Id { set; get; }

        public string LoginName { set; get; }

        public string Name { set; get; }

        public string Message { set; get; }

        public DateTime CreatedDate { set; get; }

        public Double Longitude { set; get; }

        public Double Latitude { set; get; }

        public string Mention { set; get; }

        public string Hashtag { set; get; }

        public string Url { set; get; }

        public string mimeType { set; get; }

        public bool isVisible { set; get; }

        public string FilePath { set; get; }

        public List<SocialComment> Comments { set; get; }

        public List<SocialLike> Likes { set; get; }

    }
    
    public class SocialUser
    {
        public long Id { set; get; }
        public string LoginName { set; get; }
        public string FullName { set; get; }
        public string PicUrl { set; get; }
        public string MySiteUrl { set; get; }
        public string Email { set; get; }
        public string Phone { set; get; }
        public string AuthType { set; get; }
        public SocialFollow Follow { set; get; }
    }

    
    public class People : SocialUser
    {
        public int TotalFollowers { set; get; }
        public int TotalFollowing { set; get; }
        public int TotalPost { set; get; }
        public bool IsFollowing { set; get; }

    }

    
    public class FollowResult
    {

        public string LoginName { set; get; }
        public int Hasil { set; get; }
    }

    
    public class SocialComment
    {
        public long Id { set; get; }
        //public long ItemId { set; get; }
        public string Message { set; get; }
        public DateTime CreatedDate { set; get; }
        public string LoginName { set; get; }
        public string Name { set; get; }
    }
    
    public class SocialLike
    {
        public long Id { set; get; }
        //public long ItemId { set; get; }
        public string Name { set; get; }
        public string LoginName { set; get; }
        public DateTime CreatedDate { set; get; }
    }
    
    public class SocialFollow
    {
        //public long Id { set; get; }
        //public string LoginName { set; get; }
        public HashSet<string> Hashtags { set; get; }
        public HashSet<string> Users { set; get; }
    }
    
    public class SocialCategory
    {
        public long Id { set; get; }
        public string Title { set; get; }
        public string Description { set; get; }
        public List<string> Hashtags { set; get; }
    }
    
    public class SocialHashtag
    {
        public long Id { set; get; }
        public string Hashtag { set; get; }
        public int Count { set; get; }
    }
}
