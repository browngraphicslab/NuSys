
var	html = document.documentElement;
var isRunning = false;

if(!isRunning) {
    chrome.tabs.executeScript({file: 'lib/jquery-1.11.2.js'});
    chrome.tabs.executeScript({file: 'src/Rectangle.js'});
    chrome.tabs.executeScript({file: 'src/Line.js'});
    chrome.tabs.executeScript({file: 'src/CircleBrush.js'});
    chrome.tabs.executeScript({file: 'src/LineBrush.js'});
    chrome.tabs.executeScript({file: 'src/SelectionBrush.js'});
    chrome.tabs.executeScript({file: 'src/BrushStroke.js'});
    chrome.tabs.executeScript({file: 'src/Vector2.js'});
    chrome.tabs.executeScript({file: 'src/BracketSelection.js'});
    chrome.tabs.executeScript({file: 'src/LineSelection.js'});
    chrome.tabs.executeScript({file: 'src/MarqueeSelection.js'});
    chrome.tabs.executeScript({file: 'src/UnknownSelection.js'});
    chrome.tabs.executeScript({file: 'src/MarqueeBrush.js'});
    chrome.tabs.executeScript({file: 'src/StrokeClassifier.js'});
    chrome.tabs.executeScript({file: 'src/HashMap.js'});
    chrome.tabs.executeScript({file: 'src/HighlightBrush.js'});
    chrome.tabs.executeScript({file: 'src/InkCanvas.js'});
    chrome.tabs.executeScript({file: 'src/Statistics.js'});
    chrome.tabs.executeScript({file: 'src/Stroke.js'});
    chrome.tabs.executeScript({file: 'src/DomUtil.js'});
    chrome.tabs.executeScript({file: 'src/main.js'});
    isRunning = true;


    document.getElementById("btnSend").addEventListener("click", function(){
        console.log(chrome.extension.getBackgroundPage().port);
        console.log(selections)
    });
}

var localPort;