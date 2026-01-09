import {EngineController} from './Application/Engine/EngineController';
import {EngineConfig} from './Application/EngineConfig';
import {SceneBuilder} from './Application/Scene/SceneBuilder';
import {Mesh} from './Core/Geometry/Mesh';
import {RenderMode} from './Infrastructure/Rendering/RendererManager';

class Application {
    private engineController: EngineController | null = null;

    constructor() {
        this.initialize();
    }

    private async initialize(): Promise<void> {
        try {
            console.log('🚀 3D Engine wird gestartet...');

            const canvas = document.getElementById('canvas') as HTMLCanvasElement;
            if (!canvas) {
                throw new Error('Canvas element nicht gefunden');
            }

            const config: EngineConfig = {
                canvas,
                width: window.innerWidth,
                height: window.innerHeight,
                antialias: true,
                powerPreference: 'high-performance',
            };

            this.engineController = new EngineController(config);
            await this.engineController.initialize();

            const demoScene = SceneBuilder.createDemoScene();
            const scene = this.engineController.getScene();
            if (scene) {
                demoScene.getMeshes().forEach((mesh: Mesh) => scene.addMesh(mesh));
                demoScene.getLights().forEach(light => scene.addLight(light));
            }

            this.engineController.start();

            window.addEventListener('resize', this.handleResize.bind(this));
            this.setupToggleButton();

            console.log('✅ 3D Engine erfolgreich gestartet');
        } catch (error) {
            console.error('❌ Fehler beim Initialisieren der Engine:', error);
            this.updateInfoPanel('Fehler beim Initialisieren der Engine');
        }
    }

    private handleResize(): void {
        if (this.engineController) {
            this.engineController.resize(window.innerWidth, window.innerHeight);
        }
    }

    private setupToggleButton(): void {
        const button = document.getElementById('toggleRenderer');
        const modeLabel = document.getElementById('renderMode');

        if (!button || !modeLabel) return;

        button.addEventListener('click', () => {
            if (!this.engineController) return;

            const currentMode = this.engineController.getRenderMode();
            const newMode = currentMode === RenderMode.Raytracing
                ? RenderMode.Raster
                : RenderMode.Raytracing;

            this.engineController.setRenderMode(newMode);

            if (newMode === RenderMode.Raytracing) {
                modeLabel.textContent = 'Raytracing';
                button.textContent = 'Zu Raster wechseln';
                button.classList.remove('raster');
            } else {
                modeLabel.textContent = 'Raster';
                button.textContent = 'Zu Raytracing wechseln';
                button.classList.add('raster');
            }
        });
    }

    private updateInfoPanel(message: string): void {
        const infoPanel = document.querySelector('.info-panel p');
        if (infoPanel) {
            infoPanel.textContent = message;
        }
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => new Application());
} else {
    new Application();
}

export default Application;

