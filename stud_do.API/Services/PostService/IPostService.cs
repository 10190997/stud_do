namespace stud_do.API.Services.PostService
{
    public interface IPostService
    {
        Task<ServiceResponse<List<PostOutput>>> GetPosts(int roomId);

        Task<ServiceResponse<PostOutput>> CreatePost(PostInput post);

        Task<ServiceResponse<PostOutput>> GetPost(int postId);

        Task<ServiceResponse<PostOutput>> UpdatePost(int postId, PostInput post);

        Task<ServiceResponse<List<PostOutput>>> DeletePost(int postId);
    }
}