class AbstractSelection implements ISelection {

    id: number;
    url: string;
    className: string;
    tags:string;
    public selectedElements:Array<any> = new Array<any>();

    constructor(className) {
        this.className = className;
    }

    start(x: number, y: number): void {}

    update(x: number, y: number): void {}

    end(x: number, y: number): void {}

    select(): void {
        console.log("select");  
        console.log(this.selectedElements);
        this.selectedElements.forEach((selectedElement) => {
            if (selectedElement.type == "marquee") {
                this.parseSelections(selectedElement);
            }
            else {
                var foundElement = $(selectedElement.tagName)[selectedElement.index];

                if (foundElement.tagName.toLowerCase() == "img") {
                    var label = $("<span>Selected</span>");
                    label.css({ position: "absolute", display: "block", background: "lightgrey", width: "50px", height: "20px", color: "black", "font-size": "12px" });
                    $("body").append(label);
                    label.css("top", $(foundElement).offset().top);
                    label.css("left", $(foundElement).offset().left);
                } else {
                    $(foundElement).css("background-color", "yellow");
                }
            }

      
        });
    }

    deselect(): void {
        console.log("deselect");
        this.selectedElements.forEach((selectedElement) => {
            var foundElement = $(selectedElement.tagName)[selectedElement.index];
            $(foundElement).css("background-color", "");
        });
    }
    highlightCallback(parElement: Element, obj: Object): void {
       var words = parElement.childNodes;//[obj["txtnIndx"]];

            console.log(words);
            var wordList = words[obj["txtnIndx"]]["childNodes"];
            console.log("===================================WORDLIST");
            console.log(wordList);
            console.log(obj);
            var word = wordList[obj["wordIndx"]];
            console.log(word);
        //    $(word).replaceWith("<word style=\"background-color: yellow\">" + word[obj["wordIndx"]].textContent + "</word>");
         //   word["style"]["backgroundColor"] = "yellow";
            $(word).css("background-color" ,"yellow");
      

    };
    parseString(node: Node, par: Element, obj:Object, callback): void {
        
        $(node).replaceWith("<words>" + $(node).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
        callback(par, obj);
    }

    parseSelections(obj: Object): void {
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
        if (textN.nodeName == "#text") {
            if (tagName == "WORD") {
                console.log("W------------------------");
                $(textN).replaceWith("<words>" + $(textN).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
            }
            else if (tagName == "HILIGHT") {
                $(textN).replaceWith("<hilight>" + $(textN).text() + "</hilight>");
            }
        }

    }   



    
    selectMarqueeHighlights(obj: Object): void {
        var tagName = obj["tagName"];
        if (tagName == "WORD") {
            console.log("W------------------------");
            var parElement = $(obj["par"])[obj["parIndex"]];
            var textN = parElement.childNodes[obj["txtnIndx"]];
            this.parseString(textN, parElement, obj, this.highlightCallback);
            console.log(textN);
            
            //   $(parElement.childNodes[obj["txtnIndx"]].childNodes[obj["wordIndx"]])["style"]["backgroundColor"] = "yellow";
           //console.info(word);
            //$(word).replaceWith("<word style='background-color: 'yellow''>" + word.textContent + "</word>");
            ///console.log(word);
             // $(textN).replaceWith("<hilight>" + $(textN).text() + "</hilight>");
            //$(parElement.childNodes[obj["txtnIndx"]]).css("background-color", "yellow");
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
    }

    getBoundingRect(): Rectangle { return null; }

    analyzeContent(): void { }

    getContent(): string { return null; }

}