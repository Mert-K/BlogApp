using BlogApp.Data.Abstract;
using BlogApp.Data.Concrete.EfCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<BlogContext>(); //BlogContext nesnesi �retiliyor. Ve bu �retilen nesneyi (= dependency) EfPostRepository class'� i�erisinde, EfPostRepository class'�ndan nesne �retildi�inde constructor'dan al�p class i�indeki BlogContext tipli private field'a atad�k. Bu i�leme Dependency Injection deniyor.
builder.Services.AddScoped<IPostRepository, EfPostRepository>(); //EfPostRepository'den nesne �retilip IPostRepository'e atan�yor. Bunu da PostsController i�erisinde IPostRepository dependency'sini inject edip Controller i�indeki _repository private field'�na atad�m.
builder.Services.AddScoped<ITagRepository, EfTagRepository>();
builder.Services.AddScoped<ICommentRepository, EfCommentRepository>();
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options=>
{
    options.LoginPath = "/Users/Login"; //Login olmam�� kullan�c� [Authorize] attribute'�nden dolay� default olarak /Account/Login 'e y�nlendirilir ancak LoginPath ile bu default y�nlendirmeyi /Users/Login ile de�i�tirdik.
}); //cookie i�in ekledik.
var app = builder.Build();

SeedData.TestVerileriniDoldur(app);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRouting(); //UseAuthentication ve UseAuthorization middleware'lar�ndan �nce UseRouting in eklenmesi laz�m. S�ra �nemli.
app.UseAuthentication(); //Authentication middleware'ini aktif ettik.
app.UseAuthorization();//yetkilendirme. Yani uygulaman�n belli b�l�mlerini kullanabilmemizi sa�layacak.
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
