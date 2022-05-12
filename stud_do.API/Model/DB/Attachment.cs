namespace stud_do.API.Model
{
    /// <summary>
    /// Вложение
    /// </summary>
    public class Attachment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }

        /// <summary>
        /// Ссылка на вложение
        /// </summary>
        public string Link { get; set; }
    }
}