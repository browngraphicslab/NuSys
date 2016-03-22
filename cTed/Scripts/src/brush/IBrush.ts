/// <reference path="../InkCanvas.ts" />
interface IBrush {

    draw(x: number, y: number, inkCanvas: InkCanvas);

    redraw(stroke: Stroke, inkCanvas: InkCanvas);

    drawPrevious(stroke: Stroke, inkCanvas: InkCanvas);

    focusLine(line: Line, inkCanvas: InkCanvas);

    focusPoint(point: Point, inkCanvas: InkCanvas);
}