import { ShaderProgram } from './ShaderProgram';
import { raytracingVertexShader, raytracingFragmentShader } from './RaytracingShaders';
import { Camera } from '../../Core/Camera';
import { Scene } from '../../Core/Scene';

export class RaytracingRenderer {
    private gl: WebGLRenderingContext | WebGL2RenderingContext;
    private shaderProgram: ShaderProgram;
    private quadVertexBuffer: WebGLBuffer | null = null;
    private time: number = 0;

    constructor(gl: WebGLRenderingContext | WebGL2RenderingContext) {
        this.gl = gl;
        this.shaderProgram = new ShaderProgram(gl, raytracingVertexShader, raytracingFragmentShader);
        this.setupQuad();
        this.setupGL();
    }

    private setupGL(): void {
        this.gl.clearColor(0.0, 0.0, 0.0, 1.0);
        this.gl.disable(this.gl.DEPTH_TEST);
        this.gl.disable(this.gl.CULL_FACE);
    }

    private setupQuad(): void {
        const vertices = new Float32Array([
            -1, -1,
             1, -1,
            -1,  1,
             1,  1
        ]);

        this.quadVertexBuffer = this.gl.createBuffer();
        if (!this.quadVertexBuffer) {
            throw new Error('Failed to create quad vertex buffer');
        }

        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.quadVertexBuffer);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, vertices, this.gl.STATIC_DRAW);
    }

    clear(): void {
        this.gl.clear(this.gl.COLOR_BUFFER_BIT);
    }

    render(camera: Camera, scene: Scene, deltaTime: number): void {
        this.time += deltaTime;
        this.shaderProgram.use();

        const width = this.gl.canvas.width;
        const height = this.gl.canvas.height;


        this.shaderProgram.setUniform3f('uCameraPosition',
            camera.position.x,
            camera.position.y,
            camera.position.z
        );

        this.shaderProgram.setUniform3f('uCameraTarget',
            camera.target.x,
            camera.target.y,
            camera.target.z
        );

        const resolutionLocation = this.shaderProgram.getUniformLocation('uResolution');
        if (resolutionLocation) {
            this.gl.uniform2f(resolutionLocation, width, height);
        }

        const timeLocation = this.shaderProgram.getUniformLocation('uTime');
        if (timeLocation) {
            this.gl.uniform1f(timeLocation, this.time);
        }

        this.uploadSceneData(scene);

        const positionLoc = this.shaderProgram.getAttributeLocation('aPosition');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.quadVertexBuffer);
        this.gl.enableVertexAttribArray(positionLoc);
        this.gl.vertexAttribPointer(positionLoc, 2, this.gl.FLOAT, false, 0, 0);

        this.gl.drawArrays(this.gl.TRIANGLE_STRIP, 0, 4);
    }

    private uploadSceneData(scene: Scene): void {
        const meshes = scene.getMeshes();
        const lights = scene.getLights();

        const numLights = Math.min(lights.length, 8);
        this.shaderProgram.setUniform1i('uNumLights', numLights);


        for (let i = 0; i < numLights; i++) {
            const light = lights[i];
            this.shaderProgram.setUniform1i(`uLights[${i}].type`, light.type);
            this.shaderProgram.setUniform3f(`uLights[${i}].position`, light.position.x, light.position.y, light.position.z);
            this.shaderProgram.setUniform3f(`uLights[${i}].direction`, light.direction.x, light.direction.y, light.direction.z);
            this.shaderProgram.setUniform3f(`uLights[${i}].color`, light.color.r, light.color.g, light.color.b);
            this.shaderProgram.setUniform1f(`uLights[${i}].intensity`, light.intensity);
        }

        const spheres: number[] = [];
        const boxes: number[] = [];
        const planes: number[] = [];

        for (const mesh of meshes) {
            const bounds = this.calculateBounds(mesh.vertices);
            const center = [
                (bounds.min[0] + bounds.max[0]) * 0.5,
                (bounds.min[1] + bounds.max[1]) * 0.5,
                (bounds.min[2] + bounds.max[2]) * 0.5
            ];
            const size = [
                (bounds.max[0] - bounds.min[0]) * 0.5,
                (bounds.max[1] - bounds.min[1]) * 0.5,
                (bounds.max[2] - bounds.min[2]) * 0.5
            ];

            const worldCenter = [
                center[0] * mesh.scale.x + mesh.position.x,
                center[1] * mesh.scale.y + mesh.position.y,
                center[2] * mesh.scale.z + mesh.position.z
            ];

            const worldSize = [
                size[0] * mesh.scale.x,
                size[1] * mesh.scale.y,
                size[2] * mesh.scale.z
            ];

            const isSphere = this.isSphereApproximation(mesh.vertices);
            const isPlane = worldSize[1] < 0.1;

            if (isSphere) {
                const radius = Math.max(worldSize[0], worldSize[1], worldSize[2]);
                spheres.push(
                    worldCenter[0], worldCenter[1], worldCenter[2], radius,
                    mesh.color.r, mesh.color.g, mesh.color.b, 0
                );
            } else if (isPlane) {
                planes.push(
                    worldCenter[0], worldCenter[1], worldCenter[2], 0,
                    0, 1, 0, 0,
                    mesh.color.r, mesh.color.g, mesh.color.b, 0
                );
            } else {
                boxes.push(
                    worldCenter[0] - worldSize[0], worldCenter[1] - worldSize[1], worldCenter[2] - worldSize[2], 0,
                    worldCenter[0] + worldSize[0], worldCenter[1] + worldSize[1], worldCenter[2] + worldSize[2], 0,
                    mesh.color.r, mesh.color.g, mesh.color.b, 0
                );
            }
        }

        const numSpheres = Math.min(Math.floor(spheres.length / 8), 32);
        const numBoxes = Math.min(Math.floor(boxes.length / 12), 32);
        const numPlanes = Math.min(Math.floor(planes.length / 12), 8);


        this.shaderProgram.setUniform1i('uNumSpheres', numSpheres);
        this.shaderProgram.setUniform1i('uNumBoxes', numBoxes);
        this.shaderProgram.setUniform1i('uNumPlanes', numPlanes);


        const sphereData = new Float32Array(spheres.slice(0, 32 * 8));
        const boxData = new Float32Array(boxes.slice(0, 32 * 12));
        const planeData = new Float32Array(planes.slice(0, 8 * 12));

        const sphereLocation = this.shaderProgram.getUniformLocation('uSpheres');
        if (sphereLocation && sphereData.length > 0) {
            this.gl.uniform4fv(sphereLocation, sphereData);
        }

        const boxLocation = this.shaderProgram.getUniformLocation('uBoxes');
        if (boxLocation && boxData.length > 0) {
            this.gl.uniform4fv(boxLocation, boxData);
        }

        const planeLocation = this.shaderProgram.getUniformLocation('uPlanes');
        if (planeLocation && planeData.length > 0) {
            this.gl.uniform4fv(planeLocation, planeData);
        }
    }

    private calculateBounds(vertices: Float32Array): { min: number[], max: number[] } {
        const min = [Infinity, Infinity, Infinity];
        const max = [-Infinity, -Infinity, -Infinity];

        for (let i = 0; i < vertices.length; i += 3) {
            min[0] = Math.min(min[0], vertices[i]);
            min[1] = Math.min(min[1], vertices[i + 1]);
            min[2] = Math.min(min[2], vertices[i + 2]);
            max[0] = Math.max(max[0], vertices[i]);
            max[1] = Math.max(max[1], vertices[i + 1]);
            max[2] = Math.max(max[2], vertices[i + 2]);
        }

        return { min, max };
    }

    private isSphereApproximation(vertices: Float32Array): boolean {
        if (vertices.length < 100) return false;

        const bounds = this.calculateBounds(vertices);
        const sizeX = bounds.max[0] - bounds.min[0];
        const sizeY = bounds.max[1] - bounds.min[1];
        const sizeZ = bounds.max[2] - bounds.min[2];

        const avgSize = (sizeX + sizeY + sizeZ) / 3;
        const variance = Math.abs(sizeX - avgSize) + Math.abs(sizeY - avgSize) + Math.abs(sizeZ - avgSize);

        return variance / avgSize < 0.2;
    }

    dispose(): void {
        if (this.quadVertexBuffer) {
            this.gl.deleteBuffer(this.quadVertexBuffer);
        }
        this.shaderProgram.dispose();
    }
}

