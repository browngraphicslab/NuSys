/// <reference path="StrokeType.ts"/>


class Main {

    static DOC_WIDTH: number;
    static DOC_HEIGHT: number;

    body: HTMLElement = document.body;
    html: HTMLElement = document.documentElement;
    canvas: HTMLCanvasElement;
    menuIframe: HTMLIFrameElement;
    bubble: HTMLElement;
    menu: any;
    inkCanvas: InkCanvas;
    currentStrokeType: StrokeType;
    isSelecting: boolean;
    bubble_focused: boolean;
    selectionOnHover: AbstractSelection;
    lineAbove: Line;
    pointAbove: Point;
    pointIndex: number = -1;
    selectionToEdit: AbstractSelection;
    isLineSelected: boolean;
    isPointSelected: boolean;
    is_above_previous: boolean;
    is_editing_selection: boolean;
    original1: Point;
    original2: Point;
    pivotPointLine: Point;
    insertionIndex: number;
    selection: AbstractSelection;
    selections: Array<AbstractSelection> = new Array<AbstractSelection>();
    //previousSelections: Array<AbstractSelection> = new Array<AbstractSelection>();
    is_active: boolean;
    _startX: number;
    _startY: number;
    _url: any;
    _tag: any;
    _parsedTextNodes = {};
    
    
    

    constructor() {

        var body = document.body,
            html = document.documentElement;
        Main.DOC_WIDTH = Math.max(body.scrollWidth, body.offsetWidth,
            html.clientWidth, html.scrollWidth, html.offsetWidth);

        Main.DOC_HEIGHT = Math.max(body.scrollHeight, body.offsetHeight,
            html.clientHeight, html.scrollHeight, html.offsetHeight);
        console.log("Starting Nusys.....!!");

        this.canvas = document.createElement("canvas");
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
        this.canvas.style.position = "fixed";
        this.canvas.style.top = "0";
        this.canvas.style.left = "0";
        this.canvas.style.zIndex = "998";

        this.bubble_focused = false;
        this.bubble = document.createElement("p");
        this.bubble.innerHTML ="<textarea style=' width: 200px; height: 90px; text-align: center;  -moz-border-radius: 30px;  -webkit-border-radius: 30px; border-radius: 30px; border: none; outline: none; '>";
        $(this.bubble).addClass("noteBubble");
        document.body.appendChild(this.bubble);
        document.styleSheets[0]["insertRule"]('p.noteBubble {position: absolute; width: 200px; height: 100px; text - align: center; line - height: 100px; background: #fff; border: 8px solid #666; -moz-border-radius: 30px;  -webkit-border-radius: 30px; border-radius: 30px; -moz -box-shadow: 2px 2px 4px #888; -webkit-box-shadow: 2px 2px 4px #888; box-shadow: 2px 2px 4px #888; }', 0);
        document.styleSheets[0]["insertRule"]('p.noteBubble:before { content: " "; width: 0; height: 0; position: absolute; top: 100px; left: 30px; border: 25px solid #666; border-color: #666 transparent transparent #666; }', 0);
        document.styleSheets[0]["insertRule"]('p.noteBubble:after { content: " "; width: 0; height: 0; position: absolute; top: 100px; left: 38px; border: 15px solid #fff; border-color: #fff transparent transparent #fff; }',0);
        $(this.bubble).css("display", "none");

        this.inkCanvas = new InkCanvas(this.canvas);
        this._url = window.location.protocol + "//" + window.location.host + window.location.pathname;
        this.set_message_listener();
     //   this.showPreviousSelections();

        this.body.addEventListener("mousedown", (e) => {
            if (this.bubble_focused && !this.isAboveBubble(e)) {
                //set Bubble Speech.... 
               
                $(this.bubble).css("display", "none");
                this.body.appendChild(this.canvas);
                this.is_above_previous = false;
                this.is_editing_selection = false;
                this.bubble_focused = false;
                this.mouseDown(e);
            }
        });

    }


    showPreviousSelections(): void {
        chrome.storage.local.get((cTedStorage) => {
            console.log("STORAGE: ");
            console.info(cTedStorage);
            cTedStorage["selections"].forEach((s) => {
                if (s.url == this._url) {
      //              this.previousSelections.push(s);
                    this.selections.push(s); 
                    this.updateSelectedList();
                    if (s.type == StrokeType.Marquee) {
                        this.highlightPrevious(s);
                    }
                    if (s.type == StrokeType.Bracket) {
                        console.log(s);
                        this.highlightPrevious(s);
                    }
                    if (s.type == StrokeType.Line) {
                        this.highlightPrevious(s);
                    }
                    if (s.type == StrokeType.Null) {
                        this.highlightPrevious(s);
                    }
                    if (s.type == StrokeType.Lasso) {
                        console.log("lasso");
                        this.highlightPrevious(s);
                    }
                }
            });
        });
    }

    removeHighlight(s: AbstractSelection): void {
        $("." + s.id).each((indx, ele) => {
            ele["style"].backgroundColor = "";
            $(ele).removeClass(s.id.toString());
        });
    }

    highlightPrevious(s: AbstractSelection): void {
        var parElement;
        var parIndex;

        s.selectedElements.forEach((el) => {
            if (el.tagName == "WORD") {
                if (el.wordIndx == -1) {
                    $('WORD').get(el.index)["style"].backgroundColor = "yellow";
                } else {
                //    console.log("TAG NAME WORD");
                //    console.log(el);
                    var txtElement = $(el.par).get(el.parIndex).childNodes[el.txtnIndx];
                    if (!this._parsedTextNodes.hasOwnProperty(el.par)) {
                        $(txtElement).replaceWith("<words>" + $(txtElement).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
                        var paridx = {};
                        var txtidx = {};
                        txtidx[el.txtnIndx] = true;
                        paridx[el.parIndex] = txtidx;
                        this._parsedTextNodes[el.par] = paridx;
                        console.log("change");
                    } else if (!this._parsedTextNodes[el.par].hasOwnProperty(el.parIndex)) {
                        $(txtElement).replaceWith("<words>" + $(txtElement).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
                        var txtidx = {};
                        txtidx[el.txtnIndx] = true;
                        this._parsedTextNodes[el.par][el.parIndex] = txtidx;
                        console.log("change1");
                    } else if (!this._parsedTextNodes[el.par][el.parIndex].hasOwnProperty(el.txtnIndx)) {
                        console.log(txtElement);
                        this._parsedTextNodes[el.par][el.parIndex][el.txtnIndx] = true;
                        $(txtElement).replaceWith("<words>" + $(txtElement).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
                        console.log("change2");
                    }
                    
                    //if (parElement != el.par || parIndex != el.parIndex) {
                    //    $(txtElement).replaceWith("<words>" + $(txtElement).text().replace(/([^\s]*)/g, "<word>$1</worn d>") + "</words>");
                    //    parElement = el.par;
                    //    parIndex = el.parIndex;
                    //}
                    var ele = $(el.par).get(el.parIndex).childNodes[el.txtnIndx].childNodes[el.wordIndx];
                    //console.log(el);
                    //console.log(ele);
                    ele["style"].backgroundColor = "yellow";
                }
            } else if (el.tagName == "HILIGHT") {
                //console.log(el);
                //console.log(el.tagName);
                
                $($(el.par).get(el.parIndex).childNodes[el.txtnIndx]).replaceWith("<hilight>" + $($(el.par).get(el.parIndex).childNodes[el.txtnIndx]).text() + "</hilight>");
                //console.log(el.par);
                $(el.par).get(el.parIndex).childNodes[el.txtnIndx]["style"].backgroundColor = "yellow";
                //console.log(el);

            } else {
                if (el.tagName == "IMG") {
                    var label = $("<span class='wow'>Selected</span>");
                    label.css({ position: "absolute", display: "block", background: "yellow", width: "50px", height: "20px", color: "black", "font-size": "12px", padding: "3px 3px", "font-weight": "bold" });
                    $("body").append(label);
                    label.css("top", ($($(el.tagName).get(el.index)).offset().top - 5) + "px");
                    label.css("left", ($($(el.tagName).get(el.index)).offset().left - 5) + "px");
                } else {
                    $(el.tagName).get(el.index).style.backgroundColor = "yellow";
                }
            }
        });

    }
    //adds listener to chrome, specifying actions in relation to different incoming messages. 
    set_message_listener() {
        chrome.runtime.onMessage.addListener(
            (request, sender, sendResponse) => {
                console.log("Message: " + request.msg);
                var msg: String = request.msg;
                switch (msg) {
                    case "init":
                        this.init_menu(request.data);
                        sendResponse(true);
                        break;
                    case "show_menu":
                        $(this.menuIframe).css("display", "block");
                        if (this.is_active) { document.body.appendChild(this.canvas) };
                        break;
                    case "hide_menu":
                        if (document.body.contains(this.canvas))
                            document.body.removeChild(this.canvas);
                        $(this.menuIframe).css("display", "none");
                        break;
                    case "enable_selection":
                        this.toggleEnabled(true);
                        break;
                    case "disable_selection":
                        this.toggleEnabled(false);
                        break;
                    case "set_selections":
                        break;
                    case "tags_changed":
                        console.log("tags_changed");
                        $(this.menuIframe).contents().find("#tagfield").val(request.data);
                        break;
                }
            }
        );
    }

    //initializes the iframe menu
    init_menu(menuHtml: string) {
        console.log("init!");
        this.menuIframe = <HTMLIFrameElement>$("<iframe frameborder=0>")[0];
        document.body.appendChild(this.menuIframe);
        this.menu = $(menuHtml)[0];
        $(this.menuIframe).css({ position: "fixed", top: "1px", right: "1px", width: "410px", height: "106px", "z-index": 1001 });

        $(this.menuIframe).contents().find('html').html(this.menu.outerHTML);
        $(this.menuIframe).css("display", "none");      //initially set menu to display none.

        $(this.menuIframe).contents().find("#btnExport").click((ev) => {
            chrome.runtime.sendMessage({ msg: "export" });
        });

        $(this.menuIframe).contents().find("#btnLineSelect").click((ev) => {
            console.log("btnLineSelect==========================");
        });

        $(this.menuIframe).contents().find("#btnBlockSelect").click((ev) => {
            console.log("btnBlockSelect========================");
        });

        $(this.menuIframe).contents().find("#tagfield").change(() => {
            chrome.runtime.sendMessage({ msg: "tags_changed", data: $(this.menuIframe).contents().find("#tagfield").val() });
            this._tag = $(this.menuIframe).contents().find("#tagfield").val();
        });

        $(this.menuIframe).contents().find("#btnViewAll").click(() => {
            chrome.runtime.sendMessage({ msg: "view_all" });
        });

        $(this.menuIframe).contents().find("#toggle").change(() => {
            chrome.runtime.sendMessage({ msg: "set_active", data: $(this.menuIframe).contents().find("#toggle").prop("checked") });
            this.toggleEnabled($(this.menuIframe).contents().find("#toggle").prop("checked"));
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

    sendResponse(bool: Boolean) {  
         
    }

    makeNewLasso() {

    }
    mouseUp = (e): void => {
        console.log("mouseUp");
        if (this.selectionOnHover) {
            if (this.is_editing_selection) {
                this.inkCanvas.clear();
                if (this.selectionOnHover.type == StrokeType.Bracket) {
                //    this.selectionOnHover.samplePoints.splice(this.pointIndex - 1, 1, new Point(e.clientX, e.clientY
                    this.isLineSelected = false;
                    this.isPointSelected = false;
                    this.is_editing_selection = false;
                    this.pointIndex = -1;
                    this.countX = 0;
                    return;
                }
                if (this.countX < 3 && this.isLineSelected) {
                    console.log("=====================");
                    console.log(this.pointIndex);
                    console.log(this.selectionOnHover.samplePoints);
                    this.selectionOnHover.samplePoints.splice(this.pointIndex + 1, 0, new Point(e.clientX, e.clientY - this.selectionOnHover.yscroll + $(document).scrollTop()));
                    chrome.runtime.sendMessage({ msg: "edit_selection", data: editedSelection });
                    this.isLineSelected = false;
                    this.isPointSelected = false;
                    this.is_editing_selection = false;
                    this.pointIndex = -1;
                    return;
                }
                console.log(this.selectionOnHover);
                this.removeHighlight(this.selectionOnHover);
                this.isLineSelected = false;
                this.isPointSelected = false;
                this.is_editing_selection = false;
                this.pointIndex = -1;
                this.countX = 0;
                document.body.removeChild(this.canvas);
                var editedSelection = new LassoSelection();
                var editedStroke = new Stroke();
                editedSelection.yscroll = $(document).scrollTop();
                editedStroke.points = this.selectionOnHover.samplePoints;
                var len = editedStroke.points.length;
                for (var i = 0; i < len; i++) {
                    editedStroke.points[i] = new Point(editedStroke.points[i].x, editedStroke.points[i].y + this.selectionOnHover.yscroll );
                }
                editedSelection.stroke = editedStroke;
                editedSelection.id = this.selectionOnHover.id;

                editedSelection.end(0, 0);
                editedSelection.type = StrokeType.Lasso;
                editedSelection.url = this.selectionOnHover.url;
                editedSelection.tags = this.selectionOnHover.tags;
                for (var i = 0; i < len; i++) {
                    editedStroke.points[i] = new Point(editedStroke.points[i].x, editedStroke.points[i].y - this.selectionOnHover.yscroll);
                }
                chrome.runtime.sendMessage({ msg: "edit_selection", data: editedSelection });
                document.body.appendChild(this.canvas);

                return;
            } else {
                console.log("======BUBBE");
                //$(this.bubble).show();
                //$(this.bubble).css("top", e.clientY - 170 - $(window).scrollTop());
                //$(this.bubble).css("left", e.clientX - 30);
                //$(this.bubble).css("border", "8px solid #666");
                //document.styleSheets[0]["insertRule"]('p.noteBubble:before { content: " "; width: 0; height: 0; position: absolute; top: 100px; left: 30px; border: 25px solid #666; border-color: #666 transparent transparent #666; }', 0);
            }

            $(this.bubble).click(function () {

            });
        } else {
            //      $(this.bubble).css("display", "none");
        }
        console.log("======================================");
        document.body.removeChild(this.canvas);
        var isLineSelected = false;
        this.isSelecting = false;
        this.isLineSelected = false;
        this.isPointSelected = false;
        this.is_editing_selection = false;
        this.selection.stroke = this.inkCanvas._activeStroke;
        this.selection.end(e.clientX, e.clientY);
        this.selection.yscroll = $(document).scrollTop();
        console.log(this.selection.getContent()); //print out content 
        this.selection.type = this.currentStrokeType;
        this.selection.url = this._url;
        this.selection.tags = $(this.menuIframe).contents().find("#tagfield").val();
        console.log(this.selection);
        if (this.selection.getContent() != "" && this.selection.getContent() != " ") {
            this.selections.push(this.selection); //add selection to selections array 
   //         this.previousSelections.push(this.selection);
            this.updateSelectedList();
            chrome.runtime.sendMessage({ msg: "store_selection", data: this.selection });
        }
        this.inkCanvas.clear();
   //     this.inkCanvas.drawStroke(this.selection.stroke);
        this.currentStrokeType = StrokeType.Line;
        console.log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        document.body.appendChild(this.canvas);
    }

    updateSelectedList(): void {
        var list = $(this.menuIframe).contents().find("#selected_list");
        list.empty();
        var count = 0;
        this.selections.forEach((s: AbstractSelection) => {
            console.info(s);
            var item = document.createElement("div");
            item.setAttribute("class", "selected_list_item");
            var close_btn = document.createElement("button");
            close_btn.setAttribute("class", "btn_close_item");
            $(close_btn).click(() => {
                console.log("remove");
                var indx = this.selections.indexOf(s);
                console.log(indx);
                console.log(this.selections[indx]);
                this.removeHighlight(this.selections[indx]);
                chrome.runtime.sendMessage({ msg: "remove_selection", data: this.selections[indx]["id"] });
                this.selections.splice(this.selections.indexOf(s), 1);
        //        this.previousSelections.splice(this.previousSelections.indexOf(s), 1);
                close_btn.parentElement.remove();
            });
            item["innerHTML"] = s["_content"];
            $(item).prepend(close_btn);
            count++;
            list.append(item);
        });

    }

    //mousedown action
    mouseDown = (e): void => {
        console.log("mouse down");
        this.selection = new NullSelection();
   //     this.inkCanvas.switchBrush(this.currentStrokeType);
        try {
            document.body.removeChild(this.canvas);
        } catch (e) {
            console.log("no canvas visible." + e);
        }
        if (!this.checkAtag(e)) {
            this.isSelecting = true;
            this._startX = e.clientX;
            this._startY = e.clientY;
            this.selection.id = Date.now();
            this.selection.start(e.clientX, e.clientY);
        }

    }
    findReplacementStroke(points: Array<Point>, p: Point): number {
        console.log(p);
        console.log(points);
        var size = points.length;
        for (var i = 0; i < size; i++) {
            if (points[i].x == p.x && points[i].y == p.y)
                return i;
        }
        return -1;
    }

    findInsertionStroke(points: Array<Point>, line: Line): number {
        var size = points.length;
        for (var i = 0; i < size; i++) {
            if (points[i].x == line.p1.x && points[i].y == line.p1.y)
                return i;
        }
        //points.forEach((p, i) => {
        //    console.log(p);
        //    console.log(line);
        //    console.log(p.x == line.p1.x);
        //    if (p.x == line.p1.x && p.y == line.p1.y) {
        //        return i;
        //    }
        //});
        console.log("================!!!")
        return -1;
    }
    
    countX: number = 0;

    getSelectionOnHover = (e): AbstractSelection => {
    //    console.log(this.selections);
        for (var i = 0; i < this.selections.length; i++) {
            if (this.isAbove(e, this.selections[i])) {
                return this.selections[i];
            }
        }
        //this.selections.forEach((sel, indx) => {
        //    if (this.isAbove(e, sel)) {
        //        console.log("true!!!!");
        //        this.selectionOnHover = sel;
        //        return sel;
        //        //draw red 
        //        //onHover
        //        //clickable-remember variable(selection Current PRev);tu
        //    }
        //});

        return null;
    }

    mouseMove = (e): void => {
        if (this.is_editing_selection) {
            var sel = this.selectionOnHover;

            if (this.isPointSelected) {
                    if (this.pointIndex == -1) {
                        this.pointIndex = this.findReplacementStroke(sel.samplePoints, this.pointAbove);
                }
                var newPoint = new Point(e.clientX, e.clientY + $(document).scrollTop() - this.selectionOnHover.yscroll);
                    sel.samplePoints.join();
                    sel.samplePoints.splice(this.pointIndex, 1, newPoint);
                    sel.samplePoints.join();
                    this.inkCanvas.clear();
                    this.inkCanvas.drawPreviousGesture(sel);
                

            }

            if (this.isLineSelected) {

                    console.log(sel.samplePoints.length);
                    if (this.countX == 0) {
                        this.insertionIndex = this.findInsertionStroke(sel.samplePoints, this.lineAbove);
                        this.original1 = sel.samplePoints[this.insertionIndex];
                        this.original2 = sel.samplePoints[(this.insertionIndex+1)%sel.samplePoints.length];
                    }
                    var dx = this.pivotPointLine.x - e.clientX;
                    var dy = this.pivotPointLine.y - e.clientY;
                    sel.samplePoints.join();
                    sel.samplePoints[this.insertionIndex] = new Point(this.original1.x - dx, this.original1.y - dy + $(document).scrollTop() - this.selectionOnHover.yscroll);
                    sel.samplePoints[(this.insertionIndex + 1) % sel.samplePoints.length] = new Point(this.original2.x - dx, this.original2.y - dy + $(document).scrollTop() - this.selectionOnHover.yscroll);

                   // console.log(sel.samplePoints.length);
                    this.inkCanvas.clear();
                    this.inkCanvas.drawPreviousGesture(sel);
                    console.log("==========DRAW====");
                    this.countX++;
                

            }
            this.selectionOnHover.samplePoints = sel.samplePoints;

        } else {
            this.selectionOnHover = this.getSelectionOnHover(e);
            if (this.selectionOnHover) {
                //         console.log("there is a selection :");
                //         console.log(this.selectionOnHover);
                this.inkCanvas.drawPreviousGesture(this.selectionOnHover);
                this.pointAbove = this.inkCanvas.editPoint(this.selectionOnHover.samplePoints, e);
                if (this.pointAbove) {
   //                 console.log("point..... ");
   //                 console.log(this.pointAbove);
                } else {
                    this.lineAbove = this.inkCanvas.editStrokes(this.selectionOnHover.samplePoints, e);
                    if (this.lineAbove) {
    //                    console.log("line.....");
                    }
                }

            } else if (this.isSelecting) {
                this.inkCanvas.draw(e.clientX, e.clientY);
                if (this.currentStrokeType != StrokeType.Lasso && this.currentStrokeType != StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke)) {
                    //     console.log("strokeType changed from " + this.currentStrokeType + " to " + StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke));
                    this.currentStrokeType = StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke);
                    this.switchSelection(this.currentStrokeType);
                    this.inkCanvas.switchBrush(this.currentStrokeType);
                }

            } else {
                this.inkCanvas.clear();
                this.pointAbove = null;
                this.lineAbove = null;
                this.is_editing_selection = false;
                //         console.log("not above any selection");
            }
        }

 /*       ///////////////////
        if (this.isPointSelected) {
            var sel = this.selectionOnHover;
            if (sel.type == StrokeType.Lasso) {
                if (this.pointIndex == -1) {
                    this.pointIndex = this.findReplacementStroke(sel.samplePoints, this.pointAbove);
                }
                console.log("----------------------------------");
                console.log(index);
                var newPoint = new Point(e.clientX, e.clientY);
                sel.samplePoints.join();
                sel.samplePoints.splice(this.pointIndex, 1, newPoint);
                sel.samplePoints.join();
                this.inkCanvas.clear();
                this.inkCanvas.drawPreviousGestureL(sel.samplePoints);
 
            }
        }else if (this.isLineSelected) {
            var sel = this.selectionOnHover;
            if (sel.type == StrokeType.Lasso) {
                var index = this.findInsertionStroke(sel.samplePoints, this.lineAbove);
                index++;
                console.log("mousemove...");
                console.log(index);
                var newPoint = new Point(e.clientX, e.clientY);
                console.log(sel.samplePoints.length);
                if (this.countX == 0) {
                    sel.samplePoints.join();
                    sel.samplePoints.splice(index, 0, newPoint);
                    sel.samplePoints.join();
                } else {
                    sel.samplePoints.join();
                    sel.samplePoints.splice(index, 1, newPoint);
                    sel.samplePoints.join();
                }

                console.log(sel.samplePoints.length);
                this.inkCanvas.clear();
                this.inkCanvas.drawPreviousGestureL(sel.samplePoints);
                console.log("==========DRAW====");
                this.countX++;
            }
        }
        else if (this.isSelecting) {
            this.inkCanvas.draw(e.clientX, e.clientY);
            if (this.currentStrokeType != StrokeType.Lasso && this.currentStrokeType != StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke)) {
                console.log("strokeType changed from " + this.currentStrokeType + " to " + StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke));
                this.currentStrokeType = StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke);
                this.switchSelection(this.currentStrokeType);
                this.inkCanvas.switchBrush(this.currentStrokeType);
            }
        } else {
            if (this.is_above_previous) {
                console.log("check for line intersect...");
                var line = this.inkCanvas.editStrokes(this.selectionOnHover.samplePoints, new Point(e.clientX, e.clientY));
                var point = this.inkCanvas.editPoint(this.selectionOnHover.samplePoints, new Point(e.clientX, e.clientY));
                if (point != null) {
                    console.log("==========POINT==============");
                    console.log(point);
                    this.pointAbove = point;

                }else if (line != null) {
                    console.log("==============LINE==================");
                    console.log(line);
                    this.lineAbove = line;
                    console.log("=============EDITING LINE============");
                    console.log(this.selectionOnHover);

                }
                this.checkStillOnHover(e);
            } else {
                this.showGestureOnHover(e);
                this.lineAbove = null;
                this.pointAbove = null;
            }
     
        } */
    }

    checkStillOnHover(e): void {
        if (!this.isAbove(e, this.selectionOnHover)) {
            this.inkCanvas.clear();
            this.is_above_previous = false;
        }
    }

    showGestureOnHover(e): void {
            this.selections.forEach((sel, indx) => {
                if (this.isAbove(e, sel)) {
                    console.log("isAbove!!!! " + sel);
                    this.selectionOnHover = sel;
                    this.is_above_previous = true;
                    this.inkCanvas.drawPreviousGesture(sel);
                    //draw red 
                    //onHover
                    //clickable-remember variable(selection Current PRev);tu
                }
        });

    }

    // checks if current mouse is above previous, must consider scrollTop
    isAbove(e, sel: AbstractSelection): Boolean {

            var stroke = new Stroke();
            stroke.points = sel.samplePoints;
            var st = new Stroke();
            for (var i = 0; i < stroke.points.length; i++) {
                st.points.push(new Point(stroke.points[i].x, stroke.points[i].y +  sel.yscroll - $(document).scrollTop()));
             }
            if (sel.type == StrokeType.Bracket) {
                for (var i = 0; i < sel.selectedElements.length; i++) {
                    var el = $(sel.selectedElements[i].tagName).get(sel.selectedElements[i].index);
                    if ((e.clientX > $(el).offset().left && e.clientX < $(el).offset().left + $(el).width())
                        && (e.clientY > $(el).offset().top && e.clientY < $(el).offset().top + $(el).height())) {
                        return true
                    }
                }
                return false;
            }
            return this.isPointBound(new Point(e.clientX, e.clientY), st);
    }

    getRealHeightWidth(rectsList): Array<number> {
        //finds the real Heights and Widths of DOM elements by iterating through their clientRectList.
        var maxH = Number.NEGATIVE_INFINITY;
        var minH = Number.POSITIVE_INFINITY;
        var maxW = Number.NEGATIVE_INFINITY;
        var minW = Number.POSITIVE_INFINITY;
        $(rectsList).each(function (indx, elem) {
            console.log($(elem).height());
            if (elem["top"] + elem["height"] > maxH) {
                maxH = elem["top"] + elem["height"];
            }
            if (elem["top"] < minH) {
                minH = elem["top"];
            }
            if (elem["left"] < minW) {
                minW = elem["left"];
            }
            if (elem["left"] + elem["width"] > maxW) {
                maxW = elem["left"] + elem["width"];
            }
        });
        return [(maxH - minH), (maxW - minW), minW, minH];
    }


    isInRect(e, elem): boolean {
        if (e.clientX >= elem["left"] && e.clientX <= elem["left"] + elem["width"]) {
            return e.clientY >= elem["top"] && e.clientY <= elem["top"] + elem["height"]
        }
        return false;
    }

    sampleLines(stroke :Stroke): Array<Line> {
        var sampleStroke = stroke.points;
        var lines = [];
        if (!sampleStroke)
            return;
        for (var i = 1; i < sampleStroke.length; i++) {
            lines.push(new Line(sampleStroke[i - 1], sampleStroke[i]));
        }
        lines.push(new Line(sampleStroke[sampleStroke.length - 1], sampleStroke[0]));
        return lines;
    }
    ///directly from lasso
    isPointBound(p: Point, s: Stroke): boolean {
        var lines = this.sampleLines(s);
      //  console.log("======isPointBound ");
      //  console.log(p);
        var xPoints = [];
        if (!lines) {
            return false;
        }
        for (var i = 0; i < lines.length; i++) {
            var l = lines[i];
            if (p.y <= Math.max(l.p1.y, l.p2.y) && p.y >= Math.min(l.p1.y, l.p2.y)) {
                var x = (l.C - l.B * p.y) / l.A;
                xPoints.push(x);
            }
        }
      //  console.log(xPoints);
        if (xPoints.length == 0)
            return false;
        xPoints.sort(function (a, b) { return a - b; });
        var res = false;
        //for compromise

        
        for (var i = 0; i < xPoints.length; i++) {
            var xval = xPoints[i];
            if (i == 0)
                xval -= 30;
            if (i == xPoints.length - 1)
                xval += 30;
            if (p.x < xval)
                return res;
            res = !res;
        }
        return false;
    }



    isAboveBubble(e): Boolean {

        return (e.clientX > this.bubble.offsetLeft && e.clientX < this.bubble.offsetLeft + 200) && (e.clientY > this.bubble.offsetTop - $(window).scrollTop() && e.clientY < this.bubble.offsetTop+200 - $(window).scrollTop());
        //return bool
         
    }

    switchSelection(strokeType) {
        console.log("Iselection switched to : " + strokeType);
        var id = this.selection.id;
        switch (strokeType) {
            //////STROKE TYPE CHANGE
            case StrokeType.Marquee:
                this.selection = new MarqueeSelection();
                break;
            case StrokeType.Bracket:
                this.selection = new BracketSelection();
                break;
            case StrokeType.Line:
                this.selection = new LineSelection();
                break;
            case StrokeType.Lasso:
                console.log("============================!!!!!!!!!!!!!!========================");
                this.selection = new LassoSelection();
                break;
        }
        this.selection.id = id;
        this.selection.start(this._startX, this._startY);
    }
    checkNoteBubble = (e): void => {

    }
    showBubble(){
        $(this.bubble).show();
    }

    
    checkAtag = (e): boolean => {
        console.log("checkAtag");
        var hitElem = document.elementFromPoint(e.clientX, e.clientY);
        var res = true;
        console.log(hitElem);
        var el = this.getSelectionOnHover(e);

        if (this.pointAbove) {
            this.isPointSelected = true;
            this.is_editing_selection = true;
            document.body.appendChild(this.canvas);

        } else if (this.lineAbove) {
            this.isLineSelected = true;
            this.is_editing_selection = true;
            console.log(this.lineAbove);
            console.log(this.selectionOnHover.samplePoints);
            console.log(e.clientY + " L " + $(document).scrollTop() + " D " + this.selectionOnHover.yscroll);
            this.pivotPointLine = new Point(e.clientX, e.clientY + $(document).scrollTop() - this.selectionOnHover.yscroll);
            this.pointIndex = this.findReplacementStroke(this.selectionOnHover.samplePoints, this.lineAbove.p1);
            document.body.appendChild(this.canvas);
        } else if (hitElem.nodeName == "A") {   
            console.log("atag");
            var link = hitElem.getAttribute("href").toString();

            if (link.indexOf("http") == -1) {
                link = "http://" + window.location.host + link;
            }
            console.log(link);
            window.open(link, "_self");
        } else if (hitElem.nodeName == "TEXTAREA") {
            console.log("textarea");

            this.bubble_focused = true;
            //  $(this.bubble).css("border", "8px solid red");
        } 
        else {
            document.body.appendChild(this.canvas);
            res = false;
        }
        return res; 
    }
    isAboveLine(points: Array<Point>): boolean {

        return false;
    }


    toggleEnabled(flag: boolean): void {
        console.log("toggle state changed");
        $(this.menuIframe).contents().find("#toggle").prop("checked", flag);
        //called to add or remove canvas when toggle has been changed
        this.is_active = flag;

        if (this.is_active) {
            try {
                document.body.appendChild(this.canvas);
                console.log("added canvas");
   
            } catch (ex) {
                console.log("could't add canvas");
            }
            this.currentStrokeType = StrokeType.Null;
            this.canvas.addEventListener("mousedown", this.mouseDown);
            this.canvas.addEventListener("mouseup", this.mouseUp);
            this.canvas.addEventListener("mousemove", this.mouseMove);
        } else {
            try {
                document.body.removeChild(this.canvas);
            } catch (e) {
                console.log("no canvas visible." + e);
            }
        }
    }

}
$(document).ready(function () {
    console.log("REQDT");
    var main = new Main();
});
