interface IBrush {
    
    init(x:number, y:number, inkCanvas:InkCanvas): void;
    draw(x: number, y: number, inkCanvas: InkCanvas);
    drawStroke(stroke: Stroke, inkCanvas: InkCanvas);

}