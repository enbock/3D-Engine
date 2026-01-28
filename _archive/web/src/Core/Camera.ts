import { Vector3 } from './Math';
import { Matrix4 } from './Math';

export class Camera {
    public position: Vector3;
    public target: Vector3;
    public up: Vector3;
    public fov: number;
    public aspect: number;
    public near: number;
    public far: number;

    private viewMatrix: Matrix4;
    private projectionMatrix: Matrix4;

    constructor(
        position: Vector3 = new Vector3(0, 0, 5),
        target: Vector3 = Vector3.zero(),
        fov: number = 45 * Math.PI / 180,
        aspect: number = 1,
        near: number = 0.1,
        far: number = 100
    ) {
        this.position = position;
        this.target = target;
        this.up = new Vector3(0, 1, 0);
        this.fov = fov;
        this.aspect = aspect;
        this.near = near;
        this.far = far;

        this.viewMatrix = new Matrix4();
        this.projectionMatrix = new Matrix4();
        this.updateMatrices();
    }

    updateMatrices(): void {
        this.projectionMatrix.perspective(this.fov, this.aspect, this.near, this.far);

        this.viewMatrix.lookAt(
            new Float32Array([this.position.x, this.position.y, this.position.z]),
            new Float32Array([this.target.x, this.target.y, this.target.z]),
            new Float32Array([this.up.x, this.up.y, this.up.z])
        );
    }

    getViewMatrix(): Matrix4 {
        return this.viewMatrix;
    }

    getProjectionMatrix(): Matrix4 {
        return this.projectionMatrix;
    }

    setAspect(aspect: number): void {
        this.aspect = aspect;
        this.updateMatrices();
    }

    lookAt(target: Vector3): void {
        this.target = target;
        this.updateMatrices();
    }

    setPosition(position: Vector3): void {
        this.position = position;
        this.updateMatrices();
    }

    orbit(deltaX: number, deltaY: number, distance: number): void {
        const theta = deltaX * 0.01;
        const phi = deltaY * 0.01;

        const x = distance * Math.sin(phi) * Math.cos(theta);
        const y = distance * Math.cos(phi);
        const z = distance * Math.sin(phi) * Math.sin(theta);

        this.position = new Vector3(x, y, z);
        this.updateMatrices();
    }
}

