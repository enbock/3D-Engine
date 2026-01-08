export interface MouseState {
    x: number;
    y: number;
    normalizedX: number;
    normalizedY: number;
}

export class MouseHandler {
    private mouseState: MouseState = {
        x: 0,
        y: 0,
        normalizedX: 0,
        normalizedY: 0
    };

    constructor(private canvas: HTMLCanvasElement) {
        this.setupEventListeners();
    }

    private setupEventListeners(): void {
        this.canvas.addEventListener('mousemove', this.handleMouseMove.bind(this));
    }

    private handleMouseMove(event: MouseEvent): void {
        const rect = this.canvas.getBoundingClientRect();
        this.mouseState.x = event.clientX - rect.left;
        this.mouseState.y = event.clientY - rect.top;

        this.mouseState.normalizedX = (this.mouseState.x / rect.width) * 2 - 1;
        this.mouseState.normalizedY = -((this.mouseState.y / rect.height) * 2 - 1);
    }

    public getMouseState(): MouseState {
        return this.mouseState;
    }

    public dispose(): void {
        this.canvas.removeEventListener('mousemove', this.handleMouseMove.bind(this));
    }
}

