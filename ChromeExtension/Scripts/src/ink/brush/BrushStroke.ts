class BrushStroke {
    
    public brush:IBrush;
    public stroke: Stroke;

    constructor(brush: IBrush, stroke: Stroke) {
        this.brush = brush;
        this.stroke = stroke;
    }
}