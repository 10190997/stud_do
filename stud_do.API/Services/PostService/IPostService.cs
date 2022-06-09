namespace stud_do.API.Services.PostService
{
    public interface IPostService
    {
        /// <summary>
        /// Получить запись
        /// </summary>
        /// <param name="postId">Id записи</param>
        Task<ServiceResponse<PostOutput>> GetPostAsync(int postId);

        /// <summary>
        /// Получить все записи в комнате
        /// </summary>
        /// <param name="roomId">Id комнаты</param>
        Task<ServiceResponse<List<PostOutput>>> GetPostsAsync(int roomId);

        /// <summary>
        /// Удалить запись
        /// </summary>
        /// <param name="postId">Id записи</param>
        Task<ServiceResponse<List<PostOutput>>> DeletePostAsync(int postId);

        /// <summary>
        /// Создать запись и добавить ее в комнату
        /// </summary>
        /// <param name="post">Запись</param>
        /// <param name="roomId">Id комнаты</param>
        Task<ServiceResponse<List<PostOutput>>> AddPostAsync(PostInput post, int roomId);

        /// <summary>
        /// Редактировать запись
        /// </summary>
        /// <param name="post">Запись</param>
        /// <param name="postId">Id записи</param>
        Task<ServiceResponse<PostOutput>> UpdatePostAsync(PostInput post, int postId);
    }
}