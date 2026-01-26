import { Vector3 } from './Math';
import { Color } from './Math';

export enum LightType {
    DIRECTIONAL = 0,
    POINT = 1,
    AMBIENT = 2
}

export class Light {
    public type: LightType;
    public position: Vector3;
    public direction: Vector3;
    public color: Color;
    public intensity: number;

    constructor(
        type: LightType = LightType.DIRECTIONAL,
        position: Vector3 = Vector3.zero(),
        direction: Vector3 = new Vector3(0, -1, 0),
        color: Color = Color.white(),
        intensity: number = 1.0
    ) {
        this.type = type;
        this.position = position;
        this.direction = direction.normalize();
        this.color = color;
        this.intensity = intensity;
    }

    static createDirectional(direction: Vector3, color: Color = Color.white(), intensity: number = 1.0): Light {
        return new Light(LightType.DIRECTIONAL, Vector3.zero(), direction, color, intensity);
    }

    static createPoint(position: Vector3, color: Color = Color.white(), intensity: number = 1.0): Light {
        return new Light(LightType.POINT, position, Vector3.zero(), color, intensity);
    }

    static createAmbient(color: Color = Color.white(), intensity: number = 0.15): Light {
        return new Light(LightType.AMBIENT, Vector3.zero(), Vector3.zero(), color, intensity);
    }
}


