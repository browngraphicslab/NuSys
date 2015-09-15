/// <reference path="../typings/chrome/chrome.d.ts"/>
/// <reference path="../typings/jquery/jquery.d.ts"/>
/// <reference path="ink/InkCanvas.ts"/>
/// <reference path="selection/LineSelection.ts"/>
/// <reference path="selection/UnknownSelection.ts"/>

class Main {
    
    static DOC_WIDTH: number;
    static DOC_HEIGHT: number;

    prevStrokeType: StrokeType = StrokeType.Line;
    inkCanvas: InkCanvas;
    selection: ISelection;
    canvas: HTMLCanvasElement;

    selections: Array<ISelection> = new Array<ISelection>();
    selectedArray: Array<string> = new Array<string>();
    isSelecting: boolean;
    isEnabled: boolean;
    isCommenting: boolean;
    rectangleArray = [];
    urlGroup: number = Date.now();
    previousSelections: Array<ISelection> = new Array<ISelection>();
    constructor() {

        console.log("Starting NuSys.");
        this.init();
    }

    init() {

        // create and append canvas
        var body = document.body,
            html = document.documentElement;

        Main.DOC_WIDTH = Math.max(body.scrollWidth, body.offsetWidth,
            html.clientWidth, html.scrollWidth, html.offsetWidth);

        Main.DOC_HEIGHT = Math.max(body.scrollHeight, body.offsetHeight,
            html.clientHeight, html.scrollHeight, html.offsetHeight);

        this.canvas = document.createElement("canvas");
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
        this.canvas.style.position = "fixed";
        this.canvas.style.top = "0";
        this.canvas.style.left = "0";           //fixes canvas placements
        this.canvas.style.zIndex = "998";


        this.inkCanvas = new InkCanvas(this.canvas);
        this.selection = new LineSelection(this.inkCanvas);
        chrome.storage.local.get(null,(data) => {
            this.previousSelections = data["selections"];
        });
       
        var currToggle = false;
        chrome.runtime.onMessage.addListener(
            (request, sender, sendResponse) => {
                if (request.msg == "checkInjection")
                    sendResponse({ toggleState: currToggle, objectId: this.urlGroup })

                if (request.toggleState == true) {
                    this.toggleEnabled(true);
                    console.log("show canvas");
                    currToggle = true;
                }
                if (request.toggleState == false) {
                    console.log("hide canvas");
                    this.toggleEnabled(false);
                    currToggle = false;
                }
                if (request.pastPage != null) {
                    sendResponse({ farewell: "received Info" });
                    console.log("this is a requested page");
                    console.log(request.pastPage);

                    this.toggleEnabled(true);
                    var rects = null;

                    $(request.pastPage).each((indx, elem) => {
                        console.log(elem);
                        this.drawPastSelections(elem["boundingRects"]);
                    });
                }
            });
    }

    
    
    toggleEnabled(flag: boolean): void {
        //called to add or remove canvas when toggle has been changed
        this.isEnabled = flag;

        console.log("enabled: " + this.isEnabled);

        if (this.isEnabled) {
            window.addEventListener("mouseup", this.windowUp);
            document.body.addEventListener("mousedown", this.documentDown);
            document.addEventListener("scroll", this.documentScroll);
            this.canvas.addEventListener("mouseup", this.canvasUp);
            document.body.appendChild(this.canvas);
            this.inkCanvas.update();
        } else {
            window.removeEventListener("mouseup", this.windowUp);
            document.body.removeEventListener("mousedown", this.documentDown);
            document.removeEventListener("scroll", this.documentScroll);
            this.canvas.removeEventListener("mouseup", this.canvasUp);
            try {
                document.body.removeChild(this.canvas);
            } catch (e) {
                console.log("no canvas visible." + e)
            }
        }
    }

    mouseMove = (e): void => {
        if (!this.isSelecting) {
            return;
        }
        var currType = StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke.stroke);
        if (currType == StrokeType.MultiLine) {
            document.body.removeChild(this.canvas);
        }
        if (currType != this.prevStrokeType) {
            this.prevStrokeType = currType;
            switch (currType) {
                case StrokeType.Null:
                    this.selection = new LineSelection(this.inkCanvas);
                    break;
                case StrokeType.Line:
                    this.selection = new LineSelection(this.inkCanvas);
                    break;
                case StrokeType.MultiLine:
                    this.selection = new MultiLineSelection(this.inkCanvas);
                    this.selection.start(e.clientX, e.clientY);
                    console.log("switching to multiline Selection");
                    break; 
                case StrokeType.Bracket:
                    this.selection = new BracketSelection(this.inkCanvas, true);
                    console.log("switching to bracket!")
                    break;
                case StrokeType.Marquee:
                    this.selection = new MarqueeSelection(this.inkCanvas, true);
                    console.log("switching to marquee!")
                    break;
                case StrokeType.Scribble:
                    this.selection = new UnknownSelection(this.inkCanvas, true);
                    console.log("switching to unknown!")
                    break;
            }

            this.inkCanvas.redrawActiveStroke();
        }
        this.selection.update(e.clientX, e.clientY);
        document.body.appendChild(this.canvas); 
    }

    checkForOverlaySelection = (x: Number, y: Number): ISelection => {
        var selection = null;
        $(this.selections).each(function (indx, elem) {
            var rect = elem["getBoundingRect"]();
            if (x < rect["x"] + rect["w"] && x > rect["x"] && y < rect["y"] + rect["h"] && y > rect["y"]) {
                console.log("=========================INTERSECT================");
                selection = elem;
            }
        });
        return selection;
        //$(this.rectangleArray).each(function (indx, elem) {

        //    console.log(elem);
        //    console.log(elem["w"]);
        //    console.log(x + "$$$$" + y);
        //    if (x < elem["x"] + elem["w"] && x > elem["x"] && y < elem["y"] + elem["h"] && y > elem["y"]) {
        //        console.log(elem);
        //        console.log(x + "$$$$" + y);
        //        return true;
        //    }
        //});

        //return false;
    }
    documentDown = (e): void => {
        this.selection.start(e.clientX, e.clientY);
        document.body.appendChild(this.canvas);
        console.log("=============docuentDown=====");
        console.log(this.selections);
        var toComment = this.checkForOverlaySelection(e.clientX, e.clientY);      //checks if any selections exist at document drop
        this.canvas.addEventListener("mousemove", this.mouseMove);
        console.log(toComment);
        console.log(this.isCommenting);
        if (toComment == null) {
            this.isSelecting = true;
            this.isCommenting = false;
        }
        else if (!this.isCommenting || this.selection != toComment) {
            if (toComment["comment"] == null) {
                console.log("================NOT COMMENTING====");
                this.selection = toComment;
                this.annotate(toComment, e.clientX, e.clientY);
            }
        }
        console.log(this.isSelecting);
    }

    annotate = (sel: ISelection, x: Number, y: Number): void => {
        console.log("====================annotate====================");
        var commentBox = document.createElement("div");
        commentBox.innerHTML = "<textarea id='"+sel["id"]+"'>I am a textarea</textarea>";
        commentBox.style.left = x + "px";
        commentBox.style.top = y + "px";
      //  commentBox.style.display = "none";
        commentBox.style.position = "absolute";
        commentBox.style.zIndex = "999";
        this.isCommenting = true;
        var area: Element = commentBox.querySelector("textarea");
        this.selection["comment"] = "";
        
        area["onchange"] = () => {

         //   this.selection["comment"] = 
            console.log("========change======");
            var sel = this.selection;
            sel["comment"] = document.getElementById(this.selection["id"])["value"]; 
            this.selections[this.selections.indexOf(this.selection)] = sel;
            this.refreshChromeStorage();
            //chrome.storage.local.get(null, function (data) {
            //    var obj = data;
            //    console.log(obj["selections"]);
            //    $(obj["selections"]).each(function (indx, elem) {
            //        //iterate through takes O(N)
            //        console.log(elem);
            //        if (elem["id"] == this.selection["id"]) {
            //            elem["comment"] = document.getElementById(this.selection["id"])["value"];
            //        }
            //    });
                
            //    console.log(obj);
            //    chrome.storage.local.set(obj);
            //});
            
            console.log(this.selection);
        }


        document.body.appendChild(commentBox);


    }

    refreshChromeStorage = (): void => {
        var obj = {};
        if (this.previousSelections != null) {
            obj["selections"] = this.previousSelections.concat(this.selections);
        }
        else {
            obj["selections"] = this.selections;
        }

        chrome.storage.local.set(obj);
        chrome.storage.local.get(null, function (data) { console.log(data) });
    }
    documentScroll = (e): void => {
        this.inkCanvas.update();
    }

    windowUp = (e): void => {
        if (!this.isSelecting)
            return;

        this.canvas.removeEventListener("mousemove", this.mouseMove);
        this.inkCanvas.removeBrushStroke(this.inkCanvas._activeStroke);
        this.inkCanvas.update();
        window.getSelection().removeAllRanges();

        this.isSelecting = false;
    }

    canvasUp = (e): void => {
        if (!this.isSelecting) {

            return;
        }
        this.canvas.removeEventListener("mousemove", this.mouseMove);
        document.body.removeChild(this.canvas);
        this.selection.end(e.clientX, e.clientY);
        var stroke = this.inkCanvas._activeStroke.stroke.getCopy();
        var currType = StrokeClassifier.getStrokeType(stroke);

        if (currType == StrokeType.Null) {
            console.log("JUST A TAP");
            document.body.appendChild(this.canvas);
            this.inkCanvas.update();
            return;
        }
        else if (currType == StrokeType.Scribble) {

            var segments = stroke.breakUp();
            var p0 = stroke.points[0];
            var p1 = stroke.points[stroke.points.length - 1];
            var line = Line.fromPoint(p0, p1);

            var intersectionCount = 0;
            $.each(segments, function () {
                var intersects = line.intersectsLine(this); 
                if (intersects)
                    intersectionCount++;
            });

            if (intersectionCount > 2) {

                var strokeBB = stroke.getBoundingRect();

                strokeBB.y += stroke.documentOffsetY;

                this.selections.forEach((s: ISelection) => {
                    try {
                        if (s.getBoundingRect().intersectsRectangle(strokeBB)) {
                            s.deselect();
                            console.log("RECT INTERSECTION");

                            var selectionIndex = this.selections.indexOf(s);
                            if (selectionIndex > -1) {
                                this.selections.splice(selectionIndex, 1);
                                this.selectedArray.splice(selectionIndex, 1);
                                this.rectangleArray.splice(selectionIndex, 1);
                                chrome.storage.local.set({ 'curr': this.selectedArray });
                                }
                        }
                    } catch (e) {
                        console.log(e)
                        console.log(this);
                    }
                });
            }
            this.inkCanvas.removeBrushStroke(this.inkCanvas._activeStroke);
        }
        else {
            this.selection["id"] = Date.now();
            this.selections.push(this.selection);
            this.selectedArray.push(this.relativeToAbsolute(this.selection.getContent()));
            console.log(this.selection.getContent());
            console.log(this.relativeToAbsolute(this.selection.getContent()));
            this.rectangleArray.push(this.selection.getBoundingRect());

            chrome.storage.local.set({ 'curr': this.selectedArray });

            var currentDate = new Date();
        }
        var selectionInfo = {};
        selectionInfo["url"] = window.location.protocol + "//" + window.location.host + window.location.pathname;
        console.log(window.location.protocol + '//' + window.location.host);
      
        selectionInfo["selections"] = this.selectedArray;
        selectionInfo["boundingRects"] = this.rectangleArray;
        selectionInfo["date"] = (new Date()).toString();

        this.selection["url"] = window.location.protocol + "//" + window.location.host + window.location.pathname; 
        this.selection["date"] = (new Date()).toString();
        this.selection["title"] = document.title;
        this.selection["brushType"] = StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke.stroke);
        this.selection["urlGroup"] = this.urlGroup;
        this.selection["boundingRects"] = this.rectangleArray;


        console.log("!!!!!!!!!!!!!!!!!!");
        console.log(this.selection);
        // var obj = {};
        //obj[this.objectKeyCount] = selectionInfo;
        //obj["selections"] = this.selections;

        //chrome.storage.local.set(obj);
        //     chrome.storage.local.set(ob
        this.refreshChromeStorage();
        this.selection = new LineSelection(this.inkCanvas);
        this.prevStrokeType = StrokeType.Line;
        chrome.storage.local.get(null, function (data) { console.info(data) });
        document.body.appendChild(this.canvas);
        this.inkCanvas.update();
        this.isSelecting = false;
    }

    relativeToAbsolute(content: string): string {
        //////change relative href of hyperlink and src of image in html string to absolute
        chrome.storage.local.get(null, function (data) { console.info(data) });

        var res = content.split('href="');
        var newval = res[0];
        for (var i = 1; i < res.length; i++) {                  //first change href to absolute
            newval += 'href="';
            if (res[i].slice(0, 4) != "http") {
                newval += window.location.protocol + "//" + window.location.host;
            }
            newval += res[i];
        }


        var src = newval.split('src="');
        var finalval = src[0];
        for (var i = 1; i < src.length; i++) {
            finalval += 'src="';
            if (src[i].slice(0, 4) != "http" && src[i].slice(0,2) != "//") {
                finalval += window.location["origin"];
                
                var path = window.location.pathname;
                var pathSplit = path.split('/');
                var newpath = "";
                var pIndex = pathSplit.length - 1;

                $(pathSplit).each(function (indx, elem) {
                    if (indx < pathSplit.length-1) {
                       newpath += (elem+"/");
                    }
                });
                var newpathSplit = newpath.split("/");
                var p = "";
                pIndex = newpathSplit.length-1;
                if (src[i][0] == "/") {
                    pIndex = pIndex - 1;
                }
                else {
                    src[i] = "/" + src[i];
                }

                $(newpathSplit).each(function (index, elem) {
                    if (index < pIndex) {
                        p += (elem + "/");
                    }
                });
                p = p.substring(0, p.length - 1);
                newpath = p;

                finalval += newpath;
            }
            finalval += src[i];
        }
        return finalval;
    }

    drawPastSelections(rectArray): void {
            $.each(rectArray, (index, rect) => {
                var stroke = new Stroke();
                stroke.points.push({ x: rect.x, y: rect.y });
                stroke.points.push({ x: rect.x + rect.w, y: rect.y + rect.h });
                this.inkCanvas.drawStroke(stroke, new SelectionBrush(rect));
            });
            this.inkCanvas.update();
    }


   
} 

