using Blogify.Data;
using Blogify.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace Blogify.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _DB_Contect;

        public HomeController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ApplicationDbContext DB_Contect)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _DB_Contect = DB_Contect;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var blogs = _DB_Contect.Blogs.AsNoTracking()
                .Include(b => b.Author)
                .Select(b => new BlogPostDto()
                {
                    Id = b.Id,
                    Title = b.Title,
                    Content = b.Content,
                    AutherName = b.Author.UserName,
                    CreationTime = b.CreatedAt
                })
                .OrderByDescending(b => b.Id) // Order by Id descending (for reverse)
                .ToList();

                return View(blogs);
            }

            return View("IndexOut");
        }
        [HttpGet("MyBlog")]
        public async Task<IActionResult> MyBlog()
        {
            if (User.Identity.IsAuthenticated)
            {

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var username = User.Identity.Name;
                if (userId == null)
                {
                    // Handle case where user is not authenticated or ID is missing
                    return RedirectToAction("Login");
                }

                var blogs = _DB_Contect.Blogs.AsNoTracking()
                .Where(b => b.AuthorId == userId)
                .Select(b => new BlogPostDto()
                {
                    Id = b.Id,
                    Title = b.Title,
                    Content = b.Content,
                    AutherName = username,
                    CreationTime = b.CreatedAt
                })
                .OrderByDescending(b => b.Id) // Order by Id descending (for reverse)
                .ToList();

                return View(blogs);
            }

            return RedirectToAction("Login");
        }
        [HttpGet("blogger/{_username}")]
        public async Task<IActionResult> Profile(string _username)
        {
            if (User.Identity.IsAuthenticated)
            {
                var _usernameFormId = await _userManager.Users
                    .Where(u => u.UserName == _username)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();

                var blogs = _DB_Contect.Blogs.AsNoTracking()
                .Where(b => b.AuthorId == _usernameFormId)
                .Select(b => new BlogPostDto()
                {
                    Id = b.Id,
                    Title = b.Title,
                    Content = b.Content,
                    AutherName = _username,
                    CreationTime = b.CreatedAt
                })
                .OrderByDescending(b => b.Id) // Order by Id descending (for reverse)
                .ToList();

                return View(blogs);
            }

            return RedirectToAction("Login");
        }
        [HttpGet("showw")]
        public IActionResult Show()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("Login")]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index");

            return View();
        }
        [HttpGet("create")]
        public IActionResult create()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index");

            var model = Guid.NewGuid().ToString();
            var randomname = "viewer_" + model[..5];

            var user = new IdentityUser { UserName = randomname, Email = randomname + "@gmail.com" };
            var result = _userManager.CreateAsync(user, model).Result;

            if (result.Succeeded)
            {
                _signInManager.SignInAsync(user, isPersistent: false).Wait();

            }

            return RedirectToAction("Index");
        }
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var obj = await _DB_Contect.Blogs.FindAsync(int.Parse(id));

                if (obj == null)
                {
                    return RedirectToAction("Index");
                }

                var blogPost = new BlogPostDto()
                {
                    Id = obj.Id,
                    Title = obj.Title,
                    Content = obj.Content
                };

                return View(blogPost);
            }

            return RedirectToAction("login");
        }
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditPost(string id, BlogPostDto postblog)
        {
            if (User.Identity.IsAuthenticated)
            {
                var obj = await _DB_Contect.Blogs.FindAsync(int.Parse(id));

                if (obj == null)
                {
                    return RedirectToAction("Index");
                }

                obj.Title = postblog.Title;
                obj.Content = postblog.Content;

                _DB_Contect.Blogs.Update(obj);

                await _DB_Contect.SaveChangesAsync();

                return RedirectToAction("Index");

            }

            return RedirectToAction("login");
        }
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> delete(string id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var obj = await _DB_Contect.Blogs.FindAsync(int.Parse(id));

                if (obj == null)
                {
                    return RedirectToAction("Index");
                }

                var blogPost = new BlogPostDto()
                {
                    Id = obj.Id,
                    Title = obj.Title,
                    Content = obj.Content
                };

                return View(blogPost);
            }

            return RedirectToAction("login");
        }
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> deletePost(string id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var obj = await _DB_Contect.Blogs.FindAsync(int.Parse(id));

                //get the id of the user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (obj == null || obj.AuthorId != userId)
                {
                    return RedirectToAction("Index");
                }

                _DB_Contect.Blogs.Remove(obj);

                await _DB_Contect.SaveChangesAsync();

                return RedirectToAction("Index");

            }

            return RedirectToAction("login");
        }
        [HttpGet("block/{id}")]
        public async Task<IActionResult> block(string id)
        {
            var user = await _userManager.FindByNameAsync(id);

            if (user == null)
            {
                return RedirectToAction("Show");
            }

            await _userManager.AddClaimAsync(user, new Claim("IsBlocked", "true"));

            return RedirectToAction("Index");
        }
        [HttpGet("unblock/{id}")]
        public async Task<IActionResult> unblock(string id)
        {
            var user = await _userManager.FindByNameAsync(id);

            if (user == null)
            {
                return RedirectToAction("Show");
            }

            await _userManager.RemoveClaimAsync(user, new Claim("IsBlocked", "true"));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> LoginPost(Login login)
        {
            if (ModelState.IsValid)
            {
                IdentityUser _user;

                if (login.UserName.Contains("@"))
                    _user = await _userManager.FindByEmailAsync(login.UserName);
                else
                    _user = await _userManager.FindByNameAsync(login.UserName);

                if (_user != null)
                {
                    var claims = await _userManager.GetClaimsAsync(_user);
                    if (claims.Any(c => c.Type == "IsBlocked" && c.Value == "true"))
                    {
                        TempData["ErrorMessage"] = "Your account has been blocked";
                        return RedirectToAction("Login");
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(_user, login.Password, false, true);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "Invalid Login Attempt";

            }
            return RedirectToAction("Login");
        }
        [HttpGet("Signup")]
        public IActionResult Signup()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index");

            return View();
        }
        [HttpGet("Blog")]
        public IActionResult Blog()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View();
            }

            return RedirectToAction("Login");
        }
        [HttpPost]
        public async Task<IActionResult> BlogPost(BlogPostDto blogPost)
        {
            if (User.Identity.IsAuthenticated)
            {

                BlogPost obj = new BlogPost
                {
                    Title = blogPost.Title,
                    Content = blogPost.Content,
                    CreatedAt = DateTime.Now,
                    AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };

                await _DB_Contect.Blogs.AddAsync(obj);

                await _DB_Contect.SaveChangesAsync();

                return RedirectToAction("index");
            }
            return RedirectToAction("login");
        }
        [HttpPost]
        public IActionResult SignupPost(SignUp model)
        {
            if (ModelState.IsValid)
            {
                // Create a new user
                var user = new IdentityUser { UserName = model.UserName, Email = model.UserName + "@gmail.com" };
                var result = _userManager.CreateAsync(user, model.Password).Result;
                if (result.Succeeded)
                {
                    _signInManager.SignInAsync(user, isPersistent: false).Wait();
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View("SignUp", model);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
