/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

//To check if cordova is loaded: if(window.cordova)
//To check if DeviceReady has been fired once: if(isDeviceReady)
var isDeviceReady = false;

var SUCCESS = "Success";
var FAILURE = "Failure";
var EXCEPTION = "Exception";

var app = {
    // Application Constructor
    initialize: function ()
    {
        app.bindEvents();
        app.uiinit();
        app.applyKO();
    },
    // Bind Event Listeners
    //
    // Bind any events that are required on startup. Common events are:
    // 'load', 'deviceready', 'offline', and 'online'.
    bindEvents: function ()
    {
        document.addEventListener('deviceready', app.onDeviceReady, false);
    },
    // deviceready Event Handler
    //
    // The scope of 'this' is the event. In order to call the 'receivedEvent'
    // function, we must explicitly call 'app.receivedEvent(...);'
    onDeviceReady: function ()
    {
        isDeviceReady = true;

        appViewModel.username(window.localStorage.getItem("username"));
    },
    uiinit: function ()
    {
        $(function ()
        {
            //$("[data-role='footer']").html("<h4 class='ui-title' role='heading' aria-level='1'>XYZ</h4><a href='#' onclick='app.test();'>Test</a>");
            //$.mobile.changePage("#page_allsongs", { transition: "slidefade", changeHash: false });

            $("[data-role='footer']").html("<h4 class='ui-title' role='heading' aria-level='1'>&copy; " + new Date().getFullYear() + "</h4>");
            $("body>[data-role='panel']").panel().enhanceWithin();
        });
    },
    applyKO: function ()
    {
        ko.applyBindings(appViewModel);
    },
    test: function ()
    {
        appViewModel.username("SSsdsdS");
    },
    openPanel: function ()
    {
        if (appViewModel.username() == null)
        {
            $("#leftpanel1").panel("open");
        }
        else
        {
            $("#leftpanel2").panel("open");
        }
    },
    ratingsRadioButtonOnClick: function (event)
    {
        app.ratingAddUpdate(event.target.dataset.songfilename, event.target.value);
    },
    ratingAddUpdate: function (songfilename, rating)
    {
        $.mobile.loading("show");
        services.SingItCompleteService("AddUpdateSongRating", JSON.stringify({ songrater: appViewModel.username(), songfilename: songfilename, rating: rating }), app.ratingAddUpdateSuccess, app.ratingAddUpdateError);
    },
    ratingAddUpdateSuccess: function (x)
    {
        $.mobile.loading("hide");
    },
    ratingAddUpdateError: function (x)
    {
        $.mobile.loading("hide");
        navigator.notification.alert("Please check your network connection and try again later!", null, "Service Failure");
    },
    gotoMySongs: function ()
    {
        $.mobile.changePage("#page_mysongs", { transition: "slidefade", changeHash: false });
    },
    gotoAllSongs: function ()
    {
        $.mobile.changePage("#page_allsongs", { transition: "slidefade", changeHash: false });

        $.mobile.loading("show");
        $("#page_allsongs_listview").empty().listview("refresh").enhanceWithin();

        services.SingItCompleteService("GetAllSongsGroupedBySongTitle", JSON.stringify({ username: appViewModel.username() }), app.gotoAllSongsSuccess, app.gotoAllSongsError);
    },
    gotoAllSongsSuccess: function (x)
    {
        $("#page_allsongs_listview").append(x.Output).listview("refresh").enhanceWithin();
        $.mobile.loading("hide");
    },
    gotoAllSongsError: function (x)
    {
        $.mobile.loading("hide");
        navigator.notification.alert("Please check your network connection and try again later!", null, "Service Failure");
    },
    gotoNewSong: function ()
    {
        $.mobile.changePage("#page_newsong", { transition: "slidefade", changeHash: false });
    },
    gotoRecommendedSong: function ()
    {
        $.mobile.changePage("#page_recommendedsong", { transition: "slidefade", changeHash: false });
        $.mobile.loading("show");
        services.SingItCompleteService("GetRecommendedSongForUser", JSON.stringify({ username: appViewModel.username() }), app.gotoRecommendedSongSuccess, app.gotoRecommendedSongError);
    },
    gotoRecommendedSongSuccess: function (x)
    {
        $.mobile.loading("hide");
        if (x.Output == FAILURE || x.Output == EXCEPTION)
        {
            navigator.notification.alert("Sorry, we couldn't find you a recommended song!", null, "Recommendation Failure");
        }
        else
        {
            $("#page_newsong_songtitle").val(x.Output);
            appViewModel.songtitle($("#page_newsong_songtitle").val());
            navigator.notification.confirm("Hurray!!! We found you the following recommendation:\n" + x.Output + "\nWould you like to record this song?", app.gotoRecommendedSongOnConfirm, "Recommendation Found!", ["Yes", "No"]);
        }
    },
    gotoRecommendedSongError: function (x)
    {
        $.mobile.loading("hide");
        navigator.notification.alert("Please check your network connection and try again later!", null, "Service Failure");
    },
    gotoRecommendedSongOnConfirm: function (x)
    {
        if (x == 1)
        {
            $.mobile.changePage("#page_newsong", { transition: "slidefade", changeHash: false });
        }
    },
    recordNewSong: function ()
    {
        appViewModel.songtitle($("#page_newsong_songtitle").val());
        appViewModel.songgenre($("#page_newsong_selectgenre").val());
        audioCaptureAndUpload.capture();
    },
    login: function ()
    {
        services.SingItCompleteService("Validate", JSON.stringify({ username: $("#page_login_username").val(), password: $("#page_login_password").val() }), app.loginSuccess, app.loginError);
    },
    loginSuccess: function (x)
    {
        if (x.Output == FAILURE || x.Output == EXCEPTION)
        {
            navigator.notification.alert("Please enter valid credentials!", null, "Login Failure");
        }
        else
        {
            appViewModel.username(JSON.parse(x.Output).username);
            window.localStorage.setItem("username", appViewModel.username());
            app.gotoAllSongs();
        }
    },
    loginError: function (x)
    {
        navigator.notification.alert("Please enter valid credentials!", null, "Login Failure");
    },
    logout: function ()
    {
        appViewModel.username(null);
        window.localStorage.setItem("username", null);
        $.mobile.changePage("#page_login", { transition: "slideup", changeHash: false });
    }
};

var appViewModel = {
    username: ko.observable(null),
    songtitle: ko.observable(null),
    songgenre: ko.observable(null)
};

var ServiceURLLocalhost = "http://localhost:81";
var ServiceURLLocalhostIP = "http://192.168.1.250:81";
var ServiceURLRemote = "http://singit.azurewebsites.net";

var services = {
    SingItCompleteService: function (methodName, InputData, successCB, errorCB)
    {
        $.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: ServiceURLLocalhostIP + "/SingItCompleteService.svc/" + methodName,
            data: InputData,
            dataType: "json",
            success: successCB,
            error: errorCB
        });
    },
    serviceTest: function ()
    {
        services.SingItCompleteService("TestValidate", JSON.stringify({ username: "anuj", password: "anuj" }), services.successTest, services.errorTest);
    },
    successTest: function (x)
    {
        alert(JSON.stringify(x));
    },
    errorTest: function (x)
    {
        alert(JSON.stringify(x));
    }
};

var audioCaptureAndUpload = {

    capture: function ()
    {
        navigator.device.capture.captureAudio(audioCaptureAndUpload.captureSuccess, audioCaptureAndUpload.captureError, { limit: 1 });
    },
    captureSuccess: function (files)
    {
        var i, path, length;
        for (i = 0, length = files.length; i < length; i++)
        {
            var options = new FileUploadOptions();
            options.fileKey = "file";
            options.fileName = files[i].name;
            options.mimeType = files[i].type;
            options.headers = { Connection: "close" };
            options.chunkedMode = false;

            var params = {};
            params.username = appViewModel.username();
            params.songtitle = appViewModel.songtitle();
            params.songgenre = appViewModel.songgenre();
            options.params = params;

            var ft = new FileTransfer();
            ft.upload(files[i].fullPath, encodeURI(ServiceURLLocalhostIP + "/FileTransferService.svc/UploadSong"), audioCaptureAndUpload.win, audioCaptureAndUpload.fail, options, true);
        }
    },
    captureError: function (error)
    {
        navigator.notification.alert("An error has occurred: Code = " + error.code, null, "Capture Error");
    },
    win: function (r)
    {
        console.log("Code = " + r.responseCode);
        console.log("Response = " + r.response);
        console.log("Sent = " + r.bytesSent);
    },
    fail: function (error)
    {
        navigator.notification.alert("An error has occurred: Code = " + error.code, null, "Upload Error");
        console.log("upload error source " + error.source);
        console.log("upload error target " + error.target);
    }
};