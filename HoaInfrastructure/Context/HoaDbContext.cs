using System;
using Microsoft.EntityFrameworkCore;
using HoaEntities.Entities;

namespace HoaInfrastructure.Context
{
    public class HoaDbContext : DbContext
    {
        public HoaDbContext() { }
        public HoaDbContext(DbContextOptions<HoaDbContext> options)
            : base(options) { }

        public DbSet<Event> Events { get; set; }
        public DbSet<MeetingMinute> MeetingMinutes { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<BoardMeeting> BoardMeetings { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                        .HasMany(s => s.BoardMeetings)
                        .WithOne(g => g.CreatedBy)
                        .HasForeignKey(s => s.UserId);
            modelBuilder.Entity<User>()
                        .HasMany(s => s.Comments)
                        .WithOne(g => g.CreatedBy)
                        .HasForeignKey(s => s.UserId)
                        .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                        .HasMany(s => s.Posts)
                        .WithOne(g => g.CreatedBy)
                        .HasForeignKey(s => s.UserId);
            modelBuilder.Entity<User>()
                        .HasMany(s => s.Events)
                        .WithOne(g => g.CreatedBy)
                        .HasForeignKey(s => s.UserId);
            modelBuilder.Entity<User>()
                        .HasMany(s => s.MeetingMinutes)
                        .WithOne(g => g.CreatedBy)
                        .HasForeignKey(s => s.UserId)
                        .OnDelete(DeleteBehavior.Restrict);

            Guid userGuid = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9");
            Guid userGuid2 = new Guid("5f76bd53-b065-487a-89ca-c9ec6a9b60c9");
            Guid userGuid3 = new Guid("5f76bd54-b063-487a-89ca-c9ec6a9b6099");

            Guid eventid1 = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619");
            Guid eventid2 = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618");

            Guid postid1 = new Guid("e77551ba-78e3-4a36-8754-3ea5f12e1619");
            Guid postid2 = new Guid("e77551ba-78e4-4a36-8754-3ea5f12e1618");

            Guid meetingminuteid1 = new Guid("e77551ba-78e3-4a36-9754-3ea5f12e1619");
            Guid meetingminuteid2 = new Guid("e77551ba-78e4-4a36-7754-3ea5f12e1618");

            Guid commentsid1 = new Guid("e77551ba-78e3-4a36-9754-4ea5f12e1619");
            Guid commentsid2 = new Guid("e77551ba-78e4-4a36-7754-5ea5f12e1618");

            modelBuilder.Entity<User>()
                .HasData(new
                {
                    Id = userGuid,
                    SocialId = "12345",
                    FirstName = "VN",
                    LastName = "R",
                    Reminder = false,
                    Email = "abc@123.com",
                    Joined = DateTime.Now,
                    Active = true
                });
            modelBuilder.Entity<User>()
               .HasData(new
               {
                   Id = userGuid2,
                   SocialId = "123",
                   FirstName = "Meera",
                   LastName = "Lao",
                   Reminder = false,
                   Email = "xyz@123.com",
                   Joined = DateTime.Now,
                   Active = true
               });
            modelBuilder.Entity<User>()
               .HasData(new
               {
                   Id = userGuid3,
                   SocialId = "1234567",
                   FirstName = "A",
                   LastName = "B",
                   Reminder = true,
                   Email = "ctt@123.com",
                   Joined = DateTime.Now,
                   Active = true
               });

            modelBuilder.Entity<Post>()
                  .HasData(new
                  {
                      Id = postid1,
                      Title = "Test1",
                      Message = "Test1",
                      Created = DateTime.Now - TimeSpan.FromDays(3),
                      Important = true,
                      UserId = userGuid
                  },
                  new
                  {
                      Id = postid2,
                      Title = "Test2",
                      Message = "Test2",
                      Created = DateTime.Now,
                      Important = false,
                      UserId = userGuid2
                  },
                   new
                   {
                       Id = Guid.NewGuid(),
                       Title = "Test3",
                       Message = "Test3",
                       Created = DateTime.Now,
                       Important = false,
                       UserId = userGuid2
                   },
                    new
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test4",
                        Message = "Test4",
                        Created = DateTime.Now,
                        Important = true,
                        UserId = userGuid
                    },
                     new
                     {
                         Id = Guid.NewGuid(),
                         Title = "Test5",
                         Message = "Test5",
                         Created = DateTime.Now,
                         Important = true,
                         UserId = userGuid
                     }

                   );

            modelBuilder.Entity<Comment>()
                  .HasData(new
                  {
                      Id = commentsid1,
                      Message = "Test1",
                      Created = DateTime.Now - TimeSpan.FromDays(5),
                      UserId = userGuid2,
                      PostId = postid1
                  },
                  new
                  {
                      Id = commentsid2,
                      Message = "Test2",
                      Created = DateTime.Now - TimeSpan.FromDays(2),
                      ParentCommentId = commentsid1,
                      UserId = userGuid2,
                      PostId = postid1
                  },
                   new
                   {
                       Id = Guid.NewGuid(),
                       Message = "Test3",
                       Created = DateTime.Now - TimeSpan.FromDays(3),
                       ParentCommentId = commentsid1,
                       UserId = userGuid,
                       PostId = postid1
                   },
                    new
                    {
                        Id = Guid.NewGuid(),
                        Message = "Test4",
                        Created = DateTime.Now - TimeSpan.FromDays(1),
                        ParentCommentId = commentsid2,
                        UserId = userGuid,
                        PostId = postid1
                    },
                     new
                     {
                         Id = Guid.NewGuid(),
                         Message = "Test5",
                         Created = DateTime.Now - TimeSpan.FromDays(1),
                         UserId = userGuid,
                         PostId = postid1
                     },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Message = "Test6",
                          Created = DateTime.Now - TimeSpan.FromDays(1),
                          UserId = userGuid,
                          PostId = postid1
                      },
                       new
                       {
                           Id = Guid.NewGuid(),
                           Message = "Test7",
                           Created = DateTime.Now - TimeSpan.FromDays(2),
                           UserId = userGuid,
                           PostId = postid2
                       }
                  );

            modelBuilder.Entity<Event>()
                  .HasData(new
                  {
                      Id = eventid1,
                      Title = "Test1",
                      Message = "Test1Test1Test1Te st1Tes t1Test1T est1Test 1Test1T st1Test1Te st1Test1Tes t1Test1Test1Test 1Test1Test1" +
                      "Test1Test1Test1Test1T est1Test1Test1Test1Test1Test 1Test1Te st1T est1Test1Test1Test1Te st1Test1Tes t1Test1Test1Te st1Test1Test1" +
                      "Test1Test1Test1Test 1Test1Test1Test 1Test1Test1Te t1Test1Test1Test1 Test1Test1Te st1Tes t1Test 1Test1Test1 Test1Test1 Test1Test1" +
                      "Test1Test1Test1Te st1Test1Te st1Test1Test1 Test1Test1Test1T est1Test1Test1Test1Test1Test1Test1" +
                      "Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1 Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1 Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1" +
                      "Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1 Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1 Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1" +
                      "Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1 Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1 Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1 Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1",
                      ImageName = "Event_Original_01-30-2019_11-41-16.jpg",
                      ThumbnailName = "Event_Thumbnail_01-30-2019_11-41-16.jpg",
                      Created = DateTime.Now - TimeSpan.FromDays(3),
                      ScheduledTime = DateTime.Now.AddDays(1),
                      UserId = userGuid2
                  },
                  new
                  {
                      Id = eventid2,
                      Title = "Test2",
                      Message = "Test2",
                      ImageName = "Event_Original_01-30-2019_11-41-16.jpg",
                      ThumbnailName = "Event_Thumbnail_01-30-2019_11-41-16.jpg",
                      Created = DateTime.Now,
                      ScheduledTime = DateTime.Now.AddDays(2),
                      UserId = userGuid
                  },
                   new
                   {
                       Id = Guid.NewGuid(),
                       Title = "Test3",
                       Message = "Test3",
                       ImageName = "Event_Original_01-30-2019_11-41-16.jpg",
                       ThumbnailName = "Event_Thumbnail_01-30-2019_11-41-16.jpg",
                       Created = DateTime.Now,
                       ScheduledTime = DateTime.Now.AddDays(3),
                       UserId = userGuid
                   },
                    new
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test4",
                        Message = "Test4",
                        ImageName = "Event_Original_01-30-2019_11-41-16.jpg",
                        ThumbnailName = "Event_Thumbnail_01-30-2019_11-41-16.jpg",
                        Created = DateTime.Now,
                        ScheduledTime = DateTime.Now.AddDays(4),
                        UserId = userGuid
                    },
                     new
                     {
                         Id = Guid.NewGuid(),
                         Title = "Test5",
                         Message = "Test5",
                         ImageName = "Event_Original_01-30-2019_11-41-16.jpg",
                         ThumbnailName = "Event_Thumbnail_01-30-2019_11-41-16.jpg",
                         Created = DateTime.Now,
                         ScheduledTime = DateTime.Now.AddDays(5),
                         UserId = userGuid
                     },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Test6",
                          Message = "Test6",
                          ImageName = "Event_Original_01-30-2019_11-41-16.jpg",
                          ThumbnailName = "Event_Thumbnail_01-30-2019_11-41-16.jpg",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(6),
                          UserId = userGuid
                      },
                       new
                       {
                           Id = Guid.NewGuid(),
                           Title = "Test7",
                           Message = "Test7",
                           ImageName = "Event_Original_01-30-2019_11-41-16.jpg",
                           ThumbnailName = "Event_Thumbnail_01-30-2019_11-41-16.jpg",
                           Created = DateTime.Now,
                           ScheduledTime = DateTime.Now.AddDays(7),
                           UserId = userGuid
                       }
                  );

            modelBuilder.Entity<MeetingMinute>()
                  .HasData(new
                  {
                      Id = meetingminuteid1,
                      FileName = "test1.txt",
                      Created = DateTime.Now - TimeSpan.FromDays(3),
                      UserId = userGuid2,
                      BoardMeetingId = eventid1
                  },
                  new
                  {
                      Id = meetingminuteid2,
                      FileName = "test2.txt",
                      Created = DateTime.Now,
                      UserId = userGuid2,
                      BoardMeetingId = eventid2
                  }
                   );

            modelBuilder.Entity<BoardMeeting>()
                 .HasData(new
                 {
                     Id = eventid1,
                     Title = "Test1",
                     Description = "Test1",
                     Created = DateTime.Now - TimeSpan.FromDays(3),
                     ScheduledTime = DateTime.Now.AddDays(1),
                     UserId = userGuid2,
                     ScheduledLocation = "a"
                 },
                 new
                 {
                     Id = eventid2,
                     Title = "Test2",
                     Description = "Test2",
                     Created = DateTime.Now,
                     ScheduledTime = DateTime.Now.AddDays(2),
                     UserId = userGuid3,
                     ScheduledLocation = "a"
                 },
                  new
                  {
                      Id = Guid.NewGuid(),
                      Title = "Test3",
                      Description = "Test3",
                      Created = DateTime.Now,
                      ScheduledTime = DateTime.Now.AddDays(3),
                      UserId = userGuid3,
                      ScheduledLocation = "a"
                  },
                   new
                   {
                       Id = Guid.NewGuid(),
                       Title = "Test4",
                       Description = "Test4",
                       Created = DateTime.Now,
                       ScheduledTime = DateTime.Now.AddDays(4),
                       UserId = userGuid,
                       ScheduledLocation = "a"
                   },
                    new
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test5",
                        Description = "Test5",
                        Created = DateTime.Now,
                        ScheduledTime = DateTime.Now.AddDays(5),
                        UserId = userGuid,
                        ScheduledLocation = "a"
                    },
                     new
                     {
                         Id = Guid.NewGuid(),
                         Title = "Test6",
                         Description = "Test6",
                         Created = DateTime.Now,
                         ScheduledTime = DateTime.Now.AddDays(6),
                         UserId = userGuid,
                         ScheduledLocation = "a"
                     },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Test7",
                          Description = "Test7",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Test8",
                          Description = "Test8",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Test9",
                          Description = "Test9",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Test10",
                          Description = "Test10",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Test11",
                          Description = "Test11",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "asfasf",
                          Description = "Tesasfasft7",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "afaf",
                          Description = "Teasfasgst7",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "TsdhSFhest7",
                          Description = "Tesasgasgt7",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Test7asgasg",
                          Description = "Test7asgasgasgasg",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "TestsdghHR7",
                          Description = "TesFAHADFHDHt7",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes4634636agasgt7",
                          Description = "Te346364st7",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Test26236asg267",
                          Description = "Tes2525235t7",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Te235asgasgdg235st7",
                          Description = "Tes235235t7",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes23523asgdshgfh5t7",
                          Description = "Tes235235t7",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Te235235st7",
                          Description = "Test7235235",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Testsdghddgsdg2352357",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235sdgsdhsdh235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes23dgsdgsdggd5235t7",
                          Description = "Tes235235t7",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes25sdgsdgsdg235235235t7",
                          Description = "Tes25235t7",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes2352sdgsdg35235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235sdg235235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235235235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235235235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235235235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235235235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235235235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235235235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235235235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235235235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235235235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235235235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235235235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235235235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235235235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235235235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      },
                      new
                      {
                          Id = Guid.NewGuid(),
                          Title = "Tes235235gasgasg235t7",
                          Description = "Test2352357",
                          Created = DateTime.Now,
                          ScheduledTime = DateTime.Now.AddDays(7),
                          UserId = userGuid,
                          ScheduledLocation = "a"
                      }
                 );
        }
    }
}