/// <reference path="StrokeType.ts"/>


class Main {

    static DOC_WIDTH: number;
    static DOC_HEIGHT: number;

    body: HTMLElement = document.body;
    html: HTMLElement = document.documentElement;
    canvas: HTMLCanvasElement;
    menuIframe: HTMLIFrameElement;
    menu: any;
    inkCanvas: InkCanvas;
    currentStrokeType: StrokeType;
    isSelecting: boolean;
    selection: AbstractSelection;
    selections: Array<AbstractSelection> = new Array<AbstractSelection>();
    previousSelections: Array<AbstractSelection> = new Array<AbstractSelection>();
    is_active: boolean;
    _startX: number;
    _startY: number;
    _url: any;
    _parsedTextNodes = {};
    
    
    

    constructor() {

        var body = document.body,
            html = document.documentElement;
        Main.DOC_WIDTH = Math.max(body.scrollWidth, body.offsetWidth,
            html.clientWidth, html.scrollWidth, html.offsetWidth);

        Main.DOC_HEIGHT = Math.max(body.scrollHeight, body.offsetHeight,
            html.clientHeight, html.scrollHeight, html.offsetHeight);

        console.log("Starting Nusys.....");

        this.canvas = document.createElement("canvas");
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
        this.canvas.style.position = "fixed";
        this.canvas.style.top = "0";
        this.canvas.style.left = "0";
        this.canvas.style.zIndex = "998";
        this.inkCanvas = new InkCanvas(this.canvas);
        this._url = window.location.protocol + "//" + window.location.host + window.location.pathname;
        this.set_message_listener();

        this.showPreviousSelections();

    }

    showPreviousSelections(): void {
        chrome.storage.local.get((cTedStorage) => {
            console.log("STORAGE: ");
            console.info(cTedStorage);
            cTedStorage["selections"].forEach((s) => {
                if (s.url == this._url) {
                    if (s.type == StrokeType.Marquee) {
                        this.highlightPrevious(s);
                    }
                }
            });
        });
    }

    highlightPrevious(s: AbstractSelection): void {
        var parElement;
        var parIndex;

        s.selectedElements.forEach((el) => {
            if (el.tagName == "WORD") {
                console.log("TAG NAME WORD");
                console.log(el);
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
                    this._parsedTextNodes[el.par][el.parIndex][el.txtnIndx] = true;
                    $(txtElement).replaceWith("<words>" + $(txtElement).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
                }
                    
                //if (parElement != el.par || parIndex != el.parIndex) {
                //    $(txtElement).replaceWith("<words>" + $(txtElement).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
                //    parElement = el.par;
                //    parIndex = el.parIndex;
                //}
                var ele = $(el.par).get(el.parIndex).childNodes[el.txtnIndx].childNodes[el.wordIndx];
                console.log(el);
                console.log(ele);
                ele["style"].backgroundColor = "yellow";
            } else if (el.tagName == "HILIGHT") {
                console.log(el);
                console.log(el.tagName);
                
                $($(el.par).get(el.parIndex).childNodes[el.txtnIndx]).replaceWith("<hilight>" + $($(el.par).get(el.parIndex).childNodes[el.txtnIndx]).text() + "</hilight>");
                console.log(el.par);
                $(el.par).get(el.parIndex).childNodes[el.txtnIndx]["style"].backgroundColor = "yellow";
                console.log(el);

            } else {
                console.log(el);
                $(el.tagName).get(el.index).style.backgroundColor = "yellow";
                //ele["style"].backgroundColor = "yellow";
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
                        break;
                    case "hide_menu":
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
       //     chrome.runtime.sendMessage({ msg: "tags_changed", data: $(this.menuIframe).contents().find("#tagfield").val() });
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
    mouseUp = (e): void => {
        console.log("mouseUp");
        document.body.removeChild(this.canvas);
        this.isSelecting = false;
        this.selection.stroke = this.inkCanvas._activeStroke;
        this.selection.end(e.clientX, e.clientY);
        console.log(this.selection.getContent()); //print out content 
        this.selection.id = Date.now(); //assign contents of the selection 
        this.selection.type = this.currentStrokeType;
        this.selection.url = this._url;
        this.selection.tags = $(this.menuIframe).contents().find("#tag").val();
        this.selections.push(this.selection); //add selection to selections array 
        this.updateSelectedList();
        chrome.runtime.sendMessage({ msg: "store_selection", data: this.selection });
        this.inkCanvas.clear();
        document.body.appendChild(this.canvas);
    }

    updateSelectedList(): void {
        var list = $(this.menuIframe).contents().find("#selected_list");
        list.empty();
        this.selections.forEach((s) => {
            list.append("<div class='selected_list_item'>" + s.getContent() + "</div>");
        });

    }

    //mousedown action
    mouseDown = (e): void => {
        console.log("mouse down");
        this.selection = new BracketSelection();
   //     this.inkCanvas.switchBrush(this.currentStrokeType);
        try {
            document.body.removeChild(this.canvas);
        } catch (e) {
            console.log("no canvas visible." + e);
        }
        this.checkAtag(e);
        this.isSelecting = true;
        this._startX = e.clientX;
        this._startY = e.clientY;
        this.selection.start(e.clientX, e.clientY);
    }

    mouseMove = (e): void => {
        if (this.isSelecting) {
            this.inkCanvas.draw(e.clientX, e.clientY);
            if (this.currentStrokeType != StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke)) {
                console.log("strokeType changed from " + this.currentStrokeType + " to " + StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke));
                this.currentStrokeType = StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke);
                this.switchSelection(this.currentStrokeType);
                this.inkCanvas.switchBrush(this.currentStrokeType);
            }
        }
    }

    switchSelection(strokeType) {
        console.log("Iselection switched to : " + strokeType);
        switch (strokeType) {
            //////STROKE TYPE CHANGE
            case StrokeType.Marquee:
                this.selection = new MarqueeSelection();
                break;
            case StrokeType.Bracket:
                this.selection = new BracketSelection();
                break;
        }
        this.selection.start(this._startX, this._startY);
    }

    checkAtag = (e): void => {
        var hitElem = document.elementFromPoint(e.clientX, e.clientY);
        if (hitElem.nodeName == "A") {
            var link = hitElem.getAttribute("href").toString();

            if (link.indexOf("http") == -1) {
                link = "http://" + window.location.host + link;
            }
            console.log(link);
            window.open(link, "_self");
        }
        else {
            document.body.appendChild(this.canvas);
        }
    }

    toggleEnabled(flag: boolean): void {
        console.log("toggle state changed");
        $(this.menuIframe).contents().find("#toggle").prop("checked", flag);
        //called to add or remove canvas when toggle has been changed
        this.is_active = flag;

        //this.selections.forEach((selection) => {
        //    if (flag) {
        //        selection.select();

        //    } else
        //        selection.deselect();
        //});

        if (this.is_active) {
            try {
                document.body.appendChild(this.canvas);
                console.log("added canvas");
   
            } catch (ex) {
                console.log("could't add canvas");
            }
            this.currentStrokeType = StrokeType.Bracket;
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

var main = new Main();