using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevDiary.Data;
using DevDiary.Models;
using DevDiary.Services.Interfaces;
using DevDiary.Services;

namespace DevDiary.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostsController : ControllerBase
    {
        
        private readonly IBlogService _blogService;


        public BlogPostsController(ApplicationDbContext context, IBlogService blogService)
        {        
            _blogService = blogService;
        }


        /// <summary>
        /// This endpoint will return the most recently published blog posts.
        /// The count parameter indicates the number of recent posts to return.
        /// The maximun number of blog posts allowed per-request is 10.
        /// </summary>
        /// <param name="count">The number of blog posts to retrieve</param>
        /// <returns></returns>
        /// 


        [HttpGet("{count:int}")]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPosts(int count)
        {

            if (count > 10)
            {
                count = 10;
            }

            IEnumerable<BlogPost> blogPosts = (await _blogService.GetBlogPostAsync()).Take(count);

            return Ok(blogPosts);
        }

    }
}
