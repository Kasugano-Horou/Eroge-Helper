using System;

namespace ErogeHelper.Models
{
    class MojiResponse
    {
        public Result result { get; set; }
    }

    public class Result
    {
        public string originalSearchText { get; set; }
        public Searchresult[] searchResults { get; set; }
        public Word[] words { get; set; }
    }

    public class Searchresult
    {
        public string searchText { get; set; }
        public string tarId { get; set; }
        public int type { get; set; }
        public int count { get; set; }
        public string title { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public string objectId { get; set; }
    }

    public class Word
    {
        public string excerpt { get; set; }
        public string spell { get; set; }
        public string accent { get; set; }
        public string pron { get; set; }
        public string romaji { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public string updatedBy { get; set; }
        public string objectId { get; set; }
    }
}
