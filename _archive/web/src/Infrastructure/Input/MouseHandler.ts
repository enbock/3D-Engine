export interface MouseState {
    x: number;
    y: number;
    normalizedX: number;
    normalizedY: number;
    deltaX: number;
    deltaY: number;
}

export class MouseHandler {
    private mouseState: MouseState = {
        x: 0,
        y: 0,
        normalizedX: 0,
        normalizedY: 0,
        deltaX: 0,
        deltaY: 0
    };
    private isLocked: boolean = false;

    constructor(private canvas: HTMLCanvasElement) {
        this.setupEventListeners();
    }

    private setupEventListeners(): void {
        this.canvas.addEventListener('mousemove', this.handleMouseMove.bind(this));
        this.canvas.addEventListener('click', this.requestPointerLock.bind(this));
        document.addEventListener('pointerlockchange', this.handlePointerLockChange.bind(this));
    }

    private async requestPointerLock(): Promise<void> {
        try {
            await this.canvas.requestPointerLock();
        } catch (error) {
            if (error instanceof Error && error.name === 'SecurityError') {
                console.debug('Pointer lock request was cancelled');
            }
        }
    }

    private handlePointerLockChange(): void {
        this.isLocked = document.pointerLockElement === this.canvas;
    }

    private handleMouseMove(event: MouseEvent): void {
        if (this.isLocked) {
            this.mouseState.deltaX = event.movementX || 0;
            this.mouseState.deltaY = event.movementY || 0;
        } else {
            const rect = this.canvas.getBoundingClientRect();
            this.mouseState.x = event.clientX - rect.left;
            this.mouseState.y = event.clientY - rect.top;

            this.mouseState.normalizedX = (this.mouseState.x / rect.width) * 2 - 1;
            this.mouseState.normalizedY = -((this.mouseState.y / rect.height) * 2 - 1);

            this.mouseState.deltaX = 0;
            this.mouseState.deltaY = 0;
        }
    }

    public getMouseState(): MouseState {
        const state = { ...this.mouseState };
        this.mouseState.deltaX = 0;
        this.mouseState.deltaY = 0;
        return state;
    }

    public getIsLocked(): boolean {
        return this.isLocked;
    }

    public dispose(): void {
        this.canvas.removeEventListener('mousemove', this.handleMouseMove.bind(this));
        this.canvas.removeEventListener('click', this.requestPointerLock.bind(this));
        document.removeEventListener('pointerlockchange', this.handlePointerLockChange.bind(this));
    }
}

