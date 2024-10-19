using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blogify.Models
{
    public class BlogPost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AuthorId { get; set; }
        public IdentityUser Author { get; set; }
        public IEnumerable<Reaction> Reactions { get; set; }
    }
}
