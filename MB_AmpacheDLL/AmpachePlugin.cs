using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using MusicBeePlugin.Ampache;
using System.IO;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Threading;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private MusicBeeApiInterface mbApiInterface;
        private PluginInfo about = new PluginInfo();

        private static string DefaultServer = "ampache.example.com";

        private AmpacheClient ampache;

        private const string ConfigFileName = "mb_ampache.cfg";

        private Settings CurrentSettings { get; set; }

        private SettingsControl SettingsControl { get; set; }

        public Plugin()
        {
            SettingsControl = new SettingsControl();
        }

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);

            LoadSettings();

            StartApi();

            about.PluginInfoVersion = PluginInfoVersion;
            about.Name = "MB_Ampache";
            about.Description = "Access your Ampache music library within MusicBee";
            about.Author = "Sam Shores";
            about.TargetApplication = "";   // current only applies to artwork, lyrics or instant messenger name that appears in the provider drop down selector or target Instant Messenger
            about.Type = PluginType.Storage;
            about.VersionMajor = 0;  // your plugin version
            about.VersionMinor = 0;
            about.Revision = 1;
            about.MinInterfaceVersion = MinInterfaceVersion;
            about.MinApiRevision = MinApiRevision;
            about.ReceiveNotifications = (ReceiveNotificationFlags.PlayerEvents | ReceiveNotificationFlags.TagEvents);
            about.ConfigurationPanelHeight = SettingsControl.Height;   // height in pixels that musicbee should reserve in a panel for config settings. When set, a handle to an empty panel will be passed to the Configure function
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
                    Password = "password"
                };
            }

            using (var stream = File.OpenRead(configFile))
            {
                var serializer = new XmlSerializer(typeof(Settings));

                CurrentSettings = (Settings)serializer.Deserialize(stream);
            }

            ampache = new AmpacheClient(CurrentSettings.MakeUrl(), CurrentSettings.Username, CurrentSettings.Password);
        }

        // called by MusicBee when the user clicks Apply or Save in the MusicBee Preferences screen.
        // its up to you to figure out whether anything has changed and needs updating
        public void SaveSettings()
        {
            // save any persistent settings in a sub-folder of this path
            var dataPath = mbApiInterface.Setting_GetPersistentStoragePath();

            var configFile = Path.Combine(dataPath, ConfigFileName);

            // TODO get settings from preferences UI & see if any changed

            using(var stream = File.OpenWrite(configFile))
            {
                var serializer = new XmlSerializer(typeof(Settings));

                serializer.Serialize(stream, CurrentSettings);
            }
        }

        private void StartApi()
        {
            if (CurrentSettings.Server != DefaultServer)
            {
                ampache = new AmpacheClient(CurrentSettings.MakeUrl(), CurrentSettings.Username, CurrentSettings.Password);

                ampache.Connected += Ampache_Connected;
                ampache.Connect();
            }
            else
                ampache = null;
        }
        private void StopApi()
        {
            ampache.Disconnect();
        }

        private void Ampache_Connected(object sender, AmpacheConnectedEventArgs e)
        {
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
            switch (type)
            {
                case NotificationType.PluginStartup:
                    // perform startup initialisation
                    switch (mbApiInterface.Player_GetPlayState())
                    {
                        case PlayState.Playing:
                        case PlayState.Paused:
                            // ...
                            break;
                    }
                    break;
                case NotificationType.TrackChanged:
                    string artist = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Artist);
                    // ...
                    break;
            }
        }

        // return an array of lyric or artwork provider names this plugin supports
        // the providers will be iterated through one by one and passed to the RetrieveLyrics/ RetrieveArtwork function in order set by the user in the MusicBee Tags(2) preferences screen until a match is found
        public string[] GetProviders()
        {
            return null;
        }

        // return lyrics for the requested artist/title from the requested provider
        // only required if PluginType = LyricsRetrieval
        // return null if no lyrics are found
        public string RetrieveLyrics(string sourceFileUrl, string artist, string trackTitle, string album, bool synchronisedPreferred, string provider)
        {
            return null;
        }

        // return Base64 string representation of the artwork binary data from the requested provider
        // only required if PluginType = ArtworkRetrieval
        // return null if no artwork is found
        public string RetrieveArtwork(string sourceFileUrl, string albumArtist, string album, string provider)
        {
            //Return Convert.ToBase64String(artworkBinaryData)
            return null;
        }
   }
}