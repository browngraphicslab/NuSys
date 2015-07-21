/**
 * Created by phili_000 on 4/28/2015.
 */

console.log("Starting NuSys");

var body = document.body,
    html = document.documentElement;

var dwidth = Math.max( body.scrollWidth, body.offsetWidth,
    html.clientWidth, html.scrollWidth, html.offsetWidth );

var dheight = Math.max( body.scrollHeight, body.offsetHeight,
    html.clientHeight, html.scrollHeight, html.offsetHeight );

var port = chrome.runtime.connect({name:"content"});

var canvas = document.createElement("canvas");
canvas.width = window.innerWidth;
canvas.height = window.innerHeight;
canvas.style.position = "fixed";
canvas.style.top = 0;

document.body.appendChild(canvas);
var inkCanvas = new InkCanvas(canvas);
var selection = new LineSelection(inkCanvas);

document.body.addEventListener("mousedown", function(e) {

    document.body.appendChild(canvas);
    selection.start(e.clientX, e.clientY);
    canvas.addEventListener("mousemove", onMouseMove);
});

document.addEventListener("scroll", function(){
    inkCanvas.update();
});

var prevStrokeType = StrokeType.LINE;

function onMouseMove(e) {
    var currType = StrokeClassifier.GetStrokeType(inkCanvas._activeStroke.stroke);
    if(currType != prevStrokeType) {
        prevStrokeType = currType;
        switch (currType) {
            case StrokeType.LINE:
                selection = new LineSelection(inkCanvas);
                break;
            case StrokeType.BRACKET:
                selection = new BracketSelection(inkCanvas, true);
                console.log("switching to bracket!")
                break;
            case StrokeType.MARQUEE:
                selection = new MarqueeSelection(inkCanvas, true);
                console.log("switching to marquee!")
                break;
            case StrokeType.SCRIBBLE:
                selection = new UnknownSelection(inkCanvas, true);
                console.log("switching to unknown!")
                break;
        }

        inkCanvas.redrawActiveStroke();

    }
   selection.update(e.clientX, e.clientY);
}

var selections = [];
canvas.addEventListener("mouseup", function(e) {

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
    $.each(segments, function(){
        var intersects = line.intersectsLine(this);
        if (intersects)
            intersectionCount++;
    });

    if (intersectionCount > 2) {

        var strokeBB = stroke.getBoundingRect();

        $.each(selections, function(){
            try {
                if (this.getBoundingRect().intersectsRectangle(strokeBB)) {
                    console.log(this);
                    this.deselect();
                    console.log("RECT INTERSECTION");

                    var selectionIndex = selections.indexOf(this);
                    if (selectionIndex > -1)
                        selections.splice(selectionIndex, 1);
                }
            } catch(e) {
                console.log(e)
                console.log(this);
            }
        });

        inkCanvas.update();
    }
    
    var currType = StrokeClassifier.GetStrokeType(inkCanvas._activeStroke.stroke);
    if (currType != StrokeType.SCRIBBLE) {
        selections.push(selection);
		port.postMessage(selection.getContent());
        //var myWindow = window.open("","Selected","width=1000, height=1000");
        //myWindow.focus();
        //myWindow.document.body.innerHTML="";
       // myWindow.document.write(selection.getContent());
    }
    else {
        inkCanvas.removeBrushStroke(inkCanvas._activeStroke);
        inkCanvas.update();
    }


    ///////////// UNCOMMENT TO SHOW WINDOW!!!!!!!!!!!!!!!!!!!!//////////////////////////////////////


    ////////////////////////////////////////////////////////////////////////////////////////////////

   // console.log(content);
    document.body.appendChild(canvas);

    selection = new LineSelection(inkCanvas);
    prevStrokeType = StrokeType.LINE;
});
