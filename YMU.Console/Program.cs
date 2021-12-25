using System;
using System.IO;
using System.Threading.Tasks;
using YMU.Api;

/// <summary>
/// This namespace allows uploading a set of videos on different YouTube channels via specifically designed console application and its XML configuration file.
/// </summary>
namespace YMU.Console {
    // run in terminal: dotnet /Users/ytitov/Projects/app-vgen-client-youtube-uploader-console/Frame.Youtube.Uploader.Console.Core/bin/Debug/netcoreapp3.1/Frame.Youtube.Uploader.Console.dll path1 path2 
    /// <summary>
    /// Main class of the console application.
    /// </summary>
    class Program {
        /// <summary>
        /// Console application entry point.
        /// </summary>
        /// <param name="args">It's supports only one argument with a path to XML configuration file.</param>
        /// <returns>Default task reqired by async/await feature.</returns>
        static async Task Main(string[] args) {
            if(args.Length != 1) {
                System.Console.WriteLine($"Please, specify only one argument: path to upload configuration file!");
                return;
            }

            await Upload(args[0]);
        }

        /// <summary>
        /// Method is used to parse XML configuration file and upload set of videos specified in it to the different YouTube channels.
        /// </summary>
        /// <param name="configPath">Path to the XML configuration file.</param>
        /// <returns>Default task required by async/await feature.</returns>
        private static async Task Upload(string configPath) {
            try {
                if(!File.Exists(configPath)) {
                    System.Console.WriteLine($"Configuration file is not exist: '{configPath}'!");
                    return;
                }

                var uploadables = ParseXml.FromFile(configPath);
                if(string.IsNullOrEmpty(uploadables.Application)) {
                    System.Console.WriteLine($"Application name is not specified in XML file: '{configPath}'!");
                    return;
                }
                if(string.IsNullOrEmpty(uploadables.Secret)) {
                    System.Console.WriteLine($"Secret JSON file not specified in XML file: '{configPath}'!");
                    return;
                }
                if(!File.Exists(uploadables.Secret)) {
                    System.Console.WriteLine($"Secret JSON file '{uploadables.Secret}' is not physically exist by specified path!");
                    return;
                }

                foreach(var vn in uploadables.Videos) {
                    try {
                        var channelId = "user";
                        if(!string.IsNullOrEmpty(vn.ChannelId))
                            channelId = vn.ChannelId;

                        if(!File.Exists(vn.File)) {
                            System.Console.WriteLine($"Video file is not exist: '{vn.File}'!");
                            continue;
                        }

                        var video = new Video() {
                            Privacy = vn.Privacy,
                            Category = vn.Category,
                            Description = vn.Description,
                            Tags = vn.Tags,
                            Title = vn.Title,
                            VideoPath = vn.File,
                        };

                        var hasThumbnail = vn.HasThumbnail;
                        if(hasThumbnail && !File.Exists(vn.Thumbnail)) {
                            System.Console.WriteLine($"Thumbnail file is not exist: '{vn.File}'! Video will be uploaded without thumbnail!");
                            hasThumbnail = false;
                        }

                        State result;
                        if(hasThumbnail) {
                            var thumbnail = new Thumbnail() { ThumbnailPath = vn.Thumbnail };
                            result = await Credentials.FromSecret(uploadables.Secret, uploadables.Application, channelId).Authorize().Upload(video).Upload(thumbnail);
                        } else {
                            result = await Credentials.FromSecret(uploadables.Secret, uploadables.Application, channelId).Authorize().Upload(video);
                        }

                        if(result.Status == Statuses.Failed)
                            System.Console.WriteLine($"Upload of video '{vn.File}' is  failed due to error: {result.Error}!");
                    } catch(Exception ex) {
                        System.Console.WriteLine($"Not possible to upload video '{vn.File}' because of an error: {ex.Message}!");
                    }
                }
            } catch(Exception ex) {
                System.Console.WriteLine($"Not possible parse XML file {configPath} because of an error: {ex.Message}!");
            }
        }

        async void VideoUploadExample() {
            var video = new Video() {
                VideoPath = "/Users/Tester/YouTube/Videos/My_Video_1.mp4",
                Title = "Short title",
                Description = "Long description string",
                Tags = new string[] { "tag1", "tag2", "tag3" },
                Category = Categories.Music,
                Privacy = Privacies.Public,
            };

            var thumbnail = new Thumbnail() {
                ThumbnailPath = "/Users/Tester/YouTube/Images/My_Thumbnail_1.png",
            };

            var jsonSecret = "/Users/Tester/YouTube/Secret/secret.json";
            var appName = "Test Video Uploader";
            var channelID = "UChK-iHYx4-4O5ynuzVkUd0g";

            var result = await Credentials.FromSecret(jsonSecret, appName, channelID).Authorize().Upload(video).Upload(thumbnail);
            if(result.Status == Statuses.Failed)
                System.Console.WriteLine($"Upload of video '{video.VideoPath}' is  failed due to error: {result.Error}!");
        }
    }
}