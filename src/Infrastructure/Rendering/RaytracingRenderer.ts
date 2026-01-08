import { ShaderProgram } from './ShaderProgram';
import { raytracingVertexShader, raytracingFragmentShader } from './RaytracingShaders';
import { Camera } from '../../Core/Camera';

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

    render(camera: Camera, deltaTime: number): void {
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

        const positionLoc = this.shaderProgram.getAttributeLocation('aPosition');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.quadVertexBuffer);
        this.gl.enableVertexAttribArray(positionLoc);
        this.gl.vertexAttribPointer(positionLoc, 2, this.gl.FLOAT, false, 0, 0);

        this.gl.drawArrays(this.gl.TRIANGLE_STRIP, 0, 4);
    }

    dispose(): void {
        if (this.quadVertexBuffer) {
            this.gl.deleteBuffer(this.quadVertexBuffer);
        }
        this.shaderProgram.dispose();
    }
}

