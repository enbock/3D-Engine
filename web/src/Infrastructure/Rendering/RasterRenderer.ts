import { ShaderProgram } from './ShaderProgram';
import { Camera } from '../../Core/Camera';
import { Scene } from '../../Core/Scene';

const vertexShader = `
attribute vec3 aPosition;
attribute vec3 aNormal;
attribute vec3 aColor;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

varying vec3 vNormal;
varying vec3 vColor;
varying vec3 vPosition;

void main() {
    vec4 worldPosition = uModelMatrix * vec4(aPosition, 1.0);
    vPosition = worldPosition.xyz;
    vNormal = mat3(uModelMatrix) * aNormal;
    vColor = aColor;
    gl_Position = uProjectionMatrix * uViewMatrix * worldPosition;
}
`;

const fragmentShader = `
precision mediump float;

varying vec3 vNormal;
varying vec3 vColor;
varying vec3 vPosition;

uniform vec3 uCameraPosition;
uniform int uNumLights;
uniform vec3 uLightPositions[8];
uniform vec3 uLightDirections[8];
uniform vec3 uLightColors[8];
uniform float uLightIntensities[8];
uniform int uLightTypes[8];

void main() {
    vec3 normal = normalize(vNormal);
    vec3 viewDir = normalize(uCameraPosition - vPosition);
    
    vec3 ambient = vec3(0.0);
    vec3 diffuse = vec3(0.0);
    vec3 specular = vec3(0.0);
    
    for (int i = 0; i < 8; i++) {
        if (i >= uNumLights) break;
        
        if (uLightTypes[i] == 2) {
            ambient += uLightColors[i] * uLightIntensities[i];
        } else if (uLightTypes[i] == 0) {
            vec3 lightDir = normalize(uLightDirections[i]);
            float diff = max(dot(normal, lightDir), 0.0);
            diffuse += diff * uLightColors[i] * uLightIntensities[i];
            
            vec3 reflectDir = reflect(-lightDir, normal);
            float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
            specular += spec * uLightColors[i] * uLightIntensities[i] * 0.5;
        } else if (uLightTypes[i] == 1) {
            vec3 lightDir = normalize(uLightPositions[i] - vPosition);
            float distance = length(uLightPositions[i] - vPosition);
            float attenuation = 1.0 / (1.0 + 0.1 * distance + 0.01 * distance * distance);
            
            float diff = max(dot(normal, lightDir), 0.0);
            diffuse += diff * uLightColors[i] * uLightIntensities[i] * attenuation;
            
            vec3 reflectDir = reflect(-lightDir, normal);
            float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
            specular += spec * uLightColors[i] * uLightIntensities[i] * 0.5 * attenuation;
        }
    }
    
    vec3 color = (ambient + diffuse) * vColor + specular;
    gl_FragColor = vec4(pow(color, vec3(0.4545)), 1.0);
}
`;

export class RasterRenderer {
    private gl: WebGLRenderingContext | WebGL2RenderingContext;
    private shaderProgram: ShaderProgram;
    private vertexBuffers: Map<string, WebGLBuffer> = new Map();
    private indexBuffers: Map<string, WebGLBuffer> = new Map();
    private normalBuffers: Map<string, WebGLBuffer> = new Map();
    private colorBuffers: Map<string, WebGLBuffer> = new Map();
    private indexCounts: Map<string, number> = new Map();

    constructor(gl: WebGLRenderingContext | WebGL2RenderingContext) {
        this.gl = gl;
        this.shaderProgram = new ShaderProgram(gl, vertexShader, fragmentShader);
        this.setupGL();
    }

    private setupGL(): void {
        this.gl.clearColor(0.1, 0.1, 0.15, 1.0);
        this.gl.enable(this.gl.DEPTH_TEST);
        this.gl.enable(this.gl.CULL_FACE);
        this.gl.cullFace(this.gl.BACK);
    }

    clear(): void {
        this.gl.clear(this.gl.COLOR_BUFFER_BIT | this.gl.DEPTH_BUFFER_BIT);
    }

    render(camera: Camera, scene: Scene, deltaTime: number): void {
        this.shaderProgram.use();

        this.shaderProgram.setUniform3f('uCameraPosition', camera.position.x, camera.position.y, camera.position.z);

        const viewMatrix = camera.getViewMatrix();
        const projectionMatrix = camera.getProjectionMatrix();

        this.shaderProgram.setUniformMatrix4('uViewMatrix', viewMatrix.elements);
        this.shaderProgram.setUniformMatrix4('uProjectionMatrix', projectionMatrix.elements);

        const lights = scene.getLights();
        const numLights = Math.min(lights.length, 8);
        this.shaderProgram.setUniform1i('uNumLights', numLights);

        for (let i = 0; i < numLights; i++) {
            const light = lights[i];

            const typeLocation = this.shaderProgram.getUniformLocation(`uLightTypes[${i}]`);
            if (typeLocation) this.gl.uniform1i(typeLocation, light.type);

            const posLocation = this.shaderProgram.getUniformLocation(`uLightPositions[${i}]`);
            if (posLocation) this.gl.uniform3f(posLocation, light.position.x, light.position.y, light.position.z);

            const dirLocation = this.shaderProgram.getUniformLocation(`uLightDirections[${i}]`);
            if (dirLocation) this.gl.uniform3f(dirLocation, light.direction.x, light.direction.y, light.direction.z);

            const colorLocation = this.shaderProgram.getUniformLocation(`uLightColors[${i}]`);
            if (colorLocation) this.gl.uniform3f(colorLocation, light.color.r, light.color.g, light.color.b);

            const intensityLocation = this.shaderProgram.getUniformLocation(`uLightIntensities[${i}]`);
            if (intensityLocation) this.gl.uniform1f(intensityLocation, light.intensity);
        }

        const meshes = scene.getMeshes();
        for (const mesh of meshes) {
            this.renderMesh(mesh);
        }
    }

    private renderMesh(mesh: any): void {
        const meshId = this.getMeshId(mesh);

        if (!this.vertexBuffers.has(meshId)) {
            this.uploadMeshData(mesh, meshId);
        }

        const modelMatrix = this.getModelMatrix(mesh);
        this.shaderProgram.setUniformMatrix4('uModelMatrix', modelMatrix);

        const positionLoc = this.shaderProgram.getAttributeLocation('aPosition');
        const normalLoc = this.shaderProgram.getAttributeLocation('aNormal');
        const colorLoc = this.shaderProgram.getAttributeLocation('aColor');

        const vertexBuffer = this.vertexBuffers.get(meshId);
        if (vertexBuffer && positionLoc >= 0) {
            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, vertexBuffer);
            this.gl.enableVertexAttribArray(positionLoc);
            this.gl.vertexAttribPointer(positionLoc, 3, this.gl.FLOAT, false, 0, 0);
        }

        const normalBuffer = this.normalBuffers.get(meshId);
        if (normalBuffer && normalLoc >= 0) {
            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, normalBuffer);
            this.gl.enableVertexAttribArray(normalLoc);
            this.gl.vertexAttribPointer(normalLoc, 3, this.gl.FLOAT, false, 0, 0);
        }

        const colorBuffer = this.colorBuffers.get(meshId);
        if (colorBuffer && colorLoc >= 0) {
            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, colorBuffer);
            this.gl.enableVertexAttribArray(colorLoc);
            this.gl.vertexAttribPointer(colorLoc, 3, this.gl.FLOAT, false, 0, 0);
        }

        const indexBuffer = this.indexBuffers.get(meshId);
        const indexCount = this.indexCounts.get(meshId);
        if (indexBuffer && indexCount) {
            this.gl.bindBuffer(this.gl.ELEMENT_ARRAY_BUFFER, indexBuffer);
            this.gl.drawElements(this.gl.TRIANGLES, indexCount, this.gl.UNSIGNED_SHORT, 0);
        }
    }

    private uploadMeshData(mesh: any, meshId: string): void {
        const vertexBuffer = this.gl.createBuffer();
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, vertexBuffer);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, mesh.vertices, this.gl.STATIC_DRAW);
        this.vertexBuffers.set(meshId, vertexBuffer!);

        const normalBuffer = this.gl.createBuffer();
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, normalBuffer);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, mesh.normals, this.gl.STATIC_DRAW);
        this.normalBuffers.set(meshId, normalBuffer!);

        const vertexCount = mesh.vertices.length / 3;
        const colorData = new Float32Array(vertexCount * 3);
        for (let i = 0; i < vertexCount; i++) {
            colorData[i * 3] = mesh.color.r;
            colorData[i * 3 + 1] = mesh.color.g;
            colorData[i * 3 + 2] = mesh.color.b;
        }
        const colorBuffer = this.gl.createBuffer();
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, colorBuffer);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, colorData, this.gl.STATIC_DRAW);
        this.colorBuffers.set(meshId, colorBuffer!);

        const indexBuffer = this.gl.createBuffer();
        this.gl.bindBuffer(this.gl.ELEMENT_ARRAY_BUFFER, indexBuffer);
        this.gl.bufferData(this.gl.ELEMENT_ARRAY_BUFFER, mesh.indices, this.gl.STATIC_DRAW);
        this.indexBuffers.set(meshId, indexBuffer!);
        this.indexCounts.set(meshId, mesh.indices.length);
    }

    private getMeshId(mesh: any): string {
        return `${mesh.position.x}_${mesh.position.y}_${mesh.position.z}_${mesh.vertices.length}`;
    }

    private getModelMatrix(mesh: any): Float32Array {
        const matrix = new Float32Array(16);
        matrix[0] = mesh.scale.x;  matrix[4] = 0;             matrix[8] = 0;             matrix[12] = mesh.position.x;
        matrix[1] = 0;             matrix[5] = mesh.scale.y;  matrix[9] = 0;             matrix[13] = mesh.position.y;
        matrix[2] = 0;             matrix[6] = 0;             matrix[10] = mesh.scale.z; matrix[14] = mesh.position.z;
        matrix[3] = 0;             matrix[7] = 0;             matrix[11] = 0;            matrix[15] = 1;
        return matrix;
    }

    dispose(): void {
        this.vertexBuffers.forEach(buffer => this.gl.deleteBuffer(buffer));
        this.indexBuffers.forEach(buffer => this.gl.deleteBuffer(buffer));
        this.normalBuffers.forEach(buffer => this.gl.deleteBuffer(buffer));
        this.colorBuffers.forEach(buffer => this.gl.deleteBuffer(buffer));
        this.vertexBuffers.clear();
        this.indexBuffers.clear();
        this.normalBuffers.clear();
        this.colorBuffers.clear();
        this.indexCounts.clear();
        this.shaderProgram.dispose();
    }
}

