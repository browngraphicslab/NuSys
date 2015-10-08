class AbstractSelection implements ISelection {

    id: number;
    url: string;
    className: string;

    constructor(className) {
        this.className = className;
    }

    start(x: number, y: number): void {}

    update(x: number, y: number): void {}

    end(x: number, y: number): void {}

    deselect(): void {}

    getBoundingRect(): Rectangle { return null; }

    analyzeContent(): void { }

    getContent(): string { return null; }

}