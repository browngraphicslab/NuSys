var Main = (function () {
    function Main() {
        var _this = this;
        this.prevStrokeType = StrokeType.Line;
        this.currentStrokeType = StrokeType.Line;
        this.selections = new Array();
        this.urlGroup = Date.now();
        this.previousSelections = new Array();
        this.mouseMove = function (e) {
            if (!_this.isSelecting) {
                return;
            }
            if (_this.currentStrokeType == StrokeType.Bracket || _this.currentStrokeType == StrokeType.Marquee || _this.currentStrokeType == StrokeType.Line) {
                var currType = GestireClassifier.getGestureType(_this.inkCanvas._activeStroke.stroke);
                if (_this.currentStrokeType != StrokeType.Line && currType == GestureType.Horizontal) {
                    _this.selection = new LineSelection(_this.inkCanvas, true);
                    _this.currentStrokeType = StrokeType.Line;
                    _this.inkCanvas.redrawActiveStroke();
                }
                if (_this.currentStrokeType != StrokeType.Marquee && currType == GestureType.Diagonal) {
                    _this.selection = new MarqueeSelection(_this.inkCanvas, true);
                    _this.currentStrokeType = StrokeType.Marquee;
                    _this.inkCanvas.redrawActiveStroke();
                }
                if (_this.currentStrokeType != StrokeType.Bracket && currType == GestureType.Vertical) {
                    _this.selection = new BracketSelection(_this.inkCanvas, true);
                    _this.currentStrokeType = StrokeType.Bracket;
                    _this.inkCanvas.redrawActiveStroke();
                }
                _this.selection.update(e.clientX, e.clientY);
            }
        };
        this.documentDown = function (e) {
            try {
                document.body.removeChild(_this.canvas);
            }
            catch (e) {
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
            switch (_this.currentStrokeType) {
                case StrokeType.MultiLine:
                    _this.selection = new MultiLineSelection(_this.inkCanvas);
                    break;
                case StrokeType.Line:
                case StrokeType.Bracket:
                    _this.selection = new BracketSelection(_this.inkCanvas);
                    break;
                case StrokeType.Marquee:
                    _this.selection = new MarqueeSelection(_this.inkCanvas);
                    break;
            }
            if (_this.currentStrokeType == StrokeType.Bracket || _this.currentStrokeType == StrokeType.Marquee || _this.currentStrokeType == StrokeType.Line) {
                console.log("current selection: " + _this.currentStrokeType);
                document.body.appendChild(_this.canvas);
                _this.canvas.addEventListener("mousemove", _this.mouseMove);
            }
            else {
                try {
                    document.body.removeChild(_this.canvas);
                }
                catch (e) {
                    console.log("no canvas visible." + e);
                }
            }
            _this.selection.start(e.clientX, e.clientY);
            _this.isSelecting = true;
        };
        this.documentScroll = function (e) {
            _this.inkCanvas.update();
        };
        this.documentUp = function (e) {
            console.log("document up");
            if (!_this.isSelecting)
                return;
            _this.isSelecting = false;
            if (_this.currentStrokeType == StrokeType.MultiLine)
                _this.selection.end(e.clientX, e.clientY);
            _this.selection.id = Date.now();
            _this.selection.url = window.location.protocol + "//" + window.location.host + window.location.pathname;
            _this.selection.tags = $(_this.menuIframe).contents().find("#tagfield").val();
            _this.selections.push(_this.selection);
            chrome.runtime.sendMessage({ msg: "store_selection", data: _this.selection });
            _this.selection = new MultiLineSelection(_this.inkCanvas);
            _this.updateSelectedList();
        };
        this.windowUp = function (e) {
            if (!_this.isSelecting)
                return;
            if (_this.currentStrokeType == StrokeType.Bracket || _this.currentStrokeType == StrokeType.Marquee) {
                _this.canvas.removeEventListener("mousemove", _this.mouseMove);
                _this.inkCanvas.removeBrushStroke(_this.inkCanvas._activeStroke);
                _this.inkCanvas.update();
            }
            _this.isSelecting = false;
        };
        this.canvasUp = function (e) {
            console.log("canvas up");
            if (!_this.isSelecting) {
                return;
            }
            _this.canvas.removeEventListener("mousemove", _this.mouseMove);
            if (_this.currentStrokeType == StrokeType.Bracket || _this.currentStrokeType == StrokeType.Marquee || _this.currentStrokeType == StrokeType.Line) {
                document.body.removeChild(_this.canvas);
                $(_this.menuIframe).hide();
                _this.selection.end(e.clientX, e.clientY);
                var stroke = _this.inkCanvas._activeStroke.stroke.getCopy();
                var currType = GestireClassifier.getGestureType(stroke);
                if (currType == GestureType.Null) {
                    console.log("JUST A TAP");
                    document.body.appendChild(_this.canvas);
                    _this.inkCanvas.update();
                    return;
                }
                else {
                    document.body.appendChild(_this.canvas);
                    $(_this.menuIframe).show();
                    _this.inkCanvas.update();
                    _this.isSelecting = false;
                }
                _this.currentStrokeType = StrokeType.Bracket;
            }
            if (_this.selection.getContent() == "" || _this.selection.getContent() == " ") {
                return;
            }
            _this.selection.id = Date.now();
            _this.selection.url = window.location.protocol + "//" + window.location.host + window.location.pathname;
            _this.selection.tags = $(_this.menuIframe).contents().find("#tagfield").val();
            _this.selections.push(_this.selection);
            chrome.runtime.sendMessage({ msg: "store_selection", data: _this.selection });
            _this.updateSelectedList();
        };
        console.log("Starting NuSys.");
        var body = document.body, html = document.documentElement;
        Main.DOC_WIDTH = Math.max(body.scrollWidth, body.offsetWidth, html.clientWidth, html.scrollWidth, html.offsetWidth);
        Main.DOC_HEIGHT = Math.max(body.scrollHeight, body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight);
        this.canvas = document.createElement("canvas");
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
        this.canvas.style.position = "fixed";
        this.canvas.style.top = "0";
        this.canvas.style.left = "0";
        this.canvas.style.zIndex = "998";
        this.inkCanvas = new InkCanvas(this.canvas);
        chrome.storage.local.get(null, function (data) {
            _this.previousSelections = data["selections"];
        });
        chrome.runtime.onMessage.addListener(function (request, sender, sendResponse) {
            console.log(request);
            if (request.msg == "tags_changed") {
                console.log("tags_changed");
                $(_this.menuIframe).contents().find("#tagfield").val(request.data);
            }
            if (request.msg == "check_injection")
                sendResponse(true);
            if (request.msg == "init") {
                _this.init(request.data);
                sendResponse();
            }
            if (request.msg == "show_menu") {
                _this.showMenu();
            }
            if (request.msg == "hide_menu") {
                _this.hideMenu();
            }
            if (request.msg == "enable_selections") {
                _this.toggleEnabled(true);
            }
            if (request.msg == "disable_selections") {
                _this.toggleEnabled(false);
            }
            if (request.msg == "set_selections") {
                _this.selections = [];
                request.data.forEach(function (d) {
                    var ls = null;
                    if (d["className"] == "LineSelection")
                        ls = new LineSelection(_this.inkCanvas);
                    if (d["className"] == "BracketSelection")
                        ls = new BracketSelection(_this.inkCanvas);
                    if (d["className"] == "MarqueeSelection")
                        ls = new MarqueeSelection(_this.inkCanvas);
                    if (d["className"] == "MultiLineSelection")
                        ls = new MultiLineSelection(_this.inkCanvas);
                    $.extend(ls, d);
                    ls._inkCanvas = _this.inkCanvas;
                    if (ls._brushStroke != null) {
                        var stroke = new Stroke();
                        $.extend(stroke, ls._brushStroke.stroke);
                        ls._brushStroke.stroke = stroke;
                    }
                    _this.selections.push(ls);
                });
                var count = 0;
                _this.selections.forEach(function (sel) {
                    if (sel instanceof MultiLineSelection) {
                        console.log("found MultiSelection");
                        sel.selectedElements.forEach(function (el) {
                            var startNode = $(el.start.tagName)[el.start.parentIndex];
                            var endNode = $(el.end.tagName)[el.end.parentIndex];
                            var startParentData = count++;
                            var endParentData = count++;
                            if ($(startNode).attr("data-cTedId") == undefined) {
                                $(startNode).attr("data-cTedId", startParentData);
                            }
                            else {
                                startParentData = parseInt($(startNode).attr("data-cTedId"));
                            }
                            if ($(endNode).attr("data-cTedId") == undefined) {
                                $(endNode).attr("data-cTedId", endParentData);
                            }
                            else {
                                endParentData = parseInt($(endNode).attr("data-cTedId"));
                            }
                            $(endNode).attr("data-cTedId", endParentData);
                            el.start.id = startParentData;
                            el.end.id = endParentData;
                        });
                    }
                });
                sendResponse();
                _this.updateSelectedList();
            }
        });
    }
    Main.prototype.init = function (menuHtml) {
        var _this = this;
        console.log("init!");
        this.currentStrokeType = StrokeType.MultiLine;
        document.addEventListener("mouseup", this.documentUp);
        this.menuIframe = $("<iframe frameborder=0>")[0];
        document.body.appendChild(this.menuIframe);
        this.menu = $(menuHtml)[0];
        $(this.menuIframe).css({ position: "fixed", top: "1px", right: "1px", width: "410px", height: "106px", "z-index": 1001 });
        $(this.menuIframe).contents().find('html').html(this.menu.outerHTML);
        $(this.menuIframe).css("display", "none");
        $(this.menuIframe).contents().find("#btnExport").click(function (ev) {
            chrome.runtime.sendMessage({ msg: "export" });
        });
        $(this.menuIframe).contents().find("#btnLineSelect").click(function (ev) {
            if (_this.currentStrokeType == StrokeType.MultiLine)
                return;
            var other = $(_this.menuIframe).contents().find("#btnBlockSelect");
            console.log("switching to multiline selection");
            if ($(ev.target).hasClass("active")) {
                $(ev.target).removeClass("active");
            }
            else {
                $(ev.target).addClass("active");
                $(other).removeClass("active");
            }
            _this.currentStrokeType = StrokeType.MultiLine;
            document.addEventListener("mouseup", _this.documentUp);
        });
        $(this.menuIframe).contents().find("#btnBlockSelect").click(function (ev) {
            if (_this.currentStrokeType == StrokeType.Bracket)
                return;
            var other = $(_this.menuIframe).contents().find("#btnLineSelect");
            if ($(ev.target).hasClass("active")) {
                $(ev.target).removeClass("active");
            }
            else {
                $(ev.target).addClass("active");
                $(other).removeClass("active");
            }
            _this.currentStrokeType = StrokeType.Bracket;
            try {
                document.body.appendChild(_this.canvas);
            }
            catch (ex) {
                console.log("could't add canvas");
            }
            document.removeEventListener("mouseup", _this.documentUp);
        });
        $(this.menuIframe).contents().find("#tagfield").change(function () {
            chrome.runtime.sendMessage({ msg: "tags_changed", data: $(_this.menuIframe).contents().find("#tagfield").val() });
        });
        $(this.menuIframe).contents().find("#btnViewAll").click(function () {
            chrome.runtime.sendMessage({ msg: "view_all" });
        });
        $(this.menuIframe).contents().find("#toggle").change(function () {
            chrome.runtime.sendMessage({ msg: "set_active", data: $(_this.menuIframe).contents().find("#toggle").prop("checked") });
        });
        $(this.menuIframe).contents().find("#btnExpand").click(function (ev) {
            console.log("expand");
            var list = $(_this.menuIframe).contents().find("#selected_list");
            if ($(ev.target).hasClass("active")) {
                $(ev.target).removeClass("active");
                $(list).removeClass("open");
                $(_this.menuIframe).height(106);
            }
            else {
                $(ev.target).addClass("active");
                $(list).addClass("open");
                $(_this.menuIframe).height(500);
            }
        });
        chrome.runtime.sendMessage({ msg: "query_active" }, function (isActive) {
            $(_this.menuIframe).contents().find("#toggle").prop("checked", isActive);
        });
    };
    Main.prototype.showMenu = function () {
        this.isMenuVisible = true;
        $(this.menuIframe).css("display", "block");
    };
    Main.prototype.hideMenu = function () {
        this.isMenuVisible = false;
        $(this.menuIframe).css("display", "none");
    };
    Main.prototype.toggleEnabled = function (flag) {
        $(this.menuIframe).contents().find("#toggle").prop("checked", flag);
        this.isEnabled = flag;
        this.selections.forEach(function (selection) {
            if (flag) {
                selection.select();
            }
            else
                selection.deselect();
        });
        if (this.isEnabled) {
            this.currentStrokeType = StrokeType.Bracket;
            try {
                document.body.appendChild(this.canvas);
            }
            catch (ex) {
                console.log("could't add canvas");
            }
            document.removeEventListener("mouseup", this.documentUp);
            window.addEventListener("mouseup", this.windowUp);
            document.body.addEventListener("mousedown", this.documentDown);
            document.addEventListener("scroll", this.documentScroll);
            this.canvas.addEventListener("mouseup", this.canvasUp);
            document.body.appendChild(this.canvas);
            this.inkCanvas.update();
        }
        else {
            window.removeEventListener("mouseup", this.windowUp);
            document.body.removeEventListener("mousedown", this.documentDown);
            document.removeEventListener("scroll", this.documentScroll);
            this.canvas.removeEventListener("mouseup", this.canvasUp);
            try {
                document.body.removeChild(this.canvas);
            }
            catch (e) {
                console.log("no canvas visible." + e);
            }
        }
    };
    Main.prototype.updateSelectedList = function () {
        var list = $(this.menuIframe).contents().find("#selected_list");
        list.empty();
        this.selections.forEach(function (s) {
            list.append("<div class='selected_list_item'>" + s.getContent() + "</div>");
        });
    };
    Main.relativeToAbsolute = function (content) {
        var res = content.split('href="');
        var newval = res[0];
        for (var i = 1; i < res.length; i++) {
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
            if (src[i].slice(0, 4) != "http" && src[i].slice(0, 2) != "//") {
                finalval += window.location["origin"];
                var path = window.location.pathname;
                var pathSplit = path.split('/');
                var newpath = "";
                var pIndex = pathSplit.length - 1;
                $(pathSplit).each(function (indx, elem) {
                    if (indx < pathSplit.length - 1) {
                        newpath += (elem + "/");
                    }
                });
                var newpathSplit = newpath.split("/");
                var p = "";
                pIndex = newpathSplit.length - 1;
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
    };
    return Main;
})();
