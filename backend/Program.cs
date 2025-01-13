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
    public DbSet<Device> Devices { get; set; }
    public DbSet<Maintenance> Maintenances { get; set; }
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
    // Additional properties can be added if needed
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
    public string? ShapeType { get; set; } // 'rec', 'circle', 'line', etc.
    public bool? FillColor { get; set; } // Whether the shape is filled with color or not
    public string? Color { get; set; } // Fill color
    public bool? Border { get; set; } // Whether the shape has a border
    public string? BorderColor { get; set; } // Border color
    public int? BorderWidth { get; set; }
    public int? Width { get; set; } // Width of the shape (only for rectangle or line)
    public int? Height { get; set; } // Height of the shape (only for rectangle)
    public int? X { get; set; } // X position of the shape
    public int? Y { get; set; } // Y position of the shape
    public int? Rotation { get; set; } // Rotation angle in degrees
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
