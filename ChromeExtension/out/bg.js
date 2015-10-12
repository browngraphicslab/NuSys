var isOpen;
var _isOpen = false;
var _isActive = false;
var _tags = "";

chrome.storage.local.clear(function() {
    chrome.storage.local.set({ selections: [] });
});


chrome.extension.onMessage.addListener(function (request, sender, response) {

    if (request.msg == "tags_changed") {
        _tags = request.data;
        chrome.tabs.query({}, function(tabs) {
            for (var i = 0; i < tabs.length; ++i) {
                chrome.tabs.sendMessage(tabs[i].id, { msg: "tags_changed", data: request.data });
            }
        });
    }

    if (request.msg == "view_all")
        chrome.tabs.create({ 'url': chrome.extension.getURL('allselections/index.html') });

    if (request.msg == "set_active")
        setActive(request.data);

    if (request.msg == "query_active")
        response(_isActive);

    if (request.msg == "store_selection") {
        
        chrome.storage.local.get(function (cTedStorage) {
            cTedStorage.selections.push(request.data);
            console.log("storing selection");
            chrome.storage.local.set(cTedStorage, function() {
                printSelections();
                console.log("selection stored");
            });
        });
        
    }

    if (request.msg == "delete_selection") {
        chrome.storage.local.get("selections", function (selections) {
            selections = selections.filter(function (obj) {
                return obj.url !== sender.tab.url;
            });
            chrome.storage.local.set({selections: selections});
        });
    }

    if (request.msg == "clear_page_selections") {
        console.log(sender.tab.url);
    }
});

chrome.browserAction.onClicked.addListener(function() {
    if (!_isOpen) {
        initAllTabs();
        _isOpen = true;
    } else {
        msgAllTabs({ msg: "hide_menu" });
        setActive(false);
        _isOpen = false;
    }
});

chrome.tabs.onUpdated.addListener(function(tabId, changeInfo, tab) {
    if (_isOpen && changeInfo.status == "complete") {
        initTab(tabId);
    }
});

function initAllTabs() {
    chrome.tabs.query({}, function(tabs) {
        tabs.forEach(function(tab) {
            initTab(tab.id);
        });
    });
}

function initTab(tabId) {
    chrome.tabs.sendMessage(tabId, { msg: "check_injection" }, function (response) {

        if (!response) {
            chrome.tabs.executeScript(tabId, { file: "jquery.js" }, function (result) {
                if (chrome.runtime.lastError) { // or if (!result)
                    return;
                }
            });
            chrome.tabs.executeScript(tabId, { file: "javascriptUtil.js" }, function (result) {
                if (chrome.runtime.lastError) { // or if (!result)
                    return;
                }
            });
            chrome.tabs.executeScript(tabId, { file: "NuSysChromeExtension.js" }, function (result) {
                if (chrome.runtime.lastError) { // or if (!result)
                    return;
                }
                $.get(chrome.extension.getURL("menu.html"), function (menuData) {

                    chrome.tabs.sendMessage(tabId, { msg: "init", data: menuData }, function () {
                        chrome.tabs.sendMessage(tabId, { msg: "show_menu" });
                        chrome.tabs.sendMessage(tabId, { msg: "tags_changed", data: _tags });
                        if (_isActive) {
                            
                            chrome.storage.local.get(function (cTedStorage) {
                                chrome.tabs.get(tabId, function(tab) {
                                    var selections = cTedStorage.selections.filter(function (obj) {
                                        return obj.url == tab.url;
                                    });
                                    chrome.tabs.sendMessage(tabId, { msg: "set_selections", data: selections }, function () {
                                        chrome.tabs.sendMessage(tabId, { msg: "enable_selections" });
                                        
                                    });
                                });
                            });
                        }
                    });
                });
            });
        } else {
            chrome.tabs.sendMessage(tabId, { msg: "show_menu" });
        }
    });
}

function msgAllTabs(message) {
    chrome.tabs.query({}, function(tabs) {
        for (var i = 0; i < tabs.length; ++i) {
            chrome.tabs.sendMessage(tabs[i].id, message);
        }
    });
}

function setActive(active) {
    _isActive = active;

    // change icon based on current state
    if (_isActive) {
        chrome.browserAction.setIcon({ path: { 19: "assets/icon_active.png", 38: "assets/icon_active.png" } });
        
        chrome.storage.local.get(function (cTedStorage) {
            chrome.tabs.query({}, function (tabs) {
                tabs.forEach(function(tab) {
                    var selections = cTedStorage.selections.filter(function(obj) {
                        return obj.url === tab.url;
                    });
                    chrome.tabs.sendMessage(tab.id, { msg: "set_selections", data: selections });
                    
                });
            });
            msgAllTabs({ msg: "enable_selections" });
        });

    } else {
        chrome.browserAction.setIcon({ path: { 19: "assets/icon.png", 38: "assets/icon.png" } });
        msgAllTabs({ msg: "disable_selections" });
    }
}

function isActive() {
    return _isActive;
}

function log(msg, response) {
    console.log(msg);
}

function printSelections() {
    console.log("=== SELECTIONS ===");
    chrome.storage.local.get(function (cTedStorage) {
        console.log(cTedStorage);
        cTedStorage.selections.forEach(function (val) {
            console.log(val);
        });
    });
    console.log("==================");
}

