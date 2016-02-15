interface IBrush {

    draw(x: number, y: number, inkCanvas: InkCanvas);

    redraw(stroke: Stroke, inkCanvas: InkCanvas);
}