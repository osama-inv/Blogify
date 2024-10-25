using Blogify.Data;
using Blogify.Factories;
using Blogify.Models;
using Blogify.Services;
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
        public async Task<bool> IsAllOkayy()
        {
            var user = await _userManager.GetUserAsync(User);

            var userClaims = await _userManager.GetClaimsAsync(user);
            if (userClaims.Any(c => c.Type == "block" && c.Value == "true"))
            {
                TempData["msg1"] = "Your account has been blocked";
                return false;
            }

            return true;
        }
        public async Task<IActionResult> IsAllOkay()
        {

            if (!(await IsAllOkayy()))
            {
                return RedirectToAction("show");
            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (!(await IsAllOkayy()))
                {
                    return RedirectToAction("show");
                }

                var blogs = await _DB_Contect.Blogs.AsNoTracking()
                .Include(b => b.Author)
                .Include(b => b.Reactions)
                .Select(b => new BlogPostDto()
                {
                    Id = b.Id,
                    Title = b.Title,
                    Content = b.Content,
                    AutherName = b.Author.UserName,
                    CreationTime = b.CreatedAt,
                    NumOfLikes = b.Reactions.Count(q => q.IsLiked == 1),
                    NumOfDisLikes = b.Reactions.Count(q => q.IsLiked == 0),
                    IsLikedByCUser = b.Reactions.Any(q => q.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)) ? b.Reactions.First(q => q.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).IsLiked == 1 ? "Like" : "Dislike" : "None",
                    Premium = _DB_Contect.UserClaims.Any(c => c.UserId == b.AuthorId && c.ClaimType == "premium" && c.ClaimValue == "true")

                })
                .OrderByDescending(b => b.Id) // Order by Id descending (for reverse)
                .ToListAsync();

                var MoreLikes = blogs.OrderByDescending(b => b.NumOfLikes).Take(4).ToList();

                TrendinFeed Trendin = new TrendinFeed()
                {
                    Feeds = blogs,
                    Trends = MoreLikes
                };

                return View(Trendin);
            }

            return View("IndexOut");
        }
        [HttpPost("LogSeenBlog")]
        public async Task LogSeenBlog(string Blogid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var blogId = int.Parse(Blogid);
            var here = await _DB_Contect.SeenBlogs.AnyAsync(c => c.UserId == userId && c.BlogPostId == blogId);

            if (!here)
            {
                SeenBlog seenblog = new SeenBlog()
                {
                    BlogPostId = blogId,
                    UserId = userId
                };

                var answer = await _DB_Contect.SeenBlogs.AddAsync(seenblog);

                await _DB_Contect.SaveChangesAsync();
            }
        }

        public IQueryable<SeenBlog> GetWhereUserId(string userId)
        {
            return _DB_Contect.SeenBlogs.AsNoTracking().Where(b => b.UserId == userId);
        }
        [HttpGet("popular")]
        public async Task<IActionResult> popularBlogs()
        {
            if (User.Identity.IsAuthenticated)
            {

                var GetWhere = await _DB_Contect.SeenBlogs.AsNoTracking()
                    .GroupBy(i => i.BlogPostId)
                    .OrderByDescending(g => g.Count())
                    .Select(group => group.Key).Take(10)
                    .ToListAsync();


                var blogs = await _DB_Contect.Blogs.AsNoTracking()
                .Where(b => GetWhere.Contains(b.Id))
                .Include(b => b.Author)
                .Select(b => new BlogPostDto()
                {
                    Id = b.Id,
                    Title = b.Title,
                    Content = b.Content,
                    AutherName = b.Author.UserName,
                    CreationTime = b.CreatedAt,
                    Premium = _DB_Contect.UserClaims.Any(c => c.UserId == b.AuthorId && c.ClaimType == "premium" && c.ClaimValue == "true"),
                    NumOfLikes = b.Reactions.Count(q => q.IsLiked == 1),
                    NumOfDisLikes = b.Reactions.Count(q => q.IsLiked == 0),
                    IsLikedByCUser = b.Reactions.Any(q => q.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)) ? b.Reactions.First(q => q.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).IsLiked == 1 ? "Like" : "Dislike" : "None",
                })
                //   .OrderByDescending(b => b.Id) // Order by Id descending (for reverse)
                .ToListAsync();

                return View(blogs);
            }

            return RedirectToAction("Login");
        }
        [HttpGet("SeenBlogs")]
        public async Task<IActionResult> SeenBlogs()
        {
            if (User.Identity.IsAuthenticated)
            {

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var GetWhere = GetWhereUserId(userId).Select(s => s.BlogPostId).ToList();


                var blogs = _DB_Contect.Blogs.AsNoTracking()
                .Where(b => GetWhere.Contains(b.Id))
                .Include(b => b.Author)
                .Select(b => new BlogPostDto()
                {
                    Id = b.Id,
                    Title = b.Title,
                    Content = b.Content,
                    AutherName = b.Author.UserName,
                    CreationTime = b.CreatedAt,
                    Premium = _DB_Contect.UserClaims.Any(c => c.UserId == b.AuthorId && c.ClaimType == "premium" && c.ClaimValue == "true"),
                    NumOfLikes = b.Reactions.Count(q => q.IsLiked == 1),
                    NumOfDisLikes = b.Reactions.Count(q => q.IsLiked == 0),
                    IsLikedByCUser = b.Reactions.Any(q => q.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)) ? b.Reactions.First(q => q.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).IsLiked == 1 ? "Like" : "Dislike" : "None",
                })
                //   .OrderByDescending(b => b.Id) // Order by Id descending (for reverse)
                .ToList();

                return View(blogs);
            }

            return RedirectToAction("Login");
        }

        [HttpGet("showmyclaim")]
        public async Task<IActionResult> showmyclaim()
        {
            var _ClaimsManager = new ClaimsManager(_DB_Contect);

            var languageClaimValue = _ClaimsManager.GetClaimValue(User.FindFirstValue(ClaimTypes.NameIdentifier), "Language");

            if (languageClaimValue.Result == null)
            {
                return Json(new { success = false, message = "No language claim found" });
            }

            WelcomGuestService service = new WelcomGuestService(FactoryPresenter.GetPresenter(languageClaimValue.Result));

            TempData["msg1"] = service.Welcome();

            return RedirectToAction("show");

        }

        [HttpGet("language/ask/{languageclaim}")]
        public async Task<IActionResult> LanguageClaimChange(string languageclaim)
        {
            var requiredClaim = ClaimsManager.GetLanguageClaim(languageclaim);

            if (requiredClaim == null)
            {
                return Json(new { success = false, message = "Invalid language" });
            }

            var user = await _userManager.GetUserAsync(User);

            var _ClaimsManager = new ClaimsManager(_DB_Contect);

            var hasClaim = await _ClaimsManager.HasClaimType(user.Id, requiredClaim.Type);

            if (hasClaim is true)
            {
                // If the claim exists, replace it
                var result = await _ClaimsManager.ChnageClaimValue(user.Id, requiredClaim);

                if (result)
                    return Json(new { success = true, message = "Claim updated successfully" });
                else
                    return Json(new { success = false, message = "Failed to update claim" });

            }
            else
            {
                var result = await _userManager.AddClaimAsync(user, requiredClaim);

                if (result.Succeeded)
                    return Json(new { success = true, message = "Claim added successfully" });

                else
                    return Json(new { success = false, message = "Failed to add claim" });

            }

        }

        [HttpGet("AskForPremium")]
        public async Task<IActionResult> AskForPremium()
        {

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var userClaims = await _userManager.GetClaimsAsync(user);
            if (userClaims.Any(c => c.Type == "premium" && c.Value == "true"))
            {
                return Json(new { success = false, message = "User is already premium" });
            }

            // Add the "premium" claim
            var claim = new Claim("premium", "true");
            var result = await _userManager.AddClaimAsync(user, claim);


            if (result.Succeeded)
            {
                return Json(new { success = true, message = "User is now premium" });
            }

            return Json(new { success = false, message = "Failed to make user premium" });
        }
        [HttpGet("Admin")]
        public async Task<IActionResult> Admin()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);

                var userClaims = await _userManager.GetClaimsAsync(user);

                if (userClaims.Any(c => c.Type == "admin" && c.Value == "true"))
                {
                    return Json(new { success = false, message = "you are admin" });
                }
                else
                {
                    var claim = new Claim("block", "true");
                    var result = await _userManager.AddClaimAsync(user, claim);
                    // sign him out
                    await _signInManager.SignOutAsync();
                    if (result.Succeeded)
                    {
                        return Json(new { success = true, message = "you are blocked for tring to get something that is not yours" });
                    }
                }
            }
            else return Json(new { success = false, message = "you are not authenticated" });

            return Json(new { success = false, message = "Failed to make this request" });
        }
        [HttpPost]
        public async Task<IActionResult> Reactions(int id, string value)
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (user == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            bool found = await _DB_Contect.Reactions.AnyAsync(a => a.UserId == user && a.BlogPostId == id);

            if (found)
            {
                var reaction = await _DB_Contect.Reactions.FirstAsync(a => a.UserId == user && a.BlogPostId == id);

                string NormalizeTheSurrentState = reaction.IsLiked == 1 ? "like" : "dislike";

                if (NormalizeTheSurrentState == value)
                {
                    _DB_Contect.Reactions.Remove(reaction);
                    await _DB_Contect.SaveChangesAsync();

                    var numofa = await _DB_Contect.Reactions
                .Where(c => c.BlogPostId == id)
                .GroupBy(g => 1)
                .Select(g => new
                {
                    CLikes = g.Count(x => x.IsLiked == 1),
                    CDisLikes = g.Count(x => x.IsLiked == 0)
                })
                .FirstOrDefaultAsync();

                    return Json(new { success = true, numofLikes = numofa.CLikes, numofDislikes = numofa.CDisLikes });
                }

                reaction.IsLiked = value == "like" ? 1 : 0;

                _DB_Contect.Reactions.Update(reaction);
                await _DB_Contect.SaveChangesAsync();

                var numofe = await _DB_Contect.Reactions
                .Where(c => c.BlogPostId == id)
                .GroupBy(g => 1)
                .Select(g => new
                {
                    CLikes = g.Count(x => x.IsLiked == 1),
                    CDisLikes = g.Count(x => x.IsLiked == 0)
                })
                .FirstOrDefaultAsync();

                return Json(new { success = true, numofLikes = numofe.CLikes, numofDislikes = numofe.CDisLikes });
            }

            _DB_Contect.Reactions.Add(new Reaction()
            {
                BlogPostId = id,
                UserId = user,
                IsLiked = value == "like" ? 1 : 0
            });

            await _DB_Contect.SaveChangesAsync();



            var numof = await _DB_Contect.Reactions
                .Where(c => c.BlogPostId == id)
                .GroupBy(g => 1)
                .Select(g => new
                {
                    CLikes = g.Count(x => x.IsLiked == 1),
                    CDisLikes = g.Count(x => x.IsLiked == 0)
                })
                .FirstOrDefaultAsync();

            return Json(new { success = true, numofLikes = numof.CLikes, numofDislikes = numof.CDisLikes });
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
                    CreationTime = b.CreatedAt,
                    Premium = _DB_Contect.UserClaims.Any(c => c.UserId == b.AuthorId && c.ClaimType == "premium" && c.ClaimValue == "true")

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
                .Include(b => b.Reactions)
                .Select(b => new BlogPostDto()
                {
                    Id = b.Id,
                    Title = b.Title,
                    Content = b.Content,
                    AutherName = _username,
                    CreationTime = b.CreatedAt,
                    NumOfLikes = b.Reactions.Count(q => q.IsLiked == 1),
                    NumOfDisLikes = b.Reactions.Count(q => q.IsLiked == 0),
                    IsLikedByCUser = b.Reactions.Any(q => q.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)) ? b.Reactions.First(q => q.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).IsLiked == 1 ? "Like" : "Dislike" : "None",
                    Premium = _DB_Contect.UserClaims.Any(c => c.UserId == b.AuthorId && c.ClaimType == "premium" && c.ClaimValue == "true")

                })
                .OrderByDescending(b => b.Id) // Order by Id descending (for reverse)
                .ToList();

                return View(blogs);
            }

            return RedirectToAction("Login");
        }

        [HttpGet("show")]
        public IActionResult show()
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
            var result = _userManager.CreateAsync(user, "123456").Result;

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
        [HttpGet("ShowBlog/{id}")]
        public async Task<IActionResult> ShowBlog(string id)
        {
            var blogs = await _DB_Contect.Blogs.AsNoTracking()
                .Where(b => b.Id == int.Parse(id))
                .Include(b => b.Author)
                .Include(b => b.Reactions)
                .Select(b => new BlogPostDto()
                {
                    Id = b.Id,
                    Title = b.Title,
                    Content = b.Content,
                    AutherName = b.Author.UserName,
                    CreationTime = b.CreatedAt,
                    NumOfLikes = b.Reactions.Count(q => q.IsLiked == 1),
                    NumOfDisLikes = b.Reactions.Count(q => q.IsLiked == 0),
                    IsLikedByCUser = b.Reactions.Any(q => q.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)) ? b.Reactions.First(q => q.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).IsLiked == 1 ? "Like" : "Dislike" : "None"
                })
                .OrderByDescending(b => b.Id) // Order by Id descending (for reverse)
                .FirstOrDefaultAsync();

            if (blogs == null)
            {
                return RedirectToAction("Index");
            }

            return View(blogs);

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

            await _userManager.RemoveClaimAsync(user, new Claim("block", "true"));

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
                    return RedirectToAction("IsAllOkay");
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
