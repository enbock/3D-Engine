import {MouseHandler} from '../../Infrastructure/Input/MouseHandler';
import {KeyboardHandler} from '../../Infrastructure/Input/KeyboardHandler';
import {Camera} from '../../Core/Camera';
import {Vector3} from '../../Core/Math';

export class CameraController {
    private mouseHandler: MouseHandler;
    private keyboardHandler: KeyboardHandler;
    private cameraPosition: Vector3;
    private moveSpeed: number = 5.0;
    private mouseSensitivity: number = 0.002;

    private yaw: number = 0;
    private pitch: number = 0;
    private maxPitch: number = Math.PI / 2 - 0.01;

    constructor(
        canvas: HTMLCanvasElement,
        private camera: Camera,
        startPosition: Vector3 = new Vector3(0, 2, 12),
        startYaw: number = 0
    ) {
        this.mouseHandler = new MouseHandler(canvas);
        this.keyboardHandler = new KeyboardHandler();
        this.cameraPosition = startPosition;
        this.yaw = startYaw;
        this.pitch = 0;

        this.camera.setPosition(this.cameraPosition);
        this.updateCameraTarget();
    }

    public update(deltaTime: number = 0.016): void {
        const mouseState = this.mouseHandler.getMouseState();

        if (this.mouseHandler.getIsLocked()) {
            this.yaw -= mouseState.deltaX * this.mouseSensitivity;
            this.pitch -= mouseState.deltaY * this.mouseSensitivity;

            this.pitch = Math.max(-this.maxPitch, Math.min(this.maxPitch, this.pitch));

            this.updateCameraTarget();
        }

        const keyState = this.keyboardHandler.getKeyboardState();

        const forward = this.getForwardVector();
        const right = this.getRightVector();
        const up = new Vector3(0, 1, 0);

        const speed = this.moveSpeed * deltaTime;

        if (keyState.forward) {
            this.cameraPosition = this.cameraPosition.add(forward.multiply(speed));
        }
        if (keyState.backward) {
            this.cameraPosition = this.cameraPosition.add(forward.multiply(-speed));
        }
        if (keyState.left) {
            this.cameraPosition = this.cameraPosition.add(right.multiply(-speed));
        }
        if (keyState.right) {
            this.cameraPosition = this.cameraPosition.add(right.multiply(speed));
        }
        if (keyState.up) {
            this.cameraPosition = this.cameraPosition.add(up.multiply(speed));
        }
        if (keyState.down) {
            this.cameraPosition = this.cameraPosition.add(up.multiply(-speed));
        }

        this.camera.setPosition(this.cameraPosition);
        this.updateCameraTarget();
    }

    private getForwardVector(): Vector3 {
        return new Vector3(
            Math.cos(this.pitch) * Math.sin(this.yaw),
            Math.sin(this.pitch),
            Math.cos(this.pitch) * Math.cos(this.yaw)
        );
    }

    private getRightVector(): Vector3 {
        return new Vector3(
            -Math.cos(this.yaw),
            0,
            Math.sin(this.yaw)
        );
    }

    private updateCameraTarget(): void {
        const forward = this.getForwardVector();
        const target = this.cameraPosition.add(forward);
        this.camera.lookAt(target);
    }

    public setPosition(position: Vector3): void {
        this.cameraPosition = position;
        this.camera.setPosition(position);
        this.updateCameraTarget();
    }

    public setRotation(yaw: number, pitch: number): void {
        this.yaw = yaw;
        this.pitch = Math.max(-this.maxPitch, Math.min(this.maxPitch, pitch));
        this.updateCameraTarget();
    }

    public setMoveSpeed(speed: number): void {
        this.moveSpeed = speed;
    }

    public setMouseSensitivity(sensitivity: number): void {
        this.mouseSensitivity = sensitivity;
    }

    public dispose(): void {
        this.mouseHandler.dispose();
        this.keyboardHandler.dispose();
    }
}

