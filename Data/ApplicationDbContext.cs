﻿using Blogify.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<BlogPost> Blogs { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<SeenBlog> SeenBlogs { get; set; }
    }
}
