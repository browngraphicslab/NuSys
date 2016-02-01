interface Selectable {
    start(x: number, y: number): void;
    end(x: number, y: number): void;
    getContent(): string;
}