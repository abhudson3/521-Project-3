using Humanizer;

namespace _521_Project_3.Models
{
    public class ActorDetailsVM
    {
        public Actor Actor { get; set; }

        public List<Movie> Movies { get; set; }

        public string Sentiment { get; set; }
        public List<string> Reddit { get; set; }

        public List<PostSentiment> PostSentiments { get; set; }

        public ActorDetailsVM()
        {
            Movies = new List<Movie>();
            Reddit = new List<string>();
            PostSentiments = new List<PostSentiment>();
        }

//        On the actors details view, in the view folder, change the namespace, just foo
        
    }
}
