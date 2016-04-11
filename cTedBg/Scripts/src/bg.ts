class Background {
    _isOpen: Boolean = false;
    _isEnabled: Boolean = false;
    _tags: string;
    constructor() {
        console.log("starting background");
        this.initAllTabs();
        this.setExtensionClick();
        this.init_tab_listener();
    }      

    //add_message_listener() {
    //    chrome.extension.onMessage.addListener(function (request, sender, response) {
    //        if (){
    //        }
    //    });
    //}

    setExtensionClick() {
        chrome.browserAction.onClicked.addListener(()=> {
            if (this._isOpen) {
                this.msgAllTabs({ msg: "hide_menu" });
                this._isOpen = false;
           //     chrome.browserAction.setIcon({ path: { 19: "assets/icon.png", 38: "assets/icon.png" } });
            }
            else {
                console.log("=========showMenu==================");
                this.msgAllTabs({ msg: "show_menu" });
                this._isOpen = true;
                chrome.browserAction.setIcon({ path: { 19: "assets/icon_active.png", 38: "assets/icon_active.png" } });
            }
        });
    }


    initAllTabs() {
        chrome.tabs.query({}, (tabs) => {
            tabs.forEach((tab) => {
                console.log("=====initTabl=======");
                this.initTab(tab.id);
            });
       });
    }

   msgAllTabs(message) {
        chrome.tabs.query({}, function (tabs) {
            for (var i = 0; i < tabs.length; ++i) {
                chrome.tabs.sendMessage(tabs[i].id, message);
            }
        });
    }

    initTab(tabId){
        chrome.tabs.executeScript(tabId, { file: "jquery.js" }, (result) => {

            if (chrome.runtime.lastError) {
                console.log("error in loading jquery");
                return;
            }

        });
        chrome.tabs.executeScript(tabId, { file: "jquery.js" }, (result) => {

            if (chrome.runtime.lastError) {
                console.log("error in loading jquery");
                return;
            }

        });
        chrome.tabs.executeScript(tabId, { file: "cTed.js" }, (result) => {
            if (chrome.runtime.lastError) {
                console.log("error in loading cTed");
                return;
            }
            $.get(chrome.extension.getURL("menu.html"), (menuData) => {
                chrome.tabs.sendMessage(tabId, { msg: "init", data: menuData });
                console.log(this._isEnabled);

                if (this._isOpen) {
                    chrome.tabs.sendMessage(tabId, { msg: "show_menu" });
                }
                if (this._isEnabled && this._isOpen) {
                    chrome.tabs.sendMessage(tabId, { msg: "enable_selection" });
                }
            });
        });


   }

    init_tab_listener() {
        chrome.tabs.onUpdated.addListener((tabId, changeInfo, tab) => {
            if (changeInfo.status == "complete") {
                this.initTab(tabId);
            }
        });
        chrome.storage.local.clear(function () {
            chrome.storage.local.set({ selections: [] });
        });

        chrome.runtime.onMessage.addListener((request, sender, response) => {
            if (request.msg == "store_selection") {
                console.log("store_selection to bg");
                chrome.storage.local.get(function (cTedStorage) {
                    console.log(cTedStorage);
                    cTedStorage["selections"].push(request.data);
                    console.log("storing selection");
                    chrome.storage.local.set(cTedStorage, function () {
                        //     printSelections();
                        console.log(cTedStorage);
                        console.log("selection stored");
                    });
                });

            }

            if (request.msg == "edit_selection") {
                console.log("edit_selection");
                console.log(request.data);
                chrome.storage.local.get(function (cTedStorage) {
                    var selections = cTedStorage["selections"];
                    for (var i = 0; i < selections.length; i++) {
                        if (selections[i].id == request.data.id) {
                            selections[i] = request.data;
                            continue;
                        }
                    }
                    cTedStorage["selections"] = selections;
                    chrome.storage.local.set(cTedStorage, function () {
                        console.log("selection edited");
                    });
                });
            }

            if (request.msg == "remove_selection") {
                console.log("remove_selection to bg");
                chrome.storage.local.get(function (cTedStorage) {
                    console.log(cTedStorage);
                    console.log(request.data);
                    for (var i = 0; i < cTedStorage["selections"].length; i++) {
                        console.log(cTedStorage["selections"][i]["id"]);
                        if (cTedStorage["selections"][i]["id"] == request.data) {
                            cTedStorage["selections"].splice(i, 1);
                            break;
                        }
                    }
                    chrome.storage.local.set(cTedStorage, function () {
                        console.log("selection removed");
                    });
                });
            }

            if (request.msg == "set_active") {
                this._isEnabled = request.data;
                console.log(this._isEnabled);
                if (this._isEnabled) {
                    this.msgAllTabs({ msg: "enable_selection" });
                } else {
                    this.msgAllTabs({ msg: "disable_selection" });
                }
            }

            if (request.msg == "view_all")
                chrome.tabs.create({ 'url': chrome.extension.getURL('allselections/index.html') });

            if (request.msg == "tags_changed") {
                this._tags = request.data;
                this.msgAllTabs({ msg: "tags_changed", data: request.data });
            }
        });
    }



     
    
}


var bg = new Background();


 
