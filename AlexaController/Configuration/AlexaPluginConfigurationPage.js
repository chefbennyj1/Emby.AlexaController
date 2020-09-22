define(["require", "loading", "dialogHelper", "formDialogStyle", "emby-checkbox", "emby-select", "emby-toggle"],
    function(require, loading, dialogHelper) {
        var pluginId = "6995F8F3-FD4C-4CB6-A8F4-99A1D8828199";

        function openNotificationModelDialog(view) {
            loading.show();

            var dlg = dialogHelper.createDialog({
                size: "medium-tall",
                removeOnClose: false,
                scrollY: !0
            });

            dlg.classList.add("formDialog");
            dlg.classList.add("ui-body-a");
            dlg.classList.add("background-theme-a");
            dlg.classList.add("scrollY");
            dlg.style = "max-width:50%; max-height:75%";

            var html = '';

            html += '<div class="formDialogHeader" style="display:flex">';
            html += '<button is="paper-icon-button-light" class="btnCloseDialog autoSize paper-icon-button-light" tabindex="-1"><i class="md-icon"></i></button>';
            html += '<h3 id="headerContent" class="formDialogHeaderTitle">Alexa Events and Notifications Model</h3>';
            html += '</div>';

            html += '<div style="margin:2em;">';
            html += '<p>Alexa has the ability to alert notifications and events to the user with a glowing yellow light ring or card, or new media added to the library.</p>';
            html += '</div>';


            //Enable proactive events
            html += '<div class="inputContainer" style="text-align: left;">';
            html += '<label style = "width: auto;" class="mdl-switch mdl-js-switch">';
            html += '<input is="emby-toggle" type="checkbox" id="enableProactiveEvent" class="chkProactiveEvents noautofocus mdl-switch__input" data-embytoggle="true">';
            html += '<span class="toggleButtonLabel mdl-switch__label">Enable new media item notification</span>';
            html += '<div class="mdl-switch__trackContainer">';
            html += '<div class="mdl-switch__track"></div>';
            html += '<div class="mdl-switch__thumb">';
            html += '<span class="mdl-switch__focus-helper"></span>';
            html += '</div>';
            html += '</div>';
            html += '</label>';
            html += '</div>';

             
            //Client Id
            html += '<div style = "align-items: center; padding-bottom: 6%">';
            html += '<!--Client Id-->';
            html += '<div class="inputContainer" style="padding-bottom: 3em;">';
            html += '<div style="flex-grow: 1;">';
            html += '<label class="inputLabel inputLabelUnfocused" disabled for="alexaClientId">Alexa Client Id:</label>';
            html += '<input id="alexaClientId" type="text" required="required" label="Alexa Client Id:" class="emby-input">';
            html += '</div> ';
            html += '<div class="fieldDescription">';
            html += '';
            html += '</div>';
            html += '</div>';

            //Client Secret
            html += '<div style = "align-items: center; padding-bottom: 6%">';
            html += '<!--Client Secret-->';
            html += '<div class="inputContainer" style="padding-bottom: 3em;">';
            html += '<div style="flex-grow: 1;">';
            html += '<label class="inputLabel inputLabelUnfocused" for="alexaClientSecret">Alexa Client Id:</label>';
            html += '<input id="alexaClientSecret" type="text" disabled required="required" label="Alexa Client Secret:" class="emby-input">';
            html += '</div> ';
            html += '<div class="fieldDescription">';
            html += 'Select the skill in the developer console. Next, select the Build tab, scroll down to the Permissions section, and locate the ClientId and ClientSecret values from the Alexa Skill Messaging section at the bottom of the page.';
            html += '</div>';
            html += '</div>';

            
            html += '<!--Save Button-->';
            html += '<button is="emby-button" class="raised button-submit block emby-button" id="saveButton">Save</button>';

            dlg.innerHTML = html;

            dlg.querySelector('.btnCloseDialog').addEventListener('click',
                () => {
                    dialogHelper.close(dlg);
                });
              
            loading.hide();
            dialogHelper.open(dlg);

            var secretInput   = dlg.querySelector('#alexaClientSecret');
            var clientIdInput = dlg.querySelector('#alexaClientId');

            ApiClient.getPluginConfiguration(pluginId).then((config) => {

                if (config.EnableNotifications) {
                    dlg.querySelector('#enableProactiveEvent').checked = config.EnableNotifications;
                    clientIdInput.disabled = false;
                    secretInput.disabled   = false;
                } else {
                    clientIdInput.disabled = true;
                    secretInput.disabled   = true;
                }
            });

            dlg.querySelector('#enableProactiveEvent').addEventListener('change',
                (e) => {
                    var enableProactiveEvent = dlg.querySelector('#enableProactiveEvent');
                    var secretInput          = dlg.querySelector('#alexaClientSecret');
                    var clientIdInput        = dlg.querySelector('#alexaClientId');

                    switch (enableProactiveEvent.checked) {
                    case true:
                            secretInput.disabled = false;
                            clientIdInput.disabled = false;
                        break;
                    case false:
                            secretInput.disabled = true;
                            clientIdInput.disabled =  true;
                        break;
                    }
                    ApiClient.getPluginConfiguration(pluginId).then((config) => {
                        config.EnableNotifications = enableProactiveEvent.checked;
                        ApiClient.updatePluginConfiguration(pluginId, config).then((result) => {
                            Dashboard.processPluginConfigurationUpdateResult(result);
                        });
                    });
                });

            dlg.querySelector('#saveButton').addEventListener('click',
                (e) => {
                    e.preventDefault();

                    //var secretInput   = dlg.querySelector('#alexaClientSecret');
                    //var clientIdInput = dlg.querySelector('#alexaClientId');
                    var secret        = secretInput.value;
                    var id            = clientIdInput.value; 
                    
                    ApiClient.getPluginConfiguration(pluginId).then((config) => {
                        config.ClientSecret = secret;
                        config.ClientId     = id;
                        ApiClient.updatePluginConfiguration(pluginId, config).then(
                            (result) => { 
                                Dashboard.processPluginConfigurationUpdateResult(result);
                                dialogHelper.close(dlg);
                            });
                    });

                });

        }

        function openPersonalizationModelDialog(view) {
            loading.show();

            var dlg = dialogHelper.createDialog({
                size: "medium-tall",
                removeOnClose: false,
                scrollY: !0
            });

            dlg.classList.add("formDialog");
            dlg.classList.add("ui-body-a");
            dlg.classList.add("background-theme-a");
            dlg.classList.add("scrollY");
            dlg.style = "max-width:50%; max-height:75%";

            var html = '';

            html += '<div class="formDialogHeader" style="display:flex">';
            html += '<button is="paper-icon-button-light" class="btnCloseDialog autoSize paper-icon-button-light" tabindex="-1"><i class="md-icon"></i></button>';
            html += '<h3 id="headerContent" class="formDialogHeaderTitle">Alexa Personalization Model</h3>';
            html += '</div>';

            html += '<div style="margin:2em;">';
            html += '<p>Alexa has the ability to personalize requests made by the user. She will recognize the user making the request.</p>';
            html += '<p>If Alexa recognizes a user by name, you can make the proper correlation with their emby account below.</p>';
            html += '<p>Make sure "Personalization" has been turned on in the "Build -> Permissions" tabs of the Custom SKill.</p>';
            html += '</div>';

            html += '<div style="margin: 0 auto; text-align:center; width: 50%;">';
            html += '<h1 class="learningPhrase" style="font-weight: bold">Alexa, ask home theater to learn my voice<h1>';
            html += '<h1 class="hide successMessage" style="color:#52B54B"> Success! We have learned to recognize your voice.</h1>';
            html += '</div>'; 

            html += '<div style = "align-items: center; margin: 0 5em;">';
            html += '<!--Person Id-->';
            html += '<div class="inputContainer" style="padding-bottom: 3em;">';
            html += '<div style="flex-grow: 1;">';
            html += '<label class="inputLabel inputLabelUnfocused" for="personId">Person Id:</label>';
            html += '<input id="personId" type="text" required="required" readonly label="Person Id:" class="emby-input">';
            html += '</div> ';
            html += '<div class="fieldDescription">';
            html += 'The account Id Alexa recognizes you as.';
            html += '</div>';
            html += '</div>';
            
            html += '<!--Emby User List-->';
            html += '<div style="flex-grow: 1;" class="selectContainer">';
            html += '<label class="selectLabel" for="selectEmbyUsers">Emby Users:</label> ';
            html += '<select is="emby-select" name="selectEmbyUsers" id="selectEmbyUsers" label="Emby Users:" data-mini="true" class="emby-select-withcolor emby-select"></select>';
            html += '<div class="selectArrowContainer"> ';
            html += '<div style="visibility: hidden;">0</div><i class="selectArrow md-icon"></i>';
            html += '</div>';
            html += '<div class="fieldDescription">';
            html += 'Choose an Emby user from the list to add to a corresponding person Id Alexa recognizes.';
            html += '</div>';
            html += '</div> ';
            html += '</div>';

            html += '<!--Save Button-->';
            html += '<button is="emby-button" class="raised button-submit block emby-button" style="margin: 2em auto; width: 50%;" id="saveButton">Save</button>';

            dlg.innerHTML = html;

            dlg.querySelector('.btnCloseDialog').addEventListener('click',
                () => {
                    dialogHelper.close(dlg);
                });

            ApiClient.getJSON(ApiClient.getUrl("Users")).then((users) => {
                users.forEach(user => { 
                    dlg.querySelector('#selectEmbyUsers').innerHTML += ('<option value="' + user.Name + '" data-id="' + user.Id + '" data-name="' + user.Name + '">' + user.Name + '</option>'); 
                });
                
            });

            loading.hide();
            dialogHelper.open(dlg);

            ApiClient._webSocket.addEventListener('message', function (message) {
                var json = JSON.parse(message.data);
                if (json.MessageType === "SpeechAuthentication") {
                    dlg.querySelector('.successMessage').classList.remove('hide');
                    dlg.querySelector('.learningPhrase').classList.add('hide');
                    dlg.querySelector('#personId').value = json.Data;
                }
            });

            dlg.querySelector('#saveButton').addEventListener('click',
                (e) => {
                    e.preventDefault();
                    var userSelect = dlg.querySelector('#selectEmbyUsers');
                    var personId = dlg.querySelector('#personId').value;
                    var userId = userSelect.options[userSelect.selectedIndex >= 0 ? userSelect.selectedIndex : 0].dataset.id;
                    var userName = dlg.querySelector('#selectEmbyUsers').value;

                    var correlation = {
                        EmbyUserId    : userId,
                        AlexaPersonId : personId,
                        EmbyUserName  : userName
                    }

                    ApiClient.getPluginConfiguration(pluginId).then((config) => {
                        config.UserCorrelations.push(correlation);
                        ApiClient.updatePluginConfiguration(pluginId, config).then(
                            (result) => {
                                view.querySelector('.personalizationTableResultBody').innerHTML = getPersonalizationTableHtml(config.UserCorrelations);
                                view.querySelectorAll('.removeUser').forEach(button => {
                                    button.addEventListener('click',
                                        (ele) => {
                                            removeUserCorrelationOnClick(ele, view);
                                        });
                                });
                                Dashboard.processPluginConfigurationUpdateResult(result);
                                dialogHelper.close(dlg);
                            });
                    });

                }); 

        }  
          
        function openClearAllConfirmationDialog(e, view) {
            
            var dlg = dialogHelper.createDialog({
                size: "medium-tall",
                removeOnClose: false,
                scrollY: !0
            });

            dlg.classList.add("formDialog");
            dlg.classList.add("ui-body-a");
            dlg.classList.add("background-theme-a");
            dlg.classList.add("colorChooser");
            dlg.style = "max-width:23%; max-height:28%;";

            var html = '';
             
            html += '<div style = "align-items: center; padding-bottom: 6%; margin:1.2em">';
              
            var name = e.classList.contains('btnClearAllPersonalizationResults') ? "personalization" : "room";

            html += '<div style="margin: auto;width: 50%;">';
            html += '<h2 style="text-align: center;">You are about to remove all saved ' + name + ' options.</h2>';
            html += '</div>';

            html += '<div class="formDialogFooter formDialogFooter-clear formDialogFooter-flex">';
            html += '<button id="cancel" is="emby-button" type="button" class="btnOption raised formDialogFooterItem formDialogFooterItem-autosize button-submit emby-button" data-id="cancel" autofocus="">Cancel</button>';
            html += '<button id="ok" is="emby-button" type="button" class="btnOption raised formDialogFooterItem formDialogFooterItem-autosize button-cancel emby-button" data-id="ok">Ok</button>';
            html += '</div>';

            html += '</div>';

            dlg.innerHTML = html;

            dialogHelper.open(dlg);

            dlg.querySelector('#ok').addEventListener('click',
                () => {
                    switch (name) {
                        case "personalization":
                            ApiClient.getPluginConfiguration(pluginId).then((config) => {
                                config.UserCorrelations = [];
                                ApiClient.updatePluginConfiguration(pluginId, config).then(
                                    (result) => {
                                        view.querySelector('.personalizationTableResultBody').innerHTML = "";

                                        // Hide the "clear all" button
                                        view.querySelector('.btnClearAllPersonalizationResults').classList.add('hide');

                                        Dashboard.processPluginConfigurationUpdateResult(result);
                                        dialogHelper.close(dlg);
                                    });
                            });
                            break;
                        case "room":
                            ApiClient.getPluginConfiguration(pluginId).then((config) => {
                                config.Rooms = [];
                                ApiClient.updatePluginConfiguration(pluginId, config).then(
                                    (result) => {
                                        view.querySelector('.roomTableResultBody').innerHTML = "";

                                        // Hide the "clear all" button
                                        view.querySelector('.btnClearAllRoomsDevicesResults').classList.remove('hide');

                                        Dashboard.processPluginConfigurationUpdateResult(result);
                                        dialogHelper.close(dlg);
                                    });
                            });
                            break;
                    }
                    
                });

            dlg.querySelector('#cancel').addEventListener('click',
                () => {
                    dialogHelper.close(dlg);
                }); 
            
        } 
         
        function openRoomModelDialog(view) {
            loading.show();

            var dlg = dialogHelper.createDialog({
                size: "medium-tall",
                removeOnClose: false,
                scrollY: !0
            });

            dlg.classList.add("formDialog");
            dlg.classList.add("ui-body-a");
            dlg.classList.add("background-theme-a");
            dlg.classList.add("colorChooser");
            dlg.style = "max-width:50%; max-height:85%;";

            var html = '';

            html += '<div class="formDialogHeader" style="display:flex">';
            html += '<button is="paper-icon-button-light" class="btnCloseDialog autoSize paper-icon-button-light" tabindex="-1"><i class="md-icon"></i></button>';
            html += '<h3 id="headerContent" class="formDialogHeaderTitle">Create a room</h3>';
            html += '</div>';

            html += '<h2 class="sectionTitle sectionTitle-cards">Rooms and Devices</h2>';
            html += '<div style="align-items: center;padding-bottom: 2%;margin: 0 1.2em;">';

            html += '<div class="infoBanner restartInfoBanner flex align-items-center" style="margin: 1em 0;">';
            html += 'While configuring rooms, please do not leave this dialog.';
            html += '</div >';
            html += '<div style="margin:2em;">';
            html += '<p>In order for Alexa to best understand the room name to assign an Emby device in, use the following phrases to set up rooms.</p>';
            html += '</div>';

            html += '<div style="margin:2em; text-align:center;width:100%">';
            html += '<h2 class="learningPhrase" style="font-weight: bold">Alexa, ask home theater to set up a new room.</h2>';
            html += '<h2 class="learningPhrase" style="font-weight: bold">Alexa, ask home theater to add a new room.</h2>';
            html += '</div>';

            html += '<!--Room Name-->';
            html += '<div class="inputContainer" style="padding-bottom: 3em;">';
            html += '<div style="flex-grow: 1;">';
            html += '<label class="inputLabel inputLabelUnfocused" for="roomName">Room Name:</label>';
            html += '<input id="roomName" readonly type="text" required="required" label="Room Name:" class="emby-input">';
            html += '</div> ';
            html += '<div class="fieldDescription">';
            html += 'The room name the Emby device corresponds too.';
            html += '</div>';
            html += '</div>';

            //html += '<!-Add Echo device to a room-->';
            //html += '<div class="detailSectionHeader" style="margin:2em;display: inline-flex;">';
            //html += '<h2 style="margin: .6em 0; vertical-align: middle; display: inline-block;">';
            //html += 'Add echo device to room';
            //html += '</h2>';
            //html += '<button is="emby-button" type="button" class="btnAddEcho raised button-submit fab emby-button" style="margin-left: 1em;width: 5em;">';
            //html += '<i class="md-icon">add</i>';
            //html += '</button>';
            //html += '</div>';



            html += '<!--Emby Devices List-->';
            html += '<div style="flex-grow: 1;" class="selectContainer">';
            html += '<label class="selectLabel" for="selectEmbyDevice">Emby Devices:</label> ';
            html += '<select is="emby-select" name="selectEmbyDevice" id="selectEmbyDevice" label="Emby Network Devices:" data-mini="true" class="emby-select-withcolor emby-select"></select>';
            html += '<div class="selectArrowContainer"> ';
            html += '<div style="visibility: hidden;">0</div><i class="selectArrow md-icon"></i>';
            html += '</div>';
            html += '<div class="fieldDescription">';
            html += 'Choose an Emby device from the list to add to a corresponding room.';
            html += '</div>';
            html += '</div> ';


            html += '</div>';
            
            html += '<!--Save Button-->';
            html += '<div style="margin:1.2em">';
            html += '<button is="emby-button" class="raised button-submit block emby-button" id="saveButton">Create room</button>';
            html += '</div>';    

            dlg.innerHTML = html;

            loading.hide();
            dialogHelper.open(dlg);
            
            dlg.querySelector('.btnCloseDialog').addEventListener('click',
                () => {
                    dialogHelper.close(dlg);
                });

            var embyDeviceList = dlg.querySelector('#selectEmbyDevice');

            ApiClient.getJSON(ApiClient.getUrl("Devices")).then((devices) => {
                devices.Items.forEach(
                    (device) => {
                        embyDeviceList.innerHTML +=
                            ('<option value="' + device.Name + '" data-app="' + device.AppName + '" data-name="' + device.Name + '">' + device.Name + ' - ' + device.AppName + '</option>');
                    });
            });
            
            dlg.querySelector('#saveButton').addEventListener('click',
                (e) => {
                    e.preventDefault();
                    var name   = dlg.querySelector('#roomName').value;
                    var device = dlg.querySelector('#selectEmbyDevice').value;

                    var room = {
                        Name: name,
                        Device: device
                    }

                    ApiClient.getPluginConfiguration(pluginId).then((config) => {
                        config.Rooms.push(room);
                        ApiClient.updatePluginConfiguration(pluginId, config).then(
                            (result) => {
                                view.querySelector('.roomTableResultBody').innerHTML = getRoomTableHtml(config.Rooms);
                                view.querySelectorAll('.removeRoom').forEach(button => {
                                    button.addEventListener('click',
                                        (ele) => {
                                            ele.preventDefault();
                                            removeRoomOnClick(ele, view);
                                        });
                                });
                                Dashboard.processPluginConfigurationUpdateResult(result);
                                dialogHelper.close(dlg);
                            });
                    });

                }); 

            ApiClient._webSocket.addEventListener('message', function (message) {
                var json = JSON.parse(message.data);
                if (json.MessageType === "RoomAndDeviceUtility") {
                    dlg.querySelector('#roomName').value = json.Data;
                }
            });
        }   

        function openInteractionModelDialog() {
            loading.show();
            
            var dlg = dialogHelper.createDialog({
                size: "medium-tall",
                removeOnClose: false,
                scrollY: !0
            });

            dlg.classList.add("formDialog");
            dlg.classList.add("ui-body-a");
            dlg.classList.add("background-theme-a");
            dlg.classList.add("colorChooser");
            dlg.style = "max-width:50%; max-height:42em";

            var html = '';

            html += '<div class="formDialogHeader" style="display:flex">';
            html += '<button is="paper-icon-button-light" class="btnCloseDialog autoSize paper-icon-button-light" tabindex="-1"><i class="md-icon"></i></button>';
            html += '<h3 id="headerContent" class="formDialogHeaderTitle">Alexa Interaction Model</h3>';
            html += '</div>';

            html += '<h2 class="sectionTitle sectionTitle-cards">Interaction Model:</h2>';
            html += '<div class="inputContainer" style="margin:3em; height:60%">';
            html += '<label class="textareaLabel" for="txtInteractionModel"></label>';
            html += '<textarea is="emby-textarea" id="txtInteractionModel" readonly label="Interaction Model:" class="textarea-mono emby-textarea" rows="10" readonly style="width:100%; height:100%">';
            html += '</textarea>';
            html += '<div class="fieldDescription">The Alexa custom skill interaction model.</div>';
            html += '</div>';


            html += '<div class="detailSectionHeader" style="margin:2em;display: inline-flex;">';
            html += '<h2 style="margin: .6em 0; vertical-align: middle; display: inline-block;">';
            html += 'Copy';
            html += '</h2>';
            html += '<button is="emby-button" type="button" class="btnCopy raised button-submit fab emby-button" style="margin-left: 1em;width: 5em;">';
            html += '<i class="md-icon" style="font-size: x-large;">copy</i>';
            html += '</button>';
            html += '</div>';
               

            dlg.innerHTML = html;
            
            dlg.querySelector('.btnCloseDialog').addEventListener('click',
                () => {
                    dialogHelper.close(dlg);
                });

            dlg.querySelector('.btnCopy').addEventListener('click',
                (e) => {
                    e.preventDefault();

                    var interactionModelJson = dlg.querySelector('#txtInteractionModel').innerHTML;

                    navigator.clipboard.writeText(interactionModelJson).then(
                        () => {
                            Dashboard.alert('Interaction Model Copied to clipboard');
                            dialogHelper.close(dlg);
                        }, () => {
                            Dashboard.alert('Could not Interaction Model Copied to clipboard');
                        });

                });

            ApiClient.getJSON(ApiClient.getUrl("InteractionModel")).then((result) => {
                dlg.querySelector('#txtInteractionModel').innerHTML = result.InteractionModel;
            });

            loading.hide();
            dialogHelper.open(dlg); 
        }  

        function getRoomTableHtml(rooms) {
            var html = '';
            for (var i = 0; i <= rooms.length -1; i++) { //rooms.forEach(room => {
                html += '<tr class="detailTableBodyRow detailTableBodyRow-shaded" id="' + rooms[i].Name.replace(" ", "_") + '">';
                html += '<td data-title="Name" class="detailTableBodyCell fileCell"><i class="md-icon navMenuOptionIcon" style=" transform: rotate(' + i*90 +'deg);">picture_in_picture</i></td>';

                html += '<td data-title="Name" class="detailTableBodyCell fileCell">' + rooms[i].Name + '</td>';
                html += '<td data-title="Name" class="detailTableBodyCell fileCell-shaded">' + rooms[i].Device + '</td>';
                html += '<td class="detailTableBodyCell fileCell">';
                html += '<button id="' + rooms[i].Name.replace(" ", "_") + '" class="fab removeRoom emby-button"><i class="md-icon">clear</i></button></td>';
                html += '<td class="detailTableBodyCell" style="whitespace:no-wrap;"></td>';
                html += '</tr>';
            }
            return html;
        }

        function getPersonalizationTableHtml(userCorrelations) {
            var html = '';
            userCorrelations.forEach(correlation => {
                html += '<tr class="detailTableBodyRow detailTableBodyRow-shaded" id="' + correlation.EmbyUserId + '">';
                html += '<td data-title="Name" class="detailTableBodyCell fileCell"><i class="md-icon navMenuOptionIcon">people_outline</i></td>';

                html += '<td data-title="EmbyUserName" class="detailTableBodyCell fileCell">' + correlation.EmbyUserName + '</td>';
                html += '<td data-title="PersonId" class="detailTableBodyCell fileCell-shaded">' + correlation.AlexaPersonId.substring(0, 25) + '</td>';
                html += '<td class="detailTableBodyCell fileCell">';
                html += '<button id="' + correlation.EmbyUserId + '" class="fab removeUser emby-button"><i class="md-icon">clear</i></button></td>';
                html += '<td class="detailTableBodyCell" style="whitespace:no-wrap;"></td>';
                html += '</tr>';
            });
            return html;
        } 

        function removeUserCorrelationOnClick(elem, view) {
            var userId = elem.target.closest('.removeUser').id;
            ApiClient.getPluginConfiguration(pluginId).then((config) => {
                var userCorrelations = config.UserCorrelations.filter(r => r.EmbyUserId !== userId);
                config.UserCorrelations = userCorrelations;
                ApiClient.updatePluginConfiguration(pluginId, config).then(
                    (result) => {
                        view.querySelector('.personalizationTableResultBody').innerHTML = getPersonalizationTableHtml(config.UserCorrelations);
                        Dashboard.processPluginConfigurationUpdateResult(result);
                        var removeButtons = view.querySelectorAll('.removeUser');
                        removeButtons.forEach((button) => {
                            button.addEventListener('click',
                                (e) => {
                                    removeUserCorrelationOnClick(e, view);
                                });
                        });
                    });
            });
        }

        function removeRoomOnClick(elem, view) {
            var room = elem.target.closest('.removeRoom').id;
            ApiClient.getPluginConfiguration(pluginId).then((config) => {
                var rooms = config.Rooms.filter(r => r.Name !== room.replace("_", " "));
                config.Rooms = rooms;
                ApiClient.updatePluginConfiguration(pluginId, config).then(
                    (result) => {
                        view.querySelector('.roomTableResultBody').innerHTML = getRoomTableHtml(rooms);
                        Dashboard.processPluginConfigurationUpdateResult(result);
                        var removeButtons = view.querySelectorAll('.removeRoom');
                        removeButtons.forEach((button) => {
                            button.addEventListener('click',
                                (e) => {
                                    e.preventDefault();
                                    removeRoomOnClick(e, view);
                                });
                        });
                    });
            });
        }

        function openTryThisDialog() {
            
            var dlg = dialogHelper.createDialog({
                size: "medium-tall",
                removeOnClose: false,
                scrollY: !0
            });

            dlg.classList.add("formDialog");
            dlg.classList.add("ui-body-a");
            dlg.classList.add("background-theme-a");
            dlg.classList.add("colorChooser");
            dlg.style = "max-width: 25%;max-height: 87%;";

            var html = '';

            html += '<div class="formDialogHeader" style="display:flex">'; 
            html += '<button is="paper-icon-button-light" class="btnCloseDialog autoSize paper-icon-button-light" tabindex="-1"><i class="md-icon"></i></button>';
            html += '<h3 id="headerContent" class="formDialogHeaderTitle">Try this!</h3>';
            html += '</div>';
            html += '<h2 class="sectionTitle sectionTitle-cards">ask home theater...</h2>';
            html += '<div class="scrollY" style="height:75%; width:90%; margin:2em">';
            html += '<h2 class="sectionTitle sectionTitle-cards">Requesting Movies:</h2>';
            html += '<p>...to show the movie {movie_name}</p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<h2 class="sectionTitle sectionTitle-cards">Requesting Television:</h2>';
            html += '<p>...to show the series {series_name}</p>';
            html += '<p style="font-weight: bold">(When the series is displayed, you may ask)</p>';
            html += '<p>...to show Season {season_number}</p>';
            html += '<p style="font-weight: bold">(When the episode list is displayed for the season, you may ask)</p>';
            html += '<p>...play episode {episode_number}.</p>';
            html += '<p> </p>';
            html += '<p style="font-weight: bold">(ask for a specific series, next up episode)</p>';
            html += '<p>...to show the next up episode for {series_name}</p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<h2 class="sectionTitle sectionTitle-cards">Requesting New Media:</h2>';
            html += '<p>...to show new movies</p>';
            html += '<p>...to show new movies from the last {time_period}</p>';
            html += '<p>...to show new movies added in the last {time_period}</p>';
            html += '<p>...to show new tv shows/series</p>';
            html += '<p>...to show new tv shows/series from the last {time_period}</p>';
            html += '<p>...to show new tv shows/series added in the last {time_period}</p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<h2 class="sectionTitle sectionTitle-cards">Requesting Collections:</h2>';
            html += '<p>...to show the {movie_collection_name} collection</p>';
            html += '<p>...to show all the {movie_name} movies</p>';
            html += '<p>...to show the {movie_collection_name} saga</p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>';
            html += '<p></p>'; 
            html += '</div>';


            dlg.innerHTML = html;

            dlg.querySelector('.btnCloseDialog').addEventListener('click',
                () => {
                    dialogHelper.close(dlg);
                });
            
            dialogHelper.open(dlg);
        }
               
        return function(view) {
            view.addEventListener('viewshow',
                () => {

                    var chkEnableParentalControlSpeechRecognition = view.querySelector('.chkEnableParentalControlSpeechRecognition');

                    ApiClient.getPluginConfiguration(pluginId).then((config) => {

                        if (config.Rooms.length > 0) {

                            view.querySelector('.roomTableResultBody').innerHTML = getRoomTableHtml(config.Rooms);

                            view.querySelector('.btnClearAllRoomsDevicesResults').classList.remove('hide');

                            var removeRoomButtons = view.querySelectorAll('.removeRoom');
                            removeRoomButtons.forEach((button) => {
                                button.addEventListener('click',
                                    (e) => {
                                        e.preventDefault();
                                        removeRoomOnClick(e, view);
                                    });
                            });
                        }

                        if (config.UserCorrelations && config.UserCorrelations.length > 0) {

                            view.querySelector('.personalizationTableResultBody').innerHTML = getPersonalizationTableHtml(config.UserCorrelations);

                            view.querySelector('.btnClearAllPersonalizationResults').classList.remove('hide');

                            var removeUserCorrelationButtons = view.querySelectorAll('.removeUser');
                            removeUserCorrelationButtons.forEach((button) => {
                                button.addEventListener('click',
                                    (e) => {
                                        e.preventDefault();
                                        removeUserCorrelationOnClick(e, view);
                                    });
                            });
                        }

                        if (config.EnableParentalControlVoiceRecognition) {
                            chkEnableParentalControlSpeechRecognition.checked = config.EnableParentalControlVoiceRecognition;
                            var lockIcon = view.querySelector('#lockIcon');
                            var lockIconPath = view.querySelector('#lockIconPath');
                            if (chkEnableParentalControlSpeechRecognition.checked) {
                                lockIcon.style.color = "mediumseagreen";
                                lockIconPath.setAttribute("d","M12,17A2,2 0 0,0 14,15C14,13.89 13.1,13 12,13A2,2 0 0,0 10,15A2,2 0 0,0 12,17M18,8A2,2 0 0,1 20,10V20A2,2 0 0,1 18,22H6A2,2 0 0,1 4,20V10C4,8.89 4.9,8 6,8H7V6A5,5 0 0,1 12,1A5,5 0 0,1 17,6V8H18M12,3A3,3 0 0,0 9,6V8H15V6A3,3 0 0,0 12,3Z");
                            } else {
                                lockIcon.style.color = "rgba(0,0,0,0.3)";
                                lockIconPath.setAttribute("d","M18 1C15.24 1 13 3.24 13 6V8H4C2.9 8 2 8.89 2 10V20C2 21.11 2.9 22 4 22H16C17.11 22 18 21.11 18 20V10C18 8.9 17.11 8 16 8H15V6C15 4.34 16.34 3 18 3C19.66 3 21 4.34 21 6V8H23V6C23 3.24 20.76 1 18 1M10 13C11.1 13 12 13.89 12 15C12 16.11 11.11 17 10 17C8.9 17 8 16.11 8 15C8 13.9 8.9 13 10 13Z");
                            } 
                        } else {
                            chkEnableParentalControlSpeechRecognition.checked = false;
                        }
                        
                    });

                    chkEnableParentalControlSpeechRecognition.addEventListener('change',
                        (e) => {
                            e.preventDefault();
                            
                            ApiClient.getPluginConfiguration(pluginId).then((config) => {
                                config.EnableParentalControlVoiceRecognition = chkEnableParentalControlSpeechRecognition.checked;
                                if (chkEnableParentalControlSpeechRecognition.checked) {
                                    lockIcon.style.color = "mediumseagreen";
                                    lockIconPath.setAttribute("d", "M12,17A2,2 0 0,0 14,15C14,13.89 13.1,13 12,13A2,2 0 0,0 10,15A2,2 0 0,0 12,17M18,8A2,2 0 0,1 20,10V20A2,2 0 0,1 18,22H6A2,2 0 0,1 4,20V10C4,8.89 4.9,8 6,8H7V6A5,5 0 0,1 12,1A5,5 0 0,1 17,6V8H18M12,3A3,3 0 0,0 9,6V8H15V6A3,3 0 0,0 12,3Z");
                                } else {
                                    lockIcon.style.color = "rgba(0,0,0,0.3)";
                                    lockIconPath.setAttribute("d", "M18 1C15.24 1 13 3.24 13 6V8H4C2.9 8 2 8.89 2 10V20C2 21.11 2.9 22 4 22H16C17.11 22 18 21.11 18 20V10C18 8.9 17.11 8 16 8H15V6C15 4.34 16.34 3 18 3C19.66 3 21 4.34 21 6V8H23V6C23 3.24 20.76 1 18 1M10 13C11.1 13 12 13.89 12 15C12 16.11 11.11 17 10 17C8.9 17 8 16.11 8 15C8 13.9 8.9 13 10 13Z");
                                } 
                                ApiClient.updatePluginConfiguration(pluginId, config).then((result) => {
                                    Dashboard.processPluginConfigurationUpdateResult(result);
                                });
                            });
                        });

                    view.querySelector('.btnInteractionModel').addEventListener('click',
                        (e) => {
                            e.preventDefault();
                            openInteractionModelDialog();
                        });

                    view.querySelector('.btnRoomsDevices').addEventListener('click',
                        (e) => {
                            e.preventDefault();
                            openRoomModelDialog(view);
                        });

                    view.querySelector('.btnPersonalization').addEventListener('click',
                        (e) => {
                            e.preventDefault();
                            openPersonalizationModelDialog(view);
                        });

                    view.querySelector('.btnClearAllPersonalizationResults').addEventListener('click',
                        (e) => {
                            e.preventDefault();
                            openClearAllConfirmationDialog(e.target.closest('.fab'), view);
                        });

                    view.querySelector('.btnClearAllRoomsDevicesResults').addEventListener('click',
                        (e) => {
                            e.preventDefault();
                            openClearAllConfirmationDialog(e.target.closest('.fab'), view);
                        });

                    view.querySelector('.btnTry').addEventListener('click',
                        (e) => {
                            e.preventDefault();
                            openTryThisDialog();
                        });
                });

        }
    });

