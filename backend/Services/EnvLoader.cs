namespace SkillGraph.Api.Services;

/// <summary>
/// Minimal .env loader so Windows users don't need export / setx.
/// Looks for .env next to the executable, then walks up to the repo root.
/// </summary>
public static class EnvLoader
{
    public static void Load(params string[] searchPaths)
    {
        foreach (var path in searchPaths)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) continue;
            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith('#')) continue;

                // Support both KEY=value and export KEY=value
                if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
                    line = line[7..].Trim();

                var eq = line.IndexOf('=');
                if (eq <= 0) continue;

                var key = line[..eq].Trim();
                var value = line[(eq + 1)..].Trim().Trim('"', '\'');

                // Don't overwrite vars already set in the real environment
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                    Environment.SetEnvironmentVariable(key, value);
            }
            return; // first file found wins
        }
    }
}
