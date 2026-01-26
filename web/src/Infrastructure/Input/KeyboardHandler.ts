export interface KeyboardState {
    forward: boolean;
    backward: boolean;
    left: boolean;
    right: boolean;
    up: boolean;
    down: boolean;
}

export class KeyboardHandler {
    private keys: KeyboardState = {
        forward: false,
        backward: false,
        left: false,
        right: false,
        up: false,
        down: false
    };

    constructor() {
        this.setupEventListeners();
    }

    private setupEventListeners(): void {
        window.addEventListener('keydown', this.handleKeyDown.bind(this));
        window.addEventListener('keyup', this.handleKeyUp.bind(this));
    }

    private handleKeyDown(event: KeyboardEvent): void {
        switch (event.code) {
            case 'KeyW':
                this.keys.forward = true;
                break;
            case 'KeyS':
                this.keys.backward = true;
                break;
            case 'KeyA':
                this.keys.left = true;
                break;
            case 'KeyD':
                this.keys.right = true;
                break;
            case 'Space':
                this.keys.up = true;
                break;
            case 'ControlLeft':
            case 'ControlRight':
                this.keys.down = true;
                break;
        }
    }

    private handleKeyUp(event: KeyboardEvent): void {
        switch (event.code) {
            case 'KeyW':
                this.keys.forward = false;
                break;
            case 'KeyS':
                this.keys.backward = false;
                break;
            case 'KeyA':
                this.keys.left = false;
                break;
            case 'KeyD':
                this.keys.right = false;
                break;
            case 'Space':
                this.keys.up = false;
                break;
            case 'ControlLeft':
            case 'ControlRight':
                this.keys.down = false;
                break;
        }
    }

    public getKeyboardState(): KeyboardState {
        return this.keys;
    }

    public dispose(): void {
        window.removeEventListener('keydown', this.handleKeyDown.bind(this));
        window.removeEventListener('keyup', this.handleKeyUp.bind(this));
    }
}

