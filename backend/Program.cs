using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


// Tambahkan layanan ke container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=data.db"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Konfigurasi CORS (Allow All Origins)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()  // Mengizinkan permintaan dari semua origin
            .AllowAnyMethod()   // Mengizinkan semua metode HTTP (GET, POST, dll.)
            .AllowAnyHeader();  // Mengizinkan semua header, termasuk Content-Type
    });
});

var app = builder.Build();

// Auto migrations saat aplikasi dijalankan
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // Terapkan migrasi otomatis
}

// Middleware Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Aktifkan HTTPS dan CORS
app.UseHttpsRedirection();
app.UseCors();

// Endpoint untuk mengambil semua user
app.MapGet("/users", async (AppDbContext db) =>
{
    var users = await db.Users
                        .Include(u => u.Fingerprints) // Mengambil data Fingerprints terkait
                        .ToListAsync();

    var userWithFingerprintData = users.Select(u => new
    {
        u.Id,
        u.Username,
        u.UID,
        u.Phone,
        FingerprintIndexes = u.Fingerprints.Select(f => f.FingerIndex).ToList()
    });

    return Results.Ok(userWithFingerprintData);
});


// Endpoint untuk menyimpan user
app.MapPost("/users", async (AppDbContext db, User user) =>
{
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", user);
});


// Endpoint untuk memperbarui user berdasarkan UID
app.MapPut("/users/{uid}", async (AppDbContext db, int uid, User user) =>
{
    var existingUser = await db.Users.FirstOrDefaultAsync(u => u.UID == uid);
    if (existingUser is null)
    {
        return Results.NotFound($"User with UID {uid} not found.");
    }

    existingUser.Username = user.Username;
    existingUser.Password = user.Password;

    // Simpan perubahan
    await db.SaveChangesAsync();

    return Results.Ok(existingUser); // Return updated user
});

// Endpoint untuk mendapatkan fingerprint yang terdaftar berdasarkan UID
app.MapGet("/fingerprints/{uid}", async (AppDbContext db, int uid) =>
{
    var fingerprints = await db.Fingerprints
                                .Where(f => f.UID == uid)
                                .Select(f => f.FingerIndex)
                                .ToListAsync();

    if (fingerprints.Any())
    {
        return Results.Ok(fingerprints);
    }
    return Results.NotFound("No fingerprints found for this UID.");
});


// Endpoint untuk menyimpan data fingerprint
app.MapPost("/fingerprints", async (AppDbContext db, Fingerprint fingerprint) =>
{
    db.Fingerprints.Add(fingerprint);
    await db.SaveChangesAsync();
    return Results.Ok(fingerprint); // Mengembalikan data fingerprint yang disimpan
});

// Endpoint untuk menghapus fingerprint berdasarkan UID dan FingerIndex
app.MapDelete("/fingerprints/{uid}/{fingerIndex}", async (AppDbContext db, int uid, int fingerIndex) =>
{
    var fingerprint = await db.Fingerprints
                                .FirstOrDefaultAsync(f => f.UID == uid && f.FingerIndex == fingerIndex);

    if (fingerprint == null)
    {
        return Results.NotFound("Fingerprint not found.");
    }

    db.Fingerprints.Remove(fingerprint);
    await db.SaveChangesAsync();

    return Results.Ok("Fingerprint deleted successfully.");
});

// Endpoint untuk mengambil data kehadiran
app.MapGet("/attendance", async (AppDbContext db) =>
{
    var attendanceRecords = await db.Attendances.ToListAsync();

    // Mengirimkan data kehadiran dalam format JSON
    return Results.Ok(attendanceRecords);
});


// Endpoint untuk menyimpan data kehadiran
app.MapPost("/attendance", async (AppDbContext db, AttendanceData attendanceData) =>
{
    // Menyimpan data kehadiran ke database
    var attendanceRecord = new AttendanceRecord
    {
        Uid = attendanceData.Uid,
        Username = attendanceData.Username,
        Timestamp = DateTime.Parse(attendanceData.Timestamp)
    };

    db.Attendances.Add(attendanceRecord);
    await db.SaveChangesAsync();

    return Results.Ok(attendanceRecord);
});


// Endpoint untuk mengambil semua shapes
app.MapGet("/shapes", async (AppDbContext db) =>
{
    return Results.Ok(await db.Shapes.ToListAsync());
});

app.MapPost("/shapes", async (AppDbContext db, Shape shape) =>
{
    db.Shapes.Add(shape);
    await db.SaveChangesAsync();
    return Results.Created($"/shapes/{shape.Id}", shape);
});


app.MapPut("/shapes/{id}", async (AppDbContext db, int id, Shape shape) =>
{
    var existingShape = await db.Shapes.FindAsync(id);
    if (existingShape == null)
    {
        return Results.NotFound($"Shape with ID {id} not found.");
    }

    // Ensure line shape has valid width and height
    if (shape.ShapeType == "line")
    {
        shape.Width = shape.Width ?? 100;  // Default width for line if null
        shape.Height = shape.Height ?? 0;  // Default height for line if null
    }

    existingShape.X = shape.X ?? existingShape.X;
    existingShape.Y = shape.Y ?? existingShape.Y;
    existingShape.Width = shape.Width ?? existingShape.Width;
    existingShape.Height = shape.Height ?? existingShape.Height;
    existingShape.Rotation = shape.Rotation ?? existingShape.Rotation;
    existingShape.Name = shape.Name ?? existingShape.Name;
    existingShape.TopicName = shape.TopicName ?? existingShape.TopicName;
    existingShape.OutputName = shape.OutputName ?? existingShape.OutputName;
    existingShape.Key = shape.Key ?? existingShape.Key;
    existingShape.Value = shape.Value ?? existingShape.Value;
    existingShape.Color = shape.Color ?? existingShape.Color;
    existingShape.BorderColor = shape.BorderColor ?? existingShape.BorderColor;
    existingShape.BorderWidth = shape.BorderWidth ?? existingShape.BorderWidth;
    existingShape.FillColor = shape.FillColor ?? existingShape.FillColor;
    existingShape.Border = shape.Border ?? existingShape.Border;

    await db.SaveChangesAsync();

    return Results.Ok(existingShape);
});

// Endpoint untuk menghapus shape
app.MapDelete("/shapes/{id}", async (AppDbContext db, int id) =>
{
    var shape = await db.Shapes.FindAsync(id);
    if (shape == null)
    {
        return Results.NotFound();
    }
    db.Shapes.Remove(shape);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


//Get all data devices
app.MapGet("/api/devices", async (AppDbContext db) =>
{
    return Results.Ok(await db.Devices.ToListAsync());
});

// Define endpoints maintenance
app.MapGet("/api/maintenance", async (AppDbContext db) =>
    await db.Maintenances.ToListAsync());

app.MapGet("/api/notify-wa", async (AppDbContext db, string? phone, string? message) =>
{
    await WhatsappMessageSender.SendWhatsapp(phone, message);

});

app.MapPost("/api/send-broadcast", async (AppDbContext context, BroadcastMessageRequest request) =>
{
    var users = await context.Users.ToListAsync();
    foreach (var user in users)
    {
        if (!string.IsNullOrWhiteSpace(user.Phone))
        {
            try
            {
                await WhatsappMessageSender.SendWhatsapp(user.Phone, request.Message); // Use request.Message here
                Console.WriteLine($"Message sent to {user.Phone}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send message to {user.Phone}: {ex.Message}");
            }
        }
    }

    return Results.Ok("Broadcast message sent.");
});

app.MapGet("/send-whatsapp-messages", async (AppDbContext context) =>
{
    var maintenanceRecords = await context.Maintenances
        .Include(m => m.User)
        .Where(m => m.Status == "Outs")
        .ToListAsync();

    foreach (var record in maintenanceRecords)
    {
        if (record.User?.Phone != null)
        {
            try
            {
                string defaultMessage = "Hello " + (record.User.Username ?? "User") + ", you have a task for Maintenance starting on " + (record.StartDate?.ToString("yyyy-MM-dd") ?? "No Start Date") + " : "; // Updated message format with StartDate
                string endMessage = " Please prepare!";
                string fullMessage = defaultMessage + (record.Description ?? "No description available.") + endMessage;

                await WhatsappMessageSender.SendWhatsapp(record.User.Phone, fullMessage);
                // Optional: Update record to indicate message has been sent
            }
            catch (Exception ex)
            {
                // Log the error (consider how you want to handle failures)
            }
        }
    }

    return Results.Ok("Messages sent successfully.");
});

app.MapPost("/send-whatsapp-message", async (int maintenanceId, AppDbContext context) =>
{
    var maintenance = await context.Maintenances.FindAsync(maintenanceId);
    if (maintenance == null || maintenance.UserId == null)
    {
        return Results.NotFound("Maintenance or related user not found.");
    }

    var user = await context.Users.FindAsync(maintenance.UserId.Value);
    if (user?.Phone == null)
    {
        return Results.NotFound("User's WhatsApp number not found.");
    }

    try
    {
        await WhatsappMessageSender.SendWhatsapp(user.Phone, maintenance.Description);
        return Results.Ok("Message sent successfully.");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});


app.MapGet("/api/maintenance/{id}", async (int id, AppDbContext db) =>
{
    var maintenance = await db.Maintenances.FirstOrDefaultAsync(m => m.Id == id);
    return maintenance != null ? Results.Ok(maintenance) : Results.NotFound();
});

app.MapPost("/api/maintenance", async (Maintenance maintenance, AppDbContext db) =>
{
    db.Maintenances.Add(maintenance);
    await db.SaveChangesAsync();
    return Results.Created($"/api/maintenance/{maintenance.Id}", maintenance);
});

app.MapPut("/api/maintenance/{id}", async (int id, Maintenance updatedMaintenance, AppDbContext db) =>
{
    var existingMaintenance = await db.Maintenances.FirstOrDefaultAsync(m => m.Id == id);
    if (existingMaintenance != null)
    {
        // Update specific fields if they are provided in the request
        existingMaintenance.Description = updatedMaintenance.Description ?? existingMaintenance.Description;
        existingMaintenance.Status = updatedMaintenance.Status ?? existingMaintenance.Status;
        existingMaintenance.StartDate = updatedMaintenance.StartDate ?? existingMaintenance.StartDate;
        existingMaintenance.EndDate = updatedMaintenance.EndDate ?? existingMaintenance.EndDate;
        existingMaintenance.Notes = updatedMaintenance.Notes ?? existingMaintenance.Notes;
        existingMaintenance.UserId = updatedMaintenance.UserId ?? existingMaintenance.UserId;

        await db.SaveChangesAsync();

        return Results.Ok(existingMaintenance);
    }

    return Results.NotFound();
});

app.MapDelete("/api/maintenance/{id}", async (int id, AppDbContext db) =>
{
    var maintenance = await db.Maintenances.FirstOrDefaultAsync(m => m.Id == id);
    if (maintenance != null)
    {
        db.Maintenances.Remove(maintenance);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    return Results.NotFound();
});

app.MapGet("/maintenance", (string? status, AppDbContext dbContext) =>
{
    var query = dbContext.Maintenances.AsQueryable();

    if (!string.IsNullOrWhiteSpace(status))
    {
        query = query.Where(m => m.Status == status);
    }

    var maintenances = query.ToList();
    return Results.Ok(maintenances);
});

//HTTP Request Testing http://localhost:5072/send-whatsapp?phone=+62yourphone&message=Hello+World
app.MapGet("/send-whatsapp", async (string phone, string message) =>
{
    if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(message))
    {
        return Results.BadRequest("Phone number or message is missing.");
    }

    // Panggil fungsi untuk mengirim pesan WhatsApp
    await WhatsappMessageSender.SendWhatsapp(phone, message);
    return Results.Ok($"Message '{message}' sent to {phone}");
});

app.MapGet("/api/topics", async (AppDbContext context) =>
{
    return await context.Topics.ToListAsync();
});

app.MapPost("/api/topics", async (Topic topic, AppDbContext context) =>
{
    context.Topics.Add(topic);
    await context.SaveChangesAsync();
    return Results.Created($"/api/topics/{topic.Id}", topic);
});
app.MapGet("/api/topics/{id}", async (int id, AppDbContext context) =>
{
    var topic = await context.Topics.FindAsync(id);
    if (topic == null)
    {
        return Results.NotFound($"Topic with ID {id} not found.");
    }
    return Results.Ok(topic);
});

app.MapPut("/api/topics/{id}", async (int id, Topic updatedTopic, AppDbContext context) =>
{
    var topic = await context.Topics.FindAsync(id);
    if (topic == null)
    {
        return Results.NotFound();
    }

    topic.Name = updatedTopic.Name;
    topic.TopicName = updatedTopic.TopicName;
    await context.SaveChangesAsync();
    return Results.Ok(topic);
});

app.MapDelete("/api/topics/{id}", async (int id, AppDbContext context) =>
{
    var topic = await context.Topics.FindAsync(id);
    if (topic == null)
    {
        return Results.NotFound();
    }

    context.Topics.Remove(topic);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

// Map endpoints for LayoutGroups
app.MapGet("/api/layoutgroups", async (AppDbContext context) =>
{
    var layoutGroups = await context.LayoutGroups.ToListAsync();
    return Results.Ok(layoutGroups);
});

app.MapGet("/api/layoutgroups/{id}", async (int id, AppDbContext context) =>
{
    var layoutGroup = await context.LayoutGroups
        .Include(lg => lg.Shapes)
        .FirstOrDefaultAsync(lg => lg.Id == id);

    if (layoutGroup == null) return Results.NotFound();
    return Results.Ok(layoutGroup);
});

app.MapPost("/api/layoutgroups", async (LayoutGroup layoutGroup, AppDbContext context) =>
{
    context.LayoutGroups.Add(layoutGroup);
    await context.SaveChangesAsync();
    return Results.Ok(layoutGroup);
});

app.MapPut("/api/layoutgroups/isuse/{id}", async (int id, LayoutGroup updatedLayoutGroup, AppDbContext context) =>
{
    var existingGroup = await context.LayoutGroups.FindAsync(id);
    if (existingGroup == null) return Results.NotFound();

    // Update the isUse field
    existingGroup.isUse = updatedLayoutGroup.isUse;
    context.LayoutGroups.Update(existingGroup);
    await context.SaveChangesAsync();

    return Results.Ok(existingGroup);
});


app.MapPut("/api/layoutgroups/{id}", async (int id, LayoutGroup updatedLayoutGroup, AppDbContext context) =>
{
    var existingGroup = await context.LayoutGroups.FindAsync(id);
    if (existingGroup == null) return Results.NotFound();

    existingGroup.Name = updatedLayoutGroup.Name;
    existingGroup.isUse = updatedLayoutGroup.isUse;
    await context.SaveChangesAsync();
    return Results.Ok(existingGroup);
});

app.MapDelete("/api/layoutgroups/{id}", async (int id, AppDbContext context) =>
{
    var existingGroup = await context.LayoutGroups.FindAsync(id);
    if (existingGroup == null) return Results.NotFound();

    context.LayoutGroups.Remove(existingGroup);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.MapGet("/layoutgroups/{layoutGroupId}/shapes", async (AppDbContext db, int layoutGroupId) =>
{
    var shapes = await db.Shapes
        .Where(s => s.LayoutGroupId == layoutGroupId)
        .ToListAsync();

    if (!shapes.Any())
    {
        return Results.NotFound($"No shapes found for LayoutGroupId {layoutGroupId}");
    }

    return Results.Ok(shapes);
});

app.MapPost("/api/layoutgroups/{id}/upload-image", async (int id, HttpRequest request, AppDbContext context) =>
{
    var layoutGroup = await context.LayoutGroups.FindAsync(id);
    if (layoutGroup == null)
    {
        return Results.NotFound("LayoutGroup not found.");
    }

    // Read the image file from the request
    var formFile = request.Form.Files.FirstOrDefault();
    if (formFile == null || formFile.Length == 0)
    {
        return Results.BadRequest("No image file provided.");
    }

    using var memoryStream = new MemoryStream();
    await formFile.CopyToAsync(memoryStream);

    // Save the image data to the database
    layoutGroup.ImageData = memoryStream.ToArray();
    await context.SaveChangesAsync();

    return Results.Ok("Image uploaded successfully.");
});


app.MapGet("/api/layoutgroups/{id}/image", async (int id, AppDbContext context) =>
{
    var layoutGroup = await context.LayoutGroups.FindAsync(id);
    if (layoutGroup == null || layoutGroup.ImageData == null)
    {
        return Results.NotFound("Image not found.");
    }

    return Results.File(layoutGroup.ImageData, "image/jpeg"); // Or "image/png" based on your images
});


app.MapGet("/dashboard/shapes", async (AppDbContext db) =>
{
    var shapes = await db.Shapes
        .Include(s => s.LayoutGroup)
        .Where(s => s.LayoutGroup.isUse == true)
        .Select(s => new ShapeDto
        {
            Id = s.Id,
            Name = s.Name,
            TopicId = s.TopicId,
            TopicName = s.TopicName,
            OutputName = s.OutputName,
            Key = s.Key,
            Value = s.Value,
            ShapeType = s.ShapeType,
            FillColor = s.FillColor,
            Color = s.Color,
            Border = s.Border,
            BorderColor = s.BorderColor,
            BorderWidth = s.BorderWidth,
            Width = s.Width,
            Height = s.Height,
            Rotation = s.Rotation,
            X = s.X,
            Y = s.Y
        })
        .ToListAsync();

    if (!shapes.Any())
    {
        return Results.NotFound("No shapes found for LayoutGroups with isUse = true.");
    }

    return Results.Ok(shapes);
});


// Map CRUD APIs for ContainerRack
app.MapGet("/containerRacks", async (AppDbContext db) =>
{
    var containerRacks = await db.ContainerRacks.Include(c => c.DeviceRacks).ToListAsync();
    return Results.Ok(containerRacks);
});

app.MapGet("/containerRacks/{id}", async (int id, AppDbContext db) =>
{
    var containerRack = await db.ContainerRacks.Include(c => c.DeviceRacks).FirstOrDefaultAsync(c => c.Id == id);
    return containerRack == null ? Results.NotFound() : Results.Ok(containerRack);
});

app.MapPost("/containerRacks", async (ContainerRack containerRack, AppDbContext db) =>
{
    db.ContainerRacks.Add(containerRack);
    await db.SaveChangesAsync();
    return Results.Created($"/containerRacks/{containerRack.Id}", containerRack);
});

app.MapPut("/containerRacks/{id}", async (int id, ContainerRack updatedContainerRack, AppDbContext db) =>
{
    var containerRack = await db.ContainerRacks.Include(c => c.DeviceRacks).FirstOrDefaultAsync(c => c.Id == id);
    if (containerRack == null) return Results.NotFound();

    containerRack.RackName = updatedContainerRack.RackName;
    containerRack.Topic = updatedContainerRack.Topic;
    containerRack.HeightPercentage = updatedContainerRack.HeightPercentage;
    containerRack.DeviceRacks = updatedContainerRack.DeviceRacks;

    await db.SaveChangesAsync();
    return Results.Ok(containerRack);
});

app.MapDelete("/containerRacks/{id}", async (int id, AppDbContext db) =>
{
    var containerRack = await db.ContainerRacks.FindAsync(id);
    if (containerRack == null) return Results.NotFound();

    db.ContainerRacks.Remove(containerRack);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapGet("/deviceRacks/byContainer/{containerRackId}", async (int containerRackId, AppDbContext db) =>
{
    var deviceRacks = await db.DeviceRacks
        .Where(d => d.ContainerRackId == containerRackId)
        .ToListAsync();

    if (!deviceRacks.Any())
    {
        return Results.NotFound($"No devices found for ContainerRackId {containerRackId}");
    }

    return Results.Ok(deviceRacks);
});


app.MapGet("/deviceRacks", async (AppDbContext db) =>
{
    var deviceRacks = await db.DeviceRacks.Include(d => d.ContainerRack).ToListAsync();
    return Results.Ok(deviceRacks);
});

app.MapGet("/deviceRacks/{id}", async (int id, AppDbContext db) =>
{
    var deviceRack = await db.DeviceRacks.Include(d => d.ContainerRack).FirstOrDefaultAsync(d => d.Id == id);
    return deviceRack == null ? Results.NotFound() : Results.Ok(deviceRack);
});

app.MapPost("/deviceRacks", async (DeviceRack deviceRack, AppDbContext db) =>
{
    var totalUsedU = await db.DeviceRacks
        .Where(d => d.ContainerRackId == deviceRack.ContainerRackId)
        .SumAsync(d => d.TotalU);

    if (totalUsedU + deviceRack.TotalU > 42)
    {
        return Results.BadRequest("Not enough space in the rack!");
    }

    db.DeviceRacks.Add(deviceRack);
    await db.SaveChangesAsync();
    return Results.Created($"/deviceRacks/{deviceRack.Id}", deviceRack);
});

app.MapPut("/deviceRacks/{id}", async (int id, DeviceRack updatedDeviceRack, AppDbContext db) =>
{
    var deviceRack = await db.DeviceRacks.FindAsync(id);
    if (deviceRack == null) return Results.NotFound();

    deviceRack.Name = updatedDeviceRack.Name;
    deviceRack.Position = updatedDeviceRack.Position;
    deviceRack.TotalU = updatedDeviceRack.TotalU;
    deviceRack.Person = updatedDeviceRack.Person;
    deviceRack.Customer = updatedDeviceRack.Customer;

    await db.SaveChangesAsync();
    return Results.Ok(deviceRack);
});

app.MapDelete("/deviceRacks/{id}", async (int id, AppDbContext db) =>
{
    var deviceRack = await db.DeviceRacks.FindAsync(id);
    if (deviceRack == null) return Results.NotFound();

    db.DeviceRacks.Remove(deviceRack);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Jalankan aplikasi
app.Run();

// DbContext untuk aplikasi
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
    public DbSet<Fingerprint> Fingerprints { get; set; }
    public DbSet<AttendanceRecord> Attendances { get; set; }
    public DbSet<Shape> Shapes { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<LayoutGroup> LayoutGroups { get; set; }
    public DbSet<Maintenance> Maintenances { get; set; }
    public DbSet<ContainerRack> ContainerRacks { get; set; }
    public DbSet<DeviceRack> DeviceRacks { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shape>()
            .HasOne(s => s.LayoutGroup)
            .WithMany(lg => lg.Shapes)
            .HasForeignKey(s => s.LayoutGroupId)
            .OnDelete(DeleteBehavior.Cascade); // Atur perilaku penghapusan relasi
    }
}

public class ContainerRack
{
    public int Id { get; set; }
    public string RackName { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int HeightPercentage { get; set; }
    public List<DeviceRack> DeviceRacks { get; set; }
}

public class DeviceRack
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Position { get; set; }
    public int TotalU { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7);
    public string Person { get; set; }
    public string Customer { get; set; }
    public int ContainerRackId { get; set; }

    [JsonIgnore]
    public ContainerRack ContainerRack { get; set; }
}



public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    public int UID { get; set; }
    public ICollection<Fingerprint> Fingerprints { get; set; }
}

public class Fingerprint
{
    public int Id { get; set; }
    public int UID { get; set; }
    public int FingerIndex { get; set; }
}

public class AttendanceRecord
{
    public int Id { get; set; }
    public int Uid { get; set; }
    public string Username { get; set; }
    public DateTime Timestamp { get; set; }
}

public class AttendanceData
{
    public int Uid { get; set; }
    public string Username { get; set; }
    public string Timestamp { get; set; }
}

public class Shape
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? TopicId { get; set; }
    public Topic? Topic { get; set; }
    public string? TopicName { get; set; }
    public string? OutputName { get; set; }
    public string? Key { get; set; }
    public string? Value { get; set; }
    public string? ShapeType { get; set; } // 'rec', 'circle', 'line', etc.
    public bool? FillColor { get; set; } // Whether the shape is filled with color or not
    public string? Color { get; set; } // Fill color
    public bool? Border { get; set; } // Whether the shape has a border
    public string? BorderColor { get; set; } // Border color
    public int? BorderWidth { get; set; }
    public int? Width { get; set; } // Width of the shape (only for rectangle or line)
    public int? Rotation { get; set; } // Rotation angle in degrees
    public int? Height { get; set; } // Height of the shape (only for rectangle)
    public int? X { get; set; } // X position of the shape
    public int? Y { get; set; } // Y position of the shape
    public int? LayoutGroupId { get; set; }
    [JsonIgnore]
    public LayoutGroup LayoutGroup { get; set; }
}

public class ShapeDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? TopicId { get; set; }
    public string? TopicName { get; set; }
    public string? OutputName { get; set; }
    public string? Key { get; set; }
    public string? Value { get; set; }
    public string? ShapeType { get; set; }
    public bool? FillColor { get; set; }
    public string? Color { get; set; }
    public bool? Border { get; set; }
    public string? BorderColor { get; set; }
    public int? BorderWidth { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Rotation { get; set; }
    public int? X { get; set; }
    public int? Y { get; set; }
}


public class LayoutGroup
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool? isUse { get; set; }
    public byte[]? ImageData { get; set; }

    [JsonIgnore]
    public ICollection<Shape> Shapes { get; set; }
}


public class Topic
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? TopicName { get; set; }
}

public class Device
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Desc { get; set; }

}

//Maintenance Table
public class Maintenance
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
    public int? UserId { get; set; }
    [JsonIgnore]
    public User? User { get; set; }
    public int? DeviceId { get; set; }
    [JsonIgnore]
    public Device? Device { get; set; }
}

public class BroadcastMessageRequest
{
    public string Message { get; set; }
}



namespace MyWhatsappProject.src
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string recipientNumber = "6281284842478"; // Replace with the actual recipient number
            string message = "Your message here"; // Replace with your actual message

            while (true)
            {
                try
                {
                    await WhatsappMessageSender.SendWhatsapp(recipientNumber, message);
                    Console.WriteLine($"Success to send Messege");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    // Optional: Add some error handling logic here
                }

                // Wait for 10 seconds before sending the next message
                await Task.Delay(10000);
            }
        }
    }
}
