using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace _521_Project_3.Models
{
    public class Actor
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Gender { get; set; }
        public string? Age { get; set; }
        
        public string? IMDBLink { get; set; }

        [DataType(DataType.Upload)]
        [DisplayName("Actor Image")]

        public byte[]? Photo { get; set; }
    }
}
