var Background = (function () {
    function Background() {
        this._isOpen = false;
        this._isEnabled = false;
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
    Background.prototype.setExtensionClick = function () {
        var _this = this;
        chrome.browserAction.onClicked.addListener(function () {
            if (_this._isOpen) {
                _this.msgAllTabs({ msg: "hide_menu" });
                _this._isOpen = false;
                chrome.browserAction.setIcon({ path: { 19: "assets/icon.png", 38: "assets/icon.png" } });
            }
            else {
                console.log("=========showMenu==================");
                _this.msgAllTabs({ msg: "show_menu" });
                _this._isOpen = true;
                chrome.browserAction.setIcon({ path: { 19: "assets/icon_active.png", 38: "assets/icon_active.png" } });
            }
        });
    };
    Background.prototype.initAllTabs = function () {
        var _this = this;
        chrome.tabs.query({}, function (tabs) {
            tabs.forEach(function (tab) {
                console.log("=====initTabl=======");
                _this.initTab(tab.id);
            });
        });
    };
    Background.prototype.msgAllTabs = function (message) {
        chrome.tabs.query({}, function (tabs) {
            for (var i = 0; i < tabs.length; ++i) {
                chrome.tabs.sendMessage(tabs[i].id, message);
            }
        });
    };
    Background.prototype.initTab = function (tabId) {
        var _this = this;
        chrome.tabs.executeScript(tabId, { file: "jquery.js" }, function (result) {
            if (chrome.runtime.lastError) {
                console.log("error in loading jquery");
                return;
            }
        });
        chrome.tabs.executeScript(tabId, { file: "cTed.js" }, function (result) {
            if (chrome.runtime.lastError) {
                console.log("error in loading cTed");
                return;
            }
            $.get(chrome.extension.getURL("menu.html"), function (menuData) {
                chrome.tabs.sendMessage(tabId, { msg: "init", data: menuData });
                console.log(_this._isEnabled);
                if (_this._isOpen) {
                    chrome.tabs.sendMessage(tabId, { msg: "show_menu" });
                }
                if (_this._isEnabled && _this._isOpen) {
                    chrome.tabs.sendMessage(tabId, { msg: "enable_selection" });
                }
            });
        });
    };
    Background.prototype.init_tab_listener = function () {
        var _this = this;
        chrome.tabs.onUpdated.addListener(function (tabId, changeInfo, tab) {
            if (changeInfo.status == "complete") {
                _this.initTab(tabId);
            }
        });
        chrome.storage.local.clear(function () {
            chrome.storage.local.set({ selections: [] });
        });
        chrome.runtime.onMessage.addListener(function (request, sender, response) {
            if (request.msg == "store_selection") {
                console.log("store_selection to bg");
                chrome.storage.local.get(function (cTedStorage) {
                    console.log(cTedStorage);
                    cTedStorage["selections"].push(request.data);
                    console.log("storing selection");
                    chrome.storage.local.set(cTedStorage, function () {
                        //     printSelections();
                        console.log("selection stored");
                    });
                });
            }
            if (request.msg == "set_active") {
                _this._isEnabled = request.data;
                console.log(_this._isEnabled);
                if (_this._isEnabled) {
                    _this.msgAllTabs({ msg: "enable_selection" });
                }
                else {
                    _this.msgAllTabs({ msg: "disable_selection" });
                }
            }
            if (request.msg == "view_all")
                chrome.tabs.create({ 'url': chrome.extension.getURL('allselections/index.html') });
            if (request.msg == "tags_changed") {
                _this._tags = request.data;
                _this.msgAllTabs({ msg: "tags_changed", data: request.data });
            }
        });
    };
    return Background;
})();
var bg = new Background();
//# sourceMappingURL=bg.js.map