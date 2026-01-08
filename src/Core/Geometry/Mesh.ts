import { Vector3, Color } from '../Math';

export interface MeshData {
    vertices: number[];
    indices: number[];
    normals: number[];
    colors?: number[];
}

export class Mesh {
    public position: Vector3 = Vector3.zero();
    public rotation: Vector3 = Vector3.zero();
    public scale: Vector3 = Vector3.one();
    public color: Color = Color.white();

    constructor(
        public vertices: Float32Array,
        public indices: Uint16Array,
        public normals: Float32Array,
        public colors?: Float32Array
    ) {}

    static createCube(size: number = 1): Mesh {
        const s = size / 2;

        const vertices = new Float32Array([
            -s, -s, s,  s, -s, s,  s, s, s,  -s, s, s,
            -s, -s, -s,  -s, s, -s,  s, s, -s,  s, -s, -s,
            -s, s, -s,  -s, s, s,  s, s, s,  s, s, -s,
            -s, -s, -s,  s, -s, -s,  s, -s, s,  -s, -s, s,
            s, -s, -s,  s, s, -s,  s, s, s,  s, -s, s,
            -s, -s, -s,  -s, -s, s,  -s, s, s,  -s, s, -s
        ]);

        const indices = new Uint16Array([
            0, 1, 2, 0, 2, 3,
            4, 5, 6, 4, 6, 7,
            8, 9, 10, 8, 10, 11,
            12, 13, 14, 12, 14, 15,
            16, 17, 18, 16, 18, 19,
            20, 21, 22, 20, 22, 23
        ]);

        const normals = new Float32Array([
            0, 0, 1,  0, 0, 1,  0, 0, 1,  0, 0, 1,
            0, 0, -1,  0, 0, -1,  0, 0, -1,  0, 0, -1,
            0, 1, 0,  0, 1, 0,  0, 1, 0,  0, 1, 0,
            0, -1, 0,  0, -1, 0,  0, -1, 0,  0, -1, 0,
            1, 0, 0,  1, 0, 0,  1, 0, 0,  1, 0, 0,
            -1, 0, 0,  -1, 0, 0,  -1, 0, 0,  -1, 0, 0
        ]);

        return new Mesh(vertices, indices, normals);
    }

    static createSphere(radius: number = 1, segments: number = 16, rings: number = 16): Mesh {
        const vertices: number[] = [];
        const indices: number[] = [];
        const normals: number[] = [];

        for (let ring = 0; ring <= rings; ring++) {
            const phi = (ring * Math.PI) / rings;
            const sinPhi = Math.sin(phi);
            const cosPhi = Math.cos(phi);

            for (let seg = 0; seg <= segments; seg++) {
                const theta = (seg * 2 * Math.PI) / segments;
                const sinTheta = Math.sin(theta);
                const cosTheta = Math.cos(theta);

                const x = cosTheta * sinPhi;
                const y = cosPhi;
                const z = sinTheta * sinPhi;

                vertices.push(x * radius, y * radius, z * radius);
                normals.push(x, y, z);
            }
        }

        for (let ring = 0; ring < rings; ring++) {
            for (let seg = 0; seg < segments; seg++) {
                const first = ring * (segments + 1) + seg;
                const second = first + segments + 1;

                indices.push(first, second, first + 1);
                indices.push(second, second + 1, first + 1);
            }
        }

        return new Mesh(
            new Float32Array(vertices),
            new Uint16Array(indices),
            new Float32Array(normals)
        );
    }

    static createPlane(width: number = 1, height: number = 1): Mesh {
        const w = width / 2;
        const h = height / 2;

        const vertices = new Float32Array([
            -w, 0, -h,
            w, 0, -h,
            w, 0, h,
            -w, 0, h
        ]);

        const indices = new Uint16Array([
            0, 1, 2,
            0, 2, 3
        ]);

        const normals = new Float32Array([
            0, 1, 0,
            0, 1, 0,
            0, 1, 0,
            0, 1, 0
        ]);

        return new Mesh(vertices, indices, normals);
    }
}

