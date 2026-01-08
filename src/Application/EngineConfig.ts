
export interface EngineConfig {
    canvas: HTMLCanvasElement;
    width: number;
    height: number;
    antialias?: boolean;
    powerPreference?: 'default' | 'high-performance' | 'low-power';
}

