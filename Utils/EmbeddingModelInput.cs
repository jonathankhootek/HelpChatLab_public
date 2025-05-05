namespace HelpChat.Utils;

internal class EmbeddingModelInput
{
    public required long[] InputIds { get; init; }

    public required long[] AttentionMask { get; init; }

    public required long[] TokenTypeIds { get; init; }
}