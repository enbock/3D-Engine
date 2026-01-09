export class Matrix4 {
    public elements: Float32Array;

    constructor() {
        this.elements = new Float32Array(16);
        this.identity();
    }

    identity(): Matrix4 {
        const e = this.elements;
        e[0] = 1; e[4] = 0; e[8] = 0; e[12] = 0;
        e[1] = 0; e[5] = 1; e[9] = 0; e[13] = 0;
        e[2] = 0; e[6] = 0; e[10] = 1; e[14] = 0;
        e[3] = 0; e[7] = 0; e[11] = 0; e[15] = 1;
        return this;
    }

    perspective(fov: number, aspect: number, near: number, far: number): Matrix4 {
        const f = 1.0 / Math.tan(fov / 2);
        const nf = 1 / (near - far);
        const e = this.elements;

        e[0] = f / aspect; e[4] = 0; e[8] = 0; e[12] = 0;
        e[1] = 0; e[5] = f; e[9] = 0; e[13] = 0;
        e[2] = 0; e[6] = 0; e[10] = (far + near) * nf; e[14] = 2 * far * near * nf;
        e[3] = 0; e[7] = 0; e[11] = -1; e[15] = 0;

        return this;
    }

    lookAt(eye: Float32Array, center: Float32Array, up: Float32Array): Matrix4 {
        let fx = center[0] - eye[0];
        let fy = center[1] - eye[1];
        let fz = center[2] - eye[2];
        const rlf = 1 / Math.sqrt(fx * fx + fy * fy + fz * fz);
        fx *= rlf;
        fy *= rlf;
        fz *= rlf;

        let sx = fy * up[2] - fz * up[1];
        let sy = fz * up[0] - fx * up[2];
        let sz = fx * up[1] - fy * up[0];
        const rls = 1 / Math.sqrt(sx * sx + sy * sy + sz * sz);
        sx *= rls;
        sy *= rls;
        sz *= rls;

        const ux = sy * fz - sz * fy;
        const uy = sz * fx - sx * fz;
        const uz = sx * fy - sy * fx;

        const e = this.elements;
        e[0] = sx;  e[4] = ux;  e[8] = -fx;  e[12] = 0;
        e[1] = sy;  e[5] = uy;  e[9] = -fy;  e[13] = 0;
        e[2] = sz;  e[6] = uz;  e[10] = -fz;  e[14] = 0;
        e[3] = 0;   e[7] = 0;   e[11] = 0;    e[15] = 1;

        return this.translate(-eye[0], -eye[1], -eye[2]);
    }

    translate(x: number, y: number, z: number): Matrix4 {
        const e = this.elements;
        e[12] += e[0] * x + e[4] * y + e[8] * z;
        e[13] += e[1] * x + e[5] * y + e[9] * z;
        e[14] += e[2] * x + e[6] * y + e[10] * z;
        e[15] += e[3] * x + e[7] * y + e[11] * z;
        return this;
    }

    multiply(m: Matrix4): Matrix4 {
        const a = this.elements;
        const b = m.elements;
        const result = new Float32Array(16);

        for (let i = 0; i < 4; i++) {
            for (let j = 0; j < 4; j++) {
                result[i * 4 + j] =
                    a[i * 4 + 0] * b[0 * 4 + j] +
                    a[i * 4 + 1] * b[1 * 4 + j] +
                    a[i * 4 + 2] * b[2 * 4 + j] +
                    a[i * 4 + 3] * b[3 * 4 + j];
            }
        }

        this.elements = result;
        return this;
    }

    rotateX(angle: number): Matrix4 {
        const c = Math.cos(angle);
        const s = Math.sin(angle);
        const e = this.elements;

        const m1 = e[1], m2 = e[2];
        const m5 = e[5], m6 = e[6];
        const m9 = e[9], m10 = e[10];
        const m13 = e[13], m14 = e[14];

        e[1] = m1 * c + m2 * s;
        e[2] = m2 * c - m1 * s;
        e[5] = m5 * c + m6 * s;
        e[6] = m6 * c - m5 * s;
        e[9] = m9 * c + m10 * s;
        e[10] = m10 * c - m9 * s;
        e[13] = m13 * c + m14 * s;
        e[14] = m14 * c - m13 * s;

        return this;
    }

    rotateY(angle: number): Matrix4 {
        const c = Math.cos(angle);
        const s = Math.sin(angle);
        const e = this.elements;

        const m0 = e[0], m2 = e[2];
        const m4 = e[4], m6 = e[6];
        const m8 = e[8], m10 = e[10];
        const m12 = e[12], m14 = e[14];

        e[0] = m0 * c - m2 * s;
        e[2] = m0 * s + m2 * c;
        e[4] = m4 * c - m6 * s;
        e[6] = m4 * s + m6 * c;
        e[8] = m8 * c - m10 * s;
        e[10] = m8 * s + m10 * c;
        e[12] = m12 * c - m14 * s;
        e[14] = m12 * s + m14 * c;

        return this;
    }

    rotateZ(angle: number): Matrix4 {
        const c = Math.cos(angle);
        const s = Math.sin(angle);
        const e = this.elements;

        const m0 = e[0], m1 = e[1];
        const m4 = e[4], m5 = e[5];
        const m8 = e[8], m9 = e[9];
        const m12 = e[12], m13 = e[13];

        e[0] = m0 * c + m1 * s;
        e[1] = m1 * c - m0 * s;
        e[4] = m4 * c + m5 * s;
        e[5] = m5 * c - m4 * s;
        e[8] = m8 * c + m9 * s;
        e[9] = m9 * c - m8 * s;
        e[12] = m12 * c + m13 * s;
        e[13] = m13 * c - m12 * s;

        return this;
    }

    scale(x: number, y: number, z: number): Matrix4 {
        const e = this.elements;
        e[0] *= x; e[4] *= y; e[8] *= z;
        e[1] *= x; e[5] *= y; e[9] *= z;
        e[2] *= x; e[6] *= y; e[10] *= z;
        e[3] *= x; e[7] *= y; e[11] *= z;
        return this;
    }

    clone(): Matrix4 {
        const m = new Matrix4();
        m.elements.set(this.elements);
        return m;
    }
}

