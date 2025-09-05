using System.Text.Json;

public interface IScreen
{
    void LoadContent();
    void Update();
    void Draw(JsonElement? state = null);
    void Reset();
}
