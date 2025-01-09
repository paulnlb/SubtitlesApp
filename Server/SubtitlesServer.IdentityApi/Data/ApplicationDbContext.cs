using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SubtitlesServer.IdentityApi.Models;

namespace SubtitlesServer.IdentityApi.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<SubAppUser>(options)
{
}