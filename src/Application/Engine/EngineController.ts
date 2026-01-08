import { Engine } from '../../Core/Engine';
import { EngineConfig } from '../EngineConfig';
import { Scene } from '../../Core/Scene';
import { Camera } from '../../Core/Camera';
import { CameraController } from '../Camera/CameraController';

export class EngineController {
    private engine: Engine | null = null;
    private canvas: HTMLCanvasElement;
    private isInitialized: boolean = false;
    private cameraController: CameraController | null = null;

    constructor(private config: EngineConfig) {
        this.canvas = config.canvas;
    }

    public async initialize(): Promise<void> {
        if (this.isInitialized) {
            console.warn('Engine wurde bereits initialisiert');
            return;
        }

        try {
            console.log('🚀 Initialisiere Engine Controller...');

            this.engine = new Engine(this.config);
            await this.engine.initialize();

            const camera = this.engine.getCamera();
            this.cameraController = new CameraController(this.canvas, camera);

            this.engine.setUpdateCallback((deltaTime: number) => {
                if (this.cameraController) {
                    this.cameraController.update(deltaTime);
                }
            });

            this.isInitialized = true;
            console.log('✅ Engine Controller erfolgreich initialisiert');
        } catch (error) {
            console.error('❌ Fehler beim Initialisieren des Engine Controllers:', error);
            throw error;
        }
    }

    public start(): void {
        if (!this.engine) {
            throw new Error('Engine muss zuerst initialisiert werden');
        }
        this.engine.start();
    }

    public stop(): void {
        if (this.engine) {
            this.engine.stop();
        }
    }

    public resize(width: number, height: number): void {
        if (this.engine) {
            this.engine.resize(width, height);
        }
    }

    public update(): void {
        if (this.cameraController) {
            this.cameraController.update();
        }
    }

    public getFPS(): number {
        return this.engine ? this.engine.getFPS() : 0;
    }

    public isRunning(): boolean {
        return this.engine ? this.engine.isRunning() : false;
    }

    public dispose(): void {
        if (this.cameraController) {
            this.cameraController.dispose();
            this.cameraController = null;
        }
        if (this.engine) {
            this.engine.dispose();
            this.engine = null;
        }
        this.isInitialized = false;
        console.log('Engine Controller Ressourcen freigegeben');
    }

    public getScene(): Scene | null {
        return this.engine ? this.engine.getScene() : null;
    }

    public getCamera(): Camera | null {
        return this.engine ? this.engine.getCamera() : null;
    }

    public getCameraController(): CameraController | null {
        return this.cameraController;
    }

    public getEngine(): Engine | null {
        return this.engine;
    }
}

