using System;
using System.Windows.Forms;
using MusicBeePlugin.Ampache;
using System.IO;
using System.Xml.Serialization;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private MusicBeeApiInterface mbApiInterface;
        private PluginInfo about = new PluginInfo();

        private const string DefaultServer = "ampache.example.com";
        private const string ConfigFileName = "mb_ampache.cfg";

        private Settings CurrentSettings { get; set; }
        private SettingsControl SettingsControl { get; set; }

        private AmpacheClient Ampache { get; set; }

        private Exception LastException { get; set; }

        public Plugin()
        {
            SettingsControl = new SettingsControl();
        }

        #region MusicBee Plugin Infrastructure

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);

            about.PluginInfoVersion = PluginInfoVersion;
            about.Name = "MB_Ampache";
            about.Description = "Access your Ampache music library within MusicBee";
            about.Author = "Sam Shores";
            about.TargetApplication = "Ampache";
            about.Type = PluginType.Storage;
            about.VersionMajor = 0;
            about.VersionMinor = 0;
            about.Revision = 1;
            about.MinInterfaceVersion = MinInterfaceVersion;
            about.MinApiRevision = MinApiRevision;
            about.ReceiveNotifications = ReceiveNotificationFlags.StartupOnly;
            about.ConfigurationPanelHeight = TextRenderer.MeasureText("FirstRowText", SystemFonts.DefaultFont).Height * 12;   // height in pixels that musicbee should reserve in a panel for config settings. When set, a handle to an empty panel will be passed to the Configure function
            return about;
        }

        public bool Configure(IntPtr panelHandle)
        {
            // panelHandle will only be set if you set about.ConfigurationPanelHeight to a non-zero value
            // keep in mind the panel width is scaled according to the font the user has selected
            // if about.ConfigurationPanelHeight is set to 0, you can display your own popup window
            if (panelHandle != IntPtr.Zero)
            {
                Panel configPanel = (Panel)Control.FromHandle(panelHandle);

                SettingsControl = new SettingsControl();

                SettingsControl.Protocol = CurrentSettings.Protocol;
                SettingsControl.Server = CurrentSettings.Server;
                SettingsControl.Port = CurrentSettings.Port;
                SettingsControl.Username = CurrentSettings.Username;
                SettingsControl.Password = "";

                configPanel.Controls.Add(SettingsControl);
            }

            return false;
        }

        public void LoadSettings()
        {
            // save any persistent settings in a sub-folder of this path
            var dataPath = mbApiInterface.Setting_GetPersistentStoragePath();

            var configFile = Path.Combine(dataPath, ConfigFileName);

            if (!File.Exists(configFile))
            {
                CurrentSettings = new Settings
                {
                    Protocol = Protocol.HTTP,
                    Server = DefaultServer,
                    Port = 80,
                    Username = "username",
                    PasswordHash = "password"
                };
            }
            else
            {
                using (var stream = File.OpenRead(configFile))
                {
                    try
                    {
                        var serializer = new XmlSerializer(typeof(Settings));

                        CurrentSettings = (Settings)serializer.Deserialize(stream);
                    }
                    catch(Exception e)
                    {
                        var errText = "Could not load MB_Ampache settings file, using defaults." + Environment.NewLine + Environment.NewLine + "Details: " + e.Message;
                        MessageBox.Show(errText, "MB_Ampache Plugin Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);

                        CurrentSettings = new Settings
                        {
                            Protocol = Protocol.HTTP,
                            Server = DefaultServer,
                            Port = 80,
                            Username = "username",
                            PasswordHash = "password"
                        };
                    }
                }

                Ampache = new AmpacheClient(CurrentSettings.MakeUrl(), CurrentSettings.Username, CurrentSettings.PasswordHash);
            }
        }

        // called by MusicBee when the user clicks Apply or Save in the MusicBee Preferences screen.
        // its up to you to figure out whether anything has changed and needs updating
        public void SaveSettings()
        {
            var newSettings = new Settings
            {
                Protocol = SettingsControl.Protocol,
                Server = SettingsControl.Server,
                Port = SettingsControl.Port,
                Username = SettingsControl.Username,
                PasswordHash = AmpacheClient.PreHash(SettingsControl.Password)
        };

            if (newSettings == CurrentSettings)
                return;

            StopApi();

            // save any persistent settings in a sub-folder of this path
            var dataPath = mbApiInterface.Setting_GetPersistentStoragePath();

            var configFile = Path.Combine(dataPath, ConfigFileName);

            using(var stream = File.OpenWrite(configFile))
            {
                var serializer = new XmlSerializer(typeof(Settings));

                serializer.Serialize(stream, newSettings);
            }

            CurrentSettings = newSettings;

            StartApi();
        }

        private void StartApi()
        {
            if (CurrentSettings.Server != DefaultServer)
            {
                Ampache = new AmpacheClient(CurrentSettings.MakeUrl(), CurrentSettings.Username, CurrentSettings.PasswordHash);

                Ampache.Connect();
            }
            else
                Ampache = null;
        }
        private void StopApi()
        {
            Ampache?.Disconnect();
        }

        // MusicBee is closing the plugin (plugin is being disabled by user or MusicBee is shutting down)
        public void Close(PluginCloseReason reason)
        {
            StopApi();
        }

        // uninstall this plugin - clean up any persisted files
        public void Uninstall()
        {
            var dataPath = mbApiInterface.Setting_GetPersistentStoragePath();

            var configFile = Path.Combine(dataPath, ConfigFileName);

            if (File.Exists(configFile))
                File.Delete(configFile);
        }

        // receive event notifications from MusicBee
        // you need to set about.ReceiveNotificationFlags = PlayerEvents to receive all notifications, and not just the startup event
        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            // perform some action depending on the notification type
            if (type == NotificationType.PluginStartup)
            {
                try
                {
                    LoadSettings();

                    StartApi();

                    mbApiInterface.MB_SendNotification(CallbackType.StorageReady);
                }
                catch(Exception)
                {
                    mbApiInterface.MB_SendNotification(CallbackType.StorageFailed);
                }
            }
        }

        #endregion

        #region Storage Plugin API

        public bool IsReady()
        {
            return Ampache?.IsConnected ?? false;
        }

        public Bitmap GetIcon()
        {
            try
            {
                // TODO nav icon
                // return an 16x16 icon for the main navigation node
                throw new NotImplementedException();
            }
            catch(Exception e)
            {
                LastException = e;
                return null;
            }
        }

        public void Refresh()
        {
            // refresh any cached files - called when the user presses F5 or does some operation that requires a refresh of data.
        }

        public string[] GetFolders(string path)
        {
            try
            {
                if (path.Equals("Ampache", StringComparison.InvariantCultureIgnoreCase))
                    return new[]
                    {
                        @"Ampache\Albums",
                        @"Ampache\Artists",
                        @"Ampache\Playlists"
                    };

                return null;
            }
            catch (Exception e)
            {
                LastException = e;
                return null;
            }

            // if there is a structure, depending on whether the user has clicked the main navigation node or a specific folder, MusicBee will call GetFiles(path) with the appropriate path
        }

        public bool FolderExists(string path)
        {
            try
            {
                var rootDirs = new[] { @"Ampache", @"Ampache\Albums", @"Ampache\Artists" };

                if (rootDirs.Any(d => path.Equals(d, StringComparison.InvariantCultureIgnoreCase)))
                    return true;

                return false;
            }
            catch (Exception e)
            {
                LastException = e;
                return false;
            }
        }

        public KeyValuePair<byte, string>[][] GetFiles(string path)
        {
            try
            {

            }
            catch (Exception e)
            {
                LastException = e;
                return null;
            }
            // return an array for file tag data
            // each row on the array represents a file
            // and a file consists of an array of tags (FilePropertyType and MetaDataType) and values
        }

        public bool FileExists(string url)
        {
            try
            {
                return false;
            }
            catch (Exception e)
            {
                LastException = e;
                return false;
            }
        }

        public KeyValuePair<byte, string>[] GetFile(string url)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                LastException = e;
                return null;
            }
        }

        public byte[] GetFileArtwork(string url)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                LastException = e;
                return null;
            }
            // return base64 representation of bitmap data
        }

        public KeyValuePair<string, string>[] GetPlaylists()
        {
            try
            {
                var playlists = Ampache.GetPlaylists(null);

                return playlists.Select(p => new KeyValuePair<string, string>(p.Id.ToString(), p.Name)).ToArray();
            }
            catch (Exception e)
            {
                LastException = e;
                return null;
            }
        }

        public KeyValuePair<byte, string>[][] GetPlaylistFiles(string id)
        {
            try
            {
                var songs = Ampache.GetPlaylistSongs(int.Parse(id));

                return songs.Select(SongToFile).ToArray();
            }
            catch (Exception e)
            {
                LastException = e;
                return null;
            }
        }

        public Stream GetStream(string url)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                LastException = e;
                return null;
            }
        }

        public Exception GetError()
        {
            return LastException;
        }

        #endregion

        private KeyValuePair<byte, string>[] SongToFile(Song song)
        {
            var file = new Dictionary<byte, string>();

            file.Add((byte)MetaDataType.TrackTitle, song.Title);
            file.Add((byte)FilePropertyType.Url, song.Url);
            file.Add((byte)MetaDataType.Artwork, song.ArtworkUrl);
            file.Add((byte)MetaDataType.Artist, song.Artist.Name);
            file.Add((byte)MetaDataType.Album, song.Album.Name);
            file.Add((byte)MetaDataType.Composer, song.Composer);
            file.Add((byte)MetaDataType.Comment, song.Comment);
            file.Add((byte)MetaDataType.Publisher, song.Publisher);
            //language
            //tags
            file.Add((byte)MetaDataType.TrackNo, song.Track.ToString());

            file.Add((byte)FilePropertyType.Duration, song.TimeSeconds.ToString());

            if (song.SampleRate != 0)
                file.Add((byte)FilePropertyType.Size, song.SizeBytes.ToString());

            if (song.Year != null)
                file.Add((byte)MetaDataType.Year, song.Year.ToString());

            if (song.BitRate != 0)
                file.Add((byte)FilePropertyType.Bitrate, song.BitRate.ToString());

            if(song.SampleRate != 0)
            file.Add((byte)FilePropertyType.SampleRate, song.SampleRate.ToString());

            if (song.Channels != null)
                file.Add((byte)FilePropertyType.Channels, song.Channels.ToString());

            file.Add((byte)MetaDataType.Rating, song.Rating.ToString());

            return file.ToArray();
        }

        //// return an array of lyric or artwork provider names this plugin supports
        //// the providers will be iterated through one by one and passed to the RetrieveLyrics/ RetrieveArtwork function in order set by the user in the MusicBee Tags(2) preferences screen until a match is found
        //public string[] GetProviders()
        //{
        //    return null;
        //}

        //// return lyrics for the requested artist/title from the requested provider
        //// only required if PluginType = LyricsRetrieval
        //// return null if no lyrics are found
        //public string RetrieveLyrics(string sourceFileUrl, string artist, string trackTitle, string album, bool synchronisedPreferred, string provider)
        //{
        //    return null;
        //}

        //// return Base64 string representation of the artwork binary data from the requested provider
        //// only required if PluginType = ArtworkRetrieval
        //// return null if no artwork is found
        //public string RetrieveArtwork(string sourceFileUrl, string albumArtist, string album, string provider)
        //{
        //    //Return Convert.ToBase64String(artworkBinaryData)
        //    return null;
        //}
    }
}