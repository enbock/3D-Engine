export class ShaderProgram {
    public program: WebGLProgram;
    private gl: WebGLRenderingContext | WebGL2RenderingContext;
    private uniforms: Map<string, WebGLUniformLocation> = new Map();
    private attributes: Map<string, number> = new Map();

    constructor(
        gl: WebGLRenderingContext | WebGL2RenderingContext,
        vertexSource: string,
        fragmentSource: string
    ) {
        this.gl = gl;
        this.program = this.createProgram(vertexSource, fragmentSource);
        this.cacheLocations();
    }

    private createShader(type: number, source: string): WebGLShader {
        const shader = this.gl.createShader(type);
        if (!shader) {
            throw new Error('Failed to create shader');
        }

        this.gl.shaderSource(shader, source);
        this.gl.compileShader(shader);

        if (!this.gl.getShaderParameter(shader, this.gl.COMPILE_STATUS)) {
            const info = this.gl.getShaderInfoLog(shader);
            this.gl.deleteShader(shader);
            throw new Error('Shader compilation error: ' + info);
        }

        return shader;
    }

    private createProgram(vertexSource: string, fragmentSource: string): WebGLProgram {
        const vertexShader = this.createShader(this.gl.VERTEX_SHADER, vertexSource);
        const fragmentShader = this.createShader(this.gl.FRAGMENT_SHADER, fragmentSource);

        const program = this.gl.createProgram();
        if (!program) {
            throw new Error('Failed to create program');
        }

        this.gl.attachShader(program, vertexShader);
        this.gl.attachShader(program, fragmentShader);
        this.gl.linkProgram(program);

        if (!this.gl.getProgramParameter(program, this.gl.LINK_STATUS)) {
            const info = this.gl.getProgramInfoLog(program);
            this.gl.deleteProgram(program);
            throw new Error('Program linking error: ' + info);
        }

        this.gl.deleteShader(vertexShader);
        this.gl.deleteShader(fragmentShader);

        return program;
    }

    private cacheLocations(): void {
        const numUniforms = this.gl.getProgramParameter(this.program, this.gl.ACTIVE_UNIFORMS);
        for (let i = 0; i < numUniforms; i++) {
            const info = this.gl.getActiveUniform(this.program, i);
            if (info) {
                const location = this.gl.getUniformLocation(this.program, info.name);
                if (location) {
                    this.uniforms.set(info.name, location);
                }
            }
        }

        const numAttributes = this.gl.getProgramParameter(this.program, this.gl.ACTIVE_ATTRIBUTES);
        for (let i = 0; i < numAttributes; i++) {
            const info = this.gl.getActiveAttrib(this.program, i);
            if (info) {
                const location = this.gl.getAttribLocation(this.program, info.name);
                this.attributes.set(info.name, location);
            }
        }
    }

    use(): void {
        this.gl.useProgram(this.program);
    }

    getUniformLocation(name: string): WebGLUniformLocation | null {
        const cached = this.uniforms.get(name);
        if (cached) {
            return cached;
        }
        return this.gl.getUniformLocation(this.program, name);
    }

    getAttributeLocation(name: string): number {
        return this.attributes.get(name) ?? -1;
    }

    setUniformMatrix4(name: string, matrix: Float32Array): void {
        const location = this.getUniformLocation(name);
        if (location) {
            this.gl.uniformMatrix4fv(location, false, matrix);
        }
    }

    setUniform4f(name: string, x: number, y: number, z: number, w: number): void {
        const location = this.getUniformLocation(name);
        if (location) {
            this.gl.uniform4f(location, x, y, z, w);
        }
    }

    setUniform3f(name: string, x: number, y: number, z: number): void {
        const location = this.getUniformLocation(name);
        if (location) {
            this.gl.uniform3f(location, x, y, z);
        }
    }

    setUniform2f(name: string, x: number, y: number): void {
        const location = this.getUniformLocation(name);
        if (location) {
            this.gl.uniform2f(location, x, y);
        }
    }

    setUniform1f(name: string, x: number): void {
        const location = this.getUniformLocation(name);
        if (location) {
            this.gl.uniform1f(location, x);
        }
    }

    setUniform1i(name: string, x: number): void {
        const location = this.getUniformLocation(name);
        if (location) {
            this.gl.uniform1i(location, x);
        }
    }

    dispose(): void {
        this.gl.deleteProgram(this.program);
    }
}

