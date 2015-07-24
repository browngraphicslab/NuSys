﻿/// <reference path="../typings/chrome/chrome.d.ts"/>
/// <reference path="../typings/jquery/jquery.d.ts"/>
/// <reference path="ink/InkCanvas.ts"/>
/// <reference path="selection/LineSelection.ts"/>
/// <reference path="selection/UnknownSelection.ts"/>

class Main {
    
    static DOC_WIDTH: number;
    static DOC_HEIGHT: number;

    constructor() {
        console.log("Starting NuSys yo");
        this.init();
    }



    init() {
        var body = document.body,
            html = document.documentElement;

        Main.DOC_WIDTH = Math.max(body.scrollWidth, body.offsetWidth,
            html.clientWidth, html.scrollWidth, html.offsetWidth);

        Main.DOC_HEIGHT = Math.max(body.scrollHeight, body.offsetHeight,
            html.clientHeight, html.scrollHeight, html.offsetHeight);

        var port = chrome.runtime.connect({ name: "content" });

        var canvas = document.createElement("canvas");
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
        canvas.style.position = "fixed";
        canvas.style.top = "0";

        document.body.appendChild(canvas);
        var inkCanvas = new InkCanvas(canvas);
        var selection:ISelection = new LineSelection(inkCanvas);

        document.body.addEventListener("mousedown", function (e) {

            document.body.appendChild(canvas);
            selection.start(e.clientX, e.clientY);
            canvas.addEventListener("mousemove", onMouseMove);
        });

        document.addEventListener("scroll", function () {
            inkCanvas.update();
        });


        var prevStrokeType = StrokeType.Line;

        function onMouseMove(e) {
            var currType = StrokeClassifier.getStrokeType(inkCanvas._activeStroke.stroke);
            if (currType != prevStrokeType) {
                prevStrokeType = currType;
                switch (currType) {
                    case StrokeType.Line:
                        selection = new LineSelection(inkCanvas);
                        break;
                    case StrokeType.Bracket:
                        selection = new BracketSelection(inkCanvas, true);
                        console.log("switching to bracket!")
                        break;  
                    case StrokeType.Marquee:
                        selection = new MarqueeSelection(inkCanvas, true);
                        console.log("switching to marquee!")
                        break; 
                    case StrokeType.Scribble:
                        selection = new UnknownSelection(inkCanvas, true);
                        console.log("switching to unknown!")
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
            var currType = prevStrokeType;
            //var stroke = inkCanvas._activeStroke.stroke;
            var stroke = inkCanvas._activeStroke.stroke.getCopy();
            var currType = StrokeClassifier.getStrokeType(stroke);
            console.log("curr: " + currType);
           // console.log("scribble: " + StrokeType.Scribble);

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
                        } catch (e) {
                            console.log(e)
                            console.log(this);
                        }
                    });
                }
                inkCanvas.removeBrushStroke(inkCanvas._activeStroke);
            }
            else {
                selections.push(selection);
            }

            selection = new LineSelection(inkCanvas);
            prevStrokeType = StrokeType.Line;

            document.body.appendChild(canvas);
            inkCanvas.update();
        });
    }
} 