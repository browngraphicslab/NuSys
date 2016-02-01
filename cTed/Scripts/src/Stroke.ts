class Stroke {

    public points: Array<any>;
    
    constructor() {
        console.log("new Stroke in Inkcanvas");
        this.points = new Array<any>();
    }

    push(x: number, y: number):void {
        this.points.push({ x:x, y: y });
    }

    getBoundingRect(): Rectangle {
        var minX = 1000000;
        var maxX = -1000000;
        var minY = 1000000;
        var maxY = -1000000;
        for (var i = 0; i < this.points.length; i++) {
            var p: any = this.points[i];
            maxY = p.y > maxY ? p.y : maxY;
            maxX = p.x > maxX ? p.x : maxX;
            minX = p.x < minX ? p.x : minX;
            minY = p.y < minY ? p.y : minY;
        }

        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    }
    
}