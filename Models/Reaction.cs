using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blogify.Models
{
    public class Reaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int BlogPostId { get; set; }
        public BlogPost BlogPost { get; set; }

        public int IsLiked { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }
    }
}
