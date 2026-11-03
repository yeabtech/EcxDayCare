using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Daycare.Infrastructure.Data;

namespace ECX_Daycare.Controllers;

/// <summary>
/// Controller for viewing database schema and relationships
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SchemaController : ControllerBase
{
    private readonly DaycareDbContext _dbContext;

    public SchemaController(DaycareDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Get all tables and their columns
    /// </summary>
    [HttpGet("tables")]
    public IActionResult GetTables()
    {
        var tables = _dbContext.Model.GetEntityTypes()
            .Where(e => !e.IsOwned())
            .Select(e => new
            {
                TableName = e.GetTableName(),
                Schema = e.GetSchema(),
                Columns = e.GetProperties().Select(p => new
                {
                    ColumnName = p.GetColumnName(),
                    PropertyName = p.Name,
                    Type = p.ClrType.Name,
                    IsNullable = p.IsNullable,
                    IsPrimaryKey = p.IsPrimaryKey()
                }).ToList()
            })
            .ToList();

        return Ok(tables);
    }

    /// <summary>
    /// Get table relationships (foreign keys)
    /// </summary>
    [HttpGet("relationships")]
    public IActionResult GetRelationships()
    {
        var relationships = _dbContext.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys())
            .Select(fk => new
            {
                FromTable = fk.DeclaringEntityType.GetTableName(),
                ToTable = fk.PrincipalEntityType.GetTableName(),
                FromColumns = string.Join(", ", fk.Properties.Select(p => p.GetColumnName())),
                ToColumns = string.Join(", ", fk.PrincipalKey.Properties.Select(p => p.GetColumnName())),
                DeleteBehavior = fk.DeleteBehavior.ToString()
            })
            .ToList();

        return Ok(relationships);
    }

    /// <summary>
    /// Get HTML ER diagram visualization
    /// </summary>
    [HttpGet("diagram")]
    [Produces("text/html")]
    public IActionResult GetDiagram()
    {
        var tables = _dbContext.Model.GetEntityTypes()
            .Where(e => !e.IsOwned())
            .Select(e => new
            {
                TableName = e.GetTableName(),
                Columns = e.GetProperties().Select(p => new
                {
                    ColumnName = p.GetColumnName(),
                    Type = p.ClrType.Name,
                    IsPK = p.IsPrimaryKey()
                }).ToList()
            })
            .ToList();

        var relationships = _dbContext.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys())
            .Select(fk => new
            {
                From = fk.DeclaringEntityType.GetTableName(),
                To = fk.PrincipalEntityType.GetTableName()
            })
            .ToList();

        var html = GenerateHtmlDiagram(tables, relationships);
        return Content(html, "text/html");
    }

    private static string GenerateHtmlDiagram(dynamic tables, dynamic relationships)
    {
        var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Database Schema - ECX Daycare</title>
    <script src='https://cdn.jsdelivr.net/npm/mermaid@latest/dist/mermaid.min.js'></script>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; background: #f5f5f5; }
        .container { max-width: 1400px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; }
        h1 { color: #333; }
        .info { background: #e8f4f8; padding: 15px; border-radius: 5px; margin: 20px 0; }
        .table-info { background: white; padding: 15px; margin: 10px 0; border: 1px solid #ddd; border-radius: 5px; }
        .table-name { font-weight: bold; color: #0066cc; margin-bottom: 10px; }
        .columns { margin-left: 20px; }
        .column { padding: 5px; font-family: monospace; }
        .pk { color: #d9534f; font-weight: bold; }
        .fk { color: #5cb85c; }
        .mermaid { text-align: center; margin: 30px 0; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>📊 Database Schema - ECX Daycare</h1>

        <div class='info'>
            <strong>Database Connection:</strong> PostgreSQL - Ecxdaycare<br>
            <strong>Tables:</strong> " + tables.Count + @"<br>
            <strong>Generated:</strong> " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + @"
        </div>

        <h2>Tables & Columns</h2>";

        foreach (var table in tables)
        {
            html += $@"
        <div class='table-info'>
            <div class='table-name'>📋 {table.TableName}</div>
            <div class='columns'>";

            foreach (var column in table.Columns)
            {
                var style = column.IsPK ? "class='pk'" : "";
                var label = column.IsPK ? " [PRIMARY KEY]" : "";
                html += $"<div class='column' {style}>{column.ColumnName}: {column.Type}{label}</div>";
            }

            html += @"
            </div>
        </div>";
        }

        html += @"
        <h2>ER Diagram (Mermaid)</h2>
        <div class='mermaid'>
            erDiagram";

        // Add relationships
        var addedRels = new HashSet<string>();
        foreach (var rel in relationships)
        {
            var key = $"{rel.From}||--||{rel.To}";
            if (!addedRels.Contains(key))
            {
                html += $"\n            {rel.From} ||--|| {rel.To}";
                addedRels.Add(key);
            }
        }

        html += @"
        </div>

        <h2>Relationships</h2>";

        foreach (var rel in relationships)
        {
            html += $"<div class='column'><span class='fk'>{rel.From}</span> → <span class='fk'>{rel.To}</span></div>";
        }

        html += @"
    </div>
</body>
</html>";

        return html;
    }
}
