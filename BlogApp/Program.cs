using BlogApp.Data.Abstract;
using BlogApp.Data.Concrete.EfCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<BlogContext>(); //BlogContext nesnesi üretiliyor. Ve bu üretilen nesneyi (= dependency) EfPostRepository class'ý içerisinde, EfPostRepository class'ýndan nesne üretildiðinde constructor'dan alýp class içindeki BlogContext tipli private field'a atadýk. Bu iþleme Dependency Injection deniyor.
builder.Services.AddScoped<IPostRepository, EfPostRepository>(); //EfPostRepository'den nesne üretilip IPostRepository'e atanýyor. Bunu da PostsController içerisinde IPostRepository dependency'sini inject edip Controller içindeki _repository private field'ýna atadým.
builder.Services.AddScoped<ITagRepository, EfTagRepository>();
builder.Services.AddScoped<ICommentRepository, EfCommentRepository>();
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options=>
{
    options.LoginPath = "/Users/Login"; //Login olmamýþ kullanýcý [Authorize] attribute'ünden dolayý default olarak /Account/Login 'e yönlendirilir ancak LoginPath ile bu default yönlendirmeyi /Users/Login ile deðiþtirdik.
}); //cookie için ekledik.
var app = builder.Build();

SeedData.TestVerileriniDoldur(app);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRouting(); //UseAuthentication ve UseAuthorization middleware'larýndan önce UseRouting in eklenmesi lazým. Sýra önemli.
app.UseAuthentication(); //Authentication middleware'ini aktif ettik.
app.UseAuthorization();//yetkilendirme. Yani uygulamanýn belli bölümlerini kullanabilmemizi saðlayacak.
app.UseHttpsRedirection();
app.UseStaticFiles();

//https://localhost:7227/posts/details/react-dersleri
//https://localhost:7227/posts/details/php-dersleri
app.MapControllerRoute(
    name: "post_details", //route name (kendimiz verdik)
    pattern: "posts/details/{url}",
    defaults: new { controller = "Posts", action = "Details" }
    );

app.MapControllerRoute(
    name: "posts_by_tag",
    pattern: "posts/tag/{tag}",
    defaults: new { controller = "Posts", action = "Index" }
    );

app.MapControllerRoute(
    name: "user_profile",
    pattern: "profile/{username}",
    defaults: new { controller = "Users", action = "Profile" }
    );

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Posts}/{action=Index}/{id?}");

app.Run();
