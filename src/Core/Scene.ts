import { Mesh } from './Geometry';

export class Scene {
    public meshes: Mesh[] = [];

    addMesh(mesh: Mesh): void {
        this.meshes.push(mesh);
    }

    removeMesh(mesh: Mesh): void {
        const index = this.meshes.indexOf(mesh);
        if (index > -1) {
            this.meshes.splice(index, 1);
        }
    }

    clear(): void {
        this.meshes = [];
    }

    getMeshes(): Mesh[] {
        return this.meshes;
    }
}

