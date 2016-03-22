class Rectangle {

    public x: number;
    public y: number;
    public w: number;
    public h: number;

    constructor(x: number, y: number, w: number, h: number) {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
    }

    getLines(): Array<Line> {
        var lines = [];
        lines.push(new Line(new Point(this.x, this.y), new Point(this.x + this.w, this.y)));
        lines.push(new Line(new Point(this.x, this.y), new Point(this.x, this.y + this.h)));
        lines.push(new Line(new Point(this.x + this.w, this.y + this.h), new Point(this.x + this.w, this.y)));
        lines.push(new Line(new Point(this.x + this.w, this.y + this.h), new Point(this.x , this.y + this.h)));

        return lines;
    }
    hasPoint(p: Point): boolean {
        return p.x >= this.x && p.x <= this.x + this.w && p.y >= this.y && p.y <= this.y + this.h;
    }
    intersectsRectangle(r2): boolean {
        return !(r2.x > this.x + this.w ||
            r2.x + r2.w < this.x ||
            r2.y > this.y + this.h ||
            r2.y + r2.h < this.y);
    }

}