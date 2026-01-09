import { ShaderProgram } from './ShaderProgram';
import { raytracingVertexShader, raytracingFragmentShader } from './RaytracingShaders';
import { Camera } from '../../Core/Camera';
import { Scene } from '../../Core/Scene';

export class RaytracingRenderer {
    private gl: WebGLRenderingContext | WebGL2RenderingContext;
    private shaderProgram: ShaderProgram;
    private quadVertexBuffer: WebGLBuffer | null = null;
    private time: number = 0;
    private triangleTexture: WebGLTexture | null = null;
    private triangleTexWidth: number = 512;
    private triangleTexHeight: number = 512;

    constructor(gl: WebGLRenderingContext | WebGL2RenderingContext) {
        this.gl = gl;
        this.shaderProgram = new ShaderProgram(gl, raytracingVertexShader, raytracingFragmentShader);
        this.setupQuad();
        this.setupGL();
        this.setupTriangleTexture();
    }

    private setupGL(): void {
        this.gl.clearColor(0.0, 0.0, 0.0, 1.0);
        this.gl.disable(this.gl.DEPTH_TEST);
        this.gl.disable(this.gl.CULL_FACE);
    }

    private setupTriangleTexture(): void {
        this.triangleTexture = this.gl.createTexture();
        if (!this.triangleTexture) {
            throw new Error('Failed to create triangle texture');
        }

        const isWebGL2 = this.gl instanceof WebGL2RenderingContext;

        if (!isWebGL2) {
            const floatTextureExt = this.gl.getExtension('OES_texture_float');
            if (!floatTextureExt) {
                throw new Error('Float textures not supported');
            }
        }

        this.gl.bindTexture(this.gl.TEXTURE_2D, this.triangleTexture);
        this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_MIN_FILTER, this.gl.NEAREST);
        this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_MAG_FILTER, this.gl.NEAREST);
        this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_WRAP_S, this.gl.CLAMP_TO_EDGE);
        this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_WRAP_T, this.gl.CLAMP_TO_EDGE);

        const emptyData = new Float32Array(this.triangleTexWidth * this.triangleTexHeight * 4);

        if (isWebGL2) {
            const gl2 = this.gl as WebGL2RenderingContext;
            gl2.texImage2D(
                gl2.TEXTURE_2D,
                0,
                gl2.RGBA32F,
                this.triangleTexWidth,
                this.triangleTexHeight,
                0,
                gl2.RGBA,
                gl2.FLOAT,
                emptyData
            );
        } else {
            this.gl.texImage2D(
                this.gl.TEXTURE_2D,
                0,
                this.gl.RGBA,
                this.triangleTexWidth,
                this.triangleTexHeight,
                0,
                this.gl.RGBA,
                this.gl.FLOAT,
                emptyData
            );
        }
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

        this.gl.activeTexture(this.gl.TEXTURE0);
        this.gl.bindTexture(this.gl.TEXTURE_2D, this.triangleTexture);

        const texLocation = this.shaderProgram.getUniformLocation('uTriangleData');
        if (texLocation) {
            this.gl.uniform1i(texLocation, 0);
        }

        const texSizeLocation = this.shaderProgram.getUniformLocation('uTriangleTexSize');
        if (texSizeLocation) {
            this.gl.uniform2f(texSizeLocation, this.triangleTexWidth, this.triangleTexHeight);
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

        const triangles: number[] = [];
        const maxTriangles = Math.floor((this.triangleTexWidth * this.triangleTexHeight) / 3);

        for (const mesh of meshes) {
            const vertices = mesh.vertices;
            const indices = mesh.indices;
            const color = mesh.color;

            for (let i = 0; i < indices.length; i += 3) {
                const i0 = indices[i] * 3;
                const i1 = indices[i + 1] * 3;
                const i2 = indices[i + 2] * 3;

                const v0x = vertices[i0] * mesh.scale.x + mesh.position.x;
                const v0y = vertices[i0 + 1] * mesh.scale.y + mesh.position.y;
                const v0z = vertices[i0 + 2] * mesh.scale.z + mesh.position.z;

                const v1x = vertices[i1] * mesh.scale.x + mesh.position.x;
                const v1y = vertices[i1 + 1] * mesh.scale.y + mesh.position.y;
                const v1z = vertices[i1 + 2] * mesh.scale.z + mesh.position.z;

                const v2x = vertices[i2] * mesh.scale.x + mesh.position.x;
                const v2y = vertices[i2 + 1] * mesh.scale.y + mesh.position.y;
                const v2z = vertices[i2 + 2] * mesh.scale.z + mesh.position.z;

                triangles.push(
                    v0x, v0y, v0z, color.r,
                    v1x, v1y, v1z, color.g,
                    v2x, v2y, v2z, color.b
                );

                if (triangles.length >= maxTriangles * 12) break;
            }

            if (triangles.length >= maxTriangles * 12) break;
        }

        const numTriangles = Math.min(Math.floor(triangles.length / 12), maxTriangles);
        this.shaderProgram.setUniform1i('uNumTriangles', numTriangles);

        const texData = new Float32Array(this.triangleTexWidth * this.triangleTexHeight * 4);
        for (let i = 0; i < triangles.length; i++) {
            texData[i] = triangles[i];
        }

        const isWebGL2 = this.gl instanceof WebGL2RenderingContext;

        this.gl.bindTexture(this.gl.TEXTURE_2D, this.triangleTexture);

        if (isWebGL2) {
            const gl2 = this.gl as WebGL2RenderingContext;
            gl2.texImage2D(
                gl2.TEXTURE_2D,
                0,
                gl2.RGBA32F,
                this.triangleTexWidth,
                this.triangleTexHeight,
                0,
                gl2.RGBA,
                gl2.FLOAT,
                texData
            );
        } else {
            this.gl.texImage2D(
                this.gl.TEXTURE_2D,
                0,
                this.gl.RGBA,
                this.triangleTexWidth,
                this.triangleTexHeight,
                0,
                this.gl.RGBA,
                this.gl.FLOAT,
                texData
            );
        }
    }

    dispose(): void {
        if (this.quadVertexBuffer) {
            this.gl.deleteBuffer(this.quadVertexBuffer);
        }
        if (this.triangleTexture) {
            this.gl.deleteTexture(this.triangleTexture);
        }
        this.shaderProgram.dispose();
    }
}

