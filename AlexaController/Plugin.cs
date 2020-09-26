using MediaBrowser.Common.Plugins;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using MediaBrowser.Model.Drawing;
using System.IO;
using AlexaController.Configuration;
using MediaBrowser.Model.Plugins;

namespace AlexaController
{
    public class Plugin: BasePlugin<PluginConfiguration>, IHasThumbImage, IHasWebPages
    {
        public static Plugin Instance { get; private set; }

        public static readonly Random RandomIndex = new Random();

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer): base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public override Guid Id            => new Guid("6995F8F3-FD4C-4CB6-A8F4-99A1D8828199");
        public override string Name        => "Amazon Alexa";
        public override string Description => "End Point for Alexa Skill";

        public IEnumerable<PluginPageInfo> GetPages() => new[]
        {
            new PluginPageInfo
            {
                Name                 = "AlexaPluginConfigurationPage",
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.AlexaPluginConfigurationPage.html",
                EnableInMainMenu = true,
                DisplayName = "Alexa"
            },
            new PluginPageInfo
            {
                Name                 = "AlexaPluginConfigurationPageJS",
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.AlexaPluginConfigurationPage.js"
            }
            
        };
        
        public Stream GetThumbImage()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.gif");
        }

        public ImageFormat ThumbImageFormat => ImageFormat.Gif;

    }
}
