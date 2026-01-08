export class Color {
    constructor(
        public r: number = 1,
        public g: number = 1,
        public b: number = 1,
        public a: number = 1
    ) {}

    static white(): Color {
        return new Color(1, 1, 1, 1);
    }

    static black(): Color {
        return new Color(0, 0, 0, 1);
    }

    static red(): Color {
        return new Color(1, 0, 0, 1);
    }

    static green(): Color {
        return new Color(0, 1, 0, 1);
    }

    static blue(): Color {
        return new Color(0, 0, 1, 1);
    }

    static yellow(): Color {
        return new Color(1, 1, 0, 1);
    }

    static cyan(): Color {
        return new Color(0, 1, 1, 1);
    }

    static magenta(): Color {
        return new Color(1, 0, 1, 1);
    }

    static gray(): Color {
        return new Color(0.5, 0.5, 0.5, 1);
    }

    toArray(): number[] {
        return [this.r, this.g, this.b, this.a];
    }
}

