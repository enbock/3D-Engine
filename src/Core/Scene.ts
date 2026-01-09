import { Mesh } from './Geometry';
import { Light } from './Light';

export class Scene {
    public meshes: Mesh[] = [];
    public lights: Light[] = [];

    addMesh(mesh: Mesh): void {
        this.meshes.push(mesh);
    }

    removeMesh(mesh: Mesh): void {
        const index = this.meshes.indexOf(mesh);
        if (index > -1) {
            this.meshes.splice(index, 1);
        }
    }

    addLight(light: Light): void {
        this.lights.push(light);
    }

    removeLight(light: Light): void {
        const index = this.lights.indexOf(light);
        if (index > -1) {
            this.lights.splice(index, 1);
        }
    }

    clear(): void {
        this.meshes = [];
        this.lights = [];
    }

    getMeshes(): Mesh[] {
        return this.meshes;
    }

    getLights(): Light[] {
        return this.lights;
    }
}

