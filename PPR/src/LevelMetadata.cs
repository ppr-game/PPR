namespace PPR;

public record struct LevelMetadata(uint version, Guid guid, string name, string description, string author,
    int difficulty);
