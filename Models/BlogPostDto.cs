using System.ComponentModel.DataAnnotations;

namespace Blogify.Models
{
    public class BlogPostDto
    {
        public int Id { get; set; }
        [Required]
        [StringLength(300, MinimumLength = 2, ErrorMessage = "Title must be at least 2 characters long.")]
        public string Title { get; set; }

        [Required]
        [StringLength(5000, MinimumLength = 3, ErrorMessage = "Blog's Content must be at least 3 characters long.")]
        public string Content { get; set; }
        public int NumOfLikes { get; set; }
        public int NumOfDisLikes { get; set; }
        public string? AutherName { get; set; }
        public DateTime? CreationTime { get; set; }
        public string IsLikedByCUser { get; set; } = "None"; // "Like", "Dislike", "None"
        public BlogPostDto()
        {

        }
        public BlogPostDto(int id, string title, string content, string? autherName = null, DateTime? creationTime = null)
        {
            Id = id;
            Title = title;
            Content = content;
            AutherName = autherName;
            CreationTime = creationTime;
        }
    }

}
