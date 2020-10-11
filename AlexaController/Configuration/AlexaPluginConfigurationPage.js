define(["require", "loading", "dialogHelper", "formDialogStyle", "emby-checkbox", "emby-select", "emby-toggle"],
    function(require, loading, dialogHelper) {
        var pluginId = "6995F8F3-FD4C-4CB6-A8F4-99A1D8828199";

        function svgIcon(device, appName) {
            var d = "M11,2L6,7L7,8L2,13L7,18L8,17L13,22L18,17L17,16L22,11L17,6L16,7L11,2M10,8.5L16,12L10,15.5V8.5Z";
            if (device.toLowerCase().indexOf("xbox") > -1)
                return "M6.43,3.72C6.5,3.66 6.57,3.6 6.62,3.56C8.18,2.55 10,2 12,2C13.88,2 15.64,2.5 17.14,3.42C17.25,3.5 17.54,3.69 17.7,3.88C16.25,2.28 12,5.7 12,5.7C10.5,4.57 9.17,3.8 8.16,3.5C7.31,3.29 6.73,3.5 6.46,3.7M19.34,5.21C19.29,5.16 19.24,5.11 19.2,5.06C18.84,4.66 18.38,4.56 18,4.59C17.61,4.71 15.9,5.32 13.8,7.31C13.8,7.31 16.17,9.61 17.62,11.96C19.07,14.31 19.93,16.16 19.4,18.73C21,16.95 22,14.59 22,12C22,9.38 21,7 19.34,5.21M15.73,12.96C15.08,12.24 14.13,11.21 12.86,9.95C12.59,9.68 12.3,9.4 12,9.1C12,9.1 11.53,9.56 10.93,10.17C10.16,10.94 9.17,11.95 8.61,12.54C7.63,13.59 4.81,16.89 4.65,18.74C4.65,18.74 4,17.28 5.4,13.89C6.3,11.68 9,8.36 10.15,7.28C10.15,7.28 9.12,6.14 7.82,5.35L7.77,5.32C7.14,4.95 6.46,4.66 5.8,4.62C5.13,4.67 4.71,5.16 4.71,5.16C3.03,6.95 2,9.35 2,12A10,10 0 0,0 12,22C14.93,22 17.57,20.74 19.4,18.73C19.4,18.73 19.19,17.4 17.84,15.5C17.53,15.07 16.37,13.69 15.73,12.96Z";
            if (device.toLowerCase().indexOf("roku") > -1)
                return "M4,5V11H21V5M4,18H21V12H4V18Z";
            if (appName.toLowerCase().indexOf("android") > -1)
                return "M15,9A1,1 0 0,1 14,8A1,1 0 0,1 15,7A1,1 0 0,1 16,8A1,1 0 0,1 15,9M9,9A1,1 0 0,1 8,8A1,1 0 0,1 9,7A1,1 0 0,1 10,8A1,1 0 0,1 9,9M16.12,4.37L18.22,2.27L17.4,1.44L15.09,3.75C14.16,3.28 13.11,3 12,3C10.88,3 9.84,3.28 8.91,3.75L6.6,1.44L5.78,2.27L7.88,4.37C6.14,5.64 5,7.68 5,10V11H19V10C19,7.68 17.86,5.64 16.12,4.37M5,16C5,19.86 8.13,23 12,23A7,7 0 0,0 19,16V12H5V16Z";
            if (appName.toLowerCase().indexOf("amazon") > -1 || device.toLowerCase().indexOf("amazon") > -1)
                return "M15.93,17.09C15.75,17.25 15.5,17.26 15.3,17.15C14.41,16.41 14.25,16.07 13.76,15.36C12.29,16.86 11.25,17.31 9.34,17.31C7.09,17.31 5.33,15.92 5.33,13.14C5.33,10.96 6.5,9.5 8.19,8.76C9.65,8.12 11.68,8 13.23,7.83V7.5C13.23,6.84 13.28,6.09 12.9,5.54C12.58,5.05 11.95,4.84 11.4,4.84C10.38,4.84 9.47,5.37 9.25,6.45C9.2,6.69 9,6.93 8.78,6.94L6.18,6.66C5.96,6.61 5.72,6.44 5.78,6.1C6.38,2.95 9.23,2 11.78,2C13.08,2 14.78,2.35 15.81,3.33C17.11,4.55 17,6.18 17,7.95V12.12C17,13.37 17.5,13.93 18,14.6C18.17,14.85 18.21,15.14 18,15.31L15.94,17.09H15.93M13.23,10.56V10C11.29,10 9.24,10.39 9.24,12.67C9.24,13.83 9.85,14.62 10.87,14.62C11.63,14.62 12.3,14.15 12.73,13.4C13.25,12.47 13.23,11.6 13.23,10.56M20.16,19.54C18,21.14 14.82,22 12.1,22C8.29,22 4.85,20.59 2.25,18.24C2.05,18.06 2.23,17.81 2.5,17.95C5.28,19.58 8.75,20.56 12.33,20.56C14.74,20.56 17.4,20.06 19.84,19.03C20.21,18.87 20.5,19.27 20.16,19.54M21.07,18.5C20.79,18.14 19.22,18.33 18.5,18.42C18.31,18.44 18.28,18.26 18.47,18.12C19.71,17.24 21.76,17.5 22,17.79C22.24,18.09 21.93,20.14 20.76,21.11C20.58,21.27 20.41,21.18 20.5,21C20.76,20.33 21.35,18.86 21.07,18.5Z";
            if (appName.toLowerCase().indexOf("apple") > -1 ||
                device.toLowerCase().indexOf("apple") > -1)
                return "M18.71,19.5C17.88,20.74 17,21.95 15.66,21.97C14.32,22 13.89,21.18 12.37,21.18C10.84,21.18 10.37,21.95 9.1,22C7.79,22.05 6.8,20.68 5.96,19.47C4.25,17 2.94,12.45 4.7,9.39C5.57,7.87 7.13,6.91 8.82,6.88C10.1,6.86 11.32,7.75 12.11,7.75C12.89,7.75 14.37,6.68 15.92,6.84C16.57,6.87 18.39,7.1 19.56,8.82C19.47,8.88 17.39,10.1 17.41,12.63C17.44,15.65 20.06,16.66 20.09,16.67C20.06,16.74 19.67,18.11 18.71,19.5M13,3.5C13.73,2.67 14.94,2.04 15.94,2C16.07,3.17 15.6,4.35 14.9,5.19C14.21,6.04 13.07,6.7 11.95,6.61C11.8,5.46 12.36,4.26 13,3.5Z";
            if (appName.toLowerCase().indexOf("windows") > -1 ||
                device.toLowerCase().indexOf("windows") > -1)
                return "M3,12V6.75L9,5.43V11.91L3,12M20,3V11.75L10,11.9V5.21L20,3M3,13L9,13.09V19.9L3,18.75V13M20,13.25V22L10,20.09V13.1L20,13.25Z";
            if (appName.toLowerCase().indexOf("dlna") > -1 ||
                device.toLowerCase().indexOf("dlna") > -1)
                return "M21.38,12.56H12.85C11.97,12.56 11.1,12.96 10.61,13.61V13.6C10.12,14.28 9.32,14.72 8.41,14.72C6.92,14.72 5.71,13.5 5.71,12C5.71,10.5 6.92,9.31 8.41,9.31C9.32,9.31 10.12,9.75 10.61,10.43V10.42C11.1,11.07 11.97,11.5 12.85,11.5H21.29C21.45,11.5 22,11.4 22,10.67C21.26,6.43 17.1,3.18 12.06,3.18C8.96,3.18 6.19,4.41 4.34,6.35C4.05,6.79 4.35,6.92 4.63,6.96H10.14C11,6.96 11.89,6.54 12.38,5.89V5.91C12.88,5.23 13.67,4.78 14.58,4.78C16.07,4.78 17.28,6 17.28,7.5C17.28,9 16.07,10.2 14.58,10.2C13.67,10.2 12.88,9.75 12.38,9.07V9.08C11.89,8.44 11,8.03 10.14,8.03H4.13L4.15,8.03C4.15,8.03 3.26,8 2.72,8.75C2.3,9.42 2,10.85 2,12C2,13.16 2.17,14.21 2.72,15.27C3.19,16.03 4.15,16 4.15,16H4.11L10.14,16C11,16 11.89,15.58 12.38,14.93V14.94C12.88,14.26 13.67,13.81 14.58,13.81C16.07,13.81 17.28,15.03 17.28,16.5C17.28,18 16.07,19.23 14.58,19.23C13.67,19.23 12.88,18.78 12.38,18.1V18.12C11.89,17.47 11,17.05 10.14,17.05H4.64C4.36,17.09 4.06,17.22 4.32,17.64C6.17,19.58 8.95,20.82 12.06,20.82C17.11,20.82 21.28,17.57 22,13.31C22,12.72 21.59,12.58 21.38,12.56";
            if (appName.toLowerCase().indexOf("chromecast") > -1 ||
                device.toLowerCase().indexOf("chromecast") > -1)
                return "M12,20L15.46,14H15.45C15.79,13.4 16,12.73 16,12C16,10.8 15.46,9.73 14.62,9H19.41C19.79,9.93 20,10.94 20,12A8,8 0 0,1 12,20M4,12C4,10.54 4.39,9.18 5.07,8L8.54,14H8.55C9.24,15.19 10.5,16 12,16C12.45,16 12.88,15.91 13.29,15.77L10.89,19.91C7,19.37 4,16.04 4,12M15,12A3,3 0 0,1 12,15A3,3 0 0,1 9,12A3,3 0 0,1 12,9A3,3 0 0,1 15,12M12,4C14.96,4 17.54,5.61 18.92,8H12C10.06,8 8.45,9.38 8.08,11.21L5.7,7.08C7.16,5.21 9.44,4 12,4M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";

            return d;
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
            html += '<h2 class="learningPhrase" style="font-weight: bold"><b>Alexa, ask home theater to set up a new room.</b></h2>';
            html += '<h2 class="learningPhrase" style="font-weight: bold"><b>Alexa, ask home theater to add a new room.<b/></h2>';
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
                    var deviceNameSelect = dlg.querySelector('#selectEmbyDevice');
                    var device = deviceNameSelect.value;
                    var appName = deviceNameSelect.options[deviceNameSelect.selectedIndex >= 0 ? deviceNameSelect.selectedIndex : 0].dataset.app;

                    var svg = svgIcon(device, appName);

                    var room = {
                        Name: name,
                        DeviceName: device,
                        AppName: appName,
                        AppSvg: svg
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
                if (json.MessageType === "RoomSetupIntent") {
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
            dlg.style = "max-width:50%; max-height:85%";

            var html = '';

            html += '<div class="formDialogHeader" style="display:flex">';
            html += '<button is="paper-icon-button-light" class="btnCloseDialog autoSize paper-icon-button-light" tabindex="-1"><i class="md-icon"></i></button>';
            html += '<h3 id="headerContent" class="formDialogHeaderTitle">Alexa Interaction Model</h3>';
            html += '</div>';

            html += '<h2 class="sectionTitle sectionTitle-cards">Interaction Model:</h2>';
            html += '<div class="inputContainer" style="margin:3em; height:60%">';
            html += '<label class="textareaLabel" for="txtInteractionModel"></label>';
            html += '<textarea is="emby-textarea" id="txtInteractionModel" readonly label="Interaction Model:" class="textarea-mono emby-textarea" rows="10" readonly style="font-size:19px; width:100%; height:100%">';
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
                html += '<td data-title="Name" class="detailTableBodyCell fileCell"><svg style="width:24px;height:24px" viewBox="0 0 24 24"><path fill="rgba(0,0,0,0.3)" d="' + svgIcon(rooms[i].DeviceName, rooms[i].AppName) + '" /></svg></td>'; 
                html += '<td data-title="Name" class="detailTableBodyCell fileCell">' + rooms[i].Name + '</td>';
                html += '<td data-title="Name" class="detailTableBodyCell fileCell-shaded">' + rooms[i].DeviceName + '</td>';
                html += '<td data-title="Name" class="detailTableBodyCell fileCell-shaded">' + rooms[i].AppName + '</td>';
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
                html += '<td data-title="Name" class="detailTableBodyCell fileCell">';
                html += '<svg style="width:24px;height:24px" viewBox="0 0 24 24"><path fill="rgba(0,0,0,0.3)" d="M2,3H22C23.05,3 24,3.95 24,5V19C24,20.05 23.05,21 22,21H2C0.95,21 0,20.05 0,19V5C0,3.95 0.95,3 2,3M14,6V7H22V6H14M14,8V9H21.5L22,9V8H14M14,10V11H21V10H14M8,13.91C6,13.91 2,15 2,17V18H14V17C14,15 10,13.91 8,13.91M8,6A3,3 0 0,0 5,9A3,3 0 0,0 8,12A3,3 0 0,0 11,9A3,3 0 0,0 8,6Z" />';
                html += '</svg>';
                html += '</td>';

                html += '<td data-title="EmbyUserName" class="detailTableBodyCell fileCell">' + correlation.EmbyUserName + '</td>';
                html += '<td data-title="PersonId" class="detailTableBodyCell fileCell-shaded">' + correlation.AlexaPersonId.substring(0, 25) + '*****</td>';
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
            dlg.style = "max-width: 40%;max-height: 87%;";

            var html = '';
            html += '<div class="formDialogHeader" style="display:flex">'; 
            html += '<button is="paper-icon-button-light" class="btnCloseDialog autoSize paper-icon-button-light" tabindex="-1"><i class="md-icon"></i></button>';
            html += '<h3 id="headerContent" class="formDialogHeaderTitle">Try this!</h3>';
            html += '</div>';

            html += '<div style="margin:2em">';
            html += '<p><b>VisualModeTrigger</b>: Captures words that indicate a visual response, such as "show" or "display".</p>';
            html += '<p><b>Anaphor</b>: Captures words that are anaphors representing an item, such as "this", "that", and "it".</p>';
            html += '<p><b>Duration</b>: Words that indicate durations "2 hours", "last four days" , " past weeks" , " the last month"</p>';
            html += '<p><b>Room</b>: Room names setup in the configuration.</p>';
            html += '</div>';

            html += '<div style="margin:2em">'
            html += '<h2 class="sectionTitle sectionTitle-cards">ask home theater...</h2>';
            html += '</div>';

            html += '<div class="scrollY" style="height:75%; width:90%; margin: 1em 2em 3em;">'; 
            
            ApiClient.getJSON(ApiClient.getUrl("InteractionModel")).then(result => {
                var interactionModel = JSON.parse(result.InteractionModel);
                interactionModel.interactionModel.languageModel.intents.sort(function (a, b) {
                    return a < b ? -1 : 1;
                }).forEach(intent => {
                    //We are ignoring HELP. fix it!
                    if (!(intent.name.indexOf("AMAZON") > -1)) {
                        html += '<h2 class="sectionTitle sectionTitle-cards">' + intent.name.replace("_", ": ").replace("Intent", "") + '</h2>';
                        var samples = intent.samples;
                        samples.forEach(sample => {
                            html += '<p>...' + sample + '</p>';
                        });
                    }
                });  

                html += '</div>';
                

                dlg.innerHTML = html;

                dlg.querySelector('.btnCloseDialog').addEventListener('click',
                    () => {
                        dialogHelper.close(dlg);
                    });
            
                dialogHelper.open(dlg);

                console.log(interactionModel);
            }); 
        }
               
        return function(view) {
            view.addEventListener('viewshow',
                () => {

                    var chkEnableParentalControlSpeechRecognition = view.querySelector('.chkEnableParentalControlSpeechRecognition');
                    var chkEnableActivityLogNotifications         = view.querySelector('.chkEnableActivityLogNotifications');

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


                        if (config.EnableServerActivityLogNotifications) {
                            chkEnableActivityLogNotifications.checked = config.EnableServerActivityLogNotifications; 
                        } else {
                            chkEnableActivityLogNotifications.checked = false;
                        }


                        if (config.EnableParentalControlVoiceRecognition) {
                            chkEnableParentalControlSpeechRecognition.checked = config.EnableParentalControlVoiceRecognition; 
                        } else {
                            chkEnableParentalControlSpeechRecognition.checked = false;
                        }
                        
                    });


                    chkEnableActivityLogNotifications.addEventListener('change',
                        (e) => {
                            e.preventDefault();
                            ApiClient.getPluginConfiguration(pluginId).then((config) => {
                                config.EnableServerActivityLogNotifications = chkEnableActivityLogNotifications.checked;
                                ApiClient.updatePluginConfiguration(pluginId, config).then((result) => {
                                    Dashboard.processPluginConfigurationUpdateResult(result);
                                });
                            });
                        });

                    chkEnableParentalControlSpeechRecognition.addEventListener('change',
                        (e) => {
                            e.preventDefault();
                            ApiClient.getPluginConfiguration(pluginId).then((config) => {
                                config.EnableParentalControlVoiceRecognition = chkEnableParentalControlSpeechRecognition.checked;
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

