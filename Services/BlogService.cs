﻿using DevDiary.Data;
using DevDiary.Models;
using DevDiary.Services.Interfaces;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace DevDiary.Services
{
    public class BlogService : IBlogService
    {
        private readonly ApplicationDbContext _context;
        public BlogService(ApplicationDbContext content)
        {
            _context = content;
        }


        public async Task AddBlogPostAsync(BlogPost? blogPost)
        {
            try
            {
                if (blogPost == null) { return; }

                _context.Add(blogPost);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<BlogPost> GetBlogPostAsync(int? id)
        {
            if (id == null) { return new BlogPost(); }

            try
            {
                BlogPost? blogPost = await _context.BlogPosts
                .Include(b => b.Category)
                .Include(b => b.Comments)
                .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

                return blogPost!;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<BlogPost> GetBlogPostAsync(string? slug)
        {
            if (string.IsNullOrEmpty(slug)) { return new BlogPost(); }

            try
            {
                BlogPost? blogPost = await _context.BlogPosts
                                                   .Include(b => b.Category)
                                                   .Include(b => b.Comments)
                                                        .ThenInclude(c => c.Author)
                                                   .Include(b => b.Tags)
                                                   .FirstOrDefaultAsync(m => m.Slug == slug);

                return blogPost!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<BlogPost>> GetBlogPostAsync()
        {
            try
            {
                IEnumerable<BlogPost> blogPosts = await _context.BlogPosts.Include(b => b.Category)
                                                                          .Include(b => b.Comments)
                                                                          .ThenInclude(c => c.Author)
                                                                          .Include(b => b.Tags)
                                                                          .ToListAsync();
                return blogPosts;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<IEnumerable<BlogPost>> GetAllBlogPostAsync()
        {
            try
            {
                IEnumerable<BlogPost> blogPosts = await _context.BlogPosts.Include(b => b.Category)
                                                                          .Include(b => b.Comments)
                                                                          .ThenInclude(c => c.Author)
                                                                          .Include(b => b.Tags)
                                                                          .OrderByDescending(b => b.Created)
                                                                          .ToListAsync();
                return blogPosts;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task UpdateBlogPostAsync(BlogPost? blogPost)
        {
            if (blogPost == null) { return; }
            try
            {
                _context.Update(blogPost);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            try
            {
                IEnumerable<Category> categories = await _context.Categories.ToListAsync();
                return categories;
            }
            catch (Exception)
            {

                throw;
            }
        }



        public async Task<IEnumerable<BlogPost>> GetPopularBlogPostAsync(int? count)
        {
            try
            {
                IEnumerable<BlogPost> blogPosts = await _context.BlogPosts
                                                                .Where(b => b.IsPublished == true && b.IsDeleted == false)
                                                                .Include(b => b.Category)
                                                                .Include(b => b.Comments)
                                                                    .ThenInclude(c => c.Author)
                                                                .Include(b => b.Tags)
                                                                .ToListAsync();
                if (count == null)
                {
                    blogPosts = blogPosts.OrderByDescending(b => b.Comments.Count);
                }
                else
                {
                    blogPosts = blogPosts.OrderByDescending(b => b.Comments.Count).Take(count!.Value);
                }

                return blogPosts;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<IEnumerable<Tag>> GetTagsAsync()
        {
            try
            {
                IEnumerable<Tag> tags = await _context.Tags.ToListAsync();
                return tags;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task AddTagsToBlogPostAsync(IEnumerable<string>? tags, int? blogPostId)
        {
            if (blogPostId == null || tags == null) { return; }

            try
            {
                BlogPost? blogPost = await _context.BlogPosts.Include(b => b.Tags).FirstOrDefaultAsync(b => b.Id == blogPostId);
                if (blogPost == null) { return; }

                foreach (string tagName in tags)
                {

                    if (string.IsNullOrEmpty(tagName.Trim())) continue;

                    Tag? tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name!.Trim().ToLower() == tagName.Trim().ToLower());

                    // if not found
                    if (tag == null)
                    {
                        tag = new Tag() { Name = tagName.Trim().Titleize() }; // this is a tag => This is a Tag
                        await _context.AddAsync(tag);
                    }

                    blogPost.Tags.Add(tag);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }



        public async Task<bool> IsTagOnBlogPostAsync(int? tagId, int? blogPostId)
        {
            if (tagId == null || blogPostId == null) { return false; }
            try
            {
                Tag? tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == tagId);
                BlogPost? blogPost = await _context.BlogPosts
                                                   .Include(b => b.Tags)
                                                   .FirstOrDefaultAsync(b => b.Id == blogPostId);

                if (blogPost != null && tag != null)
                {
                    return blogPost.Tags.Contains(tag);
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveAllBlogPostTagsAsync(int? blogPostId)
        {
            try
            {
                BlogPost? blogPost = await _context.BlogPosts.Include(b => b.Tags)
                                                             .FirstOrDefaultAsync(b => b.Id == blogPostId);

                if (blogPost != null)
                {
                    blogPost.Tags.Clear();
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<BlogPost> SearchBlogPosts(string? searchString)
        {
            try
            {
                IEnumerable<BlogPost> blogPosts = new List<BlogPost>();

                if (string.IsNullOrEmpty(searchString))
                {
                    return blogPosts;
                }
                else
                {
                    searchString = searchString.Trim().ToLower();

                    blogPosts = _context.BlogPosts.Where(b => b.Title!.ToLower().Contains(searchString) ||
                                                         b.Abstract!.ToLower().Contains(searchString) ||
                                                         b.Content!.ToLower().Contains(searchString) ||
                                                         b.Category!.Name!.ToLower().Contains(searchString) ||
                                                         b.Comments.Any(c => c.Body!.ToLower().Contains(searchString) ||
                                                                        c.Author!.FirstName!.ToLower().Contains(searchString) ||
                                                                        c.Author!.LastName!.ToLower().Contains(searchString)) ||
                                                         b.Tags.Any(t => t.Name!.ToLower().Contains(searchString)))
                                                   .Include(b => b.Category)
                                                   .Include(b => b.Comments)
                                                         .ThenInclude(c => c.Author)
                                                   .Include(b => b.Tags)
                                                   .Where(b => b.IsPublished == true && b.IsDeleted == false)
                                                   .AsNoTracking()
                                                   .OrderByDescending(b => b.Created)
                                                   .AsEnumerable();

                    return blogPosts;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<bool> ValidSlugAsync(string? title, int? blogPostId)
        {
            try
            {
                // New BlogPost
                if (blogPostId == null || blogPostId == 0)
                {
                    return !await _context.BlogPosts.AnyAsync(b => b.Slug == title);
                }
                else
                {
                    BlogPost? blogPost = await _context.BlogPosts.AsNoTracking().FirstOrDefaultAsync(b => b.Id == blogPostId);

                    string? oldSlug = blogPost?.Slug;

                    if (!string.Equals(oldSlug, title))
                    {
                        return !await _context.BlogPosts.AnyAsync(b => b.Id != blogPost!.Id && b.Slug == title);
                    }
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<IEnumerable<BlogPost>> GetBlogPostByCategoryAsync(string? categoryName)
        {
            try
            {
                IEnumerable<BlogPost> blogPosts = await _context.BlogPosts.ToListAsync();

                if (!string.IsNullOrEmpty(categoryName))
                {
                    blogPosts = blogPosts.Where(b => b.Category != null && b.Category.Name == categoryName).ToList();
                }
                else
                {
                    blogPosts = Enumerable.Empty<BlogPost>();
                }

                return blogPosts;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<bool> UserLikedBlogAsync(int blogPostId, string appUserId)
        {
            try
            {
                return await _context.BlogLikes.AnyAsync(bl => bl.BlogPostId == blogPostId && bl.IsLiked == true && bl.AppUserId == appUserId);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<BlogPost>> GetFavoriteBlogPostsAsync(string? appUserId)
        {
            try
            {
                List<BlogPost> blogPosts = new();

                if (!string.IsNullOrEmpty(appUserId))
                {
                    AppUser? appUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == appUserId);
                    if (appUser != null)
                    {
                        //List<int> blogPostIds = _context.BlogLikes.Where(bl => bl.BlogUserId == blogUserId && bl.IsLiked == true).Select(b => b.BlogPostId).ToList();
                        blogPosts = await _context.BlogPosts.Where(b => b.Likes.Any(l => l.AppUserId == appUserId && l.IsLiked == true) &&
                                                                                    b.IsPublished == true &&
                                                                                    b.IsDeleted == false)
                                                            .Include(b => b.Likes)
                                                            .Include(b => b.Comments)
                                                            .Include(b => b.Category)
                                                            .Include(b => b.Tags)
                                                            .OrderByDescending(b => b.Created)
                                                            .ToListAsync();
                    }
                }
                return blogPosts;
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
