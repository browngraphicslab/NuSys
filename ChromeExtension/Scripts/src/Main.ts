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
    rectangleArray = [];
    objectKeyCount: number = 0;

    constructor() {
        console.log("Starting NuSys.");
        this.init();
    }

    
    
    toggleEnabled(flag: boolean): void {
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

    mouseMove = (e):void => {
        var currType = StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke.stroke);
        if (currType != this.prevStrokeType) {
            this.prevStrokeType = currType;
            switch (currType) {
                case StrokeType.Null:
                    this.selection = new LineSelection(this.inkCanvas);
                    break;
                case StrokeType.Line:
                    this.selection = new LineSelection(this.inkCanvas);
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
    }

    documentDown = (e): void => {
        document.body.appendChild(this.canvas);
        this.selection.start(e.clientX, e.clientY);
        this.canvas.addEventListener("mousemove", this.mouseMove);
        this.isSelecting = true;
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
            this.selections.push(this.selection);
            this.selectedArray.push(this.relativeToAbsolute(this.selection.getContent()));
            console.log(this.selection.getContent());
            console.log(this.relativeToAbsolute(this.selection.getContent()));
            this.rectangleArray.push(this.selection.getBoundingRect());

            chrome.storage.local.set({ 'curr': this.selectedArray });

            var currentDate = new Date();

            chrome.storage.local.get(null, function (data) { console.info(data) });
        }
        var selectionInfo = {};
        selectionInfo["url"] = window.location.protocol + "//" + window.location.host + window.location.pathname;
        console.log(window.location.protocol + '//' + window.location.host);
      
        selectionInfo["selections"] = this.selectedArray;
        selectionInfo["boundingRects"] = this.rectangleArray;
        selectionInfo["date"] = (new Date()).toString();
        selectionInfo["title"] = document.title;
        var obj = {};
        obj[this.objectKeyCount] = selectionInfo;

        chrome.storage.local.set(obj);
        this.selection = new LineSelection(this.inkCanvas);
        this.prevStrokeType = StrokeType.Line;

        document.body.appendChild(this.canvas);
        this.inkCanvas.update();
        this.isSelecting = false;
    }

    relativeToAbsolute(content: string): string {
        //////change relative path in html string to absolute
         
        console.log(content);
        var res = content.split('href="');
        var newVal = res[0];
        for (var i = 1; i < res.length; i++) {
            newVal += 'href="';
            if (res[i].slice(0, 4) != "http") {
                newVal += window.location.protocol + "//" + window.location.host;
            }
            newVal += res[i];
        }
        
        return newVal;
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


        this.inkCanvas = new InkCanvas(this.canvas);
        this.selection = new LineSelection(this.inkCanvas);    
        
        chrome.storage.local.get(null, (data) => {
            console.log(data);
            this.objectKeyCount = Object.keys(data).length;
        });   



        var currToggle = false;
        chrome.runtime.onMessage.addListener(
            (request, sender, sendResponse) => {
                if (request.msg == "checkInjection")
                    sendResponse({ toggleState: currToggle })
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
                    console.log("$$$$$$$$$$$$$$$$$$" + request.pastPage);
                    this.toggleEnabled(true);
                    var rects = null;

                    chrome.storage.local.get(null, (data) => {
                        console.info(data);
                        console.log(data[request.pastPage]);
                        rects = data[request.pastPage]["boundingRects"];
                        console.log(rects);
                        this.drawPastSelections(rects);
                    });
                }

            });
    }
} 

