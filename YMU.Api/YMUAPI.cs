﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.Upload;
using YTVideo = Google.Apis.YouTube.v3.Data.Video;
using YTSnippet = Google.Apis.YouTube.v3.Data.VideoSnippet;
using YTStatus = Google.Apis.YouTube.v3.Data.VideoStatus;
using System.Text;

/// <summary>
/// This namespace contains implementation of YoutubeUploader which could be used to upload Video and(or) Thumbnail on the specified YouTube channel.
/// For more information on how this class should be used, please, check my YouTube video: 
/// </summary>
namespace YMU.Api {

    /// <summary>
    /// List of categories which could be used to upload video on YouTube.
    /// It was generated via YouTube API available by the following URL: https://developers.google.com/youtube/v3/docs/videoCategories/list?apix_params=%7B%22part%22%3A%5B%22snippet%22%5D%2C%22regionCode%22%3A%22US%22%7D
    /// </summary>
    public enum Categories {
        FilmAndAnimation = 1,
        AutoAndVehicles = 2,
        Music = 10,
        PetsAndAnimals = 15,
        Sports = 17,
        ShortMovies = 18,
        TravelAndEvents = 19,
        Gaming = 20,
        VideoBlogging = 21,
        PeopleAndBlogs = 22,
        Comedy = 23,
        Entertainment = 24,
        NewsAndPolitics = 25,
        HowtoAndStyle = 26,
        Education = 27,
        ScienceAndTechnology = 28,
        NonprofitsAndActivism = 29,
        Movies = 30,
        Anime = 31,
        ActionAndAdventure = 32,
        Classics = 33,
        //Comedy = 34, - not possible to assign video to this category. More details: https://developers.google.com/youtube/v3/docs/videoCategories#resource
        Documentary = 35,
        Drama = 36,
        Family = 37,
        Foreign = 38,
        Horror = 39,
        SciFiAndFantasy = 40,
        Thriller = 41,
        Shorts = 42,
        Shows = 43,
        Trailers = 44,
    }

    /// <summary>
    /// Determine accessibility of the video after uploading.
    /// </summary>
    public enum Privacies {
        /// <summary>
        /// Video will be visible by everyone who visit your channel. 
        /// </summary>
        Public,
        /// <summary>
        /// Video will be available only by the link.
        /// </summary>
        Unlisted,
        /// <summary>
        /// Video will be available only to your account via YouTube Studio (or users with corresponded credentials).
        /// </summary>
        Private,
    }

    /// <summary>
    /// Use this class to specify your authorization credentials.
    /// </summary>
    public class Credentials {
        private Credentials() { }

        /// <summary>
        /// Name of your client application.
        /// </summary>
        public string ApplicationName { get; private set; } = string.Empty;

        /// <summary>
        /// Path to the JSON file with client ID and secret.
        /// This file must be generated by Google Cloud Console: https://console.cloud.google.com/
        /// If 'SecretsPath' is not set the following properties must be provided: 'ClientID', 'ClientSecret'.
        /// </summary>
        public string SecretsPath { get; private set; } = string.Empty;

        /// <summary>
        /// Contains text from JSON secret file with client ID and secret.
        /// This file must be generated by Google Cloud Console: https://console.cloud.google.com/
        /// If 'SecretsPath' is not set the following properties must be provided: 'ClientID', 'ClientSecret'.
        /// </summary>
        public string SecretJSON { get; private set; } = string.Empty;

        /// <summary>
        /// Identifier of the target channel where video will be uploaded.
        /// This property is optional.
        /// </summary>
        public string ChennelID { get; private set; } = "user";

        /// <summary>
        /// Contains Client Identifier, which must be created by Google Cloud Console (https://console.cloud.google.com/) or could be found in JSON file with client ID and secret.  
        /// This property must be set if property 'SecretsPath' is not used.
        /// </summary>
        public string ClientID { get; private set; }
        /// <summary>
        /// Contains Client Secret, which must be generated by Google Cloud Console (https://console.cloud.google.com/) or could be found in JSON file with client ID and secret.  
        /// This property must be set if property 'SecretsPath' is not used.
        /// </summary>
        public string ClientSecret { get; private set; }

        /// <summary>
        /// Use this property to provide specific folder where 'GoogleWebAuthorizationBroker' will store Authorization data.
        /// </summary>
        public string AuthorizationDataStorePath { get; private set; }

        /// <summary>
        /// Use this method to generate authorization credentials basing on JSON secret file with client ID and secret.
        /// </summary>
        /// <param name="secretPath">Path to the file with JSON file with client ID and secret.</param>
        /// <param name="applicationName">Name of your client application. This parameter is optional.</param>
        /// <param name="chennelID">Identifier of the target channel where video will be uploaded. This parameter is optional.</param>
        /// <param name="authorizationDataStorePath">Use this parameter to provide specific folder where 'GoogleWebAuthorizationBroker' will store Authorization data. This parameter is optional.</param>
        /// <returns>Instance with provided credentials.</returns>
        public static Credentials FromSecret(string secretPath, string applicationName, string chennelID = "user", string authorizationDataStorePath = null) {
            return new Credentials() { ApplicationName = applicationName, SecretsPath = secretPath, ChennelID = chennelID, AuthorizationDataStorePath = authorizationDataStorePath ?? typeof(YoutubeUploader).ToString() };
        }

        /// <summary>
        /// Use this method to generate authorization credentials basing on text from JSON secret file with client ID and secret.
        /// </summary>
        /// <param name="secretJSON">Contains text from JSON secret file with client ID and secret.</param>
        /// <param name="applicationName">Name of your client application. This parameter is optional.</param>
        /// <param name="chennelID">Identifier of the target channel where video will be uploaded. This parameter is optional.</param>
        /// <param name="authorizationDataStorePath">Use this parameter to provide specific folder where 'GoogleWebAuthorizationBroker' will store Authorization data. This parameter is optional.</param>
        /// <returns>Instance with provided credentials.</returns>
        public static Credentials FromJSON(string secretJSON, string applicationName, string chennelID = "user", string authorizationDataStorePath = null) {
            return new Credentials() { ApplicationName = applicationName, SecretJSON = secretJSON, ChennelID = chennelID, AuthorizationDataStorePath = authorizationDataStorePath ?? typeof(YoutubeUploader).ToString() };
        }

        /// <summary>
        /// Use this method to generate authorization credentials basing on provided client ID and secret.
        /// </summary>
        /// <param name="clientID">Contains Client Identifier, which must be created by Google Cloud Console (https://console.cloud.google.com/) or could be found in JSON file with client ID and secret.</param>
        /// <param name="clientSecret">Contains Client Secret, which must be generated by Google Cloud Console (https://console.cloud.google.com/) or could be found in JSON file with client ID and secret. </param>
        /// <param name="applicationName">Name of your client application. This parameter is optional.</param>
        /// <param name="chennelID">Identifier of the target channel where video will be uploaded.</param>
        /// <param name="authorizationDataStorePath">Use this parameter to provide specific folder where 'GoogleWebAuthorizationBroker' will store Authorization data. This parameter is optional.</param>
        /// <returns>Instance with provided credentials.</returns>
        public static Credentials FromIdentifiers(string clientID, string clientSecret, string applicationName, string chennelID = "user", string authorizationDataStorePath = null) {
            return new Credentials() { ApplicationName = applicationName, ClientID = clientID, ClientSecret = clientSecret, ChennelID = chennelID, AuthorizationDataStorePath = authorizationDataStorePath ?? typeof(YoutubeUploader).ToString() };
        }
    }

    /// <summary>
    /// This class contains information which is required to upload video.
    /// </summary>
    public class Video {
        /// <summary>
        /// Path to the video file.
        /// </summary>
        public string VideoPath;
        /// <summary>
        /// Title of the video.
        /// </summary>
        public string Title;
        /// <summary>
        /// Description of the video.
        /// </summary>
        public string Description;
        /// <summary>
        /// List video's tags.
        /// </summary>
        public string[] Tags;
        /// <summary>
        /// Category of the video.
        /// </summary>
        public Categories Category;
        /// <summary>
        /// Determine accessibility of the video after uploading.
        /// This property is optional. By default, video will be public.
        /// </summary>
        public Privacies Privacy = Privacies.Public;
    }

    /// <summary>
    /// This class contains information which is required to upload thumbnail.
    /// </summary>
    public class Thumbnail {
        /// <summary>
        /// Identifier of the video where thumbnail must be set.
        /// </summary>
        public string VideoID;
        /// <summary>
        /// Path to the image file which will be used as thumbnail.
        /// </summary>
        public string ThumbnailPath;
    }

    /// <summary>
    /// Represents status of the operation.
    /// </summary>
    public enum Statuses {
        /// <summary>
        /// Operation is succeeded.
        /// </summary>
        Succeed,
        /// <summary>
        /// Operation is failed.
        /// </summary>
        Failed,
    }

    /// <summary>
    /// Contains system information which is required to perform operations like authorization and content uploading such as video and thumbnail.
    /// </summary>
    public class State {
        /// <summary>
        /// Create state with 'YouTubeService' instance.
        /// </summary>
        /// <param name="service">'YouTubeService' instance.</param>
        public State(YouTubeService service) {
            Status = Statuses.Succeed;
            Service = service;
        }

        /// <summary>
        /// Create state with 'YouTubeService' instance and video identifier.
        /// </summary>
        /// <param name="service">'YouTubeService' instance.</param>
        /// <param name="videoID">Video identifier</param>
        public State(YouTubeService service, string videoID) {
            Status = Statuses.Succeed;
            Service = service;
            VideoID = videoID;
        }

        /// <summary>
        /// Create state with error message.
        /// </summary>
        /// <param name="sourceState">Contains useful data (such as YouTubeService and VideoID) which must be passed to the next state.</param>
        /// <param name="errorMessage">Error message.</param>
        public State(State sourceState, string errorMessage) {
            Service = sourceState.Service;
            VideoID = sourceState.VideoID;
            Status = Statuses.Failed;
            Error = errorMessage;
        }

        /// <summary>
        /// Create state with exception's message.
        /// </summary>
        /// <param name="sourceState">Contains useful data (such as YouTubeService and VideoID) which must be passed to the next state.</param>
        /// <param name="errorMessage">Exception instance.</param>
        public State(State sourceState, Exception exception) {
            if(sourceState == null) {
                Service = null;
                VideoID = string.Empty;
            } else {
                Service = sourceState.Service;
                VideoID = sourceState.VideoID ?? string.Empty;
            }
            Status = Statuses.Failed;
            Error = exception.ToString();
        }

        /// <summary>
        /// Contains instance of 'YouTubeService'.
        /// Will be available after authorization.
        /// </summary>
        public YouTubeService Service { get; private set; }
        public string VideoID { get; private set; } = string.Empty;

        /// <summary>
        /// Contains the status of the operation: was it succeeded or not.
        /// </summary>
        public Statuses Status { get; private set; }

        /// <summary>
        /// Contains the error message with the reason of operation failure.
        /// </summary>
        public string Error { get; private set; } = string.Empty;
    }

    /// <summary>
    /// Main class which could be used to perform basic operations like authorization and content uploading such as video and thumbnail.
    /// </summary>
    public static class YoutubeUploader {
        /// <summary>
        /// This operation performs authorization in YouTube API.
        /// Call this method before other ones.
        /// </summary>
        /// <param name="credentials">Instance of your authorization credentials.</param>
        /// <returns>Authorization state.</returns>
        public static async Task<State> Authorize(this Credentials credentials) {
            try {
                ClientSecrets secrets;
                if(!string.IsNullOrEmpty(credentials.SecretsPath)) {
                    using var stream = new FileStream(credentials.SecretsPath, FileMode.Open, FileAccess.Read);
                    secrets = GoogleClientSecrets.FromStream(stream).Secrets;
                } else if(!string.IsNullOrEmpty(credentials.SecretJSON)) {
                    var bytes = Encoding.Unicode.GetBytes(credentials.SecretJSON);
                    using var stream = new MemoryStream(bytes);
                    secrets = GoogleClientSecrets.FromStream(stream).Secrets;
                } else if(!string.IsNullOrEmpty(credentials.ClientID) && !string.IsNullOrEmpty(credentials.ClientSecret)) {
                    secrets = new ClientSecrets() {
                        ClientId = credentials.ClientID,
                        ClientSecret = credentials.ClientSecret,
                    };
                } else {
                    return new State((State)null, "Secret is not found! Please, provide JSON, or path to the secret file, or Client ID and Client Secret values!");
                }

                var youtubeCredentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets,
                    // This OAuth 2.0 access scope allows for full read/write access to the
                    // authenticated user's account.
                    new[] {
                            //YouTubeService.Scope.Youtube,
                            YouTubeService.Scope.YoutubeUpload,
                            //YouTubeService.Scope.YoutubeForceSsl,
                            //YouTubeService.Scope.Youtubepartner,
                            //YouTubeService.Scope.YoutubeChannelMembershipsCreator,
                            //YouTubeService.Scope.YoutubepartnerChannelAudit,
                            //YouTubeService.Scope.YoutubeReadonly,
                    },
                    credentials.ChennelID,
                    CancellationToken.None,
                    new FileDataStore(credentials.AuthorizationDataStorePath)
                );

                var service = new YouTubeService(new BaseClientService.Initializer() {
                    HttpClientInitializer = youtubeCredentials,
                    ApplicationName = credentials.ApplicationName,
                });

                return new State(service);
            } catch(Exception e) {
                return new State(null, e);
            }
        }

        /// <summary>
        /// This operation performs Video upload via YouTube API.
        /// Call this method after authorization.
        /// </summary>
        /// <param name="stateTask">Task with authorization state.</param>
        /// <param name="video">Instance with all required information to upload video.</param>
        /// <returns>Video uploading state.</returns>
        public static async Task<State> Upload(this Task<State> stateTask, Video video) {
            State state = null;
            try {
                return await Upload(state = await stateTask, video);
            } catch(Exception e) {
                return new State(state, e);
            }
        }

        /// <summary>
        /// This operation performs Video upload via YouTube API.
        /// Call this method after authorization.
        /// </summary>
        /// <param name="stateTask">Aauthorization state.</param>
        /// <param name="video">Instance with all required information to upload video.</param>
        /// <returns>Video uploading state.</returns>
        public static async Task<State> Upload(this State state, Video video) {
            try {
                if(state.Service == null)
                    return new State(state, "'YouTubeService' is not available. Please, perform Authorization first.");

                var youtubeVideo = new YTVideo {
                    Snippet = new YTSnippet {
                        Title = video.Title,
                        Description = video.Description,
                        CategoryId = ((int)video.Category).ToString(), // categoryId; // See https://developers.google.com/youtube/v3/docs/videoCategories/list
                        Tags = video.Tags,
                    },
                    Status = new YTStatus() {
                        PrivacyStatus = video.Privacy.ToString().ToLower(), // "private" or "public" or "unlisted"
                    },
                };

                var videoId = string.Empty;
                var uploadStatus = Statuses.Failed;
                Exception exception = null;
                using(var fileStream = new FileStream(video.VideoPath, FileMode.Open)) {
                    var videosInsertRequest = state.Service.Videos.Insert(youtubeVideo, "snippet,status", fileStream, "video/*");
                    videosInsertRequest.ProgressChanged += (IUploadProgress progress) => {
                        if(progress.Status == UploadStatus.Completed) {
                            uploadStatus = Statuses.Succeed;
                        } else if(progress.Status == UploadStatus.Failed) {
                            uploadStatus = Statuses.Failed;
                            exception = progress.Exception;
                        }
                    };
                    videosInsertRequest.ResponseReceived += (YTVideo video) => {
                        videoId = video.Id;
                    };
                    await videosInsertRequest.UploadAsync();
                }
                return uploadStatus == Statuses.Succeed
                    ? new State(state.Service, videoId)
                    : new State(state, exception != null ? exception.Message : "Video upload failed!");
            } catch(Exception e) {
                return new State(state, e);
            }
        }

        /// <summary>
        /// This operation performs Thumbnail upload via YouTube API.
        /// Call this method after authorization.
        /// </summary>
        /// <param name="stateTask">Task with authorization or video upload state.</param>
        /// <param name="thumbnail">Instance with all required information to upload thumbnail.</param>
        /// <returns>Thumbnail uploading state.</returns>
        public static async Task<State> Upload(this Task<State> stateTask, Thumbnail thumbnail) {
            State state = null;
            try {
                return await Upload(state = await stateTask, thumbnail);
            } catch(Exception e) {
                return new State(state, e);
            }
        }

        /// <summary>
        /// This operation performs Thumbnail upload via YouTube API.
        /// Call this method after authorization or after video upload.
        /// </summary>
        /// <param name="state">Instance with authorization or video upload state.</param>
        /// <param name="thumbnail">Instance with all required information to upload thumbnail.</param>
        /// <returns>Thumbnail uploading state.</returns>
        public static async Task<State> Upload(this State state, Thumbnail thumbnail) {
            try {
                if(state.Service == null)
                    return new State(state, "'YouTubeService' is not available. Please, perform Authorization first.");

                var videoID = string.Empty;
                if(!string.IsNullOrEmpty(thumbnail.VideoID)) {
                    videoID = thumbnail.VideoID;
                } else if(!string.IsNullOrEmpty(state.VideoID)) {
                    videoID = state.VideoID;
                } else {
                    return new State(state, "Not possible to upload thumbnail because VideoID was not provided!");
                }
                var uploadStatus = Statuses.Failed;
                Exception exception = null;
                using(var fileStream = new FileStream(thumbnail.ThumbnailPath, FileMode.Open)) {
                    var thumbnailSetRequest = state.Service.Thumbnails.Set(videoID, fileStream, string.Empty);
                    thumbnailSetRequest.ProgressChanged += (IUploadProgress progress) => {
                        if(progress.Status == UploadStatus.Completed) {
                            uploadStatus = Statuses.Succeed;
                        } else if(progress.Status == UploadStatus.Failed) {
                            uploadStatus = Statuses.Failed;
                            exception = progress.Exception;
                        }
                    };
                    await thumbnailSetRequest.UploadAsync();
                }
                return uploadStatus == Statuses.Succeed
                    ? new State(state.Service)
                    : new State(state, exception != null ? exception.Message : "Thumbnail upload failed!");
            } catch(Exception e) {
                return new State(state, e);
            }
        }
    }
}