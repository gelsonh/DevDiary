using Microsoft.AspNetCore.Mvc;
using DevDiary.Data;
using DevDiary.Models;
using DevDiary.Services.Interfaces;

namespace DevDiary.Controllers.API
{
    [Route("api/[controller]")] // Setting the base route for this controller
    [ApiController] // Indicating tha this is an APi controller
    public class BlogPostsController : ControllerBase
    {
        
        private readonly IBlogService _blogService;

        public BlogPostsController(ApplicationDbContext context, IBlogService blogService)
        {        
            _blogService = blogService; // Initializing the blog service field
        }

        [HttpGet("{count:int}")] // Difining a GET endpoint that takes an integer count as a parameter
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPosts(int count) // Method to get a list of blog posts
        {

            if (count > 10) // If the count exceeds 10
            {
                count = 10; // Limit the count to 10
            }

            IEnumerable<BlogPost> blogPosts = (await _blogService.GetBlogPostAsync()).Take(count); // Retrieve the blog posts and take the specified number 

            return Ok(blogPosts); // Return the blog posts with a 200 OK status
        }

    }
}


