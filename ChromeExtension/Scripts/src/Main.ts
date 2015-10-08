﻿/// <reference path="../typings/chrome/chrome.d.ts"/>
/// <reference path="../typings/jquery/jquery.d.ts"/>
/// <reference path="ink/InkCanvas.ts"/>
/// <reference path="ink/StrokeType.ts" />
/// <reference path="selection/LineSelection.ts"/>
/// <reference path="selection/UnknownSelection.ts"/>

class Main {
    
    static DOC_WIDTH: number;
    static DOC_HEIGHT: number;

    prevStrokeType: StrokeType = StrokeType.Line;
    currentStrokeType: StrokeType = StrokeType.Line;
    inkCanvas: InkCanvas;
    selection: AbstractSelection;
    canvas: HTMLCanvasElement;
    menuIframe:HTMLIFrameElement;
    menu:any;

    selections: Array<ISelection> = new Array<ISelection>();
    isSelecting: boolean;
    isEnabled: boolean;
    isMenuVisible: boolean;
    urlGroup: number = Date.now();
    previousSelections: Array<ISelection> = new Array<ISelection>();
    
    constructor() {
        console.log("Starting NuSys.");

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
        
        chrome.storage.local.get(null, (data) => {
            this.previousSelections = data["selections"];
        });

        chrome.runtime.onMessage.addListener(
            (request, sender, sendResponse) => {

                console.log(request);

                if (request.msg == "check_injection")
                    sendResponse(true);

                if (request.msg == "init") {
                    this.init(request.data);
                    sendResponse();
                }

                if (request.msg == "show_menu") {
                    this.showMenu();
                }

                if (request.msg == "hide_menu") {
                    this.hideMenu();
                }

                if (request.msg == "enable_selections") {
                    this.toggleEnabled(true);
                }

                if (request.msg == "disable_selections") {
                    this.toggleEnabled(false);
                }

                if (request.msg == "set_selections") {
                    this.selections = [];
                    request.data.forEach((d) => {

                        var ls = null;
                        if (d["className"] == "LineSelection")
                            ls = new LineSelection(this.inkCanvas);

                        if (d["className"] == "BracketSelection")
                            ls = new BracketSelection(this.inkCanvas);

                        if (d["className"] == "MarqueeSelection")
                            ls = new MarqueeSelection(this.inkCanvas);

                        if (d["className"] == "MultiLineSelection")
                            ls = new MultiLineSelection(this.inkCanvas);

                        
                        $.extend(ls, d);
                        ls._inkCanvas = this.inkCanvas;
                        var stroke = new Stroke();
                        console.log(ls);
                        $.extend(stroke, ls._brushStroke.stroke);
                        ls._brushStroke.stroke = stroke;

                        this.selections.push(ls);
                    });
                    sendResponse();
                }
            });
    }

    init(menuHtml:string) {

        this.menuIframe = <HTMLIFrameElement>$("<iframe frameborder=0>")[0];
        document.body.appendChild(this.menuIframe);
        this.menu = $(menuHtml)[0];
        $(this.menuIframe).css({ position: "fixed", top: "1px", right: "1px", width: "410px", height: "90px", "z-index": 1001 });
        $(this.menuIframe).contents().find('html').html(this.menu.outerHTML);
        $(this.menuIframe).css("display", "none");

        $(this.menuIframe).contents().find("#btnLineSelect").click(() => {
            console.log("switching to multiline selection");
            this.currentStrokeType = StrokeType.MultiLine;
        });

        $(this.menuIframe).contents().find("#btnBlockSelect").click(() => {
            this.currentStrokeType = StrokeType.Bracket;
        });

        $(this.menuIframe).contents().find("#btnClear").click(() => {
            chrome.runtime.sendMessage({ msg: "clear_page_selections" });
        });

        chrome.runtime.sendMessage({ msg: "query_active" }, (isActive) => {
            $(this.menuIframe).contents().find("#toggle").prop("checked", isActive);
        });
    }

    showMenu(): void {
        this.isMenuVisible = true;
        $(this.menuIframe).css("display", "block");
        
        $(this.menuIframe).contents().find("#toggle").change( () => {
            chrome.runtime.sendMessage({ msg: "set_active", data: $(this.menuIframe).contents().find("#toggle").prop("checked") });
        });

        $(this.menuIframe).contents().find("#btnExpand").click(() => {
            console.log("expanding.");
            var list = $(this.menuIframe).contents().find("#selected_list");
            if (list.css("display") == "none") {
                list.css("display", "block");
                $(this.menuIframe).css("height", "500px");
            } else {
                list.css("display", "none");
                $(this.menuIframe).css("height", "80px");
            }
        });
    }

    hideMenu(): void {
        this.isMenuVisible = false;
        $(this.menuIframe).css("display", "none");
    }
    
    toggleEnabled(flag: boolean): void {
        $(this.menuIframe).contents().find("#toggle").prop("checked", flag);
        //called to add or remove canvas when toggle has been changed
        this.isEnabled = flag;

        console.log("asdfasdf");
        console.log(this.selections)
        this.selections.forEach((selection) => {
            if (flag) {
                selection.select();
            } else
                selection.deselect();
        });

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
                console.log("no canvas visible." + e);
            }
        }


    }

    mouseMove = (e): void => {
        if (!this.isSelecting) {
            return;
        }
        
        if (this.currentStrokeType == StrokeType.Bracket || this.currentStrokeType == StrokeType.Marquee) {
            var currType = GestireClassifier.getGestureType(this.inkCanvas._activeStroke.stroke);

            if (this.currentStrokeType == StrokeType.Bracket && currType == GestureType.Diagonal) {
                this.selection = new MarqueeSelection(this.inkCanvas, true);
                this.currentStrokeType = StrokeType.Marquee;       
                this.inkCanvas.redrawActiveStroke();

            }

            if (this.currentStrokeType == StrokeType.Marquee && currType == GestureType.Horizontal) {
                this.selection = new BracketSelection(this.inkCanvas, true);
                this.currentStrokeType = StrokeType.Bracket;
                this.inkCanvas.redrawActiveStroke();
            }
        }
    
        
        this.selection.update(e.clientX, e.clientY);
        document.body.appendChild(this.canvas); 
    }

    documentDown = (e): void => {
        switch (this.currentStrokeType) {
            case StrokeType.MultiLine:
                this.selection = new MultiLineSelection(this.inkCanvas);
                break;
            case StrokeType.Bracket:
                this.selection = new BracketSelection(this.inkCanvas);
                break;
            case StrokeType.Marquee:
                this.selection = new MarqueeSelection(this.inkCanvas);
                break;
        }
        console.log("current selection:" + this.currentStrokeType);
        this.selection.start(e.clientX, e.clientY);

        document.body.appendChild(this.canvas);
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
        this.isSelecting = false;
    }

    canvasUp = (e): void => {
        if (!this.isSelecting) {
            return;
        }

        this.canvas.removeEventListener("mousemove", this.mouseMove);
        document.body.removeChild(this.canvas);
        $(this.menuIframe).hide();
        this.selection.end(e.clientX, e.clientY);
        var stroke = this.inkCanvas._activeStroke.stroke.getCopy();
        var currType = GestireClassifier.getGestureType(stroke);

        if (currType == GestureType.Null) {
            console.log("JUST A TAP");
            document.body.appendChild(this.canvas);
            this.inkCanvas.update();
            return;
        }
        else if (currType == GestureType.Scribble) {

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

                            var selectionIndex = this.selections.indexOf(s);
                            if (selectionIndex > -1) {
                                this.selections.splice(selectionIndex, 1);
                                //chrome.storage.local.set({ 'curr': this.selections });
                            }
                        }
                    } catch (e) {
                        console.log(e);
                        console.log(this);
                    }
                });
            }
            this.inkCanvas.removeBrushStroke(this.inkCanvas._activeStroke);
        }
        else {
            this.selection.id = Date.now();
            this.selection.url = window.location.protocol + "//" + window.location.host + window.location.pathname;
            this.selections.push(this.selection);

            chrome.runtime.sendMessage({ msg: "store_selection", data: this.selection });
        }
        
        if (this.currentStrokeType == StrokeType.Bracket || this.currentStrokeType == StrokeType.Marquee) {
            this.currentStrokeType = StrokeType.Bracket;
        }

        document.body.appendChild(this.canvas);
        $(this.menuIframe).show();
        this.inkCanvas.update();
        this.isSelecting = false;
        this.updateSelectedList();
    }

    updateSelectedList(): void {
        var list = $(this.menuIframe).contents().find("#selected_list");
        list.empty();
        this.selections.forEach((s) => {
            list.append("<div class='selected_list_item'>" + s.getContent() + "</div>"); 
        });
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
}