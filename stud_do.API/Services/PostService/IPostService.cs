namespace stud_do.API.Services.PostService
{
    public interface IPostService
    {
        Task<ServiceResponse<PostOutput>> GetPostAsync(int postId);

        Task<ServiceResponse<List<PostOutput>>> GetPostsAsync(int roomId);

        Task<ServiceResponse<List<PostOutput>>> DeletePostAsync(int postId);

        Task<ServiceResponse<List<PostOutput>>> AddPostAsync(PostInput post);

        Task<ServiceResponse<PostOutput>> UpdatePostAsync(Post post);
    }
}