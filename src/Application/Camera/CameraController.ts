import { MouseHandler } from '../../Infrastructure/Input/MouseHandler';
import { Camera } from '../../Core/Camera';
import { Vector3 } from '../../Core/Math';

export class CameraController {
    private mouseHandler: MouseHandler;
    private basePosition: Vector3;
    private baseTarget: Vector3;
    private maxTiltAngle: number;

    constructor(
        canvas: HTMLCanvasElement,
        private camera: Camera,
        basePosition: Vector3 = new Vector3(0, 2, 12),
        maxTiltAngleDegrees: number = 30
    ) {
        this.mouseHandler = new MouseHandler(canvas);
        this.basePosition = basePosition;
        this.baseTarget = Vector3.zero();
        this.maxTiltAngle = maxTiltAngleDegrees * Math.PI / 180;

        this.camera.setPosition(this.basePosition);
        this.camera.lookAt(this.baseTarget);
    }

    public update(): void {
        const mouseState = this.mouseHandler.getMouseState();

        const tiltX = mouseState.normalizedY * this.maxTiltAngle;
        const tiltY = mouseState.normalizedX * this.maxTiltAngle;

        const adjustedTarget = new Vector3(
            this.baseTarget.x + Math.sin(tiltY) * 5,
            this.baseTarget.y + Math.sin(tiltX) * 5,
            this.baseTarget.z
        );

        this.camera.lookAt(adjustedTarget);
    }

    public setBasePosition(position: Vector3): void {
        this.basePosition = position;
        this.camera.setPosition(position);
    }

    public setBaseTarget(target: Vector3): void {
        this.baseTarget = target;
    }

    public setMaxTiltAngle(degrees: number): void {
        this.maxTiltAngle = degrees * Math.PI / 180;
    }

    public dispose(): void {
        this.mouseHandler.dispose();
    }
}

