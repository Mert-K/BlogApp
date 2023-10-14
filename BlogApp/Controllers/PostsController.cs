using BlogApp.Data.Abstract;
using BlogApp.Data.Concrete.EfCore;
using BlogApp.Entity;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using System.Reflection;
using System.Security.Claims;

namespace BlogApp.Controllers
{
    public class PostsController : Controller
    {
        private IPostRepository _postRepository;
        private ICommentRepository _commentRepository;
        private ITagRepository _tagRepository;

        public PostsController(IPostRepository postRepository, ICommentRepository commentRepository,ITagRepository tagRepository) //servisde IPostRepository repository = new EfPostRepository(); şeklinde EfPostRepository nesnesi üretilip , interface tipine atanıyor. Aşağıdaki Index metodunda _postRepository'nin karşılığı aslında EfPostRepository nesnesi. _postRepository.Post yaparak interface'in Posts property'si çağırılıyor olsa da aslında bu interface'i implemente eden EfPostRepository class'ının Posts property'si çalıştırılır.
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _tagRepository = tagRepository;
        }

        public async Task<IActionResult> Index(string tag)
        {
            var posts = _postRepository.Posts.Where(i=>i.IsActive == true);
            if (!string.IsNullOrEmpty(tag))
            {
                posts = posts.Where(x => x.Tags.Any(t => t.Url == tag));
            }
            return View(new PostsViewModel
            {
                Posts = await posts.ToListAsync()
            });
        }

        public async Task<IActionResult> Details(string url)
        {
            return View(await _postRepository
                .Posts
                .Include(x=>x.User)
                .Include(x => x.Tags)
                .Include(x => x.Comments)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Url == url));
        }

        [HttpPost]
        public JsonResult AddComment(int PostId, string Text)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //userClaims.Add(new Claim(ClaimTypes.NameIdentifier, isUser.UserId.ToString()));
                                                                         //Users Controller Login Post metodunda claim type olarak ClaimTypes.NameIdentifier'e value olarak kullanıcının UserId'sini verdik. Yani burada User.FindFirstValue(ClaimTypes.NameIdentifier); çağırarak user'ın Id'sini almak istiyoruz.
            var username = User.FindFirstValue(ClaimTypes.Name);
            var avatar = User.FindFirstValue(ClaimTypes.UserData);

            var entity = new Comment
            {
                Text = Text,
                PublishedOn = DateTime.Now,
                PostId = PostId,
                UserId = int.Parse(userId ?? "")
            };
            _commentRepository.CreateComment(entity);

            return Json(new
            {
                username,
                Text,
                entity.PublishedOn,
                avatar
            });

        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create(PostCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                _postRepository.CreatePost(
                    new Post
                    {
                        Title = model.Title,
                        Description = model.Description,
                        Content = model.Content,
                        Url = model.Url,
                        UserId = int.Parse(userId ?? ""),
                        PublishedOn = DateTime.Now,
                        Image = "1.jpg",
                        IsActive = false
                    }
                );
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> List()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? ""); //UserController Login Post Action'a bakılabilir.
            var role = User.FindFirstValue(ClaimTypes.Role); //UserController Login Post Action'a bakılabilir. Eğer ki email'i info@sadikturan.com değilse Role Claim tipi atanmayacaktır Login olunca. Eğer ki email'i info@sadikturan.com ise Role Claim tipi atanacaktır ve bu Role Claim tipinin value'sunu admin olarak verdik.

            var posts = _postRepository.Posts; //IQueryable<T> olunca veritabanına bir sorgu gitmez.

            if (string.IsNullOrEmpty(role))
            {
                posts = posts.Where(x => x.UserId == userId);
            }
            return View(await posts.ToListAsync());
        }

        [HttpGet]
        [Authorize]
        public IActionResult Edit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            Post? post = _postRepository.Posts.Include(x=>x.Tags).FirstOrDefault(x=>x.PostId==id);
            if(post==null)
            {
                return NotFound();
            }

            ViewBag.Tags = _tagRepository.Tags.ToList();

            return View(new PostCreateViewModel
            {
                PostId = post.PostId,
                Title = post.Title,
                Description = post.Description,
                Content = post.Content,
                Url = post.Url,
                IsActive=post.IsActive,
                Tags = post.Tags
            });
        }

        [HttpPost]
        [Authorize]
        public IActionResult Edit(PostCreateViewModel model, int[] tagIds)
        {
            if(ModelState.IsValid)
            {
                var entityToUpdate = new Post()
                {
                    PostId = model.PostId,
                    Title = model.Title,
                    Description = model.Description,
                    Content = model.Content,
                    Url = model.Url
                };

                if(User.FindFirstValue(ClaimTypes.Role) == "admin")
                {
                    entityToUpdate.IsActive = model.IsActive;
                }

                _postRepository.EditPost(entityToUpdate,tagIds);
                return RedirectToAction("List");   
            }
            ViewBag.Tags = _tagRepository.Tags.ToList();
            return View(model);
        }
    }
}
