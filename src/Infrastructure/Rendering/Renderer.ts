import { Mesh } from '../../Core/Geometry';
import { Camera } from '../../Core/Camera';
import { Matrix4 } from '../../Core/Math';
import { ShaderProgram } from './ShaderProgram';
import { vertexShaderSource, fragmentShaderSource } from './Shaders';

export class Renderer {
    private gl: WebGLRenderingContext | WebGL2RenderingContext;
    private shaderProgram: ShaderProgram;
    private meshBuffers: Map<Mesh, {
        vertexBuffer: WebGLBuffer;
        indexBuffer: WebGLBuffer;
        normalBuffer: WebGLBuffer;
        colorBuffer?: WebGLBuffer;
    }> = new Map();

    constructor(gl: WebGLRenderingContext | WebGL2RenderingContext) {
        this.gl = gl;
        this.shaderProgram = new ShaderProgram(gl, vertexShaderSource, fragmentShaderSource);
        this.setupGL();
    }

    private setupGL(): void {
        this.gl.enable(this.gl.DEPTH_TEST);
        this.gl.depthFunc(this.gl.LEQUAL);
        this.gl.enable(this.gl.CULL_FACE);
        this.gl.cullFace(this.gl.BACK);
        this.gl.clearColor(0.1, 0.1, 0.15, 1.0);
    }

    private getOrCreateBuffers(mesh: Mesh) {
        let buffers = this.meshBuffers.get(mesh);

        if (!buffers) {
            const vertexBuffer = this.gl.createBuffer();
            const indexBuffer = this.gl.createBuffer();
            const normalBuffer = this.gl.createBuffer();
            const colorBuffer = mesh.colors ? this.gl.createBuffer() : undefined;

            if (!vertexBuffer || !indexBuffer || !normalBuffer) {
                throw new Error('Failed to create buffers');
            }

            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, vertexBuffer);
            this.gl.bufferData(this.gl.ARRAY_BUFFER, mesh.vertices, this.gl.STATIC_DRAW);

            this.gl.bindBuffer(this.gl.ELEMENT_ARRAY_BUFFER, indexBuffer);
            this.gl.bufferData(this.gl.ELEMENT_ARRAY_BUFFER, mesh.indices, this.gl.STATIC_DRAW);

            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, normalBuffer);
            this.gl.bufferData(this.gl.ARRAY_BUFFER, mesh.normals, this.gl.STATIC_DRAW);

            if (colorBuffer && mesh.colors) {
                this.gl.bindBuffer(this.gl.ARRAY_BUFFER, colorBuffer);
                this.gl.bufferData(this.gl.ARRAY_BUFFER, mesh.colors, this.gl.STATIC_DRAW);
            }

            buffers = { vertexBuffer, indexBuffer, normalBuffer, colorBuffer };
            this.meshBuffers.set(mesh, buffers);
        }

        return buffers;
    }

    clear(): void {
        this.gl.clear(this.gl.COLOR_BUFFER_BIT | this.gl.DEPTH_BUFFER_BIT);
    }

    render(mesh: Mesh, camera: Camera): void {
        this.shaderProgram.use();

        const buffers = this.getOrCreateBuffers(mesh);

        const modelMatrix = new Matrix4();
        modelMatrix
            .translate(mesh.position.x, mesh.position.y, mesh.position.z)
            .rotateX(mesh.rotation.x)
            .rotateY(mesh.rotation.y)
            .rotateZ(mesh.rotation.z)
            .scale(mesh.scale.x, mesh.scale.y, mesh.scale.z);

        this.shaderProgram.setUniformMatrix4('uModelMatrix', modelMatrix.elements);
        this.shaderProgram.setUniformMatrix4('uViewMatrix', camera.getViewMatrix().elements);
        this.shaderProgram.setUniformMatrix4('uProjectionMatrix', camera.getProjectionMatrix().elements);

        this.shaderProgram.setUniform4f('uColor', mesh.color.r, mesh.color.g, mesh.color.b, mesh.color.a);
        this.shaderProgram.setUniform3f('uLightDirection', 0.5, 0.7, 1.0);
        this.shaderProgram.setUniform3f('uAmbientLight', 0.3, 0.3, 0.3);

        const positionLoc = this.shaderProgram.getAttributeLocation('aPosition');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, buffers.vertexBuffer);
        this.gl.enableVertexAttribArray(positionLoc);
        this.gl.vertexAttribPointer(positionLoc, 3, this.gl.FLOAT, false, 0, 0);

        const normalLoc = this.shaderProgram.getAttributeLocation('aNormal');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, buffers.normalBuffer);
        this.gl.enableVertexAttribArray(normalLoc);
        this.gl.vertexAttribPointer(normalLoc, 3, this.gl.FLOAT, false, 0, 0);

        const colorLoc = this.shaderProgram.getAttributeLocation('aColor');
        if (buffers.colorBuffer && mesh.colors) {
            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, buffers.colorBuffer);
            this.gl.enableVertexAttribArray(colorLoc);
            this.gl.vertexAttribPointer(colorLoc, 4, this.gl.FLOAT, false, 0, 0);
        } else {
            this.gl.disableVertexAttribArray(colorLoc);
            this.gl.vertexAttrib4f(colorLoc, 0, 0, 0, 0);
        }

        this.gl.bindBuffer(this.gl.ELEMENT_ARRAY_BUFFER, buffers.indexBuffer);
        this.gl.drawElements(this.gl.TRIANGLES, mesh.indices.length, this.gl.UNSIGNED_SHORT, 0);
    }

    dispose(): void {
        this.meshBuffers.forEach(buffers => {
            this.gl.deleteBuffer(buffers.vertexBuffer);
            this.gl.deleteBuffer(buffers.indexBuffer);
            this.gl.deleteBuffer(buffers.normalBuffer);
            if (buffers.colorBuffer) {
                this.gl.deleteBuffer(buffers.colorBuffer);
            }
        });
        this.meshBuffers.clear();
        this.shaderProgram.dispose();
    }
}

