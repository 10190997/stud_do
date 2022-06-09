namespace stud_do.API.Model
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRoom>().HasKey(p => new { p.RoomId, p.UserId });

            modelBuilder.Entity<UserRoom>()
                .HasOne(pt => pt.User)
                .WithMany(t => t.UsersRooms)
                .HasForeignKey(pt => pt.UserId);

            modelBuilder.Entity<UserRoom>()
                .HasOne(pt => pt.Room)
                .WithMany(t => t.UsersRooms)
                .HasForeignKey(pt => pt.RoomId);

            modelBuilder.Entity<UsersSchedules>().HasKey(p => new { p.ScheduleId, p.UserId });

            modelBuilder.Entity<UsersSchedules>()
                .HasOne(pt => pt.User)
                .WithMany(t => t.Schedules)
                .HasForeignKey(pt => pt.UserId);

            modelBuilder.Entity<UsersSchedules>()
                .HasOne(pt => pt.Schedule)
                .WithMany(t => t.Users)
                .HasForeignKey(pt => pt.ScheduleId);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRoom> UsersRooms { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<UsersSchedules> UsersSchedules { get; set; }
    }
}