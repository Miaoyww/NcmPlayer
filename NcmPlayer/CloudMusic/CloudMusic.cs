﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Windows.Markup;
using System.Windows.Shapes;

namespace NcmPlayer.CloudMusic
{
    /*
    public static class CloudMusic
    {
        public static System.Timers.Timer timer = new System.Timers.Timer();
        public static string title;
        public static string artist;
        public static string albumId;
        public static string albumPicUrl;
        public static string albumPicPath;
        public static string copiedPath;
        public static string postMes = "已提交指令:";
        public static int pid;
        public static IntPtr hwnd;
        public static IntPtr maindHwnd;
        public static string tempPath = $"{Path.GetTempPath()}alpics\\";

        public static byte[] ReadFile(string path)
        {
            bool _isExist = false;
            if (!File.Exists(path))
            {
                FileStream createSoundsJson = new FileStream(path, FileMode.Create);
                createSoundsJson.Flush();
                createSoundsJson.Close();
                _isExist = false;
            }
            else
            {
                _isExist = true;
            }
            using (FileStream fsSource = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[fsSource.Length];
                int numBytesToRead = (int)fsSource.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    // Read may return anything from 0 to numBytesToRead.
                    int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);
                    // Break when the end of the file is reached.
                    if (n == 0)
                        break;
                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                fsSource.Flush();
                fsSource.Close();
                if (_isExist)
                {
                    return bytes;
                }
                else
                {
                    return null;
                }
            }
        }
        public static string B2S(byte[] bytes)
        {
            string msg = Encoding.UTF8.GetString(bytes);
            if (msg.Equals(""))
            {
                return string.Empty;
            }
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            string sResult = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            byte[] bomBuffer = new byte[] { 0xef, 0xbb, 0xbf };
            if (buffer[0] == bomBuffer[0] && buffer[1] == bomBuffer[1] && buffer[2] == bomBuffer[2])
            {
                int copyLength = buffer.Length - 3;
                byte[] dataNew = new byte[copyLength];
                Buffer.BlockCopy(buffer, 3, dataNew, 0, copyLength);
                sResult = Encoding.UTF8.GetString(dataNew);
            }
            return sResult;
        }

        public static void GetAlumbInfo(Stream stream, string processedTitle, string processedArtists)
        {
            StreamReader objReader = new StreamReader(stream);
            JObject result = (JObject)JsonConvert.DeserializeObject(objReader.ReadLine());

            foreach (JToken token in result["result"]["songs"])
            {
                string targetTitle = string.Empty;
                string targetArtist = string.Empty;
                string soundId = string.Empty;

                targetTitle = token["name"].ToString().Replace(" ", "");

                if (targetTitle.Equals(processedTitle))
                {
                    JToken artists = token["ar"];
                    foreach (JToken artistItem in artists)
                    {
                        targetArtist += artistItem["name"].ToString().Replace(" ", "") + "/";
                    }
                    if (targetArtist.Remove(targetArtist.Length - 1).Equals(processedArtists))
                    {
                        soundId = token["id"].ToString();
                    }
                    if (!soundId.Equals(string.Empty))
                    {
                        albumId = token["al"]["id"].ToString();
                        albumPicUrl = token["al"]["picUrl"].ToString();
                        break;
                    }
                }
            }
        }
        public static void Download2ApplyAlbumPic(string url, string albumid)
        {
            albumPicPath = $"./alpics/{albumId}.jpg";
            if (!Directory.Exists("./alpics"))
            {
                Directory.CreateDirectory("./alpics");
            }

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            if (!File.Exists(albumPicPath))
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = response.GetResponseStream();
                Stream stream = new FileStream(albumPicPath, FileMode.Create);
                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, bArr.Length);
                while (size > 0)
                {
                    stream.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, bArr.Length);
                }
                stream.Close();
                responseStream.Close();
            }
            ApplyAlbumPic(albumid);

            PostInfo.InputLog("已切换封面, 歌曲名称:", title);
        }
        public static void ApplyAlbumPic(string albumid)
        {
            copiedPath = tempPath + $"{albumid}.jpg";
            if (!File.Exists(copiedPath))
            {
                File.Copy(albumPicPath, copiedPath, true);
            }
        }
        public static void GetCover()
        {
            string sURL = $"http://localhost:3000/cloudsearch?keywords={title} -{artist}&limit=10";

            string processedTitle = title.Replace(" ", "");  // 为方便对比, 将所有空格去掉
            string processedArtist = artist.Replace(" ", "");
            byte[] readResultBytes = ReadFile(Res.res.SoundsPath);
            string readResult = B2S(readResultBytes);
            string savingContent = $"{title} -{artist}"; // 用于储存在sounds.json followings中的成员
            JObject result;
            JArray? albumList = new();
            if (readResult == string.Empty)
            {
                JObject? currentSoundInfo = new JObject();
                GetAlumbInfo(HttpGet(sURL), processedTitle, processedArtist);
                currentSoundInfo.Add("albumId", albumId);
                currentSoundInfo.Add("followings", new JArray() { $"{title} -{artist}" });
                albumList.Add(currentSoundInfo);
                Download2ApplyAlbumPic(albumPicUrl, albumId);
            }
            else
            {
                JObject? readResultJObject = JObject.Parse(readResult);
                albumList = (JArray)readResultJObject["sounds"];
                bool _titleIsExist = false;
                foreach (JObject album in albumList)
                {
                    foreach (var sound in album["followings"])
                    {
                        if (sound.ToString().Equals(savingContent))
                        {
                            albumId = album["albumId"].ToString();
                            albumPicPath = $"./alpics/{albumId}.jpg";
                            if (File.Exists(albumPicPath))
                            {
                                ApplyAlbumPic(albumId);
                            }
                            else
                            {
                                GetAlumbInfo(HttpGet(sURL), processedTitle, processedArtist);
                                Download2ApplyAlbumPic(albumPicUrl, albumId);
                            }
                            _titleIsExist = true;
                        }
                    }
                }

                GetAlumbInfo(HttpGet(sURL), processedTitle, processedArtist);
                if (!_titleIsExist)
                {
                    JObject? currentSoundInfo = new JObject();
                    int index = 0;
                    foreach (JObject album in albumList)
                    {
                        if (album["albumId"].ToString().Equals(albumId))
                        {
                            JObject currentSoundAlbum = (JObject)albumList[index];
                            albumId = currentSoundAlbum["albumId"].ToString();
                            albumPicPath = $"./alpics/{albumId}.jpg";
                            JArray soundList = (JArray)currentSoundAlbum["followings"];
                            soundList.Add(savingContent);
                            albumList.RemoveAt(index);
                            albumList.Add(currentSoundAlbum);
                            _titleIsExist = true;
                            ApplyAlbumPic(albumId);
                            break;
                        }
                        index++;
                    }
                    if (!_titleIsExist)
                    {
                        currentSoundInfo.Add("albumId", albumId);
                        currentSoundInfo.Add("followings", new JArray() { savingContent });
                        albumList.Add(currentSoundInfo);
                        Download2ApplyAlbumPic(albumPicUrl, albumId);
                    }
                }
            }
            result = new JObject { { "sounds", albumList } };
            try
            {
                WriteFile(Res.res.SoundsPath, result.ToString());
            }
            catch (IOException)
            {
            }
        }
    }
    */

    public static class HttpRequest
    {
        public static Stream StreamHttpGet(string url)
        {
            WebRequest wrGETURL;
            wrGETURL = WebRequest.Create(url);

            Stream objStream;
            while (true)
            {
                try
                {
                    objStream = wrGETURL.GetResponse().GetResponseStream();
                    StreamReader objReader = new StreamReader(objStream);
                    return objStream;
                }
                catch (WebException)
                {
                    Thread.Sleep(3);
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        public static JObject JObjectHttpGet(Stream stream)
        {
            StreamReader objReader = new StreamReader(stream);
            return (JObject)JsonConvert.DeserializeObject(objReader.ReadLine());
        }

        public static JObject GetJson(string url)
        {
            return JObjectHttpGet(StreamHttpGet(url));
        }
    }

    public static class Tool
    {
        public static DateTime TimestampToDateTime(string timeStamp)
        {
            DateTime dd = DateTime.SpecifyKind(new DateTime(1970, 1, 1, 0, 0, 0, 0), DateTimeKind.Local);
            long longTimeStamp = long.Parse(timeStamp + "0000");
            TimeSpan ts = new TimeSpan(longTimeStamp);
            return dd.Add(ts);
        }
    }

    public static class Apis
    {
        public static string playListDetail(string id)
        {
            return AppConfig.ApiUrl + "/playlist/detail?id=" + id;
        }

        public static string songUrl(string id)
        {
            return AppConfig.ApiUrl + "/song/url?id=" + id;
        }

        public static string songDetail(string id)
        {
            return AppConfig.ApiUrl + "/song/detail?ids=" + id;
        }
    }

    public class CloudMusic
    {
        public string id = string.Empty;
        public string name = string.Empty;
        public Stream? cover;
        public string coverUrl;

        public string Name
        { get { return name; } }

        public Stream Cover
        { get { return cover; } }

    }

    public class PlayList : CloudMusic
    {
        private string description = string.Empty;
        private string[] tags;
        private string[] songIds;
        private DateTime createTime;

        public PlayList(string in_id)
        {
            id = in_id;
            JObject playlistDetail = (JObject)HttpRequest.GetJson(Apis.playListDetail(id))["playlist"];
            name = playlistDetail["name"].ToString();
            description = playlistDetail["description"].ToString();

            JObject jsonTags = (JObject)playlistDetail["tags"];
            tags = new string[jsonTags.Count];
            for (int index = 0; index < tags.Length; index++)
            {
                tags[index] = jsonTags[index].ToString();
            }

            coverUrl = playlistDetail["coverImgUrl"].ToString();
            cover = HttpRequest.StreamHttpGet(coverUrl);

            JObject jsonSongs = (JObject)playlistDetail["trackIds"];
            songIds = new string[jsonSongs.Count];
            for (int index = 0; index < songIds.Length; index++)
            {
                songIds[index] = jsonSongs[index]["id"].ToString();
            }

            string timestampTemp = playlistDetail["createTime"].ToString();
            createTime = Tool.TimestampToDateTime(timestampTemp.Remove(timestampTemp.Length - 2));
        }

        public void InitArtWorkList()
        {
        }
    }

    public class Song : CloudMusic
    {
        private string songUrl;
        private string songType;
        private string[] artists;
        private string albumName;
        private string albumId;

        public Song(string in_id)
        {
            id = in_id;
            JObject songDetail;
            try
            {
                songDetail = (JObject)((JArray)HttpRequest.GetJson(Apis.songDetail(id))["songs"])[0];

            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException($"未能发现此音乐{in_id}");
            }
            name = songDetail["name"].ToString();
            JObject temp = (JObject)HttpRequest.GetJson(Apis.songUrl(id))["data"][0];
            songUrl = temp["url"].ToString();
            songType = temp["type"].ToString();
            JArray artistsJson = (JArray)songDetail["ar"];
            artists = new string[artistsJson.Count];
            for (int index = 0; index < artists.Length; index++)
            {
                artists[index] = artistsJson[index]["name"].ToString();
            }

            albumId = songDetail["al"]["id"].ToString();
            albumName = songDetail["al"]["name"].ToString();
            coverUrl = songDetail["al"]["picUrl"].ToString();
            cover = HttpRequest.StreamHttpGet(coverUrl);
        }

        public string[] Artists
        { get { return artists; } }

        public string AlbumName
        { get { return albumName; } }

        public string AlbumId
        { get { return albumId; } }

        public string GetFile()
        {
            string path = AppConfig.SongsPath(id, songType);
            if (!File.Exists(path))
            {
                if (!Directory.Exists(AppConfig.SongsDirectory))
                {
                    Directory.CreateDirectory(AppConfig.SongsDirectory);
                }
                FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                Stream songStream = HttpRequest.StreamHttpGet(songUrl);
                byte[] bArr = new byte[1024];
                int size = songStream.Read(bArr, 0, bArr.Length);
                while (size > 0)
                {
                    //stream.Write(bArr, 0, size);
                    fs.Write(bArr, 0, size);
                    size = songStream.Read(bArr, 0, bArr.Length);
                }
                fs.Close();
                songStream.Close();

            }
            return path;
        }
    }

}