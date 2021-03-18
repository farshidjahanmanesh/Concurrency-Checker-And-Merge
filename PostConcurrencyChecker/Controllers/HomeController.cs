using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PostConcurrencyChecker.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PostConcurrencyChecker.Controllers
{
    public class HomeController : Controller
    {
        private readonly PostContext postContext;

        public HomeController(PostContext postContext)
        {
            this.postContext = postContext;
        }

        public IActionResult Index()
        {
            var posts = postContext.Posts.ToList();
            return View(posts);
        }

        public IActionResult CreatePost()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreatePost(Post post)
        {
            postContext.Posts.Add(post);
            postContext.SaveChanges();
            return RedirectToAction("index");
        }

        public IActionResult Edit(int id)
        {
            var post = postContext.Posts.Find(id);
            return View(post);
        }
        [HttpPost]
        public IActionResult Edit(Post post,string TimeStamps)
        {
            var oldPost = postContext.Posts.Find(post.Id);
            oldPost.Title = post.Title;
            oldPost.Description = post.Description;
            Thread.Sleep(7000);
            try
            {
                postContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                string mes = ex.Message;
                var en=ex.Entries[0];
                
                var n = (Post)en.CurrentValues.ToObject();
                en.Reload();
                var old = (Post)en.OriginalValues.ToObject();
                return View("ChangeMerge", new Tuple<Post,Post>(old, n ));
            }
           
            return RedirectToAction("index");
        }

        public IActionResult SaveMerge(Post post,int version)
        {
            if (version == 2)
            {
                var orgPost = postContext.Posts.Find(post.Id);
                orgPost.Description = post.Description;
                orgPost.Title = post.Title;
                postContext.SaveChanges();
            }
            return RedirectToAction("index");
        }
    }
}
