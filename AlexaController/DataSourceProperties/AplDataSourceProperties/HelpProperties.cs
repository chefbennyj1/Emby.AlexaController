using System.Collections.Generic;
using AlexaController.Alexa.Presentation.DataSources;

namespace AlexaController.DataSourceProperties.AplDataSourceProperties
{
    public class HelpProperties : IProperties
    {
        public RenderDocumentType documentType { get; set; } = RenderDocumentType.HELP;
        public List<HelpValue> helpContent => new List<HelpValue>()
        {
            new HelpValue() {value = "<b>Accessing Emby accounts based on your voice.</b>"},
            new HelpValue() {value = "I have the ability to access specific emby user accounts, and library data based on the sound of your voice. If you have not yet turned on Personalization in your Alexa App, and enabled it for this skill, please do that now."},
            new HelpValue() {value = "You can enable Parental Controls, so media will be filtered based on who is speaking. This way, media items will be filtered using voice verification."},
            new HelpValue() {value = "To enable this feature, open the Emby plugin configuration page, and toggle the button for \"Enable parental control using voice recognition\". If this feature is turned off, I will not filter media items, and will show media based on the Emby Administrators account, at all times."},
            new HelpValue() {value = "Select the \"New Authorization Account Link\" button, and follow the instructions to add personalization."},
            new HelpValue() {value = "<b>Accessing media Items in rooms</b>"},
            new HelpValue() {value = "The Emby plugin will allow you to create \"Rooms\", based on the devices in your home."},
            new HelpValue() {value = "In the plugin configuration, map each Emby ready device to a specific room. You will create the room name that I will understand."},
            new HelpValue() {value = "For example: map your Amazon Fire Stick 4K to a room named: \"Family Room\"."},
            new HelpValue() {value = "Now you can access titles and request them to display per room."},
            new HelpValue() {value = "You can use the phrase:"},
            new HelpValue() {value = "Ask home theater to play the movie Iron Man in the family room."},
            new HelpValue() {value = "To display the movies library on the \"Family Room\" device, you can use the phrase:"},
            new HelpValue() {value = "Ask home theater to show \"Movies\" in the Family Room."},
            new HelpValue() {value = "The same can be said for \"Collections\", and \"TV Series\" libraries"},
            new HelpValue() {value = "The Emby client must already be running on the device in order to access the room commands."},
            new HelpValue() {value = "<b>Accessing Collection Items</b>"},
            new HelpValue() {value = "I have the ability to show collection data on echo show, echo spot, or other devices with screens."},
            new HelpValue() {value = "To access this ability, you can use the following phrases:"},
            new HelpValue() {value = "Ask home theater to show all the Iron Man movies..."},
            new HelpValue() {value = "Ask home theater to show the Spiderman collection..."},
            new HelpValue() {value = "Fire TV devices, with Alexa enabled, are exempt from displaying these items because the Emby client will take care of this for you."},
            new HelpValue() {value = "<b>Accessing individual media Items</b>"},
            new HelpValue() {value = "I am able to show individual media items as well."},
            new HelpValue() {value = "You can use the phrases:"},
            new HelpValue() {value = "Ask home theater to show the movie Spiderman Far From Home..."},
            new HelpValue() {value = "Ask home theater to show the movie Spiderman Far From Home, in the Family Room."},
            new HelpValue() {value = "You can also access TV Series the same way."},
            new HelpValue() {value = "I will understand the phrases: "},
            new HelpValue() {value = "Ask home theater to show the series Westworld..."},
            new HelpValue() {value = "Ask home theater to show the next up episode for Westworld..."},
            new HelpValue() {value = "<b>Accessing new media Items</b>"},
            new HelpValue() {value = "To access new titles in your library, you can use the phrases:"},
            new HelpValue() {value = "Ask home theater about new movies..."},
            new HelpValue() {value = "Ask home theater about new TV Series..."},
            new HelpValue() {value = "You can also request a duration for new items... For example:"},
            new HelpValue() {value = "Ask home theater for new movies added in the last three days..."},
            new HelpValue() {value = "Ask home theater for new tv shows added in the last month"},
            new HelpValue() {value = "Remember that as long as the echo show, or spot is displaying an image, you are in an open session. This means you don't have to use the \"Ask home theater\" phrases to access media"},
            new HelpValue() {value = "This concludes the help section. Good luck!"},
        };

        

        public class HelpValue
        {
            public string value { get; set; }
        }

        public class Transformer : ITransformer
        {
            public string inputPath { get; set; }
            public string outputName { get; set; }
            public string transformer { get; set; }
            public string template { get; set; }
        }
    }
}

    

