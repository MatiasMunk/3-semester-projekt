using System.Text.RegularExpressions;
using Dapper;
using StudyGroups.Core.Interfaces;
using StudyGroups.Core.Models;
using StudyGroups.Infrastructure.Data;

namespace StudyGroups.Infrastructure.Services;

public class CategoryService(IDbConnectionFactory connectionFactory) : ICategoriesService
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<Result<IEnumerable<Topic>>> GetAllAsync()
    {
        using var conn = await _connectionFactory.CreateOpenAsync();

        var categories = await conn.QueryAsync<Topic>(@"
            SELECT Id, Name, Slug, Icon, Color
            FROM Topics
            ORDER BY Name
        ");

        return Result<IEnumerable<Topic>>.Success(categories);
    }

    public async Task<Result<Topic>> CreateAsync(Topic category)
    {
        category.Name = category.Name.Trim();
        category.Icon = string.IsNullOrWhiteSpace(category.Icon) ? "📚" : category.Icon.Trim();
        category.Color = string.IsNullOrWhiteSpace(category.Color) ? "#4f46e5" : category.Color.Trim();
        category.Slug = CreateSlug(category.Name);

        if (string.IsNullOrWhiteSpace(category.Name))
            return Result<Topic>.Failure("Category name is required");

        using var conn = await _connectionFactory.CreateOpenAsync();

        var exists = await conn.ExecuteScalarAsync<int>(@"
            SELECT COUNT(1)
            FROM Topics
            WHERE LOWER(Name) = LOWER(@Name) OR LOWER(Slug) = LOWER(@Slug)
        ", new { category.Name, category.Slug });

        if (exists > 0)
            return Result<Topic>.Failure("Category already exists");

        var id = await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO Topics (Name, Slug, Icon, Color)
            VALUES (@Name, @Slug, @Icon, @Color);

            SELECT CAST(SCOPE_IDENTITY() as int);
        ", category);

        category.Id = id;
        return Result<Topic>.Success(category);
    }

    public async Task<Result> UpdateAsync(Topic category)
    {
        category.Name = category.Name.Trim();
        category.Icon = string.IsNullOrWhiteSpace(category.Icon) ? "📚" : category.Icon.Trim();
        category.Color = string.IsNullOrWhiteSpace(category.Color) ? "#4f46e5" : category.Color.Trim();
        category.Slug = CreateSlug(category.Name);

        if (string.IsNullOrWhiteSpace(category.Name))
            return Result.Failure("Category name is required");

        using var conn = await _connectionFactory.CreateOpenAsync();

        var duplicate = await conn.ExecuteScalarAsync<int>(@"
            SELECT COUNT(1)
            FROM Topics
            WHERE Id <> @Id
              AND (LOWER(Name) = LOWER(@Name) OR LOWER(Slug) = LOWER(@Slug))
        ", category);

        if (duplicate > 0)
            return Result.Failure("Category already exists");

        var rows = await conn.ExecuteAsync(@"
            UPDATE Topics
            SET Name = @Name,
                Slug = @Slug,
                Icon = @Icon,
                Color = @Color
            WHERE Id = @Id
        ", category);

        return rows == 0
            ? Result.Failure("Category not found")
            : Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        using var conn = await _connectionFactory.CreateOpenAsync();

        var usedBySessions = await conn.ExecuteScalarAsync<int>(@"
            SELECT COUNT(1)
            FROM StudySessions
            WHERE TopicId = @Id
        ", new { Id = id });

        if (usedBySessions > 0)
            return Result.Failure("Category is used by existing sessions");

        var rows = await conn.ExecuteAsync("DELETE FROM Topics WHERE Id = @Id", new { Id = id });

        return rows == 0
            ? Result.Failure("Category not found")
            : Result.Success();
    }

    private static string CreateSlug(string value)
    {
        var slug = value.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");
        slug = slug.Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N") : slug;
    }
}
