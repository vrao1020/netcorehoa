# netcorehoa
A custom HOA solution written using .NET Core 2.2

# Setup 
- .NET Core 2.2 SDK (https://dotnet.microsoft.com/download)

# Configuration 
There are few important features you need to configure to get the application fully functional:
- Need to update appsettings.json in HoaIdentityServer project with a valid SendGrid API key (for Asp.Net Identity email configuration)
- Need to update appsettings.json in HoaWebApplication project with a valid SendGrid API key (for email services)
- Need to update appsettings.json in HoaWebApplication project with a valid Azure blob storage connection string + container name (for azure blob services)
- Update EmailSender service in HoaIdentityServer with a valid SenderEmail and SenderEmailName
- Update SendEmail service in HoaWebApplication with a valid SenderEmail and SenderEmailName
- (optional) Update Google / FaceBook clientid/clientsecret in HoaIdentityServer startup.cs if you want external authentication
- (optional) Update the image src in SendEmail service (HoaWebApplication) if you want email branding. There are four separate template methods that you'll need to edit  

There is a dependency on Azure blob storage and SendGrid. If you want to replace these with alternates, feel free to download the appropriate
NuGet packages and change the service configuration.

# Running the application
- Set multiple project startup - HoaIdentityServer, HoaIdentityUsers, HoaWebAPI, HoaWebApplication


# Functionality
- Login
- Admin specific functionality
- Upload / download files
- Create events, posts, meetings
- Authentication (IS4)
- Authorization (IS4)

# Future State / Unsure
- Caching on API (unsure how to do)
- Replacing razor pages Web App with SPA
- Extend unit tests
- See if its possible to replace HoaIdentityUsers API and integrate it directly into IDP (unsure how to do as it requires authentication)
