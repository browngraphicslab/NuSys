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

    intersectsRectangle(r2): boolean {
        return !(r2.x > this.x + this.w ||
            r2.x + r2.w < this.x ||
            r2.y > this.y + this.h ||
            r2.y + r2.h < this.y);
    }

}