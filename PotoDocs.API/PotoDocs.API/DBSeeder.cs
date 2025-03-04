﻿using Microsoft.AspNetCore.Identity;
using PotoDocs.API.Entities;
using PotoDocs.API.Models;

namespace PotoDocs.API
{
    public class DBSeeder
    {
        private readonly PotodocsDbContext _dbContext;
        private readonly IPasswordHasher<User> _hasher;

        public DBSeeder(PotodocsDbContext dbContext, IPasswordHasher<User> hasher)
        {
            _dbContext = dbContext;
            _hasher = hasher;
        }

        public void Seed()
        {
            if (_dbContext.Database.CanConnect())
            {
                if (!_dbContext.Roles.Any())
                {
                    var roles = GetRoles();
                    _dbContext.Roles.AddRange(roles);
                    _dbContext.SaveChanges();
                }

                if (!_dbContext.Users.Any(u => u.Email == "a@a.a"))
                {
                    var adminRole = _dbContext.Roles.FirstOrDefault(r => r.Name == "admin");

                    if (adminRole != null)
                    {
                        var adminUser = new User
                        {
                            FirstName = "Admin",
                            LastName = "User",
                            Email = "a@a.a",
                            RoleId = adminRole.Id
                        };

                        // Hashowanie hasła dla użytkownika admin
                        adminUser.PasswordHash = _hasher.HashPassword(adminUser, "a");

                        _dbContext.Users.Add(adminUser);
                        _dbContext.SaveChanges();
                    }
                }
            }
        }

        private IEnumerable<Role> GetRoles()
        {
            var roles = new List<Role>()
            {
                new Role()
                {
                    Name = "user",
                },
                new Role()
                {
                    Name = "manager"
                },
                new Role()
                {
                    Name = "admin"
                }
            };
            return roles;
        }
    }
}
