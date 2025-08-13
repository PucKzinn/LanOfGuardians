using Mirror;

/// <summary>
/// Simple chat payload broadcast to all players.
/// </summary>
public struct ChatMessage : NetworkMessage
{
    public string text;
}
