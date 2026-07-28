using System;
using System.Collections.Generic;
using System.IO;
using BallKnowledge.MatchEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BallKnowledge.Greybox
{
    public static class GreyboxConfigLoader
    {
        private static readonly JsonSerializerSettings EngineLoaderSettings = new JsonSerializerSettings
        {
            ContractResolver = new EngineJsonContractResolver(),
            Converters = { new StringEnumConverter() },
        };

        public static LoadedConfig LoadFrom(string streamingAssetsDirectory)
        {
            if (string.IsNullOrWhiteSpace(streamingAssetsDirectory))
            {
                throw new ArgumentException("StreamingAssets directory must be provided.", nameof(streamingAssetsDirectory));
            }

            var constantsPath = Path.Combine(streamingAssetsDirectory, "constants.json");
            var teamsPath = Path.Combine(streamingAssetsDirectory, "teams.json");
            var greyboxPath = Path.Combine(streamingAssetsDirectory, "greybox.json");

            var engineConfig = DeserializeEngine<EngineConfig>(constantsPath);
            var teams = DeserializeEngine<List<TeamDefinition>>(teamsPath);
            var greybox = DeserializeGreybox(greyboxPath);

            return new LoadedConfig(engineConfig, teams, greybox);
        }

        private static T DeserializeEngine<T>(string path) where T : class
        {
            EnsureFileExists(path);

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json, EngineLoaderSettings)
                   ?? throw new InvalidDataException("Failed to deserialize engine config file '" + path + "'.");
        }

        private static GreyboxSettings DeserializeGreybox(string path)
        {
            EnsureFileExists(path);

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<GreyboxSettings>(json)
                   ?? throw new InvalidDataException("Failed to deserialize greybox config file '" + path + "'.");
        }

        private static void EnsureFileExists(string path)
        {
            if (File.Exists(path))
            {
                return;
            }

            throw new MissingStreamingAssetFileException(path);
        }
    }

    public sealed class LoadedConfig
    {
        public LoadedConfig(
            EngineConfig engineConfig,
            IReadOnlyList<TeamDefinition> teams,
            GreyboxSettings greybox)
        {
            EngineConfig = engineConfig ?? throw new ArgumentNullException(nameof(engineConfig));
            Teams = teams ?? throw new ArgumentNullException(nameof(teams));
            Greybox = greybox ?? throw new ArgumentNullException(nameof(greybox));
        }

        public EngineConfig EngineConfig { get; private set; }

        public IReadOnlyList<TeamDefinition> Teams { get; private set; }

        public GreyboxSettings Greybox { get; private set; }
    }

    public sealed class MissingStreamingAssetFileException : FileNotFoundException
    {
        public MissingStreamingAssetFileException(string path)
            : base("Required StreamingAssets file is missing: " + path, path)
        {
        }
    }
}
