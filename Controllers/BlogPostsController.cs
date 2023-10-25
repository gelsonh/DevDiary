using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DevDiary.Data;
using DevDiary.Models;
using DevDiary.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using X.PagedList;
using DevDiary.Helpers;

namespace DevDiary.Controllers
{
    // [Authorize]
    public class BlogPostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IImageService _imageService;
        private readonly IBlogService _blogService;


        public BlogPostsController(ApplicationDbContext context, IImageService imageService, IBlogService blogService)
        {
            _context = context;
            _imageService = imageService;
            _blogService = blogService;


        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AuthorArea(int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            // Obtén todos los blogs sin aplicar un filtro por estado de publicación
            var allBlogPosts = await _blogService.GetAllBlogPostAsync();
            var orderedBlogPosts = allBlogPosts.OrderByDescending(b => b.Created); // Ordenar por fecha de creación

            IPagedList<BlogPost> blogPosts = await orderedBlogPosts.ToPagedListAsync(page, pageSize);

            return View(blogPosts);
        }



        // GET: BlogPosts
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            // Recupera los Blog Posts ordenados por fecha de creación descendente (los más recientes primero)
            var blogPosts = await _blogService.GetBlogPostAsync();

            // Aplica un filtro para excluir los blogs eliminados
            blogPosts = blogPosts.Where(post => !post.IsDeleted);

            blogPosts = blogPosts.OrderByDescending(post => post.Created);

            // Pagina los resultados
            IPagedList<BlogPost> pagedBlogPosts = blogPosts.ToPagedList(page, pageSize);

            ViewData["ActionName"] = nameof(Index);

            return View(pagedBlogPosts);
        }


        



        public async Task<IActionResult> SearchIndex(string? searchString, int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            IPagedList<BlogPost> blogPosts = await _blogService.SearchBlogPosts(searchString).ToPagedListAsync(page, pageSize);


            ViewData["ActionName"] = nameof(SearchIndex);
            ViewData["SearchString"] = searchString;

            return View(nameof(Index), blogPosts);
        }

        public async Task<IActionResult> Popular(int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            IPagedList<BlogPost> blogPosts = await (await _blogService.GetPopularBlogPostAsync()).ToPagedListAsync(page, pageSize);


            ViewData["ActionName"] = nameof(Popular);


            return View(nameof(Index), blogPosts);
        }

        public async Task<IActionResult> Category(int? pageNum, string categoryName)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            IPagedList<BlogPost> blogPosts = await (await _blogService.GetBlogPostByCategoryAsync(categoryName))
                .ToPagedListAsync(page, pageSize);

            ViewData["CategoryName"] = categoryName;

            return View("Index", blogPosts);
        }

        public async Task<IActionResult> Favorites(int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            IEnumerable<BlogPost> likedPosts = await _blogService.GetFavoriteBlogPostsAsync(userId);
            IPagedList<BlogPost> blogPosts = await likedPosts.ToPagedListAsync(page, pageSize);
            ViewData["ActionName"] = nameof(Favorites);
            return View(blogPosts);
        }


        // GET: BlogPosts/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(string? slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            BlogPost? blogPost = await _context.BlogPosts.Include(l => l.Likes)
                                                         .FirstOrDefaultAsync(bp => bp.Slug == slug);

            //BlogPost? blogPost = await _blogService.GetBlogPostAsync(slug);



            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // GET: BlogPosts/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            BlogPost blogPost = new BlogPost();
            blogPost.ImageData = null; // Inicializa la imagen como nula
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name"); // Aquí lo tienes
            return View(blogPost);
        }


        // POST: BlogPosts/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Abstract,Content,IsPublished,CategoryId, ImageFile")] BlogPost blogPost, string? stringTags)
        {
            ModelState.Remove("Slug");

            if (ModelState.IsValid)
            {
                // Static class StringHelper
                string? newSlug = StringHelper.BlogPostSlug(blogPost.Title);

                if (!await _blogService.ValidSlugAsync(newSlug, blogPost.Id))
                {
                    // Handle invalid slug
                    ModelState.AddModelError("Title", "A similar Title/Slug is already in use.");

                    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
                    return View(blogPost);
                }

                // Handle valid slug
                blogPost.Slug = newSlug;
                blogPost.Created = DateTime.Now;


                //Set the Image data if one has been chosen
                if (blogPost.ImageFile != null)
                {
                    blogPost.ImageData = await _imageService.ConvertFileToByteArrayAsync(blogPost.ImageFile);
                    blogPost.ImageType = blogPost.ImageFile.ContentType;
                }

                // Use the blog service
                await _blogService.AddBlogPostAsync(blogPost);

                if (string.IsNullOrEmpty(stringTags) == false)
                {
                    IEnumerable<string> tags = stringTags.Split(',');
                    await _blogService.AddTagsToBlogPostAsync(tags, blogPost.Id);
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", "Name", blogPost.CategoryId);
            return View(blogPost);
        }

        // GET: BlogPosts/Edit/5
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BlogPosts == null)
            {
                return NotFound();
            }

            BlogPost? blogPost = await _context.BlogPosts.Include(bp => bp.Category)
                                                         .FirstOrDefaultAsync(bp => bp.Id == id);

            if (blogPost == null)
            {

                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", "Name", blogPost.CategoryId);
            return View(blogPost);
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Abstract,Content,Created,Slug,IsPublished,IsDeleted,ImageFile,ImageData,ImageType,CategoryId")] BlogPost blogPost)
        {
            if (id != blogPost.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    blogPost.Created = DateTime.Now;

                    //Set the Image data if one has been chosen
                    if (blogPost.ImageFile != null)
                    {

                        blogPost.ImageData = await _imageService.ConvertFileToByteArrayAsync(blogPost.ImageFile);

                        blogPost.ImageType = blogPost.ImageFile.ContentType;
                    }

                    // Use the blog service
                    await _blogService.UpdateBlogPostAsync(blogPost);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogPostExists(blogPost.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
            return View(blogPost);
        }


        // GET: BlogPosts/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {

            if (id == null || id == 0)
            {
                return NotFound();
            }

            BlogPost? blogPost = await _blogService.GetBlogPostAsync(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            blogPost.IsDeleted = true;

            await _blogService.UpdateBlogPostAsync(blogPost);

            return RedirectToAction(nameof(AuthorArea));
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Undelete(int? id)
        {

            if (id == null || id == 0)
            {
                return NotFound();
            }

            BlogPost? blogPost = await _blogService.GetBlogPostAsync(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            blogPost.IsDeleted = false;

            await _blogService.UpdateBlogPostAsync(blogPost);

            return RedirectToAction(nameof(AuthorArea));
        }

        private bool BlogPostExists(int id)
        {
            return (_context.BlogPosts?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Publish(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            BlogPost? blogPost = await _blogService.GetBlogPostAsync(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            blogPost.IsPublished = true;
            await _blogService.UpdateBlogPostAsync(blogPost);

            return RedirectToAction(nameof(AuthorArea));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unpublish(int? id)
        {

            if (id == null || id == 0)
            {
                return NotFound();
            }

            BlogPost? blogPost = await _blogService.GetBlogPostAsync(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            blogPost.IsPublished = false;
            await _blogService.UpdateBlogPostAsync(blogPost);

            return RedirectToAction(nameof(AuthorArea));
        }



        [HttpPost]
        public async Task<IActionResult> LikeBlogPost(int? blogPostId, string? appUserId)
        {
            if (appUserId == null || blogPostId == null)
            {
                return Json(new { isLiked = false, count = 0 });
            }

            //AppUser? appUser = await _context.Users.Include(u => u.Favorites).FirstOrDefaultAsync(u => u.Id == appUserId);
            //BlogLike? blogLike = appUser?.Favorites.FirstOrDefault(bl => bl.BlogPostId == blogPostId);

            AppUser? appUser = await _context.Users.Include(u => u.BlogLikes).FirstOrDefaultAsync(u => u.Id == appUserId);
            BlogLike? blogLike = appUser?.BlogLikes.FirstOrDefault(bl => bl.BlogPostId == blogPostId);


            if (blogLike == null)
            {
                blogLike = new BlogLike { BlogPostId = blogPostId.Value, AppUserId = appUserId, IsLiked = true };
                _context.BlogLikes.Add(blogLike);
            }
            else
            {
                blogLike.IsLiked = !blogLike.IsLiked;
            }

            await _context.SaveChangesAsync();

            int likeCount = await _context.BlogLikes.CountAsync(bl => bl.BlogPostId == blogPostId && bl.IsLiked);

            return Json(new { isLiked = blogLike.IsLiked, count = likeCount });
        }
    }
}
