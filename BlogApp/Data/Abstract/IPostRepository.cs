using BlogApp.Entity;

namespace BlogApp.Data.Abstract
{
    public interface IPostRepository
    {
        IQueryable<Post> Posts { get; } //https://dotnettutorials.net/lesson/differences-between-ienumerable-and-iqueryable/

        void CreatePost(Post post);

        void EditPost(Post post);
        void EditPost(Post post, int[] tagIds);
    }
}
