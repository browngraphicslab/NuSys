/// <reference path="Stroke.ts"/>
/// <reference path="StrokeType.ts"/>


class InkCanvas {

    _canvas: HTMLCanvasElement;
    _brush: IBrush;
    _context: CanvasRenderingContext2D;
    _activeStroke: Stroke; 
    _prevBrush: IBrush; 
    _scroll: number;
    constructor(canvas: HTMLCanvasElement) {
        this._canvas = canvas;
        this._context = canvas.getContext("2d");
        this._brush = new HighlightBrush();
        this._activeStroke = new Stroke();
        console.log("new Canvas!!!!!");
    }

    drawStroke(stroke: Stroke) {
        var sample = stroke.sampleStroke();
        for (var i = 0; i < sample.points.length; i++) {
            this._brush.draw(sample[i].x, sample[i].y, this);
        }
    }

    draw(x: number, y: number) {
        this._activeStroke.push(x, y);
        this._brush.draw(x, y, this);
    }

    removeStroke(): void {
        this._context.clearRect(0, 0, this._canvas.width, this._canvas.height);
     //   this.update();
    }

    switchBrush(strokeType) {
        console.log("INKCANVAS brush switched to : " + strokeType);
        switch (strokeType) {
            //////STROKE TYPE CHANGE 
            case StrokeType.Marquee:
                this._brush = new MarqueeBrush();
                break;
            case StrokeType.Line:
                this._brush = new HighlightBrush();
                break;
            case StrokeType.Lasso:
                this._brush = new LassoBrush();
                break;
            default:
                this._brush = new HighlightBrush();
        }
        this._brush.redraw(this._activeStroke, this);
    }

    drawPreviousGestureM(stroke: Stroke) {
        console.log("======================redraw==");
        this._prevBrush = new MarqueeBrush();
        this._prevBrush.drawPrevious(stroke, this);
    }

    drawPointsAndLines(sel: AbstractSelection) {
        var points = sel.samplePoints;
        this._scroll = sel.yscroll;
        this.clear();
        this.drawPoint(points[0]);
        this.drawline(points[0], points[points.length - 1]);

        for (var i = 1; i < points.length; i++) {
            this.drawPoint(points[i]);
            this.drawline(points[i], points[i - 1]);
        }
    }

    drawPreviousGesture(sel: AbstractSelection) {
            this.drawPointsAndLines(sel);

    }

    drawPreviousGestureL(points: Array<Point>) {
        console.log("======================redraw==");
        this._prevBrush = new LassoBrush();
        var stroke = new Stroke();
        stroke.points = points;
        this._prevBrush.drawPrevious(stroke, this);
    }

    drawPoint(p: Point) {
        var ctx = this._context;

        ctx.globalCompositeOperation = "source-over";

        ctx.fillStyle = '#ff0000';
        ctx.beginPath();
        ctx.moveTo(p.x, p.y - $(document).scrollTop() + this._scroll);
        ctx.arc(p.x, p.y - $(document).scrollTop() +this._scroll, 5, 0, Math.PI * 2, false);
        ctx.fill();
    }

    drawline(p1, p2: Point) {
        var ctx = this._context;
        ctx.lineWidth = 1;
        ctx.strokeStyle = '#123456';
        ctx.setLineDash([]);
        ctx.beginPath();

        ctx.moveTo(p1.x, p1.y - $(document).scrollTop() + this._scroll);
        ctx.lineTo(p2.x, p2.y - $(document).scrollTop() + this._scroll);
        ctx.stroke();
    }
    
    editPoint(points: Array<Point>, e): Point {
        var sampleStroke = points;
        var lines = [];
        for (var i = 1; i < sampleStroke.length; i++) {
  //          console.log(e.clientY + ": " + sampleStroke[i].y + " : " + $(document).scrollTop() + " : " + this._scroll);
            if (Math.abs(e.clientX - sampleStroke[i].x) < 5  && Math.abs(e.clientY - sampleStroke[i].y + $(document).scrollTop() - this._scroll) < 5) {
                this.focusPoint(sampleStroke[i]);
                return sampleStroke[i];
            } 
        }
    }

    editStrokes(points: Array<Point>, e): Line {
        var sampleStroke = points;
        var lines = [];
        for (var i = 0; i < sampleStroke.length; i++) {
            if (i == 0) {
                var line = new Line(sampleStroke[sampleStroke.length - 1], sampleStroke[0]);
            } else {
                var line = new Line(sampleStroke[i - 1], sampleStroke[i])
            }
            if (this.checkAboveLine(line, new Point(e.clientX, e.clientY + $(document).scrollTop()))) {
                this.focusLine(line);
                return line;
            }
        }
    }

    focusLine(line: Line) {
        var ctx = this._context;
        ctx.beginPath();
      //  ctx.fillStyle = '#ff0000';
        ctx.moveTo(line.p1.x, line.p1.y - $(document).scrollTop() + this._scroll);
        ctx.lineTo(line.p2.x, line.p2.y - $(document).scrollTop() + this._scroll);
        ctx.lineWidth = 3;
        ctx.stroke();
    }

    focusPoint(p: Point) {
        var ctx = this._context;
        ctx.globalCompositeOperation = "source-over";
        ctx.fillStyle = '#0000FF';
        ctx.beginPath();
        ctx.moveTo(p.x, p.y - $(document).scrollTop() + this._scroll);
        ctx.arc(p.x, p.y - $(document).scrollTop() + this._scroll, 8, 0, Math.PI * 2, false);
        ctx.fill();
    }


    isBetween(a, b, x: number): boolean {
        if (a > b) {
            return x < a && x > b;
        } else {
            return x < b && x > a;
        }
    }

    checkAboveLine(line: Line, mouse: Point): boolean {
        if (line.p1.x == line.p2.x) {
            return (Math.abs(mouse.x - line.p1.x) < 5 && this.isBetween(line.p1.y + this._scroll, line.p2.y + this._scroll, mouse.y));
        }
        if (line.p1.y == line.p2.y) {
            return (Math.abs(mouse.y - line.p1.y - this._scroll) < 5 && this.isBetween(line.p1.x, line.p2.x, mouse.x));
        }
        var m1 = (mouse.y - line.p1.y - this._scroll) / (mouse.x - line.p1.x);
        var m2 = (line.p2.y + this._scroll - mouse.y) / (line.p2.x - mouse.x);
        return (Math.abs(m1 - m2) < 0.3) && (this.isBetween(line.p1.y + this._scroll, line.p2.y + this._scroll, mouse.y)) && this.isBetween(line.p1.x, line.p2.x, mouse.x);
    }

    clear(): void {
        this._context.clearRect(0, 0, this._canvas.width, this._canvas.height);
        this._activeStroke = new Stroke();
        this._brush = new HighlightBrush();
    }
}