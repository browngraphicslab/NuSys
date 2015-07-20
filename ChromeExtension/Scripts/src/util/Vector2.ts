class Vector2 {

    x: number;
    y: number;

    constructor(x: number, y: number) {
        this.x = x;
        this.y = y;
    }

    static fromPoint(p:any) {
        return new Vector2(p.x, p.y);
    }

    negative():Vector2 {
        return new Vector2(-this.x, -this.y);
    }

    add(v):Vector2 {
        if (v instanceof Vector2) return new Vector2(this.x + v.x, this.y + v.y);
        else return new Vector2(this.x + v, this.y + v);
    }

    subtract(v):Vector2 {
        if (v instanceof Vector2) return new Vector2(this.x - v.x, this.y - v.y);
        else return new Vector2(this.x - v, this.y - v);
    }

    multiply(v):Vector2 {
        if (v instanceof Vector2) return new Vector2(this.x * v.x, this.y * v.y);
        else return new Vector2(this.x * v, this.y * v);
    }

    divide(v):Vector2 {
        if (v instanceof Vector2) return new Vector2(this.x / v.x, this.y / v.y);
        else return new Vector2(this.x / v, this.y / v);
    }

    equals(v):boolean {
        return this.x == v.x && this.y == v.y;
    }

    dot(v):number {
        return this.x * v.x + this.y * v.y;
    }

    length():number {
        return Math.sqrt(this.dot(this));
    }

    getNormalized():Vector2 {
        return this.divide(this.length());
    }

    distanceTo(other):number {
        return Math.sqrt((this.x - other.x) * (this.x - other.x) + (this.y - other.y) * (this.y - other.y));
    }

    cross(other):number {
        return this.x * other.y - this.y * other.x;
    }

    clone():Vector2 {
        return new Vector2(this.x, this.y);
    }

    angleTo(a):number {
        return Math.acos(this.dot(a) / (this.length() * a.length()));
    }

    init(x, y):Vector2 {
        this.x = x;
        this.y = y;
        return this;
    }
}