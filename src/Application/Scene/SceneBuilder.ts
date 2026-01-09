import { Scene } from '../../Core/Scene';
import { Mesh } from '../../Core/Geometry/Mesh';
import { Color } from '../../Core/Math/Color';
import { Vector3 } from '../../Core/Math/Vector3';
import { Light } from '../../Core/Light';

export class SceneBuilder {
    static createDemoScene(): Scene {
        const scene = new Scene();

        const floor = Mesh.createPlane(20, 20);
        floor.position = new Vector3(0, -1, 0);
        floor.color = Color.gray();
        scene.addMesh(floor);

        const cube1 = Mesh.createCube(1);
        cube1.position = new Vector3(-3, 0, 0);
        cube1.color = Color.red();
        scene.addMesh(cube1);

        const cube2 = Mesh.createCube(0.8);
        cube2.position = new Vector3(0, 0, 0);
        cube2.color = Color.green();
        scene.addMesh(cube2);

        const cube3 = Mesh.createCube(1.2);
        cube3.position = new Vector3(3, 0, 0);
        cube3.color = Color.blue();
        scene.addMesh(cube3);

        const sphere1 = Mesh.createSphere(0.7, 20, 20);
        sphere1.position = new Vector3(-2, 2, -2);
        sphere1.color = Color.yellow();
        scene.addMesh(sphere1);

        const sphere2 = Mesh.createSphere(0.6, 20, 20);
        sphere2.position = new Vector3(2, 2, -2);
        sphere2.color = Color.cyan();
        scene.addMesh(sphere2);

        const sphere3 = Mesh.createSphere(0.5, 20, 20);
        sphere3.position = new Vector3(0, 3, -1);
        sphere3.color = Color.magenta();
        scene.addMesh(sphere3);

        const backWall = Mesh.createCube(1);
        backWall.position = new Vector3(0, 0.5, -5);
        backWall.scale = new Vector3(10, 3, 0.5);
        backWall.color = new Color(0.3, 0.3, 0.4, 1);
        scene.addMesh(backWall);

        const pillar1 = Mesh.createCube(1);
        pillar1.position = new Vector3(-5, 1, -3);
        pillar1.scale = new Vector3(0.5, 4, 0.5);
        pillar1.color = new Color(0.6, 0.4, 0.2, 1);
        scene.addMesh(pillar1);

        const pillar2 = Mesh.createCube(1);
        pillar2.position = new Vector3(5, 1, -3);
        pillar2.scale = new Vector3(0.5, 4, 0.5);
        pillar2.color = new Color(0.6, 0.4, 0.2, 1);
        scene.addMesh(pillar2);

        scene.addLight(Light.createAmbient(Color.white(), 0.15));
        scene.addLight(Light.createDirectional(new Vector3(0.5, 0.7, 1.0).normalize(), Color.white(), 1.0));
        scene.addLight(Light.createPoint(new Vector3(0, 2, 0), new Color(1.0, 0.9, 0.8, 1.0), 1.5));

        return scene;
    }

    static createRotatingScene(): Scene {
        const scene = new Scene();

        const centerCube = Mesh.createCube(1);
        centerCube.position = Vector3.zero();
        centerCube.color = Color.white();
        scene.addMesh(centerCube);

        for (let i = 0; i < 8; i++) {
            const angle = (i / 8) * Math.PI * 2;
            const radius = 3;

            const sphere = Mesh.createSphere(0.4, 16, 16);
            sphere.position = new Vector3(
                Math.cos(angle) * radius,
                Math.sin(i * 0.5),
                Math.sin(angle) * radius
            );

            const hue = i / 8;
            sphere.color = new Color(
                Math.abs(Math.sin(hue * Math.PI * 2)),
                Math.abs(Math.sin((hue + 0.33) * Math.PI * 2)),
                Math.abs(Math.sin((hue + 0.66) * Math.PI * 2)),
                1
            );

            scene.addMesh(sphere);
        }

        scene.addLight(Light.createAmbient(Color.white(), 0.2));
        scene.addLight(Light.createDirectional(new Vector3(1, 1, 1).normalize(), Color.white(), 0.8));

        return scene;
    }
}

