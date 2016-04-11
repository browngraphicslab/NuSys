class AbstractSelection implements Selectable{

    id: number;
    url: string;
    tags: string;
    stroke: Stroke;
    type: StrokeType;
    yscroll: number;
    samplePoints: Array<Point>;

    public selectedElements: Array<any> = new Array<any>();

    start(x: number, y: number): void { }

    end(x: number, y: number): void { }

    analyzeContent(): void { }

    getContent(): string {
        return null;
    }



    







}