

class NullSelection extends AbstractSelection {

    _startX: number = 0;
    _startY: number = 0;
    _endX: number = 0;
    _endY: number = 0;
    _content: string = "";

    constructor() {
        super();
        console.log("Line SELECTION");
    }

    start(x: number, y: number): void {
        this._startX = x;
        this._startY = y;
        console.log("line start" + x + ":" + y);
    }

    end(x: number, y: number): void {
        this._endX = x;
        this._endY = y;

        this.analyzeContent();
    }
    getContent(): string {
        return this._content;
    }
    isPointAbove(p: Point): boolean {
        return false;
    }


    analyzeContent(): void{
        console.log("null selection... only for image");
        var img = document.elementFromPoint(this._startX, this._startY);
        console.info(img);
        console.log(img.tagName);
        if (img.tagName == "IMG") {
            console.log("IMAGE SELECTED!");
            var index = $(img.tagName).index(img);
            var obj = { type: "null", tagName: img.tagName, index: index };
            $(img).attr("src", $(img).prop('src'));
            $(img).removeAttr("srcset");
            this._content = $(img).prop('outerHTML');
            var label = $("<span class='wow'>Selected</span>");
            label.css({ position: "absolute", display: "block", background: "yellow", width: "50px", height: "20px", color: "black", "font-size": "12px", padding: "3px 3px", "font-weight": "bold" });
            $("body").append(label);
            label.css("top", ($(img).offset().top - 5) + "px");
            label.css("left", ($(img).offset().left - 5) + "px");
            this.selectedElements.push(obj);
       }
    }
}