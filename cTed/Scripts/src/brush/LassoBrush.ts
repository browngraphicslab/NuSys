
class LassoBrush implements IBrush {

    _img: HTMLImageElement;
    _startX: number;
    _startY: number;

    constructor() {

    }

    draw(x: number, y: number, inkCanvas: InkCanvas) {
        var canvas = inkCanvas._canvas;
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";


        ctx.fillStyle = '#ff0000';
        ctx.beginPath();
        ctx.moveTo(x, y);
        ctx.arc(x, y, 5, 0, Math.PI * 2, false);
        ctx.fill();
    }

    redraw(stroke: Stroke, inkCanvas: InkCanvas) {
        inkCanvas.removeStroke();
        for (var i = 0; i < stroke.points.length; i++) {
            this.draw(stroke.points[i].x, stroke.points[i].y, inkCanvas);
        }
    }

    focusLine(line: Line, inkCanvas: InkCanvas) {
        var c = inkCanvas._canvas;
        var ctx = c.getContext("2d");
        ctx.beginPath();
        ctx.fillStyle = '#ff0000';
        ctx.moveTo(line.p1.x, line.p1.y);
        ctx.lineTo(line.p2.x, line.p2.y);
        ctx.lineWidth = 3;
        ctx.stroke();
        ctx.lineWidth = 1;

    }

    focusPoint(p: Point, inkCanvas: InkCanvas) {
        var canvas = inkCanvas._canvas;
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";


        ctx.fillStyle = '#0000FF';
        ctx.beginPath();
        ctx.moveTo(p.x, p.y);
        ctx.arc(p.x, p.y, 8, 0, Math.PI * 2, false);
        ctx.fill();
    }
    //draw previous on hover
      
    drawline(p1, p2: Point, inkCanvas: InkCanvas) {
      //  console.log("drawline....");
        
               
        var c = inkCanvas._canvas;
        var ctx = inkCanvas._context;
        ctx.lineWidth = 30;
        ctx.fillStyle = '#000000';
        ctx.beginPath();
                   
        ctx.moveTo(p1.x, p1.y);
        ctx.lineTo(p2.x, p2.y);
        ctx.stroke(); 
    }

    drawPrevious(stroke: Stroke, inkCanvas: InkCanvas) {
        inkCanvas.clear();
     //   console.log("======DRAWPREVLASSO!!");
        stroke.points.forEach((p, i) => {
            this.draw(p.x, p.y, inkCanvas);
            if (i == 0) {
                this.drawline(p, stroke.points[stroke.points.length - 1], inkCanvas);
            } else {
                this.drawline(p, stroke.points[i - 1], inkCanvas);
            }
        });
        //var firstPoint = stroke.points[0];
        //var lastPoint = stroke.points[stroke.points.length - 1];

        //var canvas = inkCanvas._canvas;
        //var ctx = inkCanvas._context;
        //ctx.globalCompositeOperation = "source-over";

        //ctx.beginPath();
        //ctx.lineWidth = 3;
        //ctx.strokeStyle = "rgb(255,70,70)";
        //ctx.setLineDash([5]);
        //ctx.rect(firstPoint.x, firstPoint.y - $(window).scrollTop(), lastPoint.x - firstPoint.x, lastPoint.y - firstPoint.y);
        //ctx.stroke();
    }

}
