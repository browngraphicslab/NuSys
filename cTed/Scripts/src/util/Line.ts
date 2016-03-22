class Line {
    p1: Point;
    p2: Point;
    A: number;
    B: number;
    C: number;

    //line in the form of  --- Ax + By = C 
    constructor(p1, p2: Point) {
        this.p1 = p1;
        this.p2 = p2;
        this.A = p2.y - p1.y;
        this.B = p1.x - p2.x;
        this.C = p2.y * p1.x - p2.x * p1.y;
    }

    hasPoint(p: Point) {
        return (Math.min(this.p1.x, this.p2.x) <= p.x) && (Math.max(this.p1.x, this.p2.x) >= p.x)
            && (Math.min(this.p1.y, this.p2.y) <= p.y) && (Math.max(this.p1.y, this.p2.y) >= p.y);
    }
}