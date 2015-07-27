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

    constructor() {
        console.log("Starting NuSys.");
        this.init();
    }

    mouseMove = (e):void => {
        var currType = StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke.stroke);
        if (currType != this.prevStrokeType) {
            this.prevStrokeType = currType;
            switch (currType) {
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

        if (currType == StrokeType.Scribble) {

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
                            if (selectionIndex > -1)
                                this.selections.splice(selectionIndex, 1);
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
            this.selectedArray.push(this.selection.getContent());
            chrome.storage.local.set({ 'curr': this.selectedArray });

            var currentDate = new Date();
            var obj = {};
            obj[currentDate.toDateString() + currentDate.toTimeString()] = this.selectedArray;
            chrome.storage.local.set(obj);
            chrome.storage.local.get(null, function (data) { console.info(data) });
        }

        this.selection = new LineSelection(this.inkCanvas);
        this.prevStrokeType = StrokeType.Line;

        document.body.appendChild(this.canvas);
        this.inkCanvas.update();
        this.isSelecting = false;
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
        document.body.appendChild(this.canvas);

        this.inkCanvas = new InkCanvas(this.canvas);
        this.selection = new LineSelection(this.inkCanvas);        

        // register listeners
        window.addEventListener("mouseup", this.windowUp);
        document.body.addEventListener("mousedown", this.documentDown);
        document.addEventListener("scroll", this.documentScroll);
        this.canvas.addEventListener("mouseup", this.canvasUp);
        

        var currToggle = true;
        chrome.runtime.onMessage.addListener(
            (request, sender, sendResponse) => {
                if (request.msg == "checkInjection")
                    sendResponse({ toggleState: currToggle })
                if (request.toggleChanged == true) {
                    console.log("show canvas");
                    currToggle = true;
                }
                if (request.toggleChanged == false) {
                    console.log("hide canvas");
                    this.inkCanvas.hide();
                    currToggle = false;
                }
            });
    }
} 

