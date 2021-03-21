using AlexaController.Session;
using MediaBrowser.Controller.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AlexaController.EmbyAplDataSourceManagement
{
    public class DataSourceAudioSpeechPropertiesManager : SpeechBuilderService
    {
        public static DataSourceAudioSpeechPropertiesManager Instance { get; private set; }
        public DataSourceAudioSpeechPropertiesManager()
        {
            Instance = this;
        }
        public async Task<Properties<string>> PersonNotRecognized()
        {
            var speech = new StringBuilder();
            PersonNotRecognized(speech);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        public async Task<Properties<string>> OnLaunch()
        {
            var speech = new StringBuilder();
            OnLaunch(speech);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        public async Task<Properties<string>> NotUnderstood()
        {
            var speech = new StringBuilder();
            NotUnderstood(speech);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        public async Task<Properties<string>> NoItemExists()
        {
            var speech = new StringBuilder();
            NoItemExists(speech);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        public async Task<Properties<string>> ItemBrowse(BaseItem item, IAlexaSession session,
            bool correctUserPhrasing = false, bool deviceAvailable = true)
        {
            var speech = new StringBuilder();
            ItemBrowse(speech, item, session, correctUserPhrasing, deviceAvailable);

            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        public async Task<Properties<string>> BrowseNextUpEpisode(BaseItem item, IAlexaSession session)
        {
            var speech = new StringBuilder();
            BrowseNextUpEpisode(speech, item, session);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        public async Task<Properties<string>> NoNextUpEpisodeAvailable()
        {
            var speech = new StringBuilder();
            NoNextUpEpisodeAvailable(speech);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        public async Task<Properties<string>> PlayNextUpEpisode(BaseItem item, IAlexaSession session)
        {
            var speech = new StringBuilder();
            PlayNextUpEpisode(speech, item, session);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        public async Task<Properties<string>> ParentalControlNotAllowed(BaseItem item, IAlexaSession session)
        {
            var speech = new StringBuilder();
            ParentalControlNotAllowed(speech, item, session);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        public async Task<Properties<string>> PlayItem(BaseItem item)
        {
            var speech = new StringBuilder();
            PlayItem(speech, item);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        public async Task<Properties<string>> RoomContext()
        {
            var speech = new StringBuilder();
            RoomContext(speech);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        public async Task<Properties<string>> VoiceAuthenticationExists(IAlexaSession session)
        {
            var speech = new StringBuilder();
            VoiceAuthenticationExists(speech, session);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        public async Task<Properties<string>> VoiceAuthenticationAccountLinkError()
        {
            var speech = new StringBuilder();
            VoiceAuthenticationAccountLinkError(speech);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        public async Task<Properties<string>> VoiceAuthenticationAccountLinkSuccess(IAlexaSession session)
        {
            var speech = new StringBuilder();
            VoiceAuthenticationAccountLinkSuccess(speech, session);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        public async Task<Properties<string>> UpComingEpisodes(List<BaseItem> items, DateTime date)
        {
            var speech = new StringBuilder();
            UpComingEpisodes(speech, items, date);

            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        public async Task<Properties<string>> NewLibraryItems(List<BaseItem> items, DateTime date,
            IAlexaSession session)
        {
            var speech = new StringBuilder();
            NewLibraryItems(speech, items, date, session);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        public async Task<Properties<string>> BrowseItemByActor(List<BaseItem> actors)
        {
            var speech = new StringBuilder();
            BrowseItemByActor(speech, actors);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
    }
}
