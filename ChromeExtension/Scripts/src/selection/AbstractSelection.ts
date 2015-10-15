class AbstractSelection implements ISelection {

    id: number;
    url: string;
    className: string;
    tags:string;
    public selectedElements: Array<any> = new Array<any>();
    public selectedTags: Array<String> = new Array<String>();
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
                this.highlightSelection(selectedElement);
            }
            else {
                console.log("-=-=====--=");
                var foundElement = $(selectedElement.tagName)[selectedElement.index];

                if (foundElement.tagName.toLowerCase() == "img") {
                    var label = $("<span>Selected</span>");
                    //label.css({ position: "absolute", display: "block", background: "lightgrey", width: "50px", height: "20px", color: "black", "font-size": "12px" });
                    label.css({ position: "absolute", display: "block", background: "yellow", width: "50px", height: "20px", color: "black", "font-size": "12px", padding: "3px 3px", "font-weight": "bold" });
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
        //callback(par, obj);
    }
    highlightSelection(obj: Object): void {
        var tagName = obj["tagName"];
        if (tagName != "WORD" && tagName != "HILIGHT") {
            $(tagName)[obj["index"]].style.backgroundColor = "yellow";
        }
        else {
            var parent = $(obj["par"])[obj["parIndex"]];
            var textN = parent.childNodes[obj["txtnIndx"]];
            if (tagName == "WORD") {
                var word = textN.childNodes[obj["wordIndx"]];
                $(word).css("background-color","yellow");
            }
            else {
                $(textN).css("background-color", "yellow");
            }
        }

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
      //  var tag = obj["par"] + "," +obj["parIndex"] + ","+obj["txtnIndx"] ;
      //  if (this.selectedTags.indexOf(tag) > -1 || obj["txtnIndx"]==-1) {
      //      return;
      //  }
       // this.selectedTags.push(tag);
       // console.log(tag);
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