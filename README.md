# YouTube Mass Uploader (YMU)
## Introduction
YMU is a YouTube API V3 wrapper created in C#. It allows to upload video with title, description, categoty, tags and thumbnail to the YouTube chanel specified by its ID. It has simple LINQ like sintax based on extension methods and its aynchronous by its nature. You can use it to upload single or several videos at once. 
## Limitations
To use this library you will have to create your own Upload Application in Google Cloud. I created tutorial video with the description of whole process.
Amount of videos which you can upload (or try to upload) during day is limited, so, you must request additional quotas to unleash the full potential of YMU. 
Right now size of thumnail is also limited via YouTube to 2 MB. Size of title, description and tags also has limitations. YMU doesnt perform checks of those limitations, so, you have to deal with them by yourself.
## Console Application
Console application is available by the followng link [YMU v1.0](https://drive.google.com/file/d/1UGohzl4tmyR1mUI8Cva-ijrBD0h7-wjA/view?usp=sharing).
If you want to use it in your project - donwload it and unpack in suitable folder. This application could be used to upload videos with thumbnails to different YouTube chanels. It uses only one argument - path to the XML configuration.
Here is a template XML document which you can use to create of your own:
```XML
<?xml version="1.0" encoding="utf-16" ?>
<upload application="APP_NAME" secret="SECRET_PATH">
    <video file="VIDEO_PATH" privacy="PRIVACY" category="CATEGORY">
        <channel id="CHANNEL_ID"/>
        <title>TITLE</title>
        <description>
DESCRIPTION
        </description>
        <thumbnail file="THUMBNAIL_PATH"/>         
        <tags>TAGS</tags>
    </video>
</upload>
```
Explanation:
- APP_NAME - name of the application registered in Google Cloud and permission to upload videos on YouTube;
- SECRET_PATH - path to the scret JSON file which could be downloaded form your Application Page on Google Cloud;
- VIDEO_PATH - full path to the video file;
- PRIVACY - one of the following supported prvicy modes:
  - Public - video will be visible to everyone;
  - Unlisted - video will be accessible via URL;
  - Private - video will be accessible only by you;
- CATEGORY - one of the following supported categories: FilmAndAnimation, AutoAndVehicles, Music, PetsAndAnimals, Sports, ShortMovies, TravelAndEvents, Gaming, VideoBlogging, PeopleAndBlogs, Comedy, Entertainment, NewsAndPolitics, HowtoAndStyle, Education, ScienceAndTechnology, NonprofitsAndActivism, Movies, Anime, ActionAndAdventure, Classics, Documentary, Drama, Family, Foreign, Horror, SciFiAndFantasy, Thriller, Shorts, Shows, Trailers.
- CHANNEL_ID - channel ID which could be esilly obtained from the channel URL. Here is how it looks: ***UCy6Py3BjosZP7LjpZJO4gqA***.
- TITLE - title of the video. It could contain only 100 symbols;
- DESCRIPTION - description of the video. It could contain only 5000 symbols;
- THUMBNAIL_PATH - full path to the thumbnail image file. It must not be larger then 2 MB;
- TAGS - comma separted tags. It could contain only 500 symbols inclusing commas;

Here is an example of XML configuration:
```XML
<?xml version="1.0" encoding="utf-16" ?>
<upload application="Test Video Uploader" secret="/Users/Tester/YouTube/Secret/secret.json">
    <video file="/Users/Tester/YouTube/Videos/My_Video_1.mp4" privacy="Public" category="Entertainment">
        <channel id="UChK-iHYx4-4O5ynuzVkUd0g"/>
        <title>Really good video #1!</title>
        <description>
Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce convallis eros nunc, vel molestie tortor blandit sed. Vestibulum eget aliquet odio. Integer eu volutpat lacus, vel finibus leo. Mauris non vehicula purus.  
        </description>
        <thumbnail file="/Users/Tester/YouTube/Images/My_Thumbnail_1.png"/>         
        <tags>lorem,ipsum,dolor,sit,amet consectetur,adipiscing elit,fusce convallis eros,nunc vel molestie tortor, blandit sed vestibulum</tags>
    </video>
    <video file="/Users/Tester/YouTube/Videos/My_Video_2.mp4" privacy="Public" category="Entertainment">
        <channel id="UChK-iHYx4-4O5ynuzVkUd0g"/>
        <title>Really good video #2!</title>
        <description>
Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce convallis eros nunc, vel molestie tortor blandit sed. Vestibulum eget aliquet odio. Integer eu volutpat lacus, vel finibus leo. Mauris non vehicula purus.  
        </description>
        <thumbnail file="/Users/Tester/YouTube/Images/My_Thumbnail_2.png"/>         
        <tags>lorem,ipsum,dolor,sit,amet consectetur,adipiscing elit,fusce convallis eros,nunc vel molestie tortor, blandit sed vestibulum</tags>
    </video>  
</upload>
```
This configuration used to upload two videos ***My_Video_1.mp4*** and ***My_Video_2.mp4*** with thumbnails ***My_Thumbnail_1.png*** and ***My_Thumbnail_2.png*** on YouTube channel with following ID: '***UChK-iHYx4-4O5ynuzVkUd0g***'. 
## Upload video
To upload video (or videos) you must execute console aplication in the Terminal and specify path to the configuration file as first paramter. Here is na example:
```
dotnet /Users/Tester/YMU/YMU.Console.dll /User/Tester/YouTube/My_YMU_Configuration.xml 
```
When the upload started YMU could open web browser and ask you to authorize to your channels. This procedure initiated by YouTube API and couldn't be skipped.

## Code Structure
Solution consists of three projects:
- YMU.Core - contains simple XML deserealizer implemented by me;
- YMU.Console - contains console application logic;
- YMU.API - the API itself. It contains only one file '**YMUAPI.cs**' which you can download and integrate into your project. Please, also make sure to add following Nuget Package to your project: '***Google.Apis.YouTube.v3***' and all of its dependencies.

## Code Examples
Here is an example how you can upload single video and its thumbnail via code:
```C#
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
```
## Nuget Package
You can include YMU.API as a Nuget Package - here is [link](https://www.nuget.org/packages/YouTube.Mass.Uploader/) to it.
# Сonclusion
I'm would be happy if YMU will make your life easier. Please, ping me if you have a questions, or you decided to use it in your project: [yulian.titov@gmail.com](yulian.titov@gmail.com). 
