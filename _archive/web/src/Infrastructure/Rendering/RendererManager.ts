import { RaytracingRenderer } from './RaytracingRenderer';
import { RasterRenderer } from './RasterRenderer';
import { Camera } from '../../Core/Camera';
import { Scene } from '../../Core/Scene';

export enum RenderMode {
    Raytracing = 'raytracing',
    Raster = 'raster'
}

export interface Renderer {
    clear(): void;
    render(camera: Camera, scene: Scene, deltaTime: number): void;
    dispose(): void;
}

export class RendererManager {
    private gl: WebGLRenderingContext | WebGL2RenderingContext;
    private raytracingRenderer: RaytracingRenderer;
    private rasterRenderer: RasterRenderer;
    private currentMode: RenderMode = RenderMode.Raytracing;

    constructor(gl: WebGLRenderingContext | WebGL2RenderingContext) {
        this.gl = gl;
        this.raytracingRenderer = new RaytracingRenderer(gl);
        this.rasterRenderer = new RasterRenderer(gl);
    }

    getCurrentRenderer(): Renderer {
        return this.currentMode === RenderMode.Raytracing
            ? this.raytracingRenderer
            : this.rasterRenderer;
    }

    setRenderMode(mode: RenderMode): void {
        this.currentMode = mode;
        console.log(`Render-Modus gewechselt zu: ${mode}`);
    }

    getRenderMode(): RenderMode {
        return this.currentMode;
    }

    clear(): void {
        this.getCurrentRenderer().clear();
    }

    render(camera: Camera, scene: Scene, deltaTime: number): void {
        this.getCurrentRenderer().render(camera, scene, deltaTime);
    }

    dispose(): void {
        this.raytracingRenderer.dispose();
        this.rasterRenderer.dispose();
    }
}

