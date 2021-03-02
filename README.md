# Emby.AlexaController

![alt text](https://github.com/chefbennyj1/Emby.AlexaController/blob/master/AlexaController/GithubAssets/asset1.png?raw=true "APL")
https://apl.ninja/BenjaminAnderson/xstJTwn4

https://apl.ninja/BenjaminAnderson/lU8KwtLw


<b>Setup</b>

Because this endpoint is hosted inside Emby Server, your server should have an SSL/TLS cert with a reachable domain to send the Alexa skill requests.

The plugin will open the follow endpoint insided your emby server: "emby/Alexa".

This is the endpoint you want to point your custom skill to.

example: "https://{YOUR_DOMAIN_NAME}/emby/Alexa"

<b>Interaction model</b>

While creating your custom skill on amazon.developer.com in the dev console, you'll have the opportunity to upload an Interaction model.

This model (JSON) can be found in the emby dashboard plugin configuration under <b>Interaction model</b>.

Copy and paste this JSON data into your custom skills Interaction model using the <b>JSON Editor</b>.

<b>Alexa Presentation Language</b>
Make sure that all the Echo devices (check boxes) are selected in the developer console -> Alexa Presentation Language tag of your custom skill.

<b>Personalization</b>

Be sure to enable personalization for you skill as well.
This is found in the skill developer console under <b>Permissions</b>.

Next open the "Personalization" options in the configuration page.

Use the phrase "Alexa, ask {Your_custom_skill_invocation_name} to learn my voice."
   
The personId will appear in the dialog box.
Choose the user name you have in Emby and press the 'save' button!

Alexa will now access your media from emby libraries, and only your media based on your voice recognition.

Alexa will not load media in emby which doesn't adhere to the users watch status or parental rating  based on their voice recognition.

If you do not allow personalztion, the skill will not let you access the emby libraries, and will immediately end the Alexa Session if the user if not recognized.

Example: My 8 year son has access to only "rated G" media items. Alexa will not load media items he does not have access to, 
like 'rated R' movies.

Instead Alexa will say: "I don't believe you have access to the/this item/item.Name {user.Name}


<b>Help</b>

You may ask for help by using the phrase: "Alexa, ask {Your_custom_skill_invocation_name} for help.



