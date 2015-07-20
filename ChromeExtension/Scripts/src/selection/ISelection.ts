interface ISelection {
    
    start(x:number, y:number):void;
    update(x: number, y: number):void;
    end(x: number, y: number): void;
    deselect(): void;
    getBoundingRect(): Rectangle;
    analyzeContent():void;
    getContent():string;

}