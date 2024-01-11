using DevDiary.Models;

namespace DevDiary.Services.Interfaces
{
    public interface IBlogService
    {
        public Task AddBlogPostAsync(BlogPost? blogPost);
        public Task<BlogPost> GetBlogPostAsync(int? id);
        public Task<BlogPost> GetBlogPostAsync(string? slug);
        public Task<IEnumerable<BlogPost>> GetBlogPostAsync();
        public Task<IEnumerable<BlogPost>> GetAllBlogPostAsync();
        public Task UpdateBlogPostAsync(BlogPost? blogPost);


        // TODO
        public Task<IEnumerable<Category>> GetCategoriesAsync();
        public Task<IEnumerable<BlogPost>> GetPopularBlogPostAsync(int? count = null);
        public Task<IEnumerable<Tag>> GetTagsAsync();
        public Task AddTagsToBlogPostAsync(IEnumerable<string>? tags, int? blogPostId);
        public Task<bool> IsTagOnBlogPostAsync(int? tagId, int? blogPostId);
        public Task RemoveAllBlogPostTagsAsync(int? blogPostId);
        public IEnumerable<BlogPost> SearchBlogPosts(string? searchString);
        public Task<bool> ValidSlugAsync(string? title, int? blogPostId);

        public Task<IEnumerable<BlogPost>> GetBlogPostByCategoryAsync(string? categoryName);

        // This is a simple method to check whether the current authenticated
        // user has previously "Liked" the blog post that they are viewing
        public Task<bool> UserLikedBlogAsync(int blogPostId, string appUserId);

        public Task<IEnumerable<BlogPost>> GetFavoriteBlogPostsAsync(string? appUserId);

       
    }
}
