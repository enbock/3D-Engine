import { MouseHandler } from '../../Infrastructure/Input/MouseHandler';
import { KeyboardHandler } from '../../Infrastructure/Input/KeyboardHandler';
import { Camera } from '../../Core/Camera';
import { Vector3 } from '../../Core/Math';

export class CameraController {
    private mouseHandler: MouseHandler;
    private keyboardHandler: KeyboardHandler;
    private cameraPosition: Vector3;
    private baseTarget: Vector3;
    private maxTiltAngle: number;
    private moveSpeed: number = 5.0;

    constructor(
        canvas: HTMLCanvasElement,
        private camera: Camera,
        startPosition: Vector3 = new Vector3(0, 2, 12),
        maxTiltAngleDegrees: number = 30
    ) {
        this.mouseHandler = new MouseHandler(canvas);
        this.keyboardHandler = new KeyboardHandler();
        this.cameraPosition = startPosition;
        this.baseTarget = Vector3.zero();
        this.maxTiltAngle = maxTiltAngleDegrees * Math.PI / 180;

        this.camera.setPosition(this.cameraPosition);
        this.camera.lookAt(this.baseTarget);
    }

    public update(deltaTime: number = 0.016): void {
        const keyState = this.keyboardHandler.getKeyboardState();

        const forward = this.camera.target.subtract(this.cameraPosition).normalize();
        const right = forward.cross(new Vector3(0, 1, 0)).normalize();
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

        const mouseState = this.mouseHandler.getMouseState();

        const tiltX = mouseState.normalizedY * this.maxTiltAngle;
        const tiltY = mouseState.normalizedX * this.maxTiltAngle;

        const adjustedTarget = new Vector3(
            this.cameraPosition.x + Math.sin(tiltY) * 5,
            this.cameraPosition.y + Math.sin(tiltX) * 5,
            this.cameraPosition.z - 5
        );

        this.camera.lookAt(adjustedTarget);
    }

    public setPosition(position: Vector3): void {
        this.cameraPosition = position;
        this.camera.setPosition(position);
    }

    public setTarget(target: Vector3): void {
        this.baseTarget = target;
    }

    public setMaxTiltAngle(degrees: number): void {
        this.maxTiltAngle = degrees * Math.PI / 180;
    }

    public setMoveSpeed(speed: number): void {
        this.moveSpeed = speed;
    }

    public dispose(): void {
        this.mouseHandler.dispose();
        this.keyboardHandler.dispose();
    }
}

