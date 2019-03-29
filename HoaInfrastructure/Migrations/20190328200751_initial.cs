using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HoaInfrastructure.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SocialId = table.Column<string>(nullable: false),
                    FirstName = table.Column<string>(nullable: false),
                    LastName = table.Column<string>(nullable: false),
                    Reminder = table.Column<bool>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    Joined = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoardMeetings",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(maxLength: 50, nullable: false),
                    Description = table.Column<string>(maxLength: 1000, nullable: false),
                    ScheduledTime = table.Column<DateTime>(nullable: false),
                    ScheduledLocation = table.Column<string>(maxLength: 300, nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardMeetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardMeetings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(maxLength: 50, nullable: false),
                    Message = table.Column<string>(maxLength: 1000, nullable: false),
                    ImageName = table.Column<string>(nullable: true),
                    ThumbnailName = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    ScheduledTime = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(maxLength: 50, nullable: false),
                    Message = table.Column<string>(maxLength: 1000, nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Important = table.Column<bool>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeetingMinutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FileName = table.Column<string>(maxLength: 50, nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    BoardMeetingId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingMinutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingMinutes_BoardMeetings_BoardMeetingId",
                        column: x => x.BoardMeetingId,
                        principalTable: "BoardMeetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetingMinutes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Message = table.Column<string>(maxLength: 1000, nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    ParentCommentId = table.Column<Guid>(nullable: true),
                    PostId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Active", "Email", "FirstName", "Joined", "LastName", "Reminder", "SocialId" },
                values: new object[] { new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9"), true, "abc@123.com", "VN", new DateTime(2019, 3, 28, 13, 7, 50, 467, DateTimeKind.Local).AddTicks(7818), "R", false, "12345" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Active", "Email", "FirstName", "Joined", "LastName", "Reminder", "SocialId" },
                values: new object[] { new Guid("5f76bd53-b065-487a-89ca-c9ec6a9b60c9"), true, "xyz@123.com", "Meera", new DateTime(2019, 3, 28, 13, 7, 50, 480, DateTimeKind.Local).AddTicks(6237), "Lao", false, "123" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Active", "Email", "FirstName", "Joined", "LastName", "Reminder", "SocialId" },
                values: new object[] { new Guid("5f76bd54-b063-487a-89ca-c9ec6a9b6099"), true, "ctt@123.com", "A", new DateTime(2019, 3, 28, 13, 7, 50, 480, DateTimeKind.Local).AddTicks(6515), "B", true, "1234567" });

            migrationBuilder.InsertData(
                table: "BoardMeetings",
                columns: new[] { "Id", "Created", "Description", "ScheduledLocation", "ScheduledTime", "Title", "UserId" },
                values: new object[,]
                {
                    { new Guid("dba13a97-b4b1-47ba-ada2-4fa4be2d116c"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1281), "Test4", "a", new DateTime(2019, 4, 1, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1284), "Test4", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("a993c2a8-a2bc-4333-a130-577f8647b039"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1510), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1513), "Tes235sdg235235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("a90ca1a6-11e5-412f-b2d6-e3b951d4b7e0"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1518), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1522), "Tes235235235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("cb126c20-1f81-41bf-965d-81c046d0a6ee"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1529), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1533), "Tes235235235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1215), "Test2", "a", new DateTime(2019, 3, 30, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1230), "Test2", new Guid("5f76bd54-b063-487a-89ca-c9ec6a9b6099") },
                    { new Guid("75eaa8c2-ab30-4396-b31c-ef258bffde51"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1549), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1552), "Tes235235235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("7c158e13-81b0-4ec8-ab91-774dffcf4695"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1558), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1561), "Tes235235235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("3b6891c0-5391-433d-a47e-6040c8b96645"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1566), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1569), "Tes235235235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("7a550a23-e95c-4bdb-9be7-653c2b8f2d0a"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1573), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1577), "Tes235235235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("c5d8664c-3705-4d58-ae63-a4200306ac89"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1605), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1609), "Tes235235235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("74f3ff3c-5623-4a86-b2f1-eb448ccbc565"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1614), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1617), "Tes235235235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("af1b438d-714b-4aef-8cbf-a41e81fc844f"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1624), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1628), "Tes235235235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("7d437767-98d5-4c0e-8b56-2215e9ecc010"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1633), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1636), "Tes235235235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("2a13c6bb-8a4e-4746-ad7e-3476c98a17f1"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1776), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1783), "Tes235235235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("69faef0d-a88a-4d70-93cd-e65ce180ad82"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1787), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1793), "Tes235235235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("e0a56604-60cb-4a5b-8e45-8f2cf10c4a98"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1801), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1806), "Tes235235235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("fd4445e2-954b-42bc-9aaf-8d4cac99a82e"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1813), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1816), "Tes235235gasgasg235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"), new DateTime(2019, 3, 25, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(9066), "Test1", "a", new DateTime(2019, 3, 29, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(9075), "Test1", new Guid("5f76bd53-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("aaefd224-03fb-4c04-b70e-6028c50e145e"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1502), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1505), "Tes2352sdgsdg35235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("4a12e195-c5c0-4f71-a81e-6016b510baab"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1495), "Tes25235t7", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1498), "Tes25sdgsdgsdg235235235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("5a5f5108-ec4a-487b-bfb2-3520bc518979"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1539), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1542), "Tes235235235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("ebd9b054-bb6b-479f-8d2a-9d3d4721fd3f"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1477), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1479), "Tes235sdgsdhsdh235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("5e2ff88d-eef2-4b01-a513-c5ca2164cbb0"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1303), "Test5", "a", new DateTime(2019, 4, 2, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1308), "Test5", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("04c11869-1f6f-4caf-b50a-6bf76a965338"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1313), "Test6", "a", new DateTime(2019, 4, 3, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1317), "Test6", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("e30b7790-78bd-4a31-9c6d-326b9fee0721"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1485), "Tes235235t7", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1488), "Tes23dgsdgsdggd5235t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("84ab11fc-7b9a-410a-a771-8ced9aa9ad15"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1332), "Test8", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1336), "Test8", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("5a57da2c-2e4d-44ce-82cf-b85c1dd7fbe1"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1343), "Test9", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1347), "Test9", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("c1e73294-9836-4339-8844-7c3ed02353ef"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1352), "Test10", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1356), "Test10", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("10e9997a-5cc9-493f-9fce-e7df9af4ce9d"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1362), "Test11", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1364), "Test11", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("4ab3d7b8-1538-47fd-92d0-124047893cc0"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1373), "Tesasfasft7", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1379), "asfasf", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("d081c247-754b-4b29-9a8b-4b6cd65c04b9"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1389), "Teasfasgst7", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1392), "afaf", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("cdaebaa6-5f3d-4c5c-9610-ca83366545f0"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1322), "Test7", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1326), "Test7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("87241e3f-fa47-4997-bfd1-42ab1f8b5d2d"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1406), "Test7asgasgasgasg", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1409), "Test7asgasg", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("765a78f4-84b7-43d2-9a82-bc4e87e4c56d"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1469), "Test2352357", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1472), "Testsdghddgsdg2352357", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("eefc119d-d88d-40fe-af3c-7707f4d164cd"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1397), "Tesasgasgt7", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1400), "TsdhSFhest7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("cc9be7f1-8c1b-464c-844d-daae7b61d1ca"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1459), "Test7235235", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1462), "Te235235st7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("5603af66-cdb8-43d4-9cc5-5774944f34de"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1448), "Tes235235t7", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1451), "Tes23523asgdshgfh5t7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("1f12fe75-6216-47f8-a2a9-27218f338f19"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1271), "Test3", "a", new DateTime(2019, 3, 31, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1275), "Test3", new Guid("5f76bd54-b063-487a-89ca-c9ec6a9b6099") },
                    { new Guid("467ca637-63a7-4c02-ada5-8c87ad42ee9d"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1431), "Tes2525235t7", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1434), "Test26236asg267", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("f3a27a5f-2a01-49ca-b175-0c998e57cf49"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1424), "Te346364st7", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1426), "Tes4634636agasgt7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("3dc78c33-50fd-4ba9-be6a-65f5e9a98c4f"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1416), "TesFAHADFHDHt7", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1419), "TestsdghHR7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("6c3f117e-0c35-4038-ad36-70d920d3e2fd"), new DateTime(2019, 3, 28, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1439), "Tes235235t7", "a", new DateTime(2019, 4, 4, 13, 7, 50, 482, DateTimeKind.Local).AddTicks(1442), "Te235asgasgdg235st7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") }
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "Id", "Created", "ImageName", "Message", "ScheduledTime", "ThumbnailName", "Title", "UserId" },
                values: new object[,]
                {
                    { new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618"), new DateTime(2019, 3, 28, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(6373), "Event_Original_01-30-2019_11-41-16.jpg", "Test2", new DateTime(2019, 3, 30, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(6388), "Event_Thumbnail_01-30-2019_11-41-16.jpg", "Test2", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("19cd0feb-ca87-4d84-ad1d-9f63656439b2"), new DateTime(2019, 3, 28, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(6425), "Event_Original_01-30-2019_11-41-16.jpg", "Test3", new DateTime(2019, 3, 31, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(6429), "Event_Thumbnail_01-30-2019_11-41-16.jpg", "Test3", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("a435b497-502c-4c58-a345-14d32229be4e"), new DateTime(2019, 3, 28, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(6439), "Event_Original_01-30-2019_11-41-16.jpg", "Test4", new DateTime(2019, 4, 1, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(6442), "Event_Thumbnail_01-30-2019_11-41-16.jpg", "Test4", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("9f78ec31-c59a-4345-90ec-7e6736611d95"), new DateTime(2019, 3, 28, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(6458), "Event_Original_01-30-2019_11-41-16.jpg", "Test6", new DateTime(2019, 4, 3, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(6461), "Event_Thumbnail_01-30-2019_11-41-16.jpg", "Test6", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("40f93431-07ea-4c22-b0ff-047100ee95bc"), new DateTime(2019, 3, 28, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(6468), "Event_Original_01-30-2019_11-41-16.jpg", "Test7", new DateTime(2019, 4, 4, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(6471), "Event_Thumbnail_01-30-2019_11-41-16.jpg", "Test7", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"), new DateTime(2019, 3, 25, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(4244), "Event_Original_01-30-2019_11-41-16.jpg", "Test1Test1Test1Te st1Tes t1Test1T est1Test 1Test1T st1Test1Te st1Test1Tes t1Test1Test1Test 1Test1Test1Test1Test1Test1Test1T est1Test1Test1Test1Test1Test 1Test1Te st1T est1Test1Test1Test1Te st1Test1Tes t1Test1Test1Te st1Test1Test1Test1Test1Test1Test 1Test1Test1Test 1Test1Test1Te t1Test1Test1Test1 Test1Test1Te st1Tes t1Test 1Test1Test1 Test1Test1 Test1Test1Test1Test1Test1Te st1Test1Te st1Test1Test1 Test1Test1Test1T est1Test1Test1Test1Test1Test1Test1Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1 Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1 Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1 Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1 Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1 Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1 Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1 Test1Test1Test1Test 1Test1Te st1Test1Test1Te st1Test1", new DateTime(2019, 3, 29, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(4254), "Event_Thumbnail_01-30-2019_11-41-16.jpg", "Test1", new Guid("5f76bd53-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("330609f8-13d3-40b6-b78a-221538d8dda0"), new DateTime(2019, 3, 28, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(6449), "Event_Original_01-30-2019_11-41-16.jpg", "Test5", new DateTime(2019, 4, 2, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(6452), "Event_Thumbnail_01-30-2019_11-41-16.jpg", "Test5", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") }
                });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "Created", "Important", "Message", "Title", "UserId" },
                values: new object[,]
                {
                    { new Guid("e77551ba-78e3-4a36-8754-3ea5f12e1619"), new DateTime(2019, 3, 25, 13, 7, 50, 480, DateTimeKind.Local).AddTicks(6692), true, "Test1", "Test1", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("0bd70845-1647-4d62-b1b2-f64626968727"), new DateTime(2019, 3, 28, 13, 7, 50, 480, DateTimeKind.Local).AddTicks(9629), true, "Test4", "Test4", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("20e5e28c-35b4-42f1-a366-3efaf30e96f1"), new DateTime(2019, 3, 28, 13, 7, 50, 480, DateTimeKind.Local).AddTicks(9633), true, "Test5", "Test5", new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("e77551ba-78e4-4a36-8754-3ea5f12e1618"), new DateTime(2019, 3, 28, 13, 7, 50, 480, DateTimeKind.Local).AddTicks(9320), false, "Test2", "Test2", new Guid("5f76bd53-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("13301ce4-0913-4681-8506-a19a99286047"), new DateTime(2019, 3, 28, 13, 7, 50, 480, DateTimeKind.Local).AddTicks(9619), false, "Test3", "Test3", new Guid("5f76bd53-b065-487a-89ca-c9ec6a9b60c9") }
                });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Created", "Message", "ParentCommentId", "PostId", "UserId" },
                values: new object[,]
                {
                    { new Guid("e77551ba-78e3-4a36-9754-4ea5f12e1619"), new DateTime(2019, 3, 23, 13, 7, 50, 480, DateTimeKind.Local).AddTicks(9987), "Test1", null, new Guid("e77551ba-78e3-4a36-8754-3ea5f12e1619"), new Guid("5f76bd53-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("e77551ba-78e4-4a36-7754-5ea5f12e1618"), new DateTime(2019, 3, 26, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(2001), "Test2", new Guid("e77551ba-78e3-4a36-9754-4ea5f12e1619"), new Guid("e77551ba-78e3-4a36-8754-3ea5f12e1619"), new Guid("5f76bd53-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("6264505f-fd9b-469e-908e-25847f9186c2"), new DateTime(2019, 3, 25, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(3755), "Test3", new Guid("e77551ba-78e3-4a36-9754-4ea5f12e1619"), new Guid("e77551ba-78e3-4a36-8754-3ea5f12e1619"), new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("c604b1c2-4d5d-4d05-bca1-0ad5f52f0a50"), new DateTime(2019, 3, 27, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(3790), "Test4", new Guid("e77551ba-78e4-4a36-7754-5ea5f12e1618"), new Guid("e77551ba-78e3-4a36-8754-3ea5f12e1619"), new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("e09a1cb7-6708-432d-9cda-5cf97e703c70"), new DateTime(2019, 3, 27, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(3795), "Test5", null, new Guid("e77551ba-78e3-4a36-8754-3ea5f12e1619"), new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("e37f45b1-8e1a-4bcd-95b5-a5a200cefa67"), new DateTime(2019, 3, 27, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(3808), "Test6", null, new Guid("e77551ba-78e3-4a36-8754-3ea5f12e1619"), new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("abb525ca-0536-43e4-bda9-fccb48f4f27c"), new DateTime(2019, 3, 26, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(3826), "Test7", null, new Guid("e77551ba-78e4-4a36-8754-3ea5f12e1618"), new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9") }
                });

            migrationBuilder.InsertData(
                table: "MeetingMinutes",
                columns: new[] { "Id", "BoardMeetingId", "Created", "FileName", "UserId" },
                values: new object[,]
                {
                    { new Guid("e77551ba-78e3-4a36-9754-3ea5f12e1619"), new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"), new DateTime(2019, 3, 25, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(6807), "test1.txt", new Guid("5f76bd53-b065-487a-89ca-c9ec6a9b60c9") },
                    { new Guid("e77551ba-78e4-4a36-7754-3ea5f12e1618"), new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618"), new DateTime(2019, 3, 28, 13, 7, 50, 481, DateTimeKind.Local).AddTicks(8775), "test2.txt", new Guid("5f76bd53-b065-487a-89ca-c9ec6a9b60c9") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardMeetings_UserId",
                table: "BoardMeetings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PostId",
                table: "Comments",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_UserId",
                table: "Events",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingMinutes_BoardMeetingId",
                table: "MeetingMinutes",
                column: "BoardMeetingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeetingMinutes_UserId",
                table: "MeetingMinutes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_UserId",
                table: "Posts",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "MeetingMinutes");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "BoardMeetings");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
