using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace _521_Project_3.Models
{
    public class Movie
    {
        public int Id { get; set; }

        public string? Title { get; set; }
        public string? Genre { get; set; }

        [Display(Name = "Year Released")]
        public string? YearReleased { get; set; }

        [Display(Name = "IMDB Link")]
          public string? IMDBLink { get; set; }


        [DataType(DataType.Upload)]
        [DisplayName("Movie Image")]
        public byte[]? MovieImage { get; set; }


    }
}
