/// <reference path="../typings/chrome/chrome.d.ts"/>
/// <reference path="../typings/jquery/jquery.d.ts"/>
var Main = (function () {
    function Main() {
        console.log("Starting NuSys yo");
        this.init();
    }
    Main.prototype.init = function () {
        var body = document.body, html = document.documentElement;
        var dwidth = Math.max(body.scrollWidth, body.offsetWidth, html.clientWidth, html.scrollWidth, html.offsetWidth);
        var dheight = Math.max(body.scrollHeight, body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight);
        var port = chrome.runtime.connect({ name: "content" });
        var canvas = document.createElement("canvas");
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
        canvas.style.position = "fixed";
        canvas.style.top = "0";
        document.body.appendChild(canvas);
        var inkCanvas = new InkCanvas(canvas);
        var selection = new LineSelection(inkCanvas);
        document.body.addEventListener("mousedown", function (e) {
            document.body.appendChild(canvas);
            selection.start(e.clientX, e.clientY);
            canvas.addEventListener("mousemove", onMouseMove);
        });
        document.addEventListener("scroll", function () {
            inkCanvas.update();
        });
        var prevStrokeType = 0 /* Line */;
        function onMouseMove(e) {
            var currType = StrokeClassifier.getStrokeType(inkCanvas._activeStroke.stroke);
            if (currType != prevStrokeType) {
                prevStrokeType = currType;
                switch (currType) {
                    case 0 /* Line */:
                        selection = new LineSelection(inkCanvas);
                        break;
                    case 1 /* Bracket */:
                        selection = new BracketSelection(inkCanvas, true);
                        console.log("switching to bracket!");
                        break;
                }
                inkCanvas.redrawActiveStroke();
            }
            selection.update(e.clientX, e.clientY);
        }
        var selections = [];
        canvas.addEventListener("mouseup", function (e) {
            canvas.removeEventListener("mousemove", onMouseMove);
            document.body.removeChild(canvas);
            selection.end(e.clientX, e.clientY);
            var stroke = inkCanvas._activeStroke.stroke.getResampled(20);
            //inkCanvas.drawStroke(stroke, new CircleBrush());
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
                $.each(selections, function () {
                    try {
                        if (this.getBoundingRect().intersectsRectangle(strokeBB)) {
                            console.log(this);
                            this.deselect();
                            console.log("RECT INTERSECTION");
                            var selectionIndex = selections.indexOf(this);
                            if (selectionIndex > -1)
                                selections.splice(selectionIndex, 1);
                        }
                    }
                    catch (e) {
                        console.log(e);
                        console.log(this);
                    }
                });
                inkCanvas.update();
            }
            var currType = StrokeClassifier.getStrokeType(inkCanvas._activeStroke.stroke);
            if (currType != 3 /* Scribble */) {
                selections.push(selection);
                var myWindow = window.open("", "Selected", "width=1000, height=1000");
                myWindow.focus();
                myWindow.document.body.innerHTML = "";
                myWindow.document.write(selection.getContent());
            }
            else {
                inkCanvas.removeBrushStroke(inkCanvas._activeStroke);
                inkCanvas.update();
            }
            // console.log("num selections: " + selections.length);
            //port.postMessage({ "text": content });
            ///////////// UNCOMMENT TO SHOW WINDOW!!!!!!!!!!!!!!!!!!!!//////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////
            // console.log(content);
            document.body.appendChild(canvas);
            selection = new LineSelection(inkCanvas);
            prevStrokeType = 0 /* Line */;
        });
    };
    return Main;
})();
/// <reference path="Main.ts"/>
document.onload = function () {
    var greeter = new Main();
};
var BrushStroke = (function () {
    function BrushStroke(brush, stroke) {
        this.brush = brush;
        this.stroke = stroke;
    }
    return BrushStroke;
})();
var HighlightBrush = (function () {
    function HighlightBrush() {
        this._img = new Image();
        this._img.src = chrome.extension.getURL("assets/brush.png");
    }
    HighlightBrush.prototype.init = function (x, y, inkCanvas) {
        // do nothing
    };
    HighlightBrush.prototype.draw = function (x, y, inkCanvas) {
        inkCanvas._context.globalCompositeOperation = "xor";
        inkCanvas._context.globalAlpha = 0.6;
        inkCanvas._context.drawImage(this._img, x - 15, y - 15, 30, 30);
    };
    HighlightBrush.prototype.drawStroke = function (stroke, inkCanvas) {
        for (var i = 0; i < stroke.points.length; i++) {
            var p = stroke.points[i];
            inkCanvas._context.globalCompositeOperation = "xor";
            inkCanvas._context.globalAlpha = 0.6;
            inkCanvas._context.drawImage(this._img, p.x - inkCanvas._scrollOffset.x + stroke.documentOffsetX - 15, p.y + stroke.documentOffsetY - inkCanvas._scrollOffset.y - 15, 30, 30);
        }
    };
    return HighlightBrush;
})();
var SelectionBrush = (function () {
    function SelectionBrush(rect) {
        this._rect = rect;
    }
    SelectionBrush.prototype.init = function (x, y, inkCanvas) {
        // do nothing
    };
    SelectionBrush.prototype.draw = function (x, y, inkCanvas) {
        // do nothing.
    };
    SelectionBrush.prototype.drawStroke = function (stroke, inkCanvas) {
        if (this._rect != null) {
            console.log(this._rect);
            stroke = new Stroke();
            stroke.points.push({ x: this._rect.x, y: this._rect.y });
            stroke.points.push({ x: this._rect.x + this._rect.w, y: this._rect.y + this._rect.h });
        }
        var startX = stroke.points[0].x;
        var startY = stroke.points[0].y;
        var w = stroke.points[stroke.points.length - 1].x - startX;
        var h = stroke.points[stroke.points.length - 1].y - startY;
        startX = startX - inkCanvas._scrollOffset.x + stroke.documentOffsetX;
        startY = startY - inkCanvas._scrollOffset.y + stroke.documentOffsetY;
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";
        ctx.globalAlpha = 0.6;
        ctx.beginPath();
        ctx.fillStyle = "rgb(222,214,0)";
        ctx.fillRect(startX, startY, w, h);
        ctx.fill();
    };
    return SelectionBrush;
})();
/// <reference path="brush/BrushStroke.ts"/>
var InkCanvas = (function () {
    function InkCanvas(canvas) {
        this._canvas = canvas;
        this._context = canvas.getContext("2d");
        this._isDrawing = false;
        this._brushStrokes = [];
        this._brush = null;
        this._scrollOffset = { x: 0, y: 0 };
    }
    InkCanvas.prototype.drawStroke = function (stroke, brush) {
        if (brush)
            this._brush = brush;
        this._scrollOffset = { x: 0, y: 0 };
        this._isDrawing = true;
        this._activeStroke = new BrushStroke(this._brush, new Stroke());
        this._activeStroke.stroke.documentOffsetY = window.pageYOffset;
        var first = stroke.points[0];
        var last = stroke.points[stroke.points.length - 1];
        this.startDrawing(first.x, first.y, brush);
        for (var i = 1; i < stroke.points.length - 2; i++) {
            this.draw(stroke.points[i].x, stroke.points[i].y);
        }
        this.endDrawing(last.x, last.y);
    };
    InkCanvas.prototype.startDrawing = function (x, y, brush) {
        if (brush)
            this._brush = brush;
        this._brush.init(x, y, this);
        this._scrollOffset = { x: 0, y: 0 };
        this._isDrawing = true;
        this._activeStroke = new BrushStroke(this._brush, new Stroke());
        this._activeStroke.stroke.documentOffsetY = window.pageYOffset;
        this.draw(x, y);
    };
    InkCanvas.prototype.draw = function (x, y) {
        if (this._isDrawing == false)
            return;
        this._activeStroke.stroke.points.push({ x: x, y: y });
        this._brush.draw(x, y, this);
    };
    InkCanvas.prototype.endDrawing = function (x, y) {
        this.draw(x, y);
        this._isDrawing = false;
        this._brushStrokes.push(this._activeStroke);
    };
    InkCanvas.prototype.removeBrushStroke = function (brushStroke) {
        var index = this._brushStrokes.indexOf(brushStroke);
        if (index > -1) {
            this._brushStrokes.splice(index, 1);
            return true;
        }
        return false;
        console.log("couldn't remove element");
    };
    InkCanvas.prototype.update = function () {
        this._scrollOffset = { x: window.pageXOffset, y: window.pageYOffset };
        this._context.clearRect(0, 0, this._canvas.width, this._canvas.height);
        for (var i = 0; i < this._brushStrokes.length; i++) {
            this._brushStrokes[i]["brush"].drawStroke(this._brushStrokes[i]["stroke"], this);
        }
    };
    InkCanvas.prototype.setBrush = function (brush) {
        this._brush = brush;
        if (this._isDrawing) {
            this._activeStroke.brush = brush;
            var p = this._activeStroke.stroke.points[0];
            this._brush.init(p.x, p.y, this);
        }
    };
    InkCanvas.prototype.redrawActiveStroke = function () {
        this.update();
        this._activeStroke.brush.drawStroke(this._activeStroke.stroke, this);
    };
    ///called after lineSelection so that highlights for line selection disappear
    ///bracket selections are yet updated
    InkCanvas.prototype.removeStroke = function () {
        this._context.clearRect(0, 0, this._canvas.width, this._canvas.height);
        this.update();
    };
    return InkCanvas;
})();
var Rectangle = (function () {
    function Rectangle(x, y, w, h) {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
    }
    Rectangle.prototype.intersectsRectangle = function (r2) {
        return !(r2.x > this.x + this.w || r2.x + r2.w < this.x || r2.y > this.y + this.h || r2.y + r2.h < this.y);
    };
    return Rectangle;
})();
/// <reference path="../util/Rectangle.ts"/>
var Stroke = (function () {
    function Stroke() {
        this.documentOffsetX = 0;
        this.documentOffsetY = 0;
        this.points = new Array();
    }
    Stroke.fromPoints = function (points) {
        var stroke = new Stroke();
        stroke.points = points.slice(0);
        return stroke;
    };
    Stroke.prototype.getBoundingRect = function () {
        var minX = 1000000;
        var maxX = -1000000;
        var minY = 1000000;
        var maxY = -1000000;
        for (var i = 0; i < this.points.length; i++) {
            var p = this.points[i];
            maxY = p.y > maxY ? p.y : maxY;
            maxX = p.x > maxX ? p.x : maxX;
            minX = p.x < minX ? p.x : minX;
            minY = p.y < minY ? p.y : minY;
        }
        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    };
    Stroke.prototype.breakUp = function () {
        var segments = new Array();
        for (var i = 1; i < this.points.length; i++) {
            var p0 = this.points[i - 1];
            var p1 = this.points[i];
            segments.push(Line.fromPoint(p0, p1));
        }
        return segments;
    };
    Stroke.prototype.getResampled = function (samples) {
        var c = this.getCopy();
        c.resample(samples);
        return c;
    };
    Stroke.prototype.getEntropy = function () {
        var angles = [];
        for (var i = 1; i < this.points.length; i++) {
            var v0 = new Vector2(this.points[i - 1].x, this.points[i - 1].y);
            var v1 = new Vector2(this.points[i].x, this.points[i].y);
            angles.push(v0.angleTo(v1));
        }
    };
    Stroke.prototype.getStrokeMetrics = function () {
        var startPoint = Vector2.fromPoint(this.points[0]);
        var endPoint = Vector2.fromPoint(this.points[this.points.length - 1]);
        var l = endPoint.subtract(startPoint);
        var ln = l.getNormalized();
        var error = 0;
        var errors = [];
        for (var i = 0; i < this.points.length; i++) {
            var a = Vector2.fromPoint(this.points[i]).subtract(startPoint);
            var b = ln.multiply(a.dot(ln));
            var c = a.subtract(b);
            error += Math.abs(c.length());
            errors.push(Math.abs(c.length()));
        }
        function median(values) {
            values.sort(function (a, b) {
                return a - b;
            });
            var half = Math.floor(values.length / 2);
            if (values.length % 2)
                return values[half];
            else
                return (values[half - 1] + values[half]) / 2.0;
        }
        var m = median(errors);
        error /= this.points.length;
        return { length: this.points.length, error: m };
    };
    Stroke.prototype.resample = function (numSamples) {
        var oldSamples = this.points;
        var scale = numSamples / oldSamples.length;
        var newSamples = new Array(numSamples);
        var radius = scale > 1 ? 1 : 1 / (2 * scale);
        var startX = oldSamples[0].x;
        var deltaX = oldSamples[oldSamples.length - 1].x - startX;
        for (var i = 0; i < numSamples; ++i) {
            var center = i / scale + (1.0 - scale) / (2.0 * scale);
            var left = Math.ceil(center - radius);
            var right = Math.floor(center + radius);
            var sum = 0;
            var sumWeights = 0;
            for (var k = left; k <= right; k++) {
                var weight = this.g(k - center, scale);
                var index = Math.max(0, Math.min(oldSamples.length - 1, k));
                sum += weight * oldSamples[index].y;
                sumWeights += weight;
            }
            sum /= sumWeights;
            newSamples[i] = { x: startX + i / numSamples * deltaX, y: sum };
        }
        this.points = newSamples.slice(0);
    };
    Stroke.prototype.g = function (x, a) {
        var radius;
        if (a < 1)
            radius = 1.0 / a;
        else
            radius = 1.0;
        if ((x < -radius) || (x > radius))
            return 0;
        else
            return (1 - Math.abs(x) / radius) / radius;
    };
    Stroke.prototype.getCopy = function () {
        var s = new Stroke();
        s.points = this.points.slice(0);
        return s;
    };
    return Stroke;
})();
var StrokeClassifier = (function () {
    function StrokeClassifier() {
    }
    StrokeClassifier.getStrokeType = function (stroke) {
        var p0 = stroke.points[0];
        var p1 = stroke.points[stroke.points.length - 1];
        var metrics = stroke.getStrokeMetrics();
        if (metrics.error > 20) {
            return 3 /* Scribble */;
        }
        if (Math.abs(p1.y - p0.y) < 20) {
            return 0 /* Line */;
        }
        if (Math.abs(p1.x - p0.x) < 20) {
            return 1 /* Bracket */;
        }
        if (Math.abs(p1.x - p0.x) > 50 && Math.abs(p1.y - p0.y) > 20) {
            return 2 /* Marquee */;
        }
    };
    return StrokeClassifier;
})();
var StrokeType;
(function (StrokeType) {
    StrokeType[StrokeType["Line"] = 0] = "Line";
    StrokeType[StrokeType["Bracket"] = 1] = "Bracket";
    StrokeType[StrokeType["Marquee"] = 2] = "Marquee";
    StrokeType[StrokeType["Scribble"] = 3] = "Scribble";
})(StrokeType || (StrokeType = {}));
var BracketSelection = (function () {
    function BracketSelection(inkCanvas, fromActiveStroke) {
        if (fromActiveStroke === void 0) { fromActiveStroke = false; }
        this._brushStroke = null;
        this._inkCanvas = inkCanvas;
        if (fromActiveStroke) {
            this._inkCanvas.setBrush(new HighlightBrush());
            var t = this;
            $.each(inkCanvas._activeStroke.stroke.points, function () {
                t._inkCanvas.draw(this.x, this.y);
            });
        }
    }
    BracketSelection.prototype.start = function (x, y) {
        this._inkCanvas.startDrawing(x, y, new HighlightBrush());
    };
    BracketSelection.prototype.update = function (x, y) {
        this._inkCanvas.draw(x, y);
    };
    BracketSelection.prototype.end = function (x, y) {
        this._inkCanvas.endDrawing(x, y);
        this._brushStroke = this._inkCanvas._activeStroke;
        this.analyzeContent();
        this._brushStroke.brush = new SelectionBrush(this.getBoundingRect());
        this._inkCanvas.update();
    };
    BracketSelection.prototype.deselect = function () {
        this._inkCanvas.removeBrushStroke(this._brushStroke);
    };
    BracketSelection.prototype.getBoundingRect = function () {
        var minX = 1000000;
        var maxX = -1000000;
        var minY = 1000000;
        var maxY = -1000000;
        for (var i = 0; i < this._clientRects.length; i++) {
            var p = this._clientRects[i];
            maxY = p.top + p.height > maxY ? p.top + p.height : maxY;
            maxX = p.left + p.width > maxX ? p.left + p.width : maxX;
            minX = p.left < minX ? p.left : minX;
            minY = p.top < minY ? p.top : minY;
        }
        return new Rectangle(minX, minY + window.pageYOffset, maxX - minX, maxY - minY);
    };
    BracketSelection.prototype.analyzeContent = function () {
        var stroke = this._brushStroke.stroke;
        var selectionBB = stroke.getBoundingRect();
        selectionBB.w = 1024 - selectionBB.x; // TODO: fix this magic number
        var samplingRate = 20;
        var numSamples = 0;
        var totalScore = 0;
        var hitCounter = new Map();
        for (var x = selectionBB.x; x < selectionBB.x + selectionBB.w; x += samplingRate) {
            for (var y = selectionBB.y; y < selectionBB.y + selectionBB.h; y += samplingRate) {
                var hitElem = document.elementFromPoint(x, y);
                numSamples++;
                if (($(hitElem).width() * $(hitElem).height()) / (selectionBB.w * selectionBB.h) < 0.1)
                    continue;
                var score = (1.0 - x / (selectionBB.x + selectionBB.w)) / (selectionBB.w * selectionBB.h);
                if (hitCounter.get(hitElem) == undefined)
                    hitCounter.set(hitElem, score);
                else
                    hitCounter.set(hitElem, hitCounter.get(hitElem) + score);
                totalScore += score;
            }
        }
        var candidates = [];
        var precision = 4;
        for (var k in hitCounter) {
            candidates.push(hitCounter[k] / totalScore);
        }
        var std = Statistics.getStandardDeviation(candidates, precision);
        var result = "";
        this._clientRects = new Array();
        for (var k in hitCounter) {
            candidates.push(hitCounter[k] / totalScore);
        }
        var count = 0;
        var result = "";
        for (var k in hitCounter) {
            if (!Statistics.isWithinStd(candidates[count++], 1, std))
                continue;
            result += k.outerHTML;
            var range = document.createRange();
            range.selectNodeContents(k);
            var rects = range.getClientRects();
            this._clientRects = this._clientRects.concat.apply([], rects);
        }
        console.log(result);
        this._content = result;
    };
    BracketSelection.prototype.getContent = function () {
        return this._content;
    };
    return BracketSelection;
})();
var LineSelection = (function () {
    function LineSelection(inkCanvas) {
        this._brushStroke = null;
        this._inkCanvas = inkCanvas;
    }
    LineSelection.prototype.start = function (x, y) {
        this._inkCanvas.startDrawing(x, y, new HighlightBrush());
    };
    LineSelection.prototype.update = function (x, y) {
        this._inkCanvas.draw(x, y);
    };
    LineSelection.prototype.end = function (x, y) {
        this._inkCanvas.endDrawing(x, y);
        this._brushStroke = this._inkCanvas._activeStroke;
        this.analyzeContent();
        this._brushStroke.brush = new SelectionBrush(this.getBoundingRect());
        this._inkCanvas.update();
    };
    LineSelection.prototype.deselect = function () {
        this._inkCanvas.removeBrushStroke(this._brushStroke);
    };
    LineSelection.prototype.getBoundingRect = function () {
        var minX = 1000000;
        var maxX = -1000000;
        var minY = 1000000;
        var maxY = -1000000;
        for (var i = 0; i < this._clientRects.length; i++) {
            var p = this._clientRects[i];
            maxY = p.top + p.height > maxY ? p.top + p.height : maxY;
            maxX = p.left + p.width > maxX ? p.left + p.width : maxX;
            minX = p.left < minX ? p.left : minX;
            minY = p.top < minY ? p.top : minY;
        }
        return new Rectangle(minX, minY + window.pageYOffset, maxX - minX, maxY - minY);
    };
    LineSelection.prototype.analyzeContent = function () {
        var stroke = this._brushStroke.stroke;
        var pStart = stroke.points[0];
        var pEnd = stroke.points[stroke.points.length - 1];
        var nStart = document.elementFromPoint(pStart.x, pStart.y);
        var nEnd = document.elementFromPoint(pEnd.x, pEnd.y);
        var commonParent = DomUtil.getCommonAncestor(nStart, nEnd);
        var nodes = $(commonParent).contents();
        if (nodes.length > 0) {
            var original_content = $(commonParent).clone();
            $.each(nodes, function () {
                if (this.nodeType == Node.TEXT_NODE) {
                    $(this).replaceWith($(this).text().replace(/([^,\s]*)/g, "<word>$1</word>"));
                }
            });
            nStart = document.elementFromPoint(pStart.x, pStart.y);
            nEnd = document.elementFromPoint(pEnd.x, pEnd.y);
            this._range = new Range();
            this._range.setStart(nStart, 0);
            this._range.setEndAfter(nEnd);
            this._clientRects = this._range.getClientRects();
            var frag = this._range.cloneContents();
            var result = "";
            $.each(frag.childNodes, function () {
                result += $(this)[0].outerHTML.replace(/<word>|<\/word>/g, " ");
            });
            result = result.replace(/\s\s+/g, ' ').trim();
            this._content = result;
            $(commonParent).replaceWith(original_content);
        }
    };
    LineSelection.prototype.getContent = function () {
        return this._content;
    };
    return LineSelection;
})();
var DomUtil = (function () {
    function DomUtil() {
    }
    DomUtil.getCommonAncestor = function (a, b) {
        var parentsa = $(a).parents().toArray();
        var parentsb = $(b).parents().toArray();
        parentsa.unshift(a);
        parentsb.unshift(b);
        var found = null;
        $.each(parentsa, function () {
            var thisa = this;
            $.each(parentsb, function () {
                if (thisa == this) {
                    found = this;
                    return false;
                }
            });
            if (found)
                return false;
        });
        return found;
    };
    return DomUtil;
})();
var Line = (function () {
    function Line() {
    }
    Line.fromPoint = function (start, end) {
        var line = new Line();
        line.start = new Vector2(start.x, start.y);
        line.end = new Vector2(end.x, end.y);
        return line;
    };
    Line.fromVector = function (start, end) {
        var line = new Line();
        line.start = start.clone();
        line.end = end.clone();
        return line;
    };
    Line.prototype.intersectsLine = function (other) {
        var s1_x = this.end.x - this.start.x;
        var s1_y = this.end.y - this.start.y;
        var s2_x = other.end.x - other.start.x;
        var s2_y = other.end.y - other.start.y;
        var s, t;
        s = (-s1_y * (this.start.x - other.start.x) + s1_x * (this.start.y - other.start.y)) / (-s2_x * s1_y + s1_x * s2_y);
        t = (s2_x * (this.start.y - other.start.y) - s2_y * (this.start.x - other.start.x)) / (-s2_x * s1_y + s1_x * s2_y);
        if (s >= 0 && s <= 1 && t >= 0 && t <= 1) {
            return true;
        }
        return false; // No collision
    };
    return Line;
})();
var Statistics = (function () {
    function Statistics() {
    }
    Statistics.getNumWithSetDec = function (num, numOfDec) {
        var pow10s = Math.pow(10, numOfDec || 0);
        return (numOfDec) ? Math.round(pow10s * num) / pow10s : num;
    };
    Statistics.getAverageFromNumArr = function (numArr, numOfDec) {
        var i = numArr.length, sum = 0;
        while (i--) {
            sum += numArr[i];
        }
        return Statistics.getNumWithSetDec((sum / numArr.length), numOfDec);
    };
    Statistics.getVariance = function (numArr, numOfDec) {
        var avg = Statistics.getAverageFromNumArr(numArr, numOfDec), i = numArr.length, v = 0;
        while (i--) {
            v += Math.pow((numArr[i] - avg), 2);
        }
        v /= numArr.length;
        return Statistics.getNumWithSetDec(v, numOfDec);
    };
    Statistics.isWithinStd = function (num, dist, std) {
        return num >= dist * std;
    };
    Statistics.getStandardDeviation = function (numArr, numOfDec) {
        var stdDev = Math.sqrt(Statistics.getVariance(numArr, numOfDec));
        return Statistics.getNumWithSetDec(stdDev, numOfDec);
    };
    return Statistics;
})();
var Vector2 = (function () {
    function Vector2(x, y) {
        this.x = x;
        this.y = y;
    }
    Vector2.fromPoint = function (p) {
        return new Vector2(p.x, p.y);
    };
    Vector2.prototype.negative = function () {
        return new Vector2(-this.x, -this.y);
    };
    Vector2.prototype.add = function (v) {
        if (v instanceof Vector2)
            return new Vector2(this.x + v.x, this.y + v.y);
        else
            return new Vector2(this.x + v, this.y + v);
    };
    Vector2.prototype.subtract = function (v) {
        if (v instanceof Vector2)
            return new Vector2(this.x - v.x, this.y - v.y);
        else
            return new Vector2(this.x - v, this.y - v);
    };
    Vector2.prototype.multiply = function (v) {
        if (v instanceof Vector2)
            return new Vector2(this.x * v.x, this.y * v.y);
        else
            return new Vector2(this.x * v, this.y * v);
    };
    Vector2.prototype.divide = function (v) {
        if (v instanceof Vector2)
            return new Vector2(this.x / v.x, this.y / v.y);
        else
            return new Vector2(this.x / v, this.y / v);
    };
    Vector2.prototype.equals = function (v) {
        return this.x == v.x && this.y == v.y;
    };
    Vector2.prototype.dot = function (v) {
        return this.x * v.x + this.y * v.y;
    };
    Vector2.prototype.length = function () {
        return Math.sqrt(this.dot(this));
    };
    Vector2.prototype.getNormalized = function () {
        return this.divide(this.length());
    };
    Vector2.prototype.distanceTo = function (other) {
        return Math.sqrt((this.x - other.x) * (this.x - other.x) + (this.y - other.y) * (this.y - other.y));
    };
    Vector2.prototype.cross = function (other) {
        return this.x * other.y - this.y * other.x;
    };
    Vector2.prototype.clone = function () {
        return new Vector2(this.x, this.y);
    };
    Vector2.prototype.angleTo = function (a) {
        return Math.acos(this.dot(a) / (this.length() * a.length()));
    };
    Vector2.prototype.init = function (x, y) {
        this.x = x;
        this.y = y;
        return this;
    };
    return Vector2;
})();
//# sourceMappingURL=NuSysChromeExtension.js.map