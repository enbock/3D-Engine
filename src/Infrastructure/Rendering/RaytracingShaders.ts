export const raytracingVertexShader = `
attribute vec2 aPosition;
varying vec2 vUV;
void main() {
    vUV = aPosition * 0.5 + 0.5;
    gl_Position = vec4(aPosition, 0.0, 1.0);
}
`;

export const raytracingFragmentShader = `
precision highp float;
varying vec2 vUV;
uniform vec2 uResolution;
uniform vec3 uCameraPosition;
uniform vec3 uCameraTarget;

struct Ray { vec3 origin; vec3 direction; };
struct Hit { bool hit; float dist; vec3 point; vec3 normal; vec3 color; };

const float EPSILON = 0.001;
const float MAX_DIST = 100.0;

vec3 lightDir = normalize(vec3(0.5, 0.7, 1.0));

Hit intersectSphere(Ray ray, vec3 center, float radius, vec3 color) {
    Hit h; h.hit = false;
    vec3 oc = ray.origin - center;
    float b = dot(oc, ray.direction);
    float c = dot(oc, oc) - radius * radius;
    float d = b * b - c;
    if (d > 0.0) {
        float t = -b - sqrt(d);
        if (t > EPSILON) {
            h.hit = true; h.dist = t;
            h.point = ray.origin + ray.direction * t;
            h.normal = normalize(h.point - center);
            h.color = color;
        }
    }
    return h;
}

Hit intersectBox(Ray ray, vec3 bmin, vec3 bmax, vec3 color) {
    Hit h; h.hit = false;
    vec3 invD = 1.0 / ray.direction;
    vec3 t0 = (bmin - ray.origin) * invD;
    vec3 t1 = (bmax - ray.origin) * invD;
    vec3 tmin = min(t0, t1);
    vec3 tmax = max(t0, t1);
    float tn = max(max(tmin.x, tmin.y), tmin.z);
    float tf = min(min(tmax.x, tmax.y), tmax.z);
    if (tn < tf && tf > EPSILON) {
        float t = tn > EPSILON ? tn : tf;
        h.hit = true; h.dist = t;
        h.point = ray.origin + ray.direction * t;
        vec3 c = (bmin + bmax) * 0.5;
        vec3 p = h.point - c;
        vec3 d = (bmin - bmax) * 0.5;
        h.normal = normalize(sign(p) * step(abs(d.yzx), abs(p)) * step(abs(d.zxy), abs(p)));
        h.color = color;
    }
    return h;
}

Hit intersectPlane(Ray ray, vec3 p, vec3 n, vec3 color) {
    Hit h; h.hit = false;
    float d = dot(n, ray.direction);
    if (abs(d) > EPSILON) {
        float t = dot(p - ray.origin, n) / d;
        if (t > EPSILON) {
            h.hit = true; h.dist = t;
            h.point = ray.origin + ray.direction * t;
            h.normal = n;
            h.color = color;
        }
    }
    return h;
}

Hit trace(Ray ray) {
    Hit closest; closest.hit = false; closest.dist = MAX_DIST;
    
    Hit h1 = intersectSphere(ray, vec3(-2, 2, -2), 0.7, vec3(1, 1, 0));
    if (h1.hit && h1.dist < closest.dist) closest = h1;
    
    Hit h2 = intersectSphere(ray, vec3(2, 2, -2), 0.6, vec3(0, 1, 1));
    if (h2.hit && h2.dist < closest.dist) closest = h2;
    
    Hit h3 = intersectSphere(ray, vec3(0, 3, -1), 0.5, vec3(1, 0, 1));
    if (h3.hit && h3.dist < closest.dist) closest = h3;
    
    Hit b1 = intersectBox(ray, vec3(-3.5, -1, -0.5), vec3(-2.5, 0, 0.5), vec3(1, 0, 0));
    if (b1.hit && b1.dist < closest.dist) closest = b1;
    
    Hit b2 = intersectBox(ray, vec3(-0.4, -1, -0.4), vec3(0.4, 0, 0.4), vec3(0, 1, 0));
    if (b2.hit && b2.dist < closest.dist) closest = b2;
    
    Hit b3 = intersectBox(ray, vec3(2.4, -1, -0.6), vec3(3.6, 0.2, 0.6), vec3(0, 0, 1));
    if (b3.hit && b3.dist < closest.dist) closest = b3;
    
    Hit p1 = intersectPlane(ray, vec3(0, -1, 0), vec3(0, 1, 0), vec3(0.5, 0.5, 0.5));
    if (p1.hit && p1.dist < closest.dist) closest = p1;
    
    Hit p2 = intersectPlane(ray, vec3(0, 0, -5), vec3(0, 0, 1), vec3(0.3, 0.3, 0.4));
    if (p2.hit && p2.dist < closest.dist) closest = p2;
    
    return closest;
}

vec3 shade(Hit hit, vec3 rayDir) {
    vec3 ambient = vec3(0.1) * hit.color;
    
    Ray shadow; shadow.origin = hit.point + hit.normal * EPSILON * 10.0;
    shadow.direction = lightDir;
    Hit sh = trace(shadow);
    float shad = sh.hit ? 0.3 : 1.0;
    float diff = max(dot(hit.normal, lightDir), 0.0);
    vec3 h = normalize(lightDir - rayDir);
    float spec = pow(max(dot(hit.normal, h), 0.0), 32.0);
    return ambient + (diff * hit.color * 0.7 + vec3(spec) * 0.5) * shad;
}

void main() {
    vec2 uv = vUV * 2.0 - 1.0;
    uv.x *= uResolution.x / uResolution.y;
    
    vec3 forward = normalize(uCameraTarget - uCameraPosition);
    vec3 right = normalize(cross(forward, vec3(0, 1, 0)));
    vec3 up = cross(right, forward);
    
    float fov = 0.785398;
    vec3 rayDir = normalize(forward + right * uv.x * tan(fov * 0.5) + up * uv.y * tan(fov * 0.5));
    
    Ray ray; ray.origin = uCameraPosition; ray.direction = rayDir;
    Hit hit = trace(ray);
    
    vec3 color = vec3(0.1, 0.1, 0.15);
    if (hit.hit) {
        color = shade(hit, rayDir);
        vec3 refl = reflect(rayDir, hit.normal);
        ray.origin = hit.point + hit.normal * EPSILON * 10.0;
        ray.direction = refl;
        Hit hit2 = trace(ray);
        if (hit2.hit) {
            color += shade(hit2, refl) * 0.3;
        }
    } else {
        color = mix(vec3(0.1, 0.1, 0.15), vec3(0.5, 0.7, 1.0), rayDir.y * 0.5 + 0.5);
    }
    
    color = pow(color, vec3(0.4545));
    gl_FragColor = vec4(color, 1.0);
}
`;

