import {EngineController} from './Application/Engine/EngineController';
import {EngineConfig} from './Application/EngineConfig';
import {SceneBuilder} from './Application/Scene/SceneBuilder';
import {Mesh} from './Core/Geometry/Mesh';

class Application {
    private engineController: EngineController | null = null;
    private cameraAngle: number = 0;

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
            }

            this.updateInfoPanel('Engine erfolgreich initialisiert!');

            window.addEventListener('resize', this.handleResize.bind(this));


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

