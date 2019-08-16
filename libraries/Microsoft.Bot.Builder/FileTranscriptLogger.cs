﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// FileTranscriptLogger which creates a .transcript file for each conversationId.
    /// </summary>
    /// <remarks>
    /// This is a useful class for unit tests.
    /// </remarks>
    public class FileTranscriptLogger : ITranscriptStore
    {
        private string folder;
        private bool unitTestMode;
        private HashSet<string> started = new HashSet<string>();

        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTranscriptLogger"/> class.
        /// </summary>
        /// <param name="folder">folder to place the transcript files (Default current folder).</param>
        /// <param name="unitTestMode">unitTestMode will overwrite transcript files.</param>
        public FileTranscriptLogger(string folder = null, bool unitTestMode = true)
        {
            if (folder == null)
            {
                folder = Environment.CurrentDirectory;
            }

            folder = PathUtils.NormalizePath(folder);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            this.folder = folder;
            this.unitTestMode = unitTestMode;
        }

        /// <summary>
        /// Log an activity to the transcript.
        /// </summary>
        /// <param name="activity">The activity to transcribe.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task LogActivityAsync(IActivity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(Activity));
            }

            var transcriptFile = GetTranscriptFile(activity.ChannelId, activity.Conversation.Id);

            if (Debugger.IsAttached && activity.Type == ActivityTypes.Message)
            {
                System.Diagnostics.Trace.TraceInformation($"{activity.From.Name ?? activity.From.Id ?? activity.From.Role} [{activity.Type}] {activity.AsMessageActivity()?.Text}");
            }
            else
            {
                System.Diagnostics.Trace.TraceInformation($"{activity.From.Name ?? activity.From.Id ?? activity.From.Role} [{activity.Type}]");
            }

            // try 3 times
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if ((this.unitTestMode == true && !started.Contains(transcriptFile)) || !File.Exists(transcriptFile))
                    {
                        System.Diagnostics.Trace.TraceInformation($"file://{transcriptFile.Replace("\\", "/")}");
                        started.Add(transcriptFile);
                        List<Activity> transcript = new List<Activity>() { (Activity)activity };
                        using (var stream = File.OpenWrite(transcriptFile))
                        {
                            using (var writer = new StreamWriter(stream) as TextWriter)
                            {
                                await writer.WriteAsync($"[{JsonConvert.SerializeObject(activity, jsonSettings)}]").ConfigureAwait(false);
                                return;
                            }
                        }
                    }
                    else
                    {
                        switch (activity.Type)
                        {
                            case ActivityTypes.MessageDelete:
                                await MessageDelete(activity, transcriptFile).ConfigureAwait(false);
                                return;

                            case ActivityTypes.MessageUpdate:
                                await MessageUpdate(activity, transcriptFile).ConfigureAwait(false);
                                return;

                            default:
                                // append
                                await LogActivity(activity, transcriptFile).ConfigureAwait(false);
                                return;
                        }
                    }
                }
                catch (Exception)
                {
                    // try again
                }
            }
        }

        public async Task<PagedResult<IActivity>> GetTranscriptActivitiesAsync(string channelId, string conversationId, string continuationToken = null, DateTimeOffset startDate = default(DateTimeOffset))
        {
            var transcriptFile = GetTranscriptFile(channelId, conversationId);

            var transcript = await LoadTranscript(transcriptFile).ConfigureAwait(false);
            var result = new PagedResult<IActivity>();
            result.ContinuationToken = null;
            result.Items = transcript.Where(activity => activity.Timestamp >= startDate).Cast<IActivity>().ToArray();
            return result;
        }

        public Task<PagedResult<TranscriptInfo>> ListTranscriptsAsync(string channelId, string continuationToken = null)
        {
            List<TranscriptInfo> transcripts = new List<TranscriptInfo>();
            var channelFolder = GetChannelFolder(channelId);

            foreach (var file in Directory.EnumerateFiles(channelFolder, "*.transcript"))
            {
                transcripts.Add(new TranscriptInfo()
                {
                    ChannelId = channelId,
                    Id = Path.GetFileNameWithoutExtension(file),
                    Created = File.GetCreationTime(file),
                });
            }

            return Task.FromResult(new PagedResult<TranscriptInfo>()
            {
                Items = transcripts.ToArray(),
                ContinuationToken = null,
            });
        }

        public Task DeleteTranscriptAsync(string channelId, string conversationId)
        {
            var transcriptFile = GetTranscriptFile(channelId, conversationId);
            File.Delete(transcriptFile);
            return Task.CompletedTask;
        }

        private static async Task<Activity[]> LoadTranscript(string transcriptFile)
        {
            if (File.Exists(transcriptFile))
            {
                using (var stream = File.OpenRead(transcriptFile))
                {
                    using (var reader = new StreamReader(stream) as TextReader)
                    {
                        var json = await reader.ReadToEndAsync().ConfigureAwait(false);
                        return JsonConvert.DeserializeObject<Activity[]>(json);
                    }
                }
            }

            return Array.Empty<Activity>();
        }

        private string GetTranscriptFile(string channelId, string conversationId)
        {
            if (channelId == null)
            {
                throw new ArgumentNullException(channelId);
            }

            if (conversationId == null)
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            var channelFolder = GetChannelFolder(channelId);
            string transcriptFile = Path.Combine(channelFolder, conversationId + ".transcript");
            return transcriptFile;
        }

        private string GetChannelFolder(string channelId)
        {
            if (channelId == null)
            {
                throw new ArgumentNullException(channelId);
            }

            var channelFolder = Path.Combine(folder, channelId);
            if (!Directory.Exists(channelFolder))
            {
                Directory.CreateDirectory(channelFolder);
            }

            return channelFolder;
        }

        private async Task LogActivity(IActivity activity, string transcriptFile)
        {
            var json = $",\n{JsonConvert.SerializeObject(activity, jsonSettings)}]";

            using (var stream = File.Open(transcriptFile, FileMode.OpenOrCreate))
            {
                if (stream.Length > 0)
                {
                    stream.Seek(-1, SeekOrigin.End);
                }

                using (TextWriter writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(json).ConfigureAwait(false);
                }
            }
        }

        private async Task MessageUpdate(IActivity activity, string transcriptFile)
        {
            // load all activities
            var transcript = await LoadTranscript(transcriptFile).ConfigureAwait(false);

            for (int i = 0; i < transcript.Length; i++)
            {
                var originalActivity = transcript[i];
                if (originalActivity.Id == activity.Id)
                {
                    var updatedActivity = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                    updatedActivity.Type = originalActivity.Type; // fixup original type (should be Message)
                    updatedActivity.LocalTimestamp = originalActivity.LocalTimestamp;
                    updatedActivity.Timestamp = originalActivity.Timestamp;
                    transcript[i] = updatedActivity;
                    var json = JsonConvert.SerializeObject(transcript, jsonSettings);
                    using (var stream = File.OpenWrite(transcriptFile))
                    {
                        using (var writer = new StreamWriter(stream) as TextWriter)
                        {
                            await writer.WriteAsync(json).ConfigureAwait(false);
                            return;
                        }
                    }
                }
            }
        }

        private async Task MessageDelete(IActivity activity, string transcriptFile)
        {
            // load all activities
            var transcript = await LoadTranscript(transcriptFile).ConfigureAwait(false);

            // if message delete comes in, delete the message from the transcript
            for (int index = 0; index < transcript.Length; index++)
            {
                var originalActivity = transcript[index];
                if (originalActivity.Id == activity.Id)
                {
                    // tombstone the original message
                    transcript[index] = new Activity()
                    {
                        Type = ActivityTypes.MessageDelete,
                        Id = originalActivity.Id,
                        From = new ChannelAccount(id: "deleted", role: originalActivity.From.Role),
                        Recipient = new ChannelAccount(id: "deleted", role: originalActivity.Recipient.Role),
                        Locale = originalActivity.Locale,
                        LocalTimestamp = originalActivity.Timestamp,
                        Timestamp = originalActivity.Timestamp,
                        ChannelId = originalActivity.ChannelId,
                        Conversation = originalActivity.Conversation,
                        ServiceUrl = originalActivity.ServiceUrl,
                        ReplyToId = originalActivity.ReplyToId,
                    };
                    var json = JsonConvert.SerializeObject(transcript, jsonSettings);
                    using (var stream = File.OpenWrite(transcriptFile))
                    {
                        using (var writer = new StreamWriter(stream) as TextWriter)
                        {
                            await writer.WriteAsync(json).ConfigureAwait(false);
                            return;
                        }
                    }
                }
            }
        }
    }
}
