using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);
//add DI - Add Service

builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));

var app = builder.Build();

//Configure pipeline -UseMethod

app.MapGet("todoitems", async (TodoDb db) =>
{
    var items = await db.Todos.ToListAsync();
    return Results.Ok(items);
});

app.MapGet("todoitems/{id}", async (TodoDb db, int id) =>
{
    var item = await db.Todos.FindAsync(id);
    if (item is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(item);
});

app.MapPost("todoitems", async (TodoItem item, TodoDb db) =>
{
    db.Todos.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{item.Id}", item);
});

app.MapPut("todoitems/{id}", async (int id, TodoItem item, TodoDb db) =>
{
    if (id != item.Id)
    {
        return Results.BadRequest();
    }
    db.Entry(item).State = EntityState.Modified;
    try
    {
        await db.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        if (await db.Todos.FindAsync(id) is null)
        {
            return Results.NotFound();
        }
        throw;
    }
    return Results.NoContent();
});

app.MapDelete("todoitems/{id}", async (int id, TodoDb db) =>
{
    var item = await db.Todos.FindAsync(id);
    if (item is null)
    {
        return Results.NotFound();
    }
    db.Todos.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});



app.Run();
