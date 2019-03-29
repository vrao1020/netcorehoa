// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Linq;
using System.Security.Claims;
using HoaIdentityServer.Data;
using HoaIdentityServer.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HoaIdentityServer
{
    public class SeedData
    {
        public static void EnsureSeedData(IServiceProvider provider)
        {
            provider.GetRequiredService<ApplicationDbContext>().Database.Migrate();

            {
                var userMgr = provider.GetRequiredService<UserManager<ApplicationUser>>();
                var test = userMgr.FindByNameAsync("abc@123.com").Result;
                if (test == null)
                {
                    test = new ApplicationUser
                    {
                        UserName = "abc@123.com",
                        Email = "abc@123.com",
                        EmailConfirmed = true
                    };
                    var result = userMgr.CreateAsync(test, "Pass123$").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    test = userMgr.FindByNameAsync("abc@123.com").Result;

                    result = userMgr.AddClaimsAsync(test, new Claim[]{
                                new Claim(JwtClaimTypes.Name, "ABC R"),
                                new Claim(JwtClaimTypes.Email, "abc@123.com"),
                                new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                                new Claim(JwtClaimTypes.Role, "admin"),
                                new Claim("readonly", "true"),
                                new Claim("postcreation", "false")

                }).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Console.WriteLine("user created");
                }
                else
                {
                    Console.WriteLine("user already exists");
                }
            }
        }
    }
}