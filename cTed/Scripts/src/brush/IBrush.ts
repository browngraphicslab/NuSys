/// <reference path="../InkCanvas.ts" />
interface IBrush {

    draw(x: number, y: number, inkCanvas: InkCanvas);

    redraw(stroke: Stroke, inkCanvas: InkCanvas);

    drawPrevious(stroke: Stroke, inkCanvas: InkCanvas);

}