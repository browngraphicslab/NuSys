/// <reference path="../typings/chrome/chrome.d.ts"/>
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
    menuIframe: HTMLIFrameElement;
    menu: any;

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
        this.canvas.style.left = "0"; //fixes canvas placements
        this.canvas.style.zIndex = "998";

        this.inkCanvas = new InkCanvas(this.canvas);

        chrome.storage.local.get(null, (data) => {
            this.previousSelections = data["selections"];
        });

        chrome.runtime.onMessage.addListener(
        (request, sender, sendResponse) => {

            console.log(request);

            if (request.msg == "tags_changed") {
                console.log("tags_changed")
                $(this.menuIframe).contents().find("#tagfield").val(request.data);
            }

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
                    if (ls._brushStroke != null) {
                        var stroke = new Stroke();
                        $.extend(stroke, ls._brushStroke.stroke);
                        ls._brushStroke.stroke = stroke;
                    }
                    this.selections.push(ls);
                });

                var count = 0;
                this.selections.forEach((sel) => {
                    if (sel instanceof MultiLineSelection) {
                        console.log("found MultiSelection");
                        sel.selectedElements.forEach((el) => {
                            var startNode = $(el.start.tagName)[el.start.parentIndex];
                            var endNode = $(el.end.tagName)[el.end.parentIndex];
                            var startParentData = count++;
                            var endParentData = count++;

                       

                            if ($(startNode).attr("data-cTedId") == undefined) {
                                $(startNode).attr("data-cTedId", startParentData);
                            } else {
                                startParentData = parseInt($(startNode).attr("data-cTedId"));
                            }

                            if ($(endNode).attr("data-cTedId") == undefined) {
                                $(endNode).attr("data-cTedId", endParentData);
                            } else {
                                endParentData = parseInt($(endNode).attr("data-cTedId"));
                            }

                            $(endNode).attr("data-cTedId", endParentData);
                            el.start.id = startParentData;
                            el.end.id = endParentData;
                        });
                    }
                });

                sendResponse();
                this.updateSelectedList();
            }
        });
    }

    init(menuHtml: string) {
        console.log("init!");

        this.currentStrokeType = StrokeType.MultiLine;
        document.addEventListener("mouseup", this.documentUp);

        this.menuIframe = <HTMLIFrameElement>$("<iframe frameborder=0>")[0];
        document.body.appendChild(this.menuIframe);
        this.menu = $(menuHtml)[0];
        $(this.menuIframe).css({ position: "fixed", top: "1px", right: "1px", width: "410px", height: "106px", "z-index": 1001 });

        $(this.menuIframe).contents().find('html').html(this.menu.outerHTML);
        $(this.menuIframe).css("display", "none");

        $(this.menuIframe).contents().find("#btnLineSelect").click((ev) => {
            if (this.currentStrokeType == StrokeType.MultiLine)
                return;
            var other = $(this.menuIframe).contents().find("#btnBlockSelect");
            console.log("switching to multiline selection");

            if ($(ev.target).hasClass("active")) {
                $(ev.target).removeClass("active");
            } else {
                $(ev.target).addClass("active");
                $(other).removeClass("active");
            }

            this.currentStrokeType = StrokeType.MultiLine;
            document.addEventListener("mouseup", this.documentUp);
            
        });

        $(this.menuIframe).contents().find("#btnBlockSelect").click((ev) => {
            if (this.currentStrokeType == StrokeType.Bracket)
                return;
            var other = $(this.menuIframe).contents().find("#btnLineSelect");
            if ($(ev.target).hasClass("active")) {
                $(ev.target).removeClass("active");
            } else {
                $(ev.target).addClass("active");
                $(other).removeClass("active");
            }

            this.currentStrokeType = StrokeType.Bracket;
            try {
                document.body.appendChild(this.canvas);
            } catch (ex) {
                console.log("could't add canvas");
            }
            document.removeEventListener("mouseup", this.documentUp);
        });

        $(this.menuIframe).contents().find("#tagfield").change(() => {
            chrome.runtime.sendMessage({ msg: "tags_changed", data: $(this.menuIframe).contents().find("#tagfield").val() });
        });

        $(this.menuIframe).contents().find("#btnViewAll").click(() => {
            chrome.runtime.sendMessage({ msg: "view_all" });
        });

        $(this.menuIframe).contents().find("#toggle").change(() => {
            chrome.runtime.sendMessage({ msg: "set_active", data: $(this.menuIframe).contents().find("#toggle").prop("checked") });
        });

        $(this.menuIframe).contents().find("#btnExpand").click((ev) => {
            console.log("expand");
            var list = $(this.menuIframe).contents().find("#selected_list");
            if ($(ev.target).hasClass("active")) {
                $(ev.target).removeClass("active");
                $(list).removeClass("open");
                $(this.menuIframe).height(106);

            } else {
                $(ev.target).addClass("active");
                $(list).addClass("open");
                $(this.menuIframe).height(500);
            }
        });

        chrome.runtime.sendMessage({ msg: "query_active" }, (isActive) => {
            $(this.menuIframe).contents().find("#toggle").prop("checked", isActive);
        });
    }

    showMenu(): void {
        this.isMenuVisible = true;
        $(this.menuIframe).css("display", "block");
    }

    hideMenu(): void {
        this.isMenuVisible = false;
        $(this.menuIframe).css("display", "none");
    }

    toggleEnabled(flag: boolean): void {

        $(this.menuIframe).contents().find("#toggle").prop("checked", flag);
        //called to add or remove canvas when toggle has been changed
        this.isEnabled = flag;

        this.selections.forEach((selection) => {
            if (flag) {
                selection.select();
                
            } else
                selection.deselect();
        });

        if (this.isEnabled) {

            this.currentStrokeType = StrokeType.Bracket;
            try {
                document.body.appendChild(this.canvas);
            } catch (ex) {
                console.log("could't add canvas");
            }
            document.removeEventListener("mouseup", this.documentUp);



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

        if (this.currentStrokeType == StrokeType.Bracket || this.currentStrokeType == StrokeType.Marquee || this.currentStrokeType == StrokeType.Line) {
            var currType = GestireClassifier.getGestureType(this.inkCanvas._activeStroke.stroke);
            
            if (this.currentStrokeType != StrokeType.Line && currType == GestureType.Horizontal) {
                this.selection = new LineSelection(this.inkCanvas, true);
                this.currentStrokeType = StrokeType.Line;
                this.inkCanvas.redrawActiveStroke();
            }

            if (this.currentStrokeType != StrokeType.Marquee && currType == GestureType.Diagonal) {
                this.selection = new MarqueeSelection(this.inkCanvas, true);
                this.currentStrokeType = StrokeType.Marquee;
                this.inkCanvas.redrawActiveStroke();
            }

            if (this.currentStrokeType != StrokeType.Bracket && currType == GestureType.Vertical) {
                this.selection = new BracketSelection(this.inkCanvas, true);
                this.currentStrokeType = StrokeType.Bracket;
                this.inkCanvas.redrawActiveStroke();
            }

            this.selection.update(e.clientX, e.clientY);
        }
    }

    documentDown = (e): void => {

        try {
            document.body.removeChild(this.canvas);
        } catch (e) {
            console.log("no canvas visible." + e);
        }

        var hitElem = document.elementFromPoint(e.clientX, e.clientY);
        console.log(hitElem);
       if (hitElem.nodeName == "A") {
           var link = hitElem.getAttribute("href").toString();

           if (link.indexOf("http") == -1) {
               link = "http://" + window.location.host + link;
           }
           console.log(link);
           window.open(link, "_self");
       }

        switch (this.currentStrokeType) {

        case StrokeType.MultiLine:
            this.selection = new MultiLineSelection(this.inkCanvas);
            break;
        case StrokeType.Line:
        case StrokeType.Bracket:
            this.selection = new BracketSelection(this.inkCanvas);
            break;
        case StrokeType.Marquee:
            this.selection = new MarqueeSelection(this.inkCanvas);
            break;
        }

        if (this.currentStrokeType == StrokeType.Bracket || this.currentStrokeType == StrokeType.Marquee || this.currentStrokeType == StrokeType.Line) {
            console.log("current selection: " + this.currentStrokeType);
            document.body.appendChild(this.canvas);
            this.canvas.addEventListener("mousemove", this.mouseMove);
        } else {
            try {
                document.body.removeChild(this.canvas);
            } catch (e) {
                console.log("no canvas visible." + e);
            }
        }

        this.selection.start(e.clientX, e.clientY);

        this.isSelecting = true;
    }

    documentScroll = (e): void => {
        this.inkCanvas.update();
    }

    documentUp = (e): void => {
        console.log("document up");
        if (!this.isSelecting)
            return;

        this.isSelecting = false;
        if (this.currentStrokeType == StrokeType.MultiLine)
            this.selection.end(e.clientX, e.clientY);

        this.selection.id = Date.now();
        this.selection.url = window.location.protocol + "//" + window.location.host + window.location.pathname;
        this.selection.tags = $(this.menuIframe).contents().find("#tagfield").val();
        this.selections.push(this.selection);
        chrome.runtime.sendMessage({ msg: "store_selection", data: this.selection });
        this.selection = new MultiLineSelection(this.inkCanvas);

        this.updateSelectedList();

    }

    windowUp = (e): void => {
        if (!this.isSelecting)
            return;

        if (this.currentStrokeType == StrokeType.Bracket || this.currentStrokeType == StrokeType.Marquee) {
            this.canvas.removeEventListener("mousemove", this.mouseMove);
            this.inkCanvas.removeBrushStroke(this.inkCanvas._activeStroke);
            this.inkCanvas.update();
        }
        this.isSelecting = false;
    }

    canvasUp = (e): void => {
        console.log("canvas up");
        if (!this.isSelecting) {
            return;
        }

        this.canvas.removeEventListener("mousemove", this.mouseMove);
        
        if (this.currentStrokeType == StrokeType.Bracket || this.currentStrokeType == StrokeType.Marquee || this.currentStrokeType == StrokeType.Line) {
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
            } else {
                document.body.appendChild(this.canvas);
                $(this.menuIframe).show();
                this.inkCanvas.update();
                this.isSelecting = false;
            }

            this.currentStrokeType = StrokeType.Bracket;
        } 
        
        this.selection.id = Date.now();
        this.selection.url = window.location.protocol + "//" + window.location.host + window.location.pathname;
        this.selection.tags = $(this.menuIframe).contents().find("#tagfield").val();
        this.selections.push(this.selection);

        chrome.runtime.sendMessage({ msg: "store_selection", data: this.selection });

        this.updateSelectedList();
    }

    updateSelectedList(): void {
        var list = $(this.menuIframe).contents().find("#selected_list");
        list.empty();
        this.selections.forEach((s) => {
            list.append("<div class='selected_list_item'>" + s.getContent() + "</div>"); 
        });
    }

    static relativeToAbsolute(content: string): string {
        //////change relative href of hyperlink and src of image in html string to absolute

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