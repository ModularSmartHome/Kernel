using Kernel.Models.Database;
using Kernel.Models.Request;
using Kernel.Modules;
using Kernel.Shared;
using Microsoft.EntityFrameworkCore;

namespace Kernel.Repository;

public class UserRepository
{

    private readonly DatabaseContext ctx;
    private readonly IPasswordHasher hasher;

    public UserRepository(DatabaseContext ctx, IPasswordHasher hasher)
    {
        this.ctx = ctx;
        this.hasher = hasher;
    }

    public async Task<User> RegisterUser(RegisterRequest request)
    {
        var user = new User();
        user.Username = request.Username;
        user.Password = hasher.HashFromPassword(request.Password);
        ctx.Add(user);
        await ctx.SaveChangesAsync();
        return user;
    }

    public async Task<bool> LoginUser(LoginRequest request)
    {
        var user = await FindUserByUsername(request.Username);
        if (user == null)
        {
            return false;
        }

        if (!hasher.CompareHashAndPassword(user.Password, request.Password)) return false;
        return true;
    }

    public async Task<User?> FindUserByUsername(string username)
    {
        return await ctx.Users
            .Where(u => u.Username == username)
            .FirstOrDefaultAsync();
    }

}