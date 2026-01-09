import { EngineConfig } from '../Application/EngineConfig';
import { Scene } from './Scene';
import { Camera } from './Camera';
import { Vector3 } from './Math';
import { RendererManager, RenderMode } from '../Infrastructure/Rendering/RendererManager';

export class Engine {
    private config: EngineConfig;
    private canvas: HTMLCanvasElement;
    private context: WebGLRenderingContext | WebGL2RenderingContext | null = null;
    private running: boolean = false;
    private lastFrameTime: number = 0;
    private frameCount: number = 0;

    private scene: Scene;
    private camera: Camera;
    private rendererManager: RendererManager | null = null;
    private updateCallback: ((deltaTime: number) => void) | null = null;

    constructor(config: EngineConfig) {
        this.config = config;
        this.canvas = config.canvas;
        this.scene = new Scene();
        this.camera = new Camera(
            new Vector3(0, 2, 12),
            Vector3.zero(),
            45 * Math.PI / 180,
            config.width / config.height
        );
    }

    public async initialize(): Promise<void> {
        console.log('Initialisiere Engine Core...');

        this.initializeContext();
        this.resize(this.config.width, this.config.height);
    }

    private initializeContext(): void {
        const contextAttributes: WebGLContextAttributes = {
            antialias: this.config.antialias,
            powerPreference: this.config.powerPreference,
            alpha: false,
            depth: true,
            stencil: false,
        };

        this.context = this.canvas.getContext('webgl2', contextAttributes)
                    || this.canvas.getContext('webgl', contextAttributes);

        if (!this.context) {
            throw new Error('WebGL wird von diesem Browser nicht unterstützt');
        }

        console.log(`WebGL Context initialisiert: ${this.context instanceof WebGL2RenderingContext ? 'WebGL2' : 'WebGL1'}`);

        this.context.clearColor(0.0, 0.0, 0.0, 1.0);

        this.rendererManager = new RendererManager(this.context);
    }

    public start(): void {
        if (this.running) return;

        this.running = true;
        this.lastFrameTime = performance.now();
        this.renderLoop(this.lastFrameTime);

        console.log('Engine Render Loop gestartet');
    }

    public stop(): void {
        this.running = false;
        console.log('Engine Render Loop gestoppt');
    }

    public isRunning(): boolean {
        return this.running;
    }

    private renderLoop(currentTime: number): void {
        if (!this.running) return;

        const deltaTime = (currentTime - this.lastFrameTime) / 1000;
        this.lastFrameTime = currentTime;
        this.frameCount++;

        this.update(deltaTime);
        this.render();

        requestAnimationFrame((time) => this.renderLoop(time));
    }

    private update(deltaTime: number): void {
        if (this.updateCallback) {
            this.updateCallback(deltaTime);
        }
    }

    private render(): void {
        if (!this.context || !this.rendererManager) return;

        this.rendererManager.clear();

        const deltaTime = (performance.now() - this.lastFrameTime) / 1000;
        this.rendererManager.render(this.camera, this.scene, deltaTime);
    }

    public resize(width: number, height: number): void {
        this.canvas.width = width;
        this.canvas.height = height;

        if (this.context) {
            this.context.viewport(0, 0, width, height);
        }

        this.camera.setAspect(width / height);

        console.log(`Viewport angepasst: ${width}x${height}`);
    }

    public getFPS(): number {
        const currentTime = performance.now();
        const deltaTime = (currentTime - this.lastFrameTime) / 1000;
        return deltaTime > 0 ? 1 / deltaTime : 0;
    }

    public dispose(): void {
        this.stop();
        if (this.rendererManager) {
            this.rendererManager.dispose();
        }
        this.context = null;
        console.log('Engine Ressourcen freigegeben');
    }

    public getScene(): Scene {
        return this.scene;
    }

    public getCamera(): Camera {
        return this.camera;
    }

    public setUpdateCallback(callback: (deltaTime: number) => void): void {
        this.updateCallback = callback;
    }

    public setRenderMode(mode: RenderMode): void {
        if (this.rendererManager) {
            this.rendererManager.setRenderMode(mode);
        }
    }

    public getRenderMode(): RenderMode | null {
        return this.rendererManager ? this.rendererManager.getRenderMode() : null;
    }
}

