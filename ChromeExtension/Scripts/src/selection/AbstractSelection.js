var AbstractSelection = (function () {
    function AbstractSelection(className) {
        this.selectedElements = new Array();
        this.selectedTags = new Array();
        this.className = className;
    }
    AbstractSelection.prototype.start = function (x, y) { };
    AbstractSelection.prototype.update = function (x, y) { };
    AbstractSelection.prototype.end = function (x, y) { };
    AbstractSelection.prototype.select = function () {
        var _this = this;
        console.log("select");
        console.log(this.selectedElements);
        this.selectedElements.forEach(function (selectedElement) {
            if (selectedElement.type == "marquee") {
                _this.parseSelections(selectedElement);
                _this.highlightSelection(selectedElement);
            }
            else {
                console.log("-=-=====--=");
                var foundElement = $(selectedElement.tagName)[selectedElement.index];
                if (foundElement.tagName.toLowerCase() == "img") {
                    var label = $("<span>Selected</span>");
                    label.css({ position: "absolute", display: "block", background: "yellow", width: "50px", height: "20px", color: "black", "font-size": "12px", padding: "3px 3px", "font-weight": "bold" });
                    $("body").append(label);
                    label.css("top", $(foundElement).offset().top);
                    label.css("left", $(foundElement).offset().left);
                }
                else {
                    $(foundElement).css("background-color", "yellow");
                }
            }
        });
    };
    AbstractSelection.prototype.deselect = function () {
        console.log("deselect");
        this.selectedElements.forEach(function (selectedElement) {
            var foundElement = $(selectedElement.tagName)[selectedElement.index];
            $(foundElement).css("background-color", "");
        });
    };
    AbstractSelection.prototype.highlightCallback = function (parElement, obj) {
        var words = parElement.childNodes;
        console.log(words);
        var wordList = words[obj["txtnIndx"]]["childNodes"];
        console.log("===================================WORDLIST");
        console.log(wordList);
        console.log(obj);
        var word = wordList[obj["wordIndx"]];
        console.log(word);
        $(word).css("background-color", "yellow");
    };
    ;
    AbstractSelection.prototype.parseString = function (node, par, obj, callback) {
        $(node).replaceWith("<words>" + $(node).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
    };
    AbstractSelection.prototype.highlightSelection = function (obj) {
        var tagName = obj["tagName"];
        if (tagName != "WORD" && tagName != "HILIGHT") {
            $(tagName)[obj["index"]].style.backgroundColor = "yellow";
        }
        else {
            var parent = $(obj["par"])[obj["parIndex"]];
            var textN = parent.childNodes[obj["txtnIndx"]];
            if (tagName == "WORD") {
                var word = textN.childNodes[obj["wordIndx"]];
                $(word).css("background-color", "yellow");
            }
            else {
                $(textN).css("background-color", "yellow");
            }
        }
    };
    AbstractSelection.prototype.parseSelections = function (obj) {
        console.log("=======parseSelections====");
        console.log(obj);
        var tagName = obj["tagName"];
        if (tagName != "WORD" && tagName != "HILIGHT") {
            console.log(tagName + "=======================");
            console.log(tagName != "WORD");
            return;
        }
        var parElement = $(obj["par"])[obj["parIndex"]];
        console.log(parElement);
        var textN = parElement.childNodes[obj["txtnIndx"]];
        console.log(textN);
        if (!textN) {
            return;
        }
        if (textN.nodeName == "#text") {
            if (tagName == "WORD") {
                console.log("W------------------------");
                console.log($(textN));
                $(textN).replaceWith("<words>" + $(textN).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
            }
            else if (tagName == "HILIGHT") {
                console.log("H-----------------------");
                console.log($(textN));
                $(textN).replaceWith("<hilight>" + $(textN).text() + "</hilight>");
            }
        }
    };
    AbstractSelection.prototype.selectMarqueeHighlights = function (obj) {
        var tagName = obj["tagName"];
        if (tagName == "WORD") {
            console.log("W------------------------");
            var parElement = $(obj["par"])[obj["parIndex"]];
            var textN = parElement.childNodes[obj["txtnIndx"]];
            this.parseString(textN, parElement, obj, this.highlightCallback);
            console.log(textN);
        }
        else if (tagName == "HILIGHT") {
            var parElement = $(obj["par"])[obj["parIndex"]];
            var textN = parElement.childNodes[obj["txtnIndx"]];
            $(textN).replaceWith("<hilight>" + $(textN).text() + "</hilight>");
            $(parElement.childNodes[obj["txtnIndx"]]).css("background-color", "yellow");
        }
        else {
            console.log(tagName);
            var foundElement = $(tagName)[obj["index"]];
            $(foundElement).css("background-color", "yellow");
        }
    };
    AbstractSelection.prototype.getBoundingRect = function () { return null; };
    AbstractSelection.prototype.analyzeContent = function () { };
    AbstractSelection.prototype.getContent = function () { return null; };
    return AbstractSelection;
})();
