using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using JetBrains.Annotations;

using NLog;

namespace PPR;

[PublicAPI]
public static class LevelSerializer {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private const string LevelsPath = "levels";
    private const string LevelMetadataFileName = "metadata.json";
    private const string MusicMetadataFileName = "music.json";
    private const string ScoresPath = "scores.json";

    public record struct LevelMetadata(uint version, Guid guid, string name, string description, string author,
        int difficulty, bool effectHeavy);


    public record struct MusicMetadata(string fileName, string author, string name, MusicMetadata.Bpm[] bpm) {
        public record struct Bpm(float time, float bpm);

        public float GetBpmAt(float time) {
            float currentBpm = 0f;
            foreach((float loopTime, float loopBpm) in bpm) {
                if(loopTime > time)
                    break;
                currentBpm = loopBpm;
            }
            return currentBpm;
        }
    }

    public readonly record struct LevelItem(LevelMetadata metadata, MusicMetadata music, string path, bool hasErrors);

    public readonly record struct LevelScore(int score, int accuracy, int maxCombo, int[] scores);

    public static IEnumerable<string> EnumerateLevelDirectories() => Directory.EnumerateDirectories(LevelsPath);

    public static IEnumerable<LevelItem> ReadLevelList() {
        foreach(string levelDirectory in EnumerateLevelDirectories()) {
            bool levelSuccess = TryReadLevelMetadata(levelDirectory, out LevelMetadata metadata);
            bool musicSuccess = TryReadMusicMetadata(levelDirectory, out MusicMetadata music);
            yield return new LevelItem(metadata, music, levelDirectory, !levelSuccess || !musicSuccess);
        }
    }

    public static bool TryReadLevelMetadata(string levelDirectory, out LevelMetadata metadata) {
        string directoryName = Path.GetFileName(levelDirectory);
        metadata = new LevelMetadata(0, Guid.Empty, directoryName, string.Empty, string.Empty, -1, false);

        string metadataPath = Path.Combine(levelDirectory, LevelMetadataFileName);
        if(!File.Exists(metadataPath)) {
            logger.Error("Level metadata file not found");
            return false;
        }

        FileStream metadataFile = File.OpenRead(metadataPath);

        JsonException? jsonException = null;
        try { metadata = JsonSerializer.Deserialize<LevelMetadata>(metadataFile); }
        catch(JsonException ex) { jsonException = ex; }

        metadataFile.Close();

        if(jsonException is null)
            return true;

        logger.Error(jsonException);
        return false;
    }

    public static bool TryReadMusicMetadata(string levelDirectory, out MusicMetadata metadata) {
        metadata = new MusicMetadata(string.Empty, "<unknown>", "<unknown>", Array.Empty<MusicMetadata.Bpm>());

        string metadataPath = Path.Combine(levelDirectory, MusicMetadataFileName);
        if(!File.Exists(metadataPath)) {
            logger.Error("Music metadata file not found");
            return false;
        }

        FileStream metadataFile = File.OpenRead(metadataPath);

        JsonException? jsonException = null;
        try { metadata = JsonSerializer.Deserialize<MusicMetadata>(metadataFile); }
        catch(JsonException ex) { jsonException = ex; }

        metadataFile.Close();

        if(jsonException is null)
            return true;

        logger.Error(jsonException);
        return false;
    }

    public static bool TryReadScoreList([NotNullWhen(true)] out Dictionary<Guid, LevelScore[]>? scores) {
        if(!File.Exists(ScoresPath)) {
            scores = null;
            return false;
        }

        FileStream scoresFile = File.OpenRead(ScoresPath);
        try { scores = JsonSerializer.Deserialize<Dictionary<Guid, LevelScore[]>>(scoresFile); }
        catch(JsonException) { scores = null; }
        scoresFile.Close();

        return scores is not null;
    }
}
