namespace ConfigApi.Models;

public class Config
{
    public CameraConfig Camera { get; set; } = new CameraConfig();
    public RendererConfig Renderer { get; set; } = new RendererConfig();
    public LightConfig Light { get; set; } = new LightConfig();
    public List<ObjectConfig> Objects { get; set; } = new List<ObjectConfig>();
}

public class CameraConfig
{
    public float Fov { get; set; } = 75f;
    public float Near { get; set; } = 0.1f;
    public float Far { get; set; } = 1000f;
    public float PositionX { get; set; } = 2f;
    public float PositionY { get; set; } = 5f;
    public float PositionZ { get; set; } = 6f;
}

public class RendererConfig
{
    public string BackgroundColor { get; set; } = "#FFFFFF";
}

public class LightConfig
{
    public string Color { get; set; } = "#FFFFFF";
    public float Intensity { get; set; } = 1f;
    public float PositionX { get; set; } = 5f;
    public float PositionY { get; set; } = 5f;
    public float PositionZ { get; set; } = 5f;
}

public class ObjectConfig
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = "box";
    public float Width { get; set; }
    public float Height { get; set; }
    public float Depth { get; set; }
    public float Radius { get; set; }
    public float Opacity { get; set; }
    public string Color { get; set; } = "#FFFFFF";
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
}



app.MapGet("/api/config", () => Results.Json(config));

app.MapPost("/api/config", async(HttpContext httpContext) =>
{
    var newConfig = await JsonSerializer.DeserializeAsync<Config>(httpContext.Request.Body);
    if (newConfig != null)
    {
        config = newConfig;
        return Results.Ok(config);
    }
    return Results.BadRequest("Invalid configuration data.");
});

