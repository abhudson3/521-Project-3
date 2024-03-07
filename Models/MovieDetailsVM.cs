namespace _521_Project_3.Models
{
    public class MovieDetailsVM
    {
        public Movie Movie { get; set; }

        public List<Actor> Actors{ get; set; }

        public string Sentiment { get; set; }
        public List<string> RedditPosts { get; set; }

        public List<PostSentiment> PostSentiments { get; set; }

        public MovieDetailsVM()
        {
            Actors = new List<Actor>();
            RedditPosts = new List<string>();
            PostSentiments = new List<PostSentiment>();
        }


    }
}
